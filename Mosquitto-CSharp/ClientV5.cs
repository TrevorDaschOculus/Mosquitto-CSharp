using System;
using System.Runtime.InteropServices;
using MosquittoPtr = System.IntPtr;
using MosquittoPropertyPtr = System.IntPtr;

namespace Mosquitto
{
    public class ClientV5 : ClientBase
    {
        #region Events

        public event OnDisconnectedV5 onDisconnectedEvent;

        public event OnMessageReceivedV5 onMessageReceivedEvent;
        #endregion

        public ClientV5(string id, bool cleanSession = true, ReconnectSettings reconnectSettings = default) : base(id, cleanSession,
            Native.PROTOCOL_VERSION_v5, reconnectSettings)
        { }

        /// <summary>
        /// Sets a final message that will be sent on disconnect. This must be called before Connect.
        /// </summary>
        /// <param name="topic">the topic on which to publish the will.</param>
        /// <param name="payload">the data to send. If payloadLength > 0 this must not be null.</param>
        /// <param name="payloadLength">the size of the payload (bytes). Valid values are between 0 and 268,435,455.</param>
        /// <param name="qos">integer value 0, 1 or 2 indicating the Quality of Service to be used for the will.</param>
        /// <param name="retain">set to true to make the will a retained message.</param>
        public Error SetWill(string topic, byte[] payload, int payloadLength, QualityOfService qos, bool retain, PropertyListV5 properties = default)
        {
            return (Error)SetWillInternal(topic, payload, payloadLength, qos, retain, properties);
        }

        /// <summary>
        /// Connect to an MQTT Broker
        /// </summary>
        /// <param name="host">the hostname or ip address of the broker to connect to.</param>
        /// <param name="port">the network port to connect to. Usually 1883.</param>
        /// <param name="keepalive">the number of seconds after which the broker should send a PING message to the client if no other messages have been exchanged in that time.</param>
        /// <param name="bindAddress">the hostname or ip address of the local network interface to bind to. If you do not want to bind to a specific interface, set this to null.</param>
        /// <returns></returns>
        public void Connect(string host, int port = 1883, int keepalive = 60, string bindAddress = null, PropertyListV5 properties = default, OnConnectedV5 onConnected = null, OnConnectFailedV5 onConnectFailedV5 = null)
        {
            ConnectInternal(host, port, keepalive, bindAddress, properties, onConnectedV5: onConnected, onConnectFailedV5: onConnectFailedV5);
        }

        /// <summary>
        /// Provides an easy way to reconnect to an MQTT Broker after the connection has been lost. Reuses
        /// the same connection parameters from the previous call to Connect. Must not be called before Connect.
        /// </summary>
        /// <returns></returns>
        public void Reconnect(OnConnectedV5 onConnected = null, OnConnectFailedV5 onConnectFailed = null)
        {
            ReconnectInternal(onConnectedV5: onConnected, onConnectFailedV5: onConnectFailed);
        }


        public Error Subscribe(string topic, QualityOfService qos, int options = 0, PropertyListV5 properties = default, OnSubscribedV5 onSubscribed = null)
        {
            return SubscribeInternal(topic, qos, options, properties, onSubscribedV5: onSubscribed);
        }

        public Error SubscribeMultiple(string[] topics, QualityOfService qos, int options, PropertyListV5 properties = default, OnSubscribedMultipleV5 onSubscribed = null)
        {
            return SubscribeMultipleInternal(topics, qos, options, properties, onSubscribedV5: onSubscribed);
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
        public Error Publish(string topic, byte[] payload, int payloadLength, QualityOfService qos, bool retain, PropertyListV5 properties = default, OnPublishedV5 onPublished = null, OnPublishFailedV5 onPublishFailed = null)
        {
            return PublishInternal(topic, payload, payloadLength, qos, retain, onPublishedV5: onPublished, onPublishFailedV5: onPublishFailed);
        }

        /// <summary>
        /// Disconnect from the MQTT Broker
        /// </summary>
        public Error Disconnect(bool sendWill = false, PropertyListV5 properties = default)
        {
            return DisconnectInternal(false, sendWill, properties);
        }

        protected override void ClearCallbacks()
        {
            base.ClearCallbacks();
            onDisconnectedEvent = null;
            onMessageReceivedEvent = null;
        }

        protected override void GetMessageReceivedCallbacks(out OnMessageReceived onMessageReceived, out OnMessageReceivedV5 onMessageReceivedV5)
        {
            onMessageReceived = null;
            onMessageReceivedV5 = onMessageReceivedEvent;
        }

        protected override void GetDisconnectedCallbacks(out OnDisconnected onDisconnected, out OnDisconnectedV5 onDisconnectedV5)
        {
            onDisconnected = null;
            onDisconnectedV5 = onDisconnectedEvent;
        }
    }
}