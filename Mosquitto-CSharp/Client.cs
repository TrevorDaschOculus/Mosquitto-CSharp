using System;
using System.Runtime.InteropServices;
using MosquittoPtr = System.IntPtr;

namespace Mosquitto
{
    public sealed class Client : ClientBase
    {
        #region Events

        public event OnDisconnected onDisconnectedEvent;

        public event OnMessageReceived onMessageReceivedEvent;

        #endregion

        public Client(string id, bool cleanSession = true, ReconnectSettings reconnectSettings = default) : base(id, cleanSession,
            Native.PROTOCOL_VERSION_v311, reconnectSettings)
        { }

        /// <summary>
        /// Sets a final message that will be sent on disconnect. This must be called before Connect.
        /// </summary>
        /// <param name="topic">the topic on which to publish the will.</param>
        /// <param name="payload">the data to send. If payloadLength > 0 this must not be null.</param>
        /// <param name="payloadLength">the size of the payload (bytes). Valid values are between 0 and 268,435,455.</param>
        /// <param name="qos">integer value 0, 1 or 2 indicating the Quality of Service to be used for the will.</param>
        /// <param name="retain">set to true to make the will a retained message.</param>
        public Error SetWill(string topic, byte[] payload, int payloadLength, QualityOfService qos, bool retain)
        {
            return (Error)SetWillInternal(topic, payload, payloadLength, qos, retain);
        }

        /// <summary>
        /// Connect to an MQTT Broker
        /// </summary>
        /// <param name="host">the hostname or ip address of the broker to connect to.</param>
        /// <param name="port">the network port to connect to. Usually 1883.</param>
        /// <param name="keepalive">the number of seconds after which the broker should send a PING message to the client if no other messages have been exchanged in that time.</param>
        /// <param name="bindAddress">the hostname or ip address of the local network interface to bind to. If you do not want to bind to a specific interface, set this to null.</param>
        /// <returns></returns>
        public void Connect(string host, int port = 1883, int keepalive = 60, string bindAddress = null, OnConnected onConnected = null, OnConnectFailed onConnectFailed = null)
        {
            ConnectInternal(host, port, keepalive, bindAddress, onConnected: onConnected, onConnectFailed: onConnectFailed);
        }

        /// <summary>
        /// Provides an easy way to reconnect to an MQTT Broker after the connection has been lost. Reuses
        /// the same connection parameters from the previous call to Connect. Must not be called before Connect.
        /// </summary>
        /// <returns></returns>
        public void Reconnect(OnConnected onConnected = null, OnConnectFailed onConnectFailed = null)
        {
            ReconnectInternal(onConnected: onConnected, onConnectFailed: onConnectFailed);
        }

        public Error Subscribe(string topic, QualityOfService qos, OnSubscribed onSubscribed = null)
        {
            return SubscribeInternal(topic, qos, onSubscribed: onSubscribed);
        }

        public Error SubscribeMultiple(string[] topics, QualityOfService qos, OnSubscribedMultiple onSubscribed = null)
        {
            return SubscribeMultipleInternal(topics, qos, onSubscribed: onSubscribed);
        }

        public Error Unsubscribe(string topic, OnUnsubscribed onUnsubscribed = null)
        {
            return UnsubscribeInternal(topic, onUnsubscribed: onUnsubscribed);
        }

        /// <summary>
        /// Publish a message to the MQTT Broker
        /// </summary>
        /// <param name="topic">the topic on which to publish the message.</param>
        /// <param name="payload">the data to send. If payloadLength > 0 this must not be null.</param>
        /// <param name="payloadLength">the size of the payload (bytes). Valid values are between 0 and 268,435,455.</param>
        /// <param name="qos">integer value 0, 1 or 2 indicating the Quality of Service to be used for the message.</param>
        /// <param name="retain">set to true to make the message a retained message.</param>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public Error Publish(string topic, byte[] payload, int payloadLength, QualityOfService qos, bool retain, OnPublished onPublished = null)
        {
            return PublishInternal(topic, payload, payloadLength, qos, retain, onPublished: onPublished);
        }

        /// <summary>
        /// Disconnect from the MQTT Broker
        /// </summary>
        public Error Disconnect()
        {
            return DisconnectInternal(false);
        }

        protected override void ClearCallbacks()
        {
            base.ClearCallbacks();
            onDisconnectedEvent = null;
            onMessageReceivedEvent = null;
        }
        protected override void GetMessageReceivedCallbacks(out OnMessageReceived onMessageReceived, out OnMessageReceivedV5 onMessageReceivedV5)
        {
            onMessageReceived = onMessageReceivedEvent;
            onMessageReceivedV5 = null;
        }

        protected override void GetDisconnectedCallbacks(out OnDisconnected onDisconnected, out OnDisconnectedV5 onDisconnectedV5)
        {
            onDisconnected = onDisconnectedEvent;
            onDisconnectedV5 = null;
        }
    }
}