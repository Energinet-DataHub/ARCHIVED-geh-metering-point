using Energinet.DataHub.MeteringPoints.Abstractions;

namespace Energinet.DataHub.B2B.Messaging
{
    #pragma warning disable CA1040 //marker interface
    /// <summary>
    /// Request for starting a business process
    /// </summary>
    public interface IRsmMessage : IOutboundMessage
    {
    }
}
