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
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Application.Common.UserIdentity;
using Energinet.DataHub.MeteringPoints.Contracts;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common.SimpleInjector;
using Energinet.DataHub.MeteringPoints.Infrastructure;
using Energinet.DataHub.MeteringPoints.Infrastructure.Correlation;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.XmlConverter;
using Energinet.DataHub.MeteringPoints.Infrastructure.EDI.XmlConverter.Mappings;
using Energinet.DataHub.MeteringPoints.Infrastructure.Ingestion;
using Energinet.DataHub.MeteringPoints.Infrastructure.Ingestion.Resilience;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf.Integration;
using Energinet.DataHub.MeteringPoints.Infrastructure.UserIdentity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SimpleInjector;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Ingestion
{
    public static class Program
    {
        public static async Task Main()
        {
            var container = new Container();
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults(options =>
                {
                    options.UseMiddleware<SimpleInjectorScopedRequest>();
                    options.UseMiddleware<HttpCorrelationIdMiddleware>();
                    options.UseMiddleware<HttpUserContextMiddleware>();
                })
                .ConfigureServices(services =>
                {
                    var descriptor = new ServiceDescriptor(
                        typeof(IFunctionActivator),
                        typeof(SimpleInjectorActivator),
                        ServiceLifetime.Singleton);
                    services.Replace(descriptor); // Replace existing activator

                    services.AddLogging();
                    services.AddSimpleInjector(container, options =>
                    {
                        options.AddLogging();
                    });

                    services.SendProtobuf<MeteringPointEnvelope>();
                })
                .Build()
                .UseSimpleInjector(container);

            // Register application components.
            container.Register<MeteringPointHttpTrigger>(Lifestyle.Scoped);
            container.Register<HttpCorrelationIdMiddleware>(Lifestyle.Scoped);
            container.Register<ICorrelationContext, CorrelationContext>(Lifestyle.Scoped);
            container.Register<HttpUserContextMiddleware>(Lifestyle.Scoped);
            container.Register<IUserContext, UserContext>(Lifestyle.Scoped);

            container.Register<MessageDispatcher, InternalDispatcher>(Lifestyle.Scoped);
            container.Register<Channel, InternalServiceBus>(Lifestyle.Scoped);

            var policyRetryCount = int.TryParse(Environment.GetEnvironmentVariable("INTERNAL_SERVICEBUS_RETRY_COUNT"), out var parsedRetryCount) ? parsedRetryCount : 0;
            container.Register<IChannelResiliencePolicy>(() => new RetryNTimesPolicy(policyRetryCount), Lifestyle.Scoped);
            container.RegisterDecorator<Channel, ChannelResilienceDecorator>(Lifestyle.Scoped);

            // TODO: Expand factory for handling other XML types
            container.Register<Func<string, string, XmlMappingConfigurationBase>>(
                () => (processType, type) => XmlMappingConfiguration(processType), Lifestyle.Singleton);
            container.Register<XmlMapper>(Lifestyle.Singleton);
            container.Register<IXmlConverter, XmlConverter>(Lifestyle.Singleton);

            var connectionString = Environment.GetEnvironmentVariable("METERINGPOINT_QUEUE_CONNECTION_STRING");
            var topic = Environment.GetEnvironmentVariable("METERINGPOINT_QUEUE_TOPIC_NAME");
            container.Register<ServiceBusSender>(
                () => new ServiceBusClient(connectionString).CreateSender(topic),
                Lifestyle.Singleton);
            container.Verify();

            await host.RunAsync().ConfigureAwait(false);

            await container.DisposeAsync().ConfigureAwait(false);
        }

        private static XmlMappingConfigurationBase XmlMappingConfiguration(string processType)
        {
            switch (processType)
            {
                case "E02":
                    return new CreateMeteringPointXmlMappingConfiguration();
                case "D15":
                    return new ConnectMeteringPointXmlMappingConfiguration();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
