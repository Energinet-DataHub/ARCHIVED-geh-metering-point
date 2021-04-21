using Microsoft.Extensions.Hosting;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Ingestion
{
    public static class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .Build();

            host.Run();
        }
    }
}
