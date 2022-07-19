// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.App.Common;
using Energinet.DataHub.Core.App.Common.Abstractions.Actor;
using Energinet.DataHub.Core.App.Common.Abstractions.Identity;
using Energinet.DataHub.Core.App.Common.Abstractions.Security;
using Energinet.DataHub.Core.App.Common.Identity;
using Energinet.DataHub.Core.App.Common.Security;
using Energinet.DataHub.Core.App.FunctionApp.Middleware;
using Energinet.DataHub.Core.App.FunctionApp.SimpleInjector;
using Energinet.DataHub.Core.Logging.RequestResponseMiddleware;
using Energinet.DataHub.Core.Logging.RequestResponseMiddleware.Storage;
using Energinet.DataHub.Core.XmlConversion.XmlConverter.Configuration;
using Energinet.DataHub.Core.XmlConversion.XmlConverter.Extensions;
using Energinet.DataHub.MeteringPoints.Application.Common;
using Energinet.DataHub.MeteringPoints.Domain.BusinessProcesses;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common;
using Energinet.DataHub.MeteringPoints.EntryPoints.Ingestion.Functions;
using Energinet.DataHub.MeteringPoints.EntryPoints.Ingestion.Monitor;
using Energinet.DataHub.MeteringPoints.Infrastructure;
using Energinet.DataHub.MeteringPoints.Infrastructure.Configuration;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.XmlConverter.Mappings;
using Energinet.DataHub.MeteringPoints.Infrastructure.Ingestion;
using Energinet.DataHub.MeteringPoints.Infrastructure.Ingestion.Resilience;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf.Integration;
using Energinet.DataHub.MeteringPoints.RequestResponse.Contract;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleInjector;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Ingestion
{
    public class Program : EntryPoint
    {
        private static readonly string[] _functionNamesToExclude =
        {
            "HealthCheck",
        };

        public static async Task Main()
        {
            var program = new Program();

            var host = program.ConfigureApplication();
            program.AssertConfiguration();
            await program.ExecuteApplicationAsync(host).ConfigureAwait(false);
        }

        protected override void ConfigureFunctionsWorkerDefaults(IFunctionsWorkerApplicationBuilder options)
        {
            base.ConfigureFunctionsWorkerDefaults(options);

            options.UseMiddleware<JwtTokenMiddleware>();
            options.UseMiddleware<ActorMiddleware>();
            options.UseMiddleware<CorrelationIdMiddleware>();
            options.UseMiddleware<EntryPointTelemetryScopeMiddleware>();
            options.UseMiddleware<RequestResponseLoggingMiddleware>();
        }

        protected override void ConfigureServiceCollection(IServiceCollection services)
        {
            base.ConfigureServiceCollection(services);
            services.AddLiveHealthCheck();
            services.AddSqlServerHealthCheck(Environment.GetEnvironmentVariable("METERINGPOINT_DB_CONNECTION_STRING")!);
            services.AddInternalDomainServiceBusQueuesHealthCheck(
                Environment.GetEnvironmentVariable("METERINGPOINT_QUEUE_MANAGE_CONNECTION_STRING")!,
                Environment.GetEnvironmentVariable("METERINGPOINT_QUEUE_NAME")!);
        }

        protected override void ConfigureContainer(Container container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            base.ConfigureContainer(container);

            // Register application components.
            container.Register<MeteringPointHttpTrigger>(Lifestyle.Scoped);
            container.Register<HealthCheckEndpoint>(Lifestyle.Scoped);
            container.Register<ICorrelationContext, CorrelationContext>(Lifestyle.Scoped);
            container.Register<CorrelationIdMiddleware>(Lifestyle.Scoped);
            container.Register<EntryPointTelemetryScopeMiddleware>(Lifestyle.Scoped);
            container.Register(typeof(ProtobufOutboundMapper<>), typeof(ProtobufOutboundMapper<>).Assembly);
            container.Register<ProtobufOutboundMapperFactory>();
            container.Register<ProtobufInboundMapperFactory>();

            var tenantId = Environment.GetEnvironmentVariable("B2C_TENANT_ID") ?? throw new InvalidOperationException(
                "B2C tenant id not found.");
            var audience = Environment.GetEnvironmentVariable("BACKEND_SERVICE_APP_ID") ?? throw new InvalidOperationException(
                "Backend service app id not found.");

            // container.AddJwtTokenSecurity($"https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration", audience);
            container.Register<JwtTokenMiddleware>(() => new JwtTokenMiddleware(
                container.GetInstance<ClaimsPrincipalContext>(),
                container.GetInstance<IJwtTokenValidator>(),
                _functionNamesToExclude));
            container.Register<IJwtTokenValidator, JwtTokenValidator>(Lifestyle.Scoped);
            container.Register<IClaimsPrincipalAccessor, ClaimsPrincipalAccessor>(Lifestyle.Scoped);
            container.Register<ClaimsPrincipalContext>(Lifestyle.Scoped);
            container.Register(() => new OpenIdSettings($"https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration", audience));

            //container.AddActorContext<ActorProvider>();
            container.Register<ActorMiddleware>(() => new ActorMiddleware(
                container.GetInstance<IClaimsPrincipalAccessor>(),
                container.GetInstance<IActorProvider>(),
                container.GetInstance<IActorContext>(),
                _functionNamesToExclude));
            container.Register<IActorContext, ActorContext>(Lifestyle.Scoped);
            container.Register(typeof(IActorProvider), typeof(ActorProvider), Lifestyle.Scoped);
            var connectionString = Environment.GetEnvironmentVariable("METERINGPOINT_DB_CONNECTION_STRING")
                                   ?? throw new InvalidOperationException(
                                       "Metering point db connection string not found.");
            container.Register<IDbConnectionFactory>(() => new SqlDbConnectionFactory(connectionString), Lifestyle.Scoped);
            Dapper.SqlMapper.AddTypeHandler(NodaTimeSqlMapper.Instance);

            container.Register<XmlSenderValidator>(Lifestyle.Scoped);

            container.Register<MessageDispatcher, InternalDispatcher>(Lifestyle.Scoped);
            container.Register<Channel, InternalServiceBus>(Lifestyle.Scoped);

            container.RegisterSingleton<IRequestResponseLogging>(
                () =>
                {
                    var logger = container.GetService<ILogger<RequestResponseLoggingBlobStorage>>();
                    var storage = new RequestResponseLoggingBlobStorage(
                        Environment.GetEnvironmentVariable("REQUEST_RESPONSE_LOGGING_CONNECTION_STRING") ?? throw new InvalidOperationException(),
                        Environment.GetEnvironmentVariable("REQUEST_RESPONSE_LOGGING_CONTAINER_NAME") ?? throw new InvalidOperationException(),
                        logger ?? throw new InvalidOperationException());
                    return storage;
                });
            container.Register<RequestResponseLoggingMiddleware>(Lifestyle.Scoped);

            var policyRetryCount = int.TryParse(Environment.GetEnvironmentVariable("INTERNAL_SERVICEBUS_RETRY_COUNT"), out var parsedRetryCount) ? parsedRetryCount : 0;
            container.Register<IChannelResiliencePolicy>(() => new RetryNTimesPolicy(policyRetryCount), Lifestyle.Scoped);
            container.RegisterDecorator<Channel, ChannelResilienceDecorator>(Lifestyle.Scoped);

            container.AddXmlDeserialization(XmlMappingConfiguration, TranslateProcessType);

            var queueConnectionString = Environment.GetEnvironmentVariable("METERINGPOINT_QUEUE_SEND_CONNECTION_STRING");
            var topic = Environment.GetEnvironmentVariable("METERINGPOINT_QUEUE_NAME");
            container.Register(() => new ServiceBusClient(queueConnectionString).CreateSender(topic), Lifestyle.Singleton);

            container.SendProtobuf<MeteringPointEnvelope>();
        }

        private static XmlMappingConfigurationBase XmlMappingConfiguration(string documentType)
        {
            return documentType.ToUpperInvariant() switch
            {
                "E58" => new MasterDataDocumentXmlMappingConfiguration(),
                _ => throw new NotImplementedException(documentType),
            };
        }

        private static string TranslateProcessType(string processType)
        {
            return processType.ToUpperInvariant() switch
            {
                "E02" => nameof(BusinessProcessType.CreateMeteringPoint),
                "D15" => nameof(BusinessProcessType.ConnectMeteringPoint),
                "E79" => nameof(BusinessProcessType.DisconnectReconnectMeteringPoint),
                "E32" => nameof(BusinessProcessType.ChangeMasterData),
                "D14" => nameof(BusinessProcessType.CloseDownMeteringPoint),
                _ => throw new NotImplementedException(processType),
            };
        }
    }
}
