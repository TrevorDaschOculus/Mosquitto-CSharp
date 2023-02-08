namespace Mosquitto
{
    public struct Message
    {
        public readonly int messageId;
        public readonly string topic;
        public readonly byte[] payload;
        public readonly int payloadLength;
        public readonly QualityOfService qos;
        public readonly bool retain;

        public Message(int mid, string topic, byte[] payload, int payloadLength, QualityOfService qos, bool retain)
        {
            this.messageId = mid;
            this.topic = topic;
            this.payload = payload;
            this.payloadLength = payloadLength;
            this.qos = qos;
            this.retain = retain;
        }
    }
}
