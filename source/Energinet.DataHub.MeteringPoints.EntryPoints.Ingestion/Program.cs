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
using Energinet.DataHub.Core.FunctionApp.Common;
using Energinet.DataHub.Core.FunctionApp.Common.Abstractions.Actor;
using Energinet.DataHub.Core.FunctionApp.Common.Middleware;
using Energinet.DataHub.Core.FunctionApp.Common.SimpleInjector;
using Energinet.DataHub.Core.XmlConversion.XmlConverter.Configuration;
using Energinet.DataHub.Core.XmlConversion.XmlConverter.SimpleInjector.Extensions;
using Energinet.DataHub.MeteringPoints.Contracts;
using Energinet.DataHub.MeteringPoints.Domain;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common;
using Energinet.DataHub.MeteringPoints.EntryPoints.Ingestion.Functions;
using Energinet.DataHub.MeteringPoints.Infrastructure;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.XmlConverter.Mappings;
using Energinet.DataHub.MeteringPoints.Infrastructure.Ingestion;
using Energinet.DataHub.MeteringPoints.Infrastructure.Ingestion.Resilience;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf.Integration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using SimpleInjector;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Ingestion
{
    public class Program : EntryPoint
    {
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
        }

        protected override void ConfigureContainer(Container container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            base.ConfigureContainer(container);

            // Register application components.
            container.Register<MeteringPointHttpTrigger>(Lifestyle.Scoped);
            container.Register<ICorrelationContext, CorrelationContext>(Lifestyle.Scoped);
            container.Register<CorrelationIdMiddleware>(Lifestyle.Scoped);
            container.Register<EntryPointTelemetryScopeMiddleware>(Lifestyle.Scoped);
            container.AddJwtTokenSecurity("https://login.microsoftonline.com/240beb65-9291-4330-89a3-459d027df97c/v2.0/.well-known/openid-configuration", "c5a9e624-9687-47a7-8f3b-ad74fcf1fb5c");
            container.AddActorContext<ActorProvider>();
            var connectionString = Environment.GetEnvironmentVariable("METERINGPOINT_DB_CONNECTION_STRING")
                                   ?? throw new InvalidOperationException(
                                       "Metering point db connection string not found.");
            container.Register<IDbConnectionFactory>(() => new SqlDbConnectionFactory(connectionString), Lifestyle.Scoped);
            Dapper.SqlMapper.AddTypeHandler(NodaTimeSqlMapper.Instance);

            container.Register<XmlSenderValidator>(Lifestyle.Scoped);

            container.Register<MessageDispatcher, InternalDispatcher>(Lifestyle.Scoped);
            container.Register<Channel, InternalServiceBus>(Lifestyle.Scoped);

            var policyRetryCount = int.TryParse(Environment.GetEnvironmentVariable("INTERNAL_SERVICEBUS_RETRY_COUNT"), out var parsedRetryCount) ? parsedRetryCount : 0;
            container.Register<IChannelResiliencePolicy>(() => new RetryNTimesPolicy(policyRetryCount), Lifestyle.Scoped);
            container.RegisterDecorator<Channel, ChannelResilienceDecorator>(Lifestyle.Scoped);

            container.AddXmlDeserialization(XmlMappingConfiguration, TranslateProcessType);

            var queueConnectionString = Environment.GetEnvironmentVariable("METERINGPOINT_QUEUE_CONNECTION_STRING");
            var topic = Environment.GetEnvironmentVariable("METERINGPOINT_QUEUE_TOPIC_NAME");
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
                "E32" => nameof(BusinessProcessType.ChangeMasterData),
                _ => throw new NotImplementedException(processType),
            };
        }
    }
}
