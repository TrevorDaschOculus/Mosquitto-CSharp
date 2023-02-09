
namespace Mosquitto
{
    public struct ReconnectSettings
    {
        public readonly bool reconnectAutomatically;
        public readonly int initialReconnectDelay;
        public readonly int maximumReconnectDelay;
        public readonly bool exponentialBackoff;

        public ReconnectSettings(bool reconnectAutomatically, int initialReconnectDelay = 1,
            int maximumReconnectDelay = 60, bool exponentialBackoff = true)
        {
            this.reconnectAutomatically = reconnectAutomatically;
            this.initialReconnectDelay = initialReconnectDelay;
            this.maximumReconnectDelay = maximumReconnectDelay;
            this.exponentialBackoff = exponentialBackoff;
        }
    }
}
