using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Application.Transport;
using Polly;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Ingestion
{
    public class ServiceBusRetryDecorator : Channel
    {
        private readonly InternalServiceBus _internalServiceBus;
        private readonly IAsyncPolicy _retryPolicy;

        public ServiceBusRetryDecorator(InternalServiceBus internalServiceBus, IAsyncPolicy retryPolicy)
        {
            _internalServiceBus = internalServiceBus;
            _retryPolicy = retryPolicy;
        }

        public override async Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                await _internalServiceBus.WriteAsync(data, cancellationToken);
            });
        }
    }
}