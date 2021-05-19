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

using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Application.UserIdentity;
using Energinet.DataHub.MeteringPoints.Contracts;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common.MediatR;
using Energinet.DataHub.MeteringPoints.EntryPoints.Common.SimpleInjector;
using Energinet.DataHub.MeteringPoints.Infrastructure;
using Energinet.DataHub.MeteringPoints.Infrastructure.BusinessRequestProcessing.Pipeline;
using Energinet.DataHub.MeteringPoints.Infrastructure.ContainerExtensions;
using Energinet.DataHub.MeteringPoints.Infrastructure.Transport.Protobuf.Integration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SimpleInjector;
using CreateMeteringPoint = Energinet.DataHub.MeteringPoints.Application.CreateMeteringPoint;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Processing
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
                    options.UseMiddleware<ServiceBusCorrelationIdMiddleware>();
                    options.UseMiddleware<ServiceBusUserContextMiddleware>();
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

                    services.ReceiveProtobuf<MeteringPointEnvelope>(
                        config => config
                            .FromOneOf(envelope => envelope.MeteringPointMessagesCase)
                            .WithParser(() => MeteringPointEnvelope.Parser));
                })
                .Build()
                .UseSimpleInjector(container);

            // Register application components.
            container.Register<QueueSubscriber>(Lifestyle.Scoped);
            container.Register<ServiceBusCorrelationIdMiddleware>(Lifestyle.Scoped);
            container.Register<ICorrelationContext, CorrelationContext>(Lifestyle.Scoped);
            container.Register<ServiceBusUserContextMiddleware>(Lifestyle.Scoped);
            container.Register<IUserContext, UserContext>(Lifestyle.Scoped);
            container.Register<UserIdentityFactory>(Lifestyle.Singleton);

            // Setup pipeline behaviors
            container.BuildMediator(
                new[]
                {
                    typeof(CreateMeteringPoint).Assembly,
                },
                new[]
                {
                    typeof(InputValidationBehavior<,>),
                    typeof(AuthorizationBehavior<,>),
                    typeof(BusinessProcessResultBehavior<,>),
                    typeof(IntegrationEventsDispatchBehavior<,>),
                    typeof(ValidationReportsBehavior<,>),
                    typeof(UnitOfWorkBehavior<,>),
                });

            container.AddInputValidation();

            container.Verify();

            await host.RunAsync().ConfigureAwait(false);

            await container.DisposeAsync().ConfigureAwait(false);
        }
    }
}
