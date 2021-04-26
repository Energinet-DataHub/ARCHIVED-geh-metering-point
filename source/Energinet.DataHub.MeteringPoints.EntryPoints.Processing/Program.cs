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
using Energinet.DataHub.MeteringPoints.Application;
using Energinet.DataHub.MeteringPoints.Infrastructure.IntegrationServices;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Processing
{
    public static class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureServices(x =>
                {
                    x.AddMediatR(typeof(CreateMeteringPointHandler).Assembly);
                    x.AddTransient(typeof(IPipelineBehavior<,>), typeof(InputValidationBehavior<,>));
                    x.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
                    x.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));
                    x.AddTransient(typeof(IPipelineBehavior<,>), typeof(IntegrationEventBehavior<,>));
                    x.AddSingleton(new AzureServiceBusConfig(Environment.GetEnvironmentVariable("METERINGPOINTINTEGRATION_QUEUE_NAME"), Environment.GetEnvironmentVariable("METERINGPOINTINTEGRATION_QUEUE_CONNECTION_STRING")));
                    x.AddTransient<IAzureBusService, AzureBusService>();
                    x.AddTransient<ICreateMeteringPointPublisher, CreateMeteringPointPublisher>();
                })
                .ConfigureFunctionsWorkerDefaults()
                .Build();

            host.Run();
        }
    }
}
