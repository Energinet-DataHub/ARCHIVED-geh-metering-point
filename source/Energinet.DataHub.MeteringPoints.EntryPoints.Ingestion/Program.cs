using Energinet.DataHub.MeteringPoints.Application;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Ingestion
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
                })
                .ConfigureFunctionsWorkerDefaults()
                .Build();

            host.Run();
        }
    }
}
