/*
Copyright (c) 2010-2020 Roger Light <roger@atchoo.org>

All rights reserved. This program and the accompanying materials
are made available under the terms of the Eclipse Public License 2.0
and Eclipse Distribution License v1.0 which accompany this distribution.

The Eclipse Public License is available at
https://www.eclipse.org/legal/epl-2.0/
and the Eclipse Distribution License is available at
http://www.eclipse.org/org/documents/edl-v10.php.

SPDX-License-Identifier: EPL-2.0 OR BSD-3-Clause

Contributors:
Roger Light - initial implementation and documentation.
*/

using System;
using System.Runtime.InteropServices;
using System.Threading;
using MosquittoPtr = System.IntPtr;
using MosquittoPropertyPtr = System.IntPtr;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Mosquitto
{
    public abstract class ClientBase : IDisposable
    {

        #region Helper Types
        private class ReusableCallbackContainer
        {
            public CallbackList callbackList;
            public CallbackArgumentList callbackArgumentList;

            public void Reset()
            {
                ManagedPropertyListV5Pool.Release(ref callbackArgumentList.properties);
                callbackList = default;
                callbackArgumentList = default;
            }
        }

        /// <summary>
        /// A callback list can have a maximum of 2 callbacks, one success and one failure.
        /// </summary>
        private struct CallbackList
        {
            private object _successCallback;
            private object _failureCallback;

            public SynchronizationContext context;

            public bool isEmpty => _successCallback == null && _failureCallback == null;

            public OnLog onLog
            {
                get
                {
                    return _successCallback as OnLog;
                }
                set
                {
                    _successCallback = value ?? _successCallback;
                }
            }
            public OnConnected onConnected
            {
                get
                {
                    return _successCallback as OnConnected;
                }
                set
                {
                    _successCallback = value ?? _successCallback;
                }
            }
            public OnConnectedV5 onConnectedV5
            {
                get
                {
                    return _successCallback as OnConnectedV5;
                }
                set
                {
                    _successCallback = value ?? _successCallback;
                }
            }
            public OnConnectFailed onConnectFailed
            {
                get
                {
                    return _failureCallback as OnConnectFailed;
                }
                set
                {
                    _failureCallback = value ?? _failureCallback;
                }
            }
            public OnConnectFailedV5 onConnectFailedV5
            {
                get
                {
                    return _failureCallback as OnConnectFailedV5;
                }
                set
                {
                    _failureCallback = value ?? _failureCallback;
                }
            }
            public OnDisconnected onDisconnected
            {
                get
                {
                    return _failureCallback as OnDisconnected;
                }
                set
                {
                    _failureCallback = value ?? _failureCallback;
                }
            }
            public OnDisconnectedV5 onDisconnectedV5
            {
                get
                {
                    return _failureCallback as OnDisconnectedV5;
                }
                set
                {
                    _failureCallback = value ?? _failureCallback;
                }
            }
            public OnSubscribed onSubscribed
            {
                get
                {
                    return _successCallback as OnSubscribed;
                }
                set
                {
                    _successCallback = value ?? _successCallback;
                }
            }
            public OnSubscribedV5 onSubscribedV5
            {
                get
                {
                    return _successCallback as OnSubscribedV5;
                }
                set
                {
                    _successCallback = value ?? _successCallback;
                }
            }
            public OnSubscribedMultiple onSubscribedMultiple
            {
                get
                {
                    return _successCallback as OnSubscribedMultiple;
                }
                set
                {
                    _successCallback = value ?? _successCallback;
                }
            }
            public OnSubscribedMultipleV5 onSubscribedMultipleV5
            {
                get
                {
                    return _successCallback as OnSubscribedMultipleV5;
                }
                set
                {
                    _successCallback = value ?? _successCallback;
                }
            }
            public OnUnsubscribed onUnsubscribed
            {
                get
                {
                    return _successCallback as OnUnsubscribed;
                }
                set
                {
                    _successCallback = value ?? _successCallback;
                }
            }
            public OnUnsubscribedV5 onUnsubscribedV5
            {
                get
                {
                    return _successCallback as OnUnsubscribedV5;
                }
                set
                {
                    _successCallback = value ?? _successCallback;
                }
            }
            public OnPublished onPublished
            {
                get
                {
                    return _successCallback as OnPublished;
                }
                set
                {
                    _successCallback = value ?? _successCallback;
                }
            }
            public OnPublishedV5 onPublishedV5
            {
                get
                {
                    return _successCallback as OnPublishedV5;
                }
                set
                {
                    _successCallback = value ?? _successCallback;
                }
            }
            public OnPublishFailedV5 onPublishFailedV5
            {
                get
                {
                    return _failureCallback as OnPublishFailedV5;
                }
                set
                {
                    _failureCallback = value ?? _failureCallback;
                }
            }
            public OnMessageReceived onMessageReceived
            {
                get
                {
                    return _successCallback as OnMessageReceived;
                }
                set
                {
                    _successCallback = value ?? _successCallback;
                }
            }
            public OnMessageReceivedV5 onMessageReceivedV5
            {
                get
                {
                    return _successCallback as OnMessageReceivedV5;
                }
                set
                {
                    _successCallback = value ?? _successCallback;
                }
            }
        }

        private struct CallbackArgumentList
        {
            private int _enum;

            public int intValue
            {
                get
                {
                    return _enum;
                }
                set
                {
                    _enum = value;
                }
            }
            public Error error
            {
                get
                {
                    return (Error)_enum;
                }
                set
                {
                    _enum = (int)value;
                }
            }
            public ConnectFailedReason connectFailedReason
            {
                get
                {
                    return (ConnectFailedReason)_enum;
                }
                set
                {
                    _enum = (int)value;
                }
            }
            public ConnectFailedReasonV5 connectFailedReasonV5
            {
                get
                {
                    return (ConnectFailedReasonV5)_enum;
                }
                set
                {
                    _enum = (int)value;
                }
            }
            public DisconnectReasonV5 disconnectReasonV5
            {
                get
                {
                    return (DisconnectReasonV5)_enum;
                }
                set
                {
                    _enum = (int)value;
                }
            }
            public PublishFailedReasonV5 publishFailedReasonV5
            {
                get
                {
                    return (PublishFailedReasonV5)_enum;
                }
                set
                {
                    _enum = (int)value;
                }
            }
            public QualityOfService qos
            {
                get
                {
                    return (QualityOfService)_enum;
                }
                set
                {
                    _enum = (int)value;
                }
            }
            public LogLevel logLevel
            {
                get
                {
                    return (LogLevel)_enum;
                }
                set
                {
                    _enum = (int)value;
                }
            }

            private object _object;

            public QualityOfService[] qosList
            {
                get
                {
                    return _object as QualityOfService[];
                }
                set
                {
                    _object = value;
                }
            }
            public string logMessage
            {
                get
                {
                    return _object as string;
                }
                set
                {
                    _object = value;
                }
            }

            public Message message;
            public ManagedPropertyListV5 properties;
        }

        private struct PendingSubscribe
        {
            public readonly string topic;
            public readonly string[] topics;
            public readonly QualityOfService qos;
            public readonly CallbackList cb;

            public PendingSubscribe(string topic, QualityOfService qos, CallbackList cb)
            {
                this.topic = topic;
                this.topics = null;
                this.qos = qos;
                this.cb = cb;
            }

            public PendingSubscribe(string[] topics, QualityOfService qos, CallbackList cb)
            {
                this.topic = null;
                this.topics = topics;
                this.qos = qos;
                this.cb = cb;
            }
        }

        private struct PendingUnsubscribe
        {
            public readonly string topic;
            public readonly CallbackList cb;

            public PendingUnsubscribe(string topic, CallbackList cb)
            {
                this.topic = topic;
                this.cb = cb;
            }
        }


        private sealed class ConnectionParams : IDisposable
        {
            public readonly string host;
            public readonly int port;
            public readonly int keepalive;
            public readonly string bindAddress;
            public PropertyListV5 properties => _properties;
            private ManagedPropertyListV5 _properties;

            public ConnectionParams(string host, int port, int keepalive, string bindAddress, PropertyListV5 properties)
            {
                this.host = host;
                this.port = port;
                this.keepalive = keepalive;
                this.bindAddress = bindAddress;
                _properties = ManagedPropertyListV5Pool.Obtain(in properties);
            }

            public void Dispose()
            {
                ManagedPropertyListV5Pool.Release(ref _properties);
            }
        }

        private enum State
        {
            New,
            Connecting,
            Connected,
            Reconnecting,
            Disconnecting,
            Disconnected,
            Disposed
        }

        #endregion

        #region Events

        /// <summary>
        /// Callback triggered when successfully connected to MQTT Broker
        /// </summary>
        public delegate void OnConnected();

        /// <summary>
        /// Callback triggered when connecting to MQTT Broker fails
        /// </summary>
        /// <param name="error">If the connection failed due to client error, this is the error</param>
        /// <param name="reason">The reason the connection failed from the server</param>
        public delegate void OnConnectFailed(Error error, ConnectFailedReason reason);

        /// <summary>
        /// Callback triggered when disconnected from the MQTT Broker
        /// </summary>
        /// <param name="reason">The reason the connection was disconnected</param>
        public delegate void OnDisconnected(Error reason);

        /// <summary>
        /// Callback triggered when we have successfully published a message
        /// </summary>
        public delegate void OnPublished();

        /// <summary>
        /// Callback triggered when we have successfully subscribed to a topic
        /// </summary>
        /// <param name="qosLevel">The supported QOS level for this subscription</param>

        public delegate void OnSubscribed(QualityOfService qosLevel);

        /// <summary>
        /// Callback triggered when we have successfully subscribed to a topic
        /// </summary>
        /// <param name="qosLevels">The supported QOS level for this subscription</param>
        public delegate void OnSubscribedMultiple(QualityOfService[] qosLevels);

        /// <summary>
        /// Callback triggered when we have successfully unsubscribed from a topic
        /// </summary>
        public delegate void OnUnsubscribed();

        /// <summary>
        /// Callback triggered when we have received a message for one of our
        /// subscribed topics
        /// </summary>
        /// <param name="message">The received message</param>
        public delegate void OnMessageReceived(Message message);

        /// <summary>
        /// Callback triggerd when successfully connected to MQTT Broker
        /// </summary>
        /// <param name="properties">A set of properties returned from the broker when the connection is acknowledged.
        /// This PropertyList must not be persisted as is. If it is needed outside of this callback, Construct a
        /// ManagedPropertyListV5 and persist that.</param>
        public delegate void OnConnectedV5(PropertyListV5 properties);

        /// <summary>
        /// Callback triggered when connecting to MQTT Broker fails
        /// </summary>
        /// <param name="error">If a client error caused connection to fail, this is the error</param>
        /// <param name="reason">The reason the connection failed</param>
        /// <param name="properties">A set of properties returned from the broker when rejecting the connection.
        /// This PropertyList must not be persisted as is. If it is needed outside of this callback, Construct a
        /// ManagedPropertyListV5 and persist that.</param>
        public delegate void OnConnectFailedV5(Error error, ConnectFailedReasonV5 reason, PropertyListV5 properties);

        /// <summary>
        /// Callback triggered when disconnected from the MQTT Broker
        /// </summary>
        /// <param name="error">An error that caused the disconnect, or Success if there was a reason from the server.</param>
        /// <param name="reason">The reason the connection was disconnected. In the current version of Mosquitto,
        /// the reason will be an <see cref="Error"/> instead of one of the <see cref="DisconnectReasonV5"/>. This should
        /// be updated when the library is fixed.</param>
        /// <param name="properties">A set of properties returned from the broker when our disconnect was acknowledged.
        /// This PropertyList must not be persisted as is. If it is needed outside of this callback, Construct a
        /// ManagedPropertyListV5 and persist that.</param>
        public delegate void OnDisconnectedV5(Error error, DisconnectReasonV5 reason, PropertyListV5 properties);

        /// <summary>
        /// Callback triggered when we have successfully published a message
        /// </summary>
        /// <param name="properties">A set of properties returned from the broker when our publish was acknowledged.
        /// This PropertyList must not be persisted as is. If it is needed outside of this callback, Construct a
        /// ManagedPropertyListV5 and persist that.</param>
        public delegate void OnPublishedV5(PropertyListV5 properties);

        /// <summary>
        /// Callback triggered when failed to published a message
        /// </summary>
        /// <param name="reason">The reason publishing the message failed</param>
        /// <param name="properties">A set of properties returned from the broker when our publish was acknowledged.
        /// This PropertyList must not be persisted as is. If it is needed outside of this callback, Construct a
        /// ManagedPropertyListV5 and persist that.</param>
        public delegate void OnPublishFailedV5(PublishFailedReasonV5 reason, PropertyListV5 properties);

        /// <summary>
        /// Callback triggered when we have successfully subscribed to a topic
        /// </summary>
        /// <param name="qos">The supported QOS level for this subscription</param>
        /// <param name="properties">A set of properties returned from the broker when our subscribe was acknowledged.
        /// This PropertyList must not be persisted as is. If it is needed outside of this callback, Construct a
        /// ManagedPropertyListV5 and persist that.</param>
        public delegate void OnSubscribedV5(QualityOfService qosLevel, PropertyListV5 properties);


        /// <summary>
        /// Callback triggered when we have successfully subscribed to a topic
        /// </summary>
        /// <param name="qosLevels">The supported QOS level for each of the subscribed topics</param>
        /// <param name="properties">A set of properties returned from the broker when our subscribe was acknowledged.
        /// This PropertyList must not be persisted as is. If it is needed outside of this callback, Construct a
        /// ManagedPropertyListV5 and persist that.</param>
        public delegate void OnSubscribedMultipleV5(QualityOfService[] qosLevels, PropertyListV5 properties);

        /// <summary>
        /// Callback triggered when we have successfully unsubscribed from a topic
        /// </summary>
        /// <param name="properties">A set of properties returned from the broker when our unsubscribe was acknowledged.
        /// This PropertyList must not be persisted as is. If it is needed outside of this callback, Construct a
        /// ManagedPropertyListV5 and persist that.</param>
        public delegate void OnUnsubscribedV5(PropertyListV5 properties);

        /// <summary>
        /// Callback triggered when we have received a message for one of our
        /// subscribed topics
        /// </summary>
        /// <param name="message">The received message</param>
        /// <param name="properties">A set of properties included with the received message.
        /// This PropertyList must not be persisted as is. If it is needed outside of this callback, Construct a
        /// ManagedPropertyListV5 and persist that.</param>
        public delegate void OnMessageReceivedV5(Message message, PropertyListV5 properties);


        /// <summary>
        /// Callback invoked when a native log is printed
        /// </summary>
        /// <param name="level">The severity level of the log</param>
        /// <param name="message">The log contents</param>
        public delegate void OnLog(LogLevel level, string message);

        /// <summary>
        /// Event to register to receive log messages from the MQTT client
        /// </summary>
        public event OnLog onLogEvent;

        public delegate byte[] GetPassword();

        private GetPassword _getPassword;
        #endregion

        private readonly bool _cleanSession;
        private readonly int _protocolVersion;
        private readonly ReconnectSettings _reconnectSettings;
        private readonly SynchronizationContext _defaultContext;

        private readonly ManualResetEvent _reconnectResetEvent = new ManualResetEvent(false);

        private Thread _thread;
        // this is set when we connect, so that we have it available if we reconnect
        private int _loopTimeout;
        // this is set to 0 every time a successful connection is made
        private int _reconnects;

        private const int _maxReusableContainers = 32;
        private readonly ConcurrentBag<ReusableCallbackContainer> _reusableCallbackContainers = new ConcurrentBag<ReusableCallbackContainer>();

        // These are only accessed on our worker thread
        private readonly Dictionary<string, QualityOfService> _subscriptions = new Dictionary<string, QualityOfService>();
        private readonly List<PendingSubscribe> _subscribeOnReconnectList = new List<PendingSubscribe>();
        private readonly List<PendingUnsubscribe> _unsubscribeOnReconnectList = new List<PendingUnsubscribe>();

        private readonly SendOrPostCallback _sendOrPostCallback;
        private readonly WaitCallback _waitCallback;

        private readonly object _connectionCallbackMutex = new object();
        private CallbackList _connectionCallbacks;
        private readonly Dictionary<int, PendingSubscribe> _subscribeCallbacks = new Dictionary<int, PendingSubscribe>();
        private readonly Dictionary<int, PendingUnsubscribe> _unsubscribeCallbacks = new Dictionary<int, PendingUnsubscribe>();
        private readonly Dictionary<int, CallbackList> _publishCallbacks = new Dictionary<int, CallbackList>();

        private MosquittoPtr _mosq;


        // Use an int backing field so we can take advantage of interlocked
        private int _currentState;

        private State currentState
        {
            get
            {
                return (State) Interlocked.CompareExchange(ref _currentState, 0, 0);
            }
            set
            {
                Interlocked.Exchange(ref _currentState, (int) value);
            }
        }


        /// <param name="id">String to use as the client id. If null, a random client id will be generated.
        /// If id is null, cleanSession must be true.</param>
        /// <param name="cleanSession">set to true to instruct the broker to clean all messages
        /// and subscriptions on disconnect, false to instruct it to keep them.
        /// See the man page mqtt(7) for more details.
        /// Note that a client will never discard its own outgoing messages on disconnect.
        /// Calling <see cref="Connect"/> or <see cref="Reconnect"/> will cause the messages to be resent.
        /// Use <see cref="Reinitialise"/> to reset a client to its original state.
        /// Must be set to true if the id parameter is null.</param>
        /// <param name="threaded">Whether this Mosquitto instance will run its own
        /// internal thread, or if the Update must be called regularly. When executing
        /// in threaded mode, events may be called on threads other than the thread that this
        /// instance was created on.</param>
        public ClientBase(string id, bool cleanSession, int protocolVersion, ReconnectSettings reconnectSettings)
        {
            // ensure our library is initialized
            Native.Initialize();
            _mosq = Native.mosquitto_new(id, cleanSession, IntPtr.Zero);
            _protocolVersion = protocolVersion;
            _cleanSession = cleanSession;
            _reconnectSettings = reconnectSettings;
            _defaultContext = SynchronizationContext.Current;
            // create delegate from our method once to avoid repeated boxing
            _sendOrPostCallback = InvokeCallbacks;
            _waitCallback = InvokeCallbacks;
            SetOption(Option.ProtocolVersion, protocolVersion);
            Native.mosquitto_threaded_set(_mosq, true);
            SetupNativeCallbacks();
        }

        ~ClientBase()
        {
            Dispose(true);
        }

        /// <summary>
        /// Resets the network state and clears all callbacks for this instance. Restores this client to the same state it was in just after construction
        /// </summary>
        public Error Reinitialize(string id, bool cleanSession = true)
        {
            DisconnectInternal(disconnectImmediately: true);
            WaitForThread();
            ClearCallbacks();
            var result = (Error)Native.mosquitto_reinitialise(_mosq, id, cleanSession, IntPtr.Zero);
            if (result != Error.Success)
            {
                return result;
            }
            // reset protocol version
            SetOption(Option.ProtocolVersion, _protocolVersion);
            Native.mosquitto_threaded_set(_mosq, true);
            SetupNativeCallbacks();
            return Error.Success;
        }

        /// <summary>
        /// Sets the username and password used to connect to the MQTT Broker
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public Error SetUsernameAndPassword(string username, string password)
        {
            return (Error)Native.mosquitto_username_pw_set(_mosq, username, password);
        }

        /// <summary>
        /// Used to set various MQTT Options
        /// </summary>
        /// <param name="option"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Error SetOption(Option option, int value)
        {
            return (Error)Native.mosquitto_int_option(_mosq, (Native.mosq_opt_t)option, value);
        }

        /// <summary>
        /// Used to set various MQTT Options
        /// </summary>
        /// <param name="option"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Error SetOption(Option option, string value)
        {
            return (Error)Native.mosquitto_string_option(_mosq, (Native.mosq_opt_t)option, value);
        }

        /// <summary>
        /// Used to set various MQTT Options
        /// </summary>
        /// <param name="option"></param>
        /// <param name="value"></param>
        public Error SetOption(Option option, IntPtr value)
        {
            return (Error)Native.mosquitto_void_option(_mosq, (Native.mosq_opt_t)option, value);
        }

        /// <summary>
        /// Set the TLS Configuration for the connection to the MQTT Broker
        /// </summary>
        /// <param name="cafile">path to a file containing the PEM encoded trusted CA certificate files.
        /// Either cafile or capath must not be NULL.</param>
        /// <param name="capath">path to a directory containing the PEM encoded trusted CA certificate files.
        /// See mosquitto.conf for more details on configuring this directory. Either cafile or capath must not be NULL.</param>
        /// <param name="certfile">path to a file containing the PEM encoded certificate file for this client.
        /// If NULL, keyfile must also be NULL and no client certificate will be used.</param>
        /// <param name="keyfile">path to a file containing the PEM encoded private key for this client.
        /// If NULL, certfile must also be NULL and no client certificate will be used.</param>
        /// <param name="getPassword">if keyfile is encrypted, set getPassword to allow your client to pass the correct password for decryption.
        /// If set to null, the password must be entered on the command line.</param>
        /// <returns></returns>
        public Error SetTls(string cafile = null, string capath = null, string certfile = null, string keyfile = null, GetPassword getPassword = null)
        {
            Interlocked.Exchange(ref _getPassword, getPassword);

            return (Error)Native.mosquitto_tls_set(_mosq, cafile, capath, certfile, keyfile, PasswordCallback);
        }

        /// <summary>
        /// <para>
        /// Configure verification of the server hostname in the server certificate. If
        /// value is set to true, it is impossible to guarantee that the host you are
        /// connecting to is not impersonating your server. This can be useful in
        /// initial server testing, but makes it possible for a malicious third party to
        /// impersonate your server through DNS spoofing, for example.
        /// </para>
        /// <para>
        /// Do not use this function in a real system. Setting value to true makes the
        /// connection encryption pointless.
        /// </para>
        /// <para>
        /// Must be called before Connect/>.
        /// </para>
        /// </summary>
        /// <param name="insecure">if set to false, the default, certificate hostname checking is performed. If set to true, no hostname checking is performed and the connection is insecure.</param>
        public Error SetTlsInsecure(bool insecure)
        {
            return (Error)Native.mosquitto_tls_insecure_set(_mosq, insecure);
        }


        public Error SetTlsOptions(int certReqs, string tlsVersion, string ciphers)
        {
            return (Error)Native.mosquitto_tls_opts_set(_mosq, certReqs, tlsVersion, ciphers);
        }

        /// <summary>
        /// Clears a will previosly set with SetWill. This must be called before Connect.
        /// </summary>
        public Error ClearWill()
        {
            return (Error)Native.mosquitto_will_clear(_mosq);
        }

        public void Dispose()
        {
            Dispose(false);
        }

        private void Dispose(bool isFinalize)
        {
            if (_mosq != IntPtr.Zero)
            {
                // disconnect if we are still connected
                DisconnectInternal(disconnectImmediately: true);
                WaitForThread();
                currentState = State.Disposed;
                Native.mosquitto_destroy(_mosq);
                _mosq = IntPtr.Zero;
            }

            ClearCallbacks();

            if (!isFinalize)
            {
                GC.SuppressFinalize(this);
            }
        }

        protected Error SetWillInternal(string topic, byte[] payload, int payloadLength, QualityOfService qos, bool retain, PropertyListV5 properties = default)
        {
            return (Error)Native.mosquitto_will_set_v5(_mosq, topic, payloadLength, payload, (int)qos, retain, properties.nativePropertyList);
        }
        /// <summary>
        /// Provides an easy way to reconnect to an MQTT Broker after the connection has been lost. Reuses
        /// the same connection parameters from the previous call to Connect. Must not be called before Connect.
        /// </summary>
        /// <returns></returns>
        protected void ReconnectInternal(OnConnected onConnected = null, OnConnectedV5 onConnectedV5 = null, OnConnectFailed onConnectFailed = null, OnConnectFailedV5 onConnectFailedV5 = null)
        {
            StartThread(null, new CallbackList { context = SynchronizationContext.Current, onConnected = onConnected, onConnectedV5 = onConnectedV5, onConnectFailed = onConnectFailed, onConnectFailedV5 = onConnectFailedV5 });
        }

        protected void ConnectInternal(string host, int port, int keepalive, string bindAddress, PropertyListV5 properties = default, OnConnected onConnected = null, OnConnectedV5 onConnectedV5 = null, OnConnectFailed onConnectFailed = null, OnConnectFailedV5 onConnectFailedV5 = null)
        {
            StartThread(new ConnectionParams(host, port, keepalive, bindAddress, properties), new CallbackList { context = SynchronizationContext.Current, onConnected = onConnected, onConnectedV5 = onConnectedV5, onConnectFailed = onConnectFailed, onConnectFailedV5 = onConnectFailedV5 });
        }

        private void StartThread(ConnectionParams connectionParams, CallbackList callbackList)
        {
            var currentState = this.currentState;
            if (currentState == State.Disposed)
            {
                callbackList.onConnectFailed?.Invoke(Error.Inval, ConnectFailedReason.ServerUnavailable);
                connectionParams?.Dispose();
                return;
            }

            if (currentState == State.Connecting || currentState == State.Connected || currentState == State.Reconnecting)
            {
                callbackList.onConnectFailed?.Invoke(Error.AlreadyExists, ConnectFailedReason.Unspecified);
                connectionParams?.Dispose();
                return;
            }

            WaitForThread();

            if (connectionParams == null)
            {
                this.currentState = State.Reconnecting;
            }
            else
            {
                ClearSessionCallbacks();
                this.currentState = State.Connecting;
            }

            lock (_connectionCallbackMutex)
            {
                _connectionCallbacks = callbackList;
            }
            _thread = new Thread(ThreadExecute);
            _thread.Start(connectionParams);
        }

        protected Error SubscribeInternal(string topic, QualityOfService qos, int options = 0, PropertyListV5 properties = default, OnSubscribed onSubscribed = null, OnSubscribedV5 onSubscribedV5 = null)
        {
            var cb = new CallbackList { context = SynchronizationContext.Current, onSubscribed = onSubscribed, onSubscribedV5 = onSubscribedV5 };            
            lock (_subscribeCallbacks)
            {
                int messageId = 0;
                Error error = (Error)Native.mosquitto_subscribe_v5(_mosq, ref messageId, topic, (int)qos, options, properties.nativePropertyList);
                if (error == Error.Success)
                {
                    _subscribeCallbacks.Add(messageId, new PendingSubscribe(topic, qos, cb));
                }
                return error;
            }
        }

        protected Error SubscribeMultipleInternal(string[] topics, QualityOfService qos, int options = 0, PropertyListV5 properties = default, OnSubscribedMultiple onSubscribed = null, OnSubscribedMultipleV5 onSubscribedV5 = null)
        {
            var cb = new CallbackList { context = SynchronizationContext.Current, onSubscribedMultiple = onSubscribed, onSubscribedMultipleV5 = onSubscribedV5 };            
            lock (_subscribeCallbacks)
            {
                int messageId = 0;
                Error error = (Error)Native.mosquitto_subscribe_multiple(_mosq, ref messageId, topics.Length, topics, (int)qos, options, properties.nativePropertyList);
                if (error == Error.Success)
                {
                    _subscribeCallbacks.Add(messageId, new PendingSubscribe(topics, qos, cb));
                }
                return error;
            }
        }

        protected Error UnsubscribeInternal(string topic, PropertyListV5 properties = default, OnUnsubscribed onUnsubscribed = null, OnUnsubscribedV5 onUnsubscribedV5 = null)
        {
            var cb = new CallbackList { context = SynchronizationContext.Current, onUnsubscribed = onUnsubscribed, onUnsubscribedV5 = onUnsubscribedV5 };            
            lock (_unsubscribeCallbacks)
            {
                int messageId = 0;
                Error error = (Error)Native.mosquitto_unsubscribe_v5(_mosq, ref messageId, topic, properties.nativePropertyList);
                if (error == Error.Success)
                {
                    _unsubscribeCallbacks.Add(messageId, new PendingUnsubscribe(topic, cb));
                }
                return error;
            }
        }

        protected Error PublishInternal(string topic, byte[] payload, int payloadLength, QualityOfService qos, bool retain, PropertyListV5 properties = default, OnPublished onPublished = null, OnPublishedV5 onPublishedV5 = null, OnPublishFailedV5 onPublishFailedV5 = null)
        {
            var cb = new CallbackList { context = SynchronizationContext.Current, onPublished = onPublished, onPublishedV5 = onPublishedV5, onPublishFailedV5 = onPublishFailedV5 };
            if (cb.isEmpty)
            {
                int messageId = 0;
                return (Error)Native.mosquitto_publish_v5(_mosq, ref messageId, topic, payloadLength, payload, (int)qos, retain, properties.nativePropertyList);
            }

            lock (_publishCallbacks)
            {
                int messageId = 0;
                Error error = (Error)Native.mosquitto_publish_v5(_mosq, ref messageId, topic, payloadLength, payload, (int)qos, retain, properties.nativePropertyList);
                if (error == Error.Success)
                {
                    _publishCallbacks.Add(messageId, cb);
                }
                return error;
            }
        }

        protected Error DisconnectInternal(bool disconnectImmediately = false, bool sendWill = false, PropertyListV5 properties = default)
        {
            var currentState = this.currentState;
            if (currentState == State.Disposed)
            {
                return Error.Inval;
            }

            if (currentState == State.Disconnected || (currentState == State.Disconnecting && !disconnectImmediately))
            {
                return Error.Success;
            }

            if (disconnectImmediately)
            {
                SetDisconnected(Error.Success);
            }
            else
            {
                this.currentState = State.Disconnecting;
            }
            _reconnectResetEvent.Set();
            return (Error)Native.mosquitto_disconnect_v5(_mosq, sendWill ? (int)DisconnectReasonV5.DisconnectWithWillMsg : (int)DisconnectReasonV5.NormalDisconnection, properties.nativePropertyList);
        }

        /// <summary>
        /// Try to update the value of state by comparing to a given state
        /// </summary>
        /// <param name="newState">The State to set state to</param>
        /// <param name="expectedState">The State we expect state is set to</param>
        /// <param name="trueIfAlreadySet">Whether we should return true if the state was already the expected state</param>
        /// <returns>true if state is now set to newState from expectedState (or if it was already set to newState, and trueIfAlreadySet is true)</returns>
        private bool TryUpdateState(State newState, State expectedState, bool trueIfAlreadySet = true)
        {
            int prevState = Interlocked.CompareExchange(ref _currentState, (int) newState, (int) expectedState);
            return prevState == (int)expectedState || (trueIfAlreadySet && prevState == (int)newState);
        }

        /// <summary>
        /// Updates the state to the given state, returns the previous state
        /// </summary>
        /// <param name="newState">The State to set our current state to</param>
        /// <returns></returns>
        private State ExchangeState(State newState)
        {
            return (State)Interlocked.Exchange(ref _currentState, (int) newState);
        }

        private void WaitForThread()
        {
            if (_thread != null && _thread.IsAlive)
            {
                _thread.Join();
            }
        }

        private void ThreadExecute(object arg)
        {
            ConnectionParams connectionParams = arg as ConnectionParams;

            Error error = Error.Success;
            while(currentState != State.Disconnected)
            {
                if (connectionParams != null)
                {
                    ClearReconnectSubscriptions();

                    _loopTimeout = connectionParams.keepalive * 1000;

                    error = (Error)Native.mosquitto_connect_bind_v5(
                        _mosq, 
                        connectionParams.host, 
                        connectionParams.port,
                        connectionParams.keepalive, 
                        connectionParams.bindAddress,
                        connectionParams.properties.nativePropertyList);
                    connectionParams.Dispose();
                    connectionParams = null;
                }
                else
                {
                    error = (Error)Native.mosquitto_reconnect(_mosq);
                }

                if (error != Error.Success)
                {
                    EnqueueCallbacks(GetConnectionCallbacks(), new CallbackArgumentList {error = error});
                }

                while (currentState != State.Disconnected && error == Error.Success)
                {
                    error = (Error)Native.mosquitto_loop(_mosq, _loopTimeout, 1);
                }

                ProcessSubscriptionsOnConnectionLost();

                if (!TryWaitForReconnect())
                {
                    break;
                }
            }
            SetDisconnected(error);
        }

        private void SetDisconnected(Error error, PropertyListV5 prop = default)
        {
            // only invoke callbacks if we aren't already disconnected
            var prevState = ExchangeState(State.Disconnected);
            if (prevState == State.Connecting || prevState == State.Disconnected)
            {
                return;
            }

            GetDisconnectedCallbacks(out var onDisconnected, out var onDisconnectedV5);
            EnqueueCallbacks(
                new CallbackList { context = _defaultContext, onDisconnected = onDisconnected, onDisconnectedV5 = onDisconnectedV5 },
                new CallbackArgumentList {error = error, properties = ManagedPropertyListV5Pool.Obtain(prop)});
        }

        private void HandleLog(MosquittoPtr mosq, IntPtr obj, int level, string message)
        {
            EnqueueCallbacks(new CallbackList { context = _defaultContext, onLog = onLogEvent }, new CallbackArgumentList { intValue = level, logMessage = message });
        }

        private int PasswordCallback(IntPtr buf, int size, int rwflag, MosquittoPtr mosq)
        {
            GetPassword getPassword = Interlocked.CompareExchange(ref _getPassword, null, null);
            if (getPassword == null)
            {
                return 0;
            }

            var pass = getPassword.Invoke();
            int len = Math.Min(pass.Length, size);

            Marshal.Copy(pass, 0, buf, len);
            return len;
        }

        private CallbackList GetConnectionCallbacks()
        {
            lock (_connectionCallbackMutex)
            {
                var callbacks = _connectionCallbacks;
                _connectionCallbacks = default;
                return callbacks;
            }
        }

        protected virtual void ClearCallbacks()
        {
            onLogEvent = null;
            Interlocked.Exchange(ref _getPassword, null);

            lock (_connectionCallbackMutex)
            {
                _connectionCallbacks = default;
            }
            ClearSessionCallbacks();
        }

        private void ClearSessionCallbacks()
        { 
            lock (_subscribeCallbacks)
            {
                _subscribeCallbacks.Clear();
            }
            lock (_unsubscribeCallbacks)
            {
                _unsubscribeCallbacks.Clear();
            }
            lock (_publishCallbacks)
            {
                _publishCallbacks.Clear();
            }
        }

        private void SetupNativeCallbacks()
        {
            Native.mosquitto_log_callback_set(_mosq, HandleLog);
            Native.mosquitto_connect_v5_callback_set(_mosq, HandleConnected);
            Native.mosquitto_disconnect_v5_callback_set(_mosq, HandleDisconnected);
            Native.mosquitto_publish_v5_callback_set(_mosq, HandlePublished);
            Native.mosquitto_subscribe_v5_callback_set(_mosq, HandleSubscribed);
            Native.mosquitto_unsubscribe_v5_callback_set(_mosq, HandleUnsubscribed);
            Native.mosquitto_message_v5_callback_set(_mosq, HandleMessageReceived);
        }

        private void ClearReconnectSubscriptions()
        {
            _subscribeOnReconnectList.Clear();
            _unsubscribeOnReconnectList.Clear();
        }

        private void ProcessSubscriptionsOnConnectionLost()
        {
            if (_cleanSession)
            {
                // First invoke callbacks for all unsubscribes, since clean session will clear them on the server anyway
                lock (_unsubscribeCallbacks)
                {
                    foreach (var unsub in _unsubscribeCallbacks.Values)
                    {
                        _subscriptions.Remove(unsub.topic);
                        EnqueueCallbacks(unsub.cb, default);
                    }
                    _unsubscribeCallbacks.Clear();
                }

                if (_reconnectSettings.reconnectAutomatically)
                {
                    // second, add all of our subscriptions to the resubscribe list
                    foreach (var sub in _subscriptions)
                    {
                        _subscribeOnReconnectList.Add(new PendingSubscribe(sub.Key, sub.Value, default));
                    }
                    _subscriptions.Clear();
                }
            }

            if (_reconnectSettings.reconnectAutomatically)
            {
                // third, readd all of our inflight subscribes to our list to resubscribe
                // to make sure we are subscribed (unclear state on disconnect)
                lock (_subscribeCallbacks)
                {
                    _subscribeOnReconnectList.AddRange(_subscribeCallbacks.Values);
                    _subscribeCallbacks.Clear();
                }

                if (!_cleanSession)
                {
                    // finally, add all unsubscribes to our list of onsubs to invoke on
                    // reconnect.
                    lock (_unsubscribeCallbacks)
                    {
                        _unsubscribeOnReconnectList.AddRange(_unsubscribeCallbacks.Values);
                        _unsubscribeCallbacks.Clear();
                    }
                }
            }
        }

        private void ResendSubscriptions()
        {
            foreach (var sub in _subscribeOnReconnectList)
            {
                if (sub.topic != null)
                {
                    SubscribeInternal(sub.topic, sub.qos, onSubscribed: sub.cb.onSubscribed, onSubscribedV5: sub.cb.onSubscribedV5);
                }
                if (sub.topics != null)
                {
                    SubscribeMultipleInternal(sub.topics, sub.qos, onSubscribed: sub.cb.onSubscribedMultiple, onSubscribedV5: sub.cb.onSubscribedMultipleV5);
                }
            }
            _subscribeOnReconnectList.Clear();

            foreach (var unsub in _unsubscribeOnReconnectList)
            {
                UnsubscribeInternal(unsub.topic, onUnsubscribed: unsub.cb.onUnsubscribed, onUnsubscribedV5: unsub.cb.onUnsubscribedV5);
            }
            _unsubscribeOnReconnectList.Clear();
        }

        private bool TryWaitForReconnect()
        {
            _reconnectResetEvent.Reset();
            if (!_reconnectSettings.reconnectAutomatically || !TryUpdateState(State.Reconnecting, State.Connected))
            {
                return false;
            }

            _reconnects++;
            int retryMultiplier = _reconnectSettings.exponentialBackoff ? _reconnects * _reconnects : _reconnects;
            int retryDelay = Math.Min(_reconnectSettings.initialReconnectDelay * retryMultiplier, _reconnectSettings.maximumReconnectDelay);

            // wait until our timeout (in which case we reconnect), or a signal, in which case we break
            if (_reconnectResetEvent.WaitOne(TimeSpan.FromSeconds(retryDelay)))
            {
                return false;
            }
            return true;
        }

        private void HandleConnected(MosquittoPtr mosq, IntPtr obj, int rc, int flags, MosquittoPropertyPtr prop)
        {
            if (rc == 0)
            {
                // only move to connected from connecting or reconnecting
                TryUpdateState(State.Connected, State.Connecting);
                if (TryUpdateState(State.Connected, State.Reconnecting, trueIfAlreadySet: false))
                {
                    ResendSubscriptions();
                }

                _reconnects = 0;
            }
            else
            {
                // Manually map the Mosquitto v3.1.1 response codes to
                // a range that doesn't overlap with client error codes
                if (rc < (int)ConnectFailedReason.Unspecified)
                {
                    rc += (int)ConnectFailedReason.Unspecified;
                }
            }

            EnqueueCallbacks(GetConnectionCallbacks(),
                new CallbackArgumentList { intValue = rc, properties = ManagedPropertyListV5Pool.Obtain(new PropertyListV5(prop)) });
        }

        private void HandleDisconnected(MosquittoPtr mosq, IntPtr obj, int rc, MosquittoPropertyPtr prop)
        {
            if (_reconnectSettings.reconnectAutomatically && currentState != State.Disconnecting)
            {
                // ignore this disconnect if we weren't disconnecting, we will try reconnecting in our thread
                return;
            }

            SetDisconnected((Error)rc, new PropertyListV5(prop));
        }

        private void HandlePublished(MosquittoPtr mosq, IntPtr obj, int mid, int rc, MosquittoPropertyPtr prop)
        {
            CallbackList cb = default;
            lock (_publishCallbacks)
            {
                if (_publishCallbacks.TryGetValue(mid, out cb))
                {
                    _publishCallbacks.Remove(mid);
                }
            }
            EnqueueCallbacks(cb,
                new CallbackArgumentList { intValue = rc, properties = ManagedPropertyListV5Pool.Obtain(new PropertyListV5(prop)) });
        }

        private void HandleSubscribed(MosquittoPtr mosq, IntPtr obj, int mid, int qos_count, IntPtr qos_list, MosquittoPropertyPtr prop)
        {
            string topic = null;
            string[] topics = null;
            CallbackList cb = default;
            lock (_subscribeCallbacks)
            {
                if (_subscribeCallbacks.TryGetValue(mid, out var subscribe))
                {
                    cb = subscribe.cb;
                    topic = subscribe.topic;
                    topics = subscribe.topics;
                    _subscribeCallbacks.Remove(mid);
                }
            }

            QualityOfService qos = default;
            QualityOfService[] qosList = null;

            // only create QoS list if necessary
            if (cb.onSubscribedMultiple != null || cb.onSubscribedMultipleV5 != null)
            {
                qosList = new QualityOfService[qos_count];
            }

            for (int i = 0; i < qos_count; i++)
            {
                if (topics != null && i < topics.Length)
                {
                    topic = topics[i];
                }
                
                qos = (QualityOfService)Marshal.ReadInt32(IntPtr.Add(qos_list, 4 * i));

                // if we use clean session, and auto reconnect, we need to manually
                // record the list of subscriptions so that we can resubscribe on reconnect
                if (_cleanSession && _reconnectSettings.reconnectAutomatically)
                {
                    _subscriptions[topic] = qos;
                }
                
                if (qosList != null)
                {
                    qosList[i] = qos;
                }
            }

            EnqueueCallbacks(cb,
                new CallbackArgumentList { qos = qos, qosList = qosList, properties = ManagedPropertyListV5Pool.Obtain(new PropertyListV5(prop)) });
        }
        private void HandleUnsubscribed(MosquittoPtr mosq, IntPtr obj, int mid, MosquittoPropertyPtr prop)
        {
            string topic = null;
            CallbackList cb = default;
            lock (_unsubscribeCallbacks)
            {
                if (_unsubscribeCallbacks.TryGetValue(mid, out var unsubscribe))
                {
                    topic = unsubscribe.topic;
                    cb = unsubscribe.cb;
                    _unsubscribeCallbacks.Remove(mid);
                }
            }

            // if we use clean session, and auto reconnect, we need to manually
            // record the list of subscriptions so that we can resubscribe on reconnect
            if (_cleanSession && _reconnectSettings.reconnectAutomatically)
            {
                _subscriptions.Remove(topic);
            }

            EnqueueCallbacks(cb,
                new CallbackArgumentList { properties = ManagedPropertyListV5Pool.Obtain(new PropertyListV5(prop)) });
        }

        private void HandleMessageReceived(MosquittoPtr mosq, IntPtr obj, ref Native.mosquitto_message msg, MosquittoPropertyPtr prop)
        {
            GetMessageReceivedCallbacks(out var onMessageReceived, out var onMessageReceivedV5);
            if (onMessageReceived == null && onMessageReceivedV5 == null)
            {
                return;
            }

            byte[] payload = new byte[msg.payloadlen];
            Marshal.Copy(msg.payload, payload, 0, msg.payloadlen);
            string topic = Marshal.PtrToStringAnsi(msg.topic);

            EnqueueCallbacks(new CallbackList { context = _defaultContext, onMessageReceived = onMessageReceived, onMessageReceivedV5 = onMessageReceivedV5 },
                new CallbackArgumentList { message = new Message(msg.mid, topic, payload, msg.payloadlen, (QualityOfService)msg.qos, msg.retain), properties = ManagedPropertyListV5Pool.Obtain(new PropertyListV5(prop)) });

        }

        private void EnqueueCallbacks(CallbackList cb, CallbackArgumentList args)
        {
            if (cb.isEmpty)
            {
                return;
            }

            var callbackContainer = ObtainReusableCallbackContainer(in cb, in args);

            // Execute our callback on a different thread (either the synchronization context thread, or a worker thread)
            if (cb.context == null)
            {
                ThreadPool.QueueUserWorkItem(_waitCallback, callbackContainer);
            }
            else
            {
                cb.context.Post(_sendOrPostCallback, callbackContainer);
            }
        }

        private void InvokeCallbacks(object obj)
        {
            if (!(obj is ReusableCallbackContainer callbackContainer))
            {
                return;
            }
            ref var cb = ref callbackContainer.callbackList;
            ref var args = ref callbackContainer.callbackArgumentList;
            try
            {
                var onLog = cb.onLog;
                if (onLog != null)
                {
                    onLog(args.logLevel, args.logMessage);
                    return;
                }

                var onMessageReceived = cb.onMessageReceived;
                if (onMessageReceived != null)
                {
                    onMessageReceived(args.message);
                    return;
                }

                var onMessageReceivedV5 = cb.onMessageReceivedV5;
                if (onMessageReceivedV5 != null)
                {
                    onMessageReceivedV5(args.message, args.properties);
                    return;
                }

                var onPublished = cb.onPublished;
                if (onPublished != null)
                {
                    onPublished();
                    return;
                }

                var onPublishedV5 = cb.onPublishedV5;
                if (onPublishedV5 != null)
                {
                    if (args.publishFailedReasonV5 == PublishFailedReasonV5.Success)
                    {
                        onPublishedV5(args.properties);
                        return;
                    }
                }

                var onPublishFailedV5 = cb.onPublishFailedV5;
                if (onPublishFailedV5 != null)
                {
                    if (args.publishFailedReasonV5 != PublishFailedReasonV5.Success)
                    {
                        onPublishFailedV5(args.publishFailedReasonV5, args.properties);
                        return;
                    }
                }

                var onSubscribed = cb.onSubscribed;
                if (onSubscribed != null)
                {
                    onSubscribed(args.qos);
                    return;
                }

                var onSubscribedV5 = cb.onSubscribedV5;
                if (onSubscribedV5 != null)
                {
                    onSubscribedV5(args.qos, args.properties);
                    return;
                }

                var onSubscribedMultiple = cb.onSubscribedMultiple;
                if (onSubscribedMultiple != null)
                {
                    onSubscribedMultiple(args.qosList);
                    return;
                }

                var onSubscribedMultipleV5 = cb.onSubscribedMultipleV5;
                if (onSubscribedMultipleV5 != null)
                {
                    onSubscribedMultipleV5(args.qosList, args.properties);
                }

                var onUnsubscribed = cb.onUnsubscribed;
                if (onUnsubscribed != null)
                {
                    onUnsubscribed();
                    return;
                }

                var onUnsubscribedV5 = cb.onUnsubscribedV5;
                if (onUnsubscribedV5 != null)
                {
                    onUnsubscribedV5(args.properties);
                    return;
                }

                var onConnected = cb.onConnected;
                if (onConnected != null)
                {
                    if (args.connectFailedReason == ConnectFailedReason.ConnectionAccepted)
                    {
                        onConnected();
                        return;
                    }
                }

                var onConnectedV5 = cb.onConnectedV5;
                if (onConnectedV5 != null)
                {
                    if (args.connectFailedReasonV5 == ConnectFailedReasonV5.Success)
                    {
                        onConnectedV5(args.properties);
                        return;
                    }
                }

                var onConnectFailed = cb.onConnectFailed;
                if (onConnectFailed != null)
                {
                    var connectionFailedReason = args.connectFailedReason;
                    if (connectionFailedReason != ConnectFailedReason.ConnectionAccepted)
                    {
                        onConnectFailed(connectionFailedReason < ConnectFailedReason.Unspecified ? args.error : Error.Success,
                            connectionFailedReason >= ConnectFailedReason.Unspecified ? connectionFailedReason : ConnectFailedReason.Unspecified);
                        return;
                    }
                }

                var onConnectFailedV5 = cb.onConnectFailedV5;
                if (onConnectFailedV5 != null)
                {
                    var connectionFailedReason = args.connectFailedReasonV5;
                    if (connectionFailedReason != ConnectFailedReasonV5.Success)
                    {
                        onConnectFailedV5(
                            connectionFailedReason < ConnectFailedReasonV5.Unspecified ? args.error : Error.Success,
                            connectionFailedReason >= ConnectFailedReasonV5.Unspecified ? connectionFailedReason : ConnectFailedReasonV5.Unspecified, args.properties);
                        return;
                    }
                }

                var onDisconnected = cb.onDisconnected;
                if (onDisconnected != null)
                {
                    onDisconnected(args.error);
                    return;
                }

                var onDisconnectedV5 = cb.onDisconnectedV5;
                if (onDisconnectedV5 != null)
                {
                    var disconnectReason = args.disconnectReasonV5;
                    onDisconnectedV5(
                        disconnectReason < DisconnectReasonV5.Unspecified ? args.error : Error.Success,
                        disconnectReason >= DisconnectReasonV5.Unspecified || disconnectReason == DisconnectReasonV5.NormalDisconnection ? disconnectReason : DisconnectReasonV5.Unspecified,
                        args.properties);
                    return;
                }
            }
            finally
            {
                ReleaseReusableCallbackContainer(callbackContainer);
            }
        }

        private ReusableCallbackContainer ObtainReusableCallbackContainer(in CallbackList callbackList, in CallbackArgumentList callbackArgumentList)
        {
            if (!_reusableCallbackContainers.TryTake(out var callbackContainer))
            {
                callbackContainer = new ReusableCallbackContainer();
            }
            callbackContainer.callbackList = callbackList;
            callbackContainer.callbackArgumentList = callbackArgumentList;
            return callbackContainer;
        }

        private void ReleaseReusableCallbackContainer(ReusableCallbackContainer callbackContainer)
        {
            callbackContainer.Reset();
            if (_reusableCallbackContainers.Count >= _maxReusableContainers)
            {
                _reusableCallbackContainers.Add(callbackContainer);
            }
        }

        protected abstract void GetMessageReceivedCallbacks(out OnMessageReceived onMessageReceived, out OnMessageReceivedV5 onMessageReceivedV5);
        protected abstract void GetDisconnectedCallbacks(out OnDisconnected onDisconnected, out OnDisconnectedV5 onDisconnectedV5);
    }
}