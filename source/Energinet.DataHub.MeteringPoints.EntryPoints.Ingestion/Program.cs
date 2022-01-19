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
using Energinet.DataHub.Core.FunctionApp.Common.Abstractions.Identity;
using Energinet.DataHub.Core.FunctionApp.Common.Identity;
using Energinet.DataHub.Core.FunctionApp.Common.Middleware;
using Energinet.DataHub.Core.Logging.RequestResponseMiddleware;
using Energinet.DataHub.Core.Logging.RequestResponseMiddleware.Storage;
using Energinet.DataHub.Core.XmlConversion.XmlConverter.Configuration;
using Energinet.DataHub.Core.XmlConversion.XmlConverter.SimpleInjector.Extensions;
using Energinet.DataHub.MeteringPoints.Contracts;
using Energinet.DataHub.MeteringPoints.Domain;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common;
using Energinet.DataHub.MeteringPoints.EntryPoints.Ingestion.Functions;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.XmlConverter.Mappings;
using Energinet.DataHub.MeteringPoints.Infrastructure.Ingestion;
using Energinet.DataHub.MeteringPoints.Infrastructure.Ingestion.Resilience;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf.Integration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
            options.UseMiddleware<CorrelationIdMiddleware>();
            options.UseMiddleware<EntryPointTelemetryScopeMiddleware>();
            options.UseMiddleware<RequestResponseLoggingMiddleware>();
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
            container.Register<JwtTokenMiddleware>(Lifestyle.Scoped);
            container.Register<IUserContext, UserContext>(Lifestyle.Scoped);
            container.Register<XmlSenderValidator>(Lifestyle.Scoped);
            container.Register<RequestResponseLoggingMiddleware>(Lifestyle.Scoped);
            container.Register<IRequestResponseLogging>(
                () => new RequestResponseLoggingBlobStorage(
                    Environment.GetEnvironmentVariable("REQUEST_RESPONSE_LOGGING_CONNECTION_STRING") ?? throw new InvalidOperationException(),
                    Environment.GetEnvironmentVariable("REQUEST_RESPONSE_LOGGING_CONTAINER_NAME") ?? throw new InvalidOperationException(),
                    container.GetService<ILogger<RequestResponseLoggingBlobStorage>>() ?? throw new InvalidOperationException()),
                Lifestyle.Scoped);

            container.Register<MessageDispatcher, InternalDispatcher>(Lifestyle.Scoped);
            container.Register<Channel, InternalServiceBus>(Lifestyle.Scoped);

            var policyRetryCount = int.TryParse(Environment.GetEnvironmentVariable("INTERNAL_SERVICEBUS_RETRY_COUNT"), out var parsedRetryCount) ? parsedRetryCount : 0;
            container.Register<IChannelResiliencePolicy>(() => new RetryNTimesPolicy(policyRetryCount), Lifestyle.Scoped);
            container.RegisterDecorator<Channel, ChannelResilienceDecorator>(Lifestyle.Scoped);

            container.AddXmlDeserialization(XmlMappingConfiguration, TranslateProcessType);

            var connectionString = Environment.GetEnvironmentVariable("METERINGPOINT_QUEUE_CONNECTION_STRING");
            var topic = Environment.GetEnvironmentVariable("METERINGPOINT_QUEUE_TOPIC_NAME");
            container.Register(() => new ServiceBusClient(connectionString).CreateSender(topic), Lifestyle.Singleton);

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
