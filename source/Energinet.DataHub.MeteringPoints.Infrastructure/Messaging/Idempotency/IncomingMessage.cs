namespace Energinet.DataHub.MeteringPoints.Infrastructure.Messaging.Idempotency
{
    public class IncomingMessage
    {
        public IncomingMessage(string messageId, string messageType)
        {
            MessageId = messageId;
            MessageType = messageType;
        }

        public string MessageId { get; }

        public string MessageType { get; }
    }
}
