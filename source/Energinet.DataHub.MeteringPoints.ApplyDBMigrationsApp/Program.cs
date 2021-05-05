using System.Linq;
using Energinet.DataHub.MeteringPoints.ApplyDBMigrationsApp.Helpers;

namespace Energinet.DataHub.MeteringPoints.ApplyDBMigrationsApp
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var connectionString = ConnectionStringFactory.GetConnectionString(args);
            var filter = EnvironmentFilter.GetFilter(args);
            var isDryRun = args.Contains("dryRun");

            var upgrader = UpgradeFactory.GetUpgradeEngine(connectionString, filter, isDryRun);

            var result = upgrader.PerformUpgrade();

            return ResultReporter.ReportResult(result);
        }
    }
}
