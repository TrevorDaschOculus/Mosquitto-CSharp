namespace Mosquitto
{
    public struct Message
    {
        public readonly int messageId;
        public readonly string topic;
        public readonly byte[] payload;
        public readonly QualityOfService qos;
        public readonly bool retain;

        public Message(int mid, string topic, byte[] payload, QualityOfService qos, bool retain)
        {
            this.messageId = mid;
            this.topic = topic;
            this.payload = payload;
            this.qos = qos;
            this.retain = retain;
        }
    }
}
