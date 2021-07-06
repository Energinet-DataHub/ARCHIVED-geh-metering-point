using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Common.Commands;
using Energinet.DataHub.MeteringPoints.Infrastructure.DataAccess;
using Energinet.DataHub.MeteringPoints.Infrastructure.Messaging.Idempotency;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.IntegrationTests
{
    #pragma warning disable
    public class IdempotencyTest : TestHost
    {
        [Fact]
        public async Task Accept_accept_command_only_once()
        {
            var command = new TestCommand();

            var messageRegister = new IncomingMessageRegistry(GetService<MeteringPointContext>());

            await messageRegister.RegisterMessageAsync(command.Id.ToString(), command).ConfigureAwait(false);
            await GetService<IUnitOfWork>().CommitAsync().ConfigureAwait(false);

            await messageRegister.RegisterMessageAsync(command.Id.ToString(), command).ConfigureAwait(false);
            await GetService<IUnitOfWork>().CommitAsync().ConfigureAwait(false);
        }
    }

    public class TestCommand : InternalCommand
    {
    }
}
