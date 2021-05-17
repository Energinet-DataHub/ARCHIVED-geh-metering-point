using Polly;

namespace Energinet.DataHub.MeteringPoints.Infrastructure.Ingestion.Resilience
{
    /// <summary>
    /// Interface for Polly policies
    /// </summary>
    public interface IChannelResiliencePolicy
    {
        /// <summary>
        /// Returns async policy
        /// </summary>
        IAsyncPolicy AsyncPolicy { get; }
    }
}
