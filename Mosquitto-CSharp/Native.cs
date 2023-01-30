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
using System.Security;
using MosquittoPtr = System.IntPtr;
using MosquittoPropertyPtr = System.IntPtr;
using MosquittoMessagePtr = System.IntPtr;
using SizeT = System.UIntPtr;
using IntArrayPtr = System.IntPtr;
using StringPtr = System.IntPtr;
using StringArrayPtr = System.IntPtr;

namespace Mosquitto
{
    /// <summary>
    /// This class contains functions and definitions for use with libmosquitto, the Mosquitto client library.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public static class Native
    {
        #region Native Library Name
#if UNITY_EDITOR
        // We are inside the Unity Editor.
#if UNITY_EDITOR_OSX
		// Unity Editor on macOS needs to use libmosquitto.
		private const string nativeLibrary = "libmosquitto";
		private const string cryptoNativeLibrary = "libcrypto";
		private const string sslNativeLibrary = "libssl";
		private const string memoryNativeLibrary = "libmemory";
#else
        private const string nativeLibrary = "mosquitto";
        private const string cryptoNativeLibrary = "crypto";
        private const string sslNativeLibrary = "ssl";
        private const string memoryNativeLibrary = "memory";
#endif
#endif

#if !UNITY_EDITOR
		// We're not inside the Unity Editor.
#if (__APPLE__ && !(__IOS__ || UNITY_IOS)) || UNITY_PS4 || UNITY_PS5
		// Use libmosquitto on macOS and Playstation.
		private const string nativeLibrary = "libmosquitto";
		private const string cryptoNativeLibrary = "libcrypto";
		private const string sslNativeLibrary = "libssl";
        private const string memoryNativeLibrary = "libmemory";
#elif __IOS__ || UNITY_IOS
        // We're building for a certain mobile fruity OS.
		private const string nativeLibrary = "__Internal";
		private const string cryptoNativeLibrary = "__Internal";
		private const string sslNativeLibrary = "__Internal";
		private const string memoryNativeLibrary = "__Internal";
#else
		// Assume everything else, Windows et al.
		private const string nativeLibrary = "mosquitto";
		private const string cryptoNativeLibrary = "crypto";
		private const string sslNativeLibrary = "ssl";
        private const string memoryNativeLibrary = "memory";
#endif
#endif
        #endregion

        #region Static Initialization
        private static bool _initialized;

        /// <summary>
        /// Because mosquitto has a dependency on ssl and crypto, we need
        /// to manually ensure they are loaded in the correct order
        /// on some platforms. To do this, we simply call the init
        /// methods in the specified order.
        /// </summary>
        private static void InitOpenSSL()
        {
            const ulong OPENSSL_INIT_LOAD_CRYPTO_STRINGS = 0x00000002L;
            const ulong OPENSSL_INIT_LOAD_SSL_STRINGS = 0x00200000L;
            const ulong OPENSSL_INIT_SSL_DEFAULT = OPENSSL_INIT_LOAD_SSL_STRINGS |
                                                   OPENSSL_INIT_LOAD_CRYPTO_STRINGS;
            // Call init crypto to load crypto first
            Native.OPENSSL_init_crypto(OPENSSL_INIT_LOAD_CRYPTO_STRINGS, IntPtr.Zero);
            // Call init ssl to load sll second
            Native.OPENSSL_init_ssl(OPENSSL_INIT_SSL_DEFAULT, IntPtr.Zero);
        }

        public static void Initialize()
        {
            if (_initialized)
            {
                return;
            }
            InitOpenSSL();
            Native.mosquitto_lib_init();
            int major = 0, minor = 0, revision = 0;
            if (Native.mosquitto_lib_version(ref major, ref minor, ref revision) != Native.LIBMOSQUITTO_VERSION_NUMBER)
            {
                Native.mosquitto_lib_cleanup();
                throw new Exception($"Invalid Native Library Version of libmosquitto! Expected {Native.LIBMOSQUITTO_MAJOR}.{Native.LIBMOSQUITTO_MINOR}.{Native.LIBMOSQUITTO_REVISION} but loaded version was {major}.{minor}.{revision}");
            }
            _initialized = true;
        }

        public static void Deinitialize()
        {
            if (!_initialized)
            {
                return;
            }

            Native.mosquitto_lib_cleanup();
            _initialized = false;
        }
        #endregion

        #region Defines
        internal const uint LIBMOSQUITTO_MAJOR = 2;
        internal const uint LIBMOSQUITTO_MINOR = 0;
        internal const uint LIBMOSQUITTO_REVISION = 15;
        /// <summary> 
        /// LIBMOSQUITTO_VERSION_NUMBER looks like 1002001 for e.g. version 1.2.1. 
        /// </summary>
        internal const uint LIBMOSQUITTO_VERSION_NUMBER = (LIBMOSQUITTO_MAJOR * 1000000 + LIBMOSQUITTO_MINOR * 1000 + LIBMOSQUITTO_REVISION);

        internal const int PROTOCOL_VERSION_v311 = 4;
        internal const int PROTOCOL_VERSION_v5 = 5;

        /// <summary> 
        /// Log types 
        /// </summary>
        internal const uint MOSQ_LOG_NONE = 0;
        internal const uint MOSQ_LOG_INFO = (1 << 0);
        internal const uint MOSQ_LOG_NOTICE = (1 << 1);
        internal const uint MOSQ_LOG_WARNING = (1 << 2);
        internal const uint MOSQ_LOG_ERR = (1 << 3);
        internal const uint MOSQ_LOG_DEBUG = (1 << 4);
        internal const uint MOSQ_LOG_SUBSCRIBE = (1 << 5);
        internal const uint MOSQ_LOG_UNSUBSCRIBE = (1 << 6);
        internal const uint MOSQ_LOG_WEBSOCKETS = (1 << 7);
        internal const uint MOSQ_LOG_INTERNAL = 0x80000000U;
        internal const uint MOSQ_LOG_ALL = 0xFFFFFFFFU;

        /// <summary> 
        /// Integer values returned from many libmosquitto functions. 
        /// </summary>
        internal enum mosq_err_t
        {
            MOSQ_ERR_AUTH_CONTINUE = -4,
            MOSQ_ERR_NO_SUBSCRIBERS = -3,
            MOSQ_ERR_SUB_EXISTS = -2,
            MOSQ_ERR_CONN_PENDING = -1,
            MOSQ_ERR_SUCCESS = 0,
            MOSQ_ERR_NOMEM = 1,
            MOSQ_ERR_PROTOCOL = 2,
            MOSQ_ERR_INVAL = 3,
            MOSQ_ERR_NO_CONN = 4,
            MOSQ_ERR_CONN_REFUSED = 5,
            MOSQ_ERR_NOT_FOUND = 6,
            MOSQ_ERR_CONN_LOST = 7,
            MOSQ_ERR_TLS = 8,
            MOSQ_ERR_PAYLOAD_SIZE = 9,
            MOSQ_ERR_NOT_SUPPORTED = 10,
            MOSQ_ERR_AUTH = 11,
            MOSQ_ERR_ACL_DENIED = 12,
            MOSQ_ERR_UNKNOWN = 13,
            MOSQ_ERR_ERRNO = 14,
            MOSQ_ERR_EAI = 15,
            MOSQ_ERR_PROXY = 16,
            MOSQ_ERR_PLUGIN_DEFER = 17,
            MOSQ_ERR_MALFORMED_UTF8 = 18,
            MOSQ_ERR_KEEPALIVE = 19,
            MOSQ_ERR_LOOKUP = 20,
            MOSQ_ERR_MALFORMED_PACKET = 21,
            MOSQ_ERR_DUPLICATE_PROPERTY = 22,
            MOSQ_ERR_TLS_HANDSHAKE = 23,
            MOSQ_ERR_QOS_NOT_SUPPORTED = 24,
            MOSQ_ERR_OVERSIZE_PACKET = 25,
            MOSQ_ERR_OCSP = 26,
            MOSQ_ERR_TIMEOUT = 27,
            MOSQ_ERR_RETAIN_NOT_SUPPORTED = 28,
            MOSQ_ERR_TOPIC_ALIAS_INVALID = 29,
            MOSQ_ERR_ADMINISTRATIVE_ACTION = 30,
            MOSQ_ERR_ALREADY_EXISTS = 31,
        };

        /// <summary> 
        /// Client options.
        /// See <see cref="mosquitto_int_option"/>, <see cref="mosquitto_string_option"/>, and <see cref="mosquitto_void_option"/>.
        /// </summary>
        internal enum mosq_opt_t
        {
            MOSQ_OPT_PROTOCOL_VERSION = 1,
            MOSQ_OPT_SSL_CTX = 2,
            MOSQ_OPT_SSL_CTX_WITH_DEFAULTS = 3,
            MOSQ_OPT_RECEIVE_MAXIMUM = 4,
            MOSQ_OPT_SEND_MAXIMUM = 5,
            MOSQ_OPT_TLS_KEYFORM = 6,
            MOSQ_OPT_TLS_ENGINE = 7,
            MOSQ_OPT_TLS_ENGINE_KPASS_SHA1 = 8,
            MOSQ_OPT_TLS_OCSP_REQUIRED = 9,
            MOSQ_OPT_TLS_ALPN = 10,
            MOSQ_OPT_TCP_NODELAY = 11,
            MOSQ_OPT_BIND_ADDRESS = 12,
            MOSQ_OPT_TLS_USE_OS_CERTS = 13,
        };


        /// <summary> 
        /// MQTT specification restricts client ids to a maximum of 23 characters
        /// </summary>
        internal const int MOSQ_MQTT_ID_MAX_LENGTH = 23;

        internal const int MQTT_PROTOCOL_V31 = 3;
        internal const int MQTT_PROTOCOL_V311 = 4;
        internal const int MQTT_PROTOCOL_V5 = 5;

        /// <summary> 
		/// Contains details of a PUBLISH message.
		/// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        internal struct mosquitto_message
        {
            /// <summary>
            /// The message/packet ID of the PUBLISH message, assuming this is a
            /// QoS 1 or 2 message. Will be set to 0 for QoS 0 messages.
            /// </summary>
            internal int mid;
            /// <summary>
            /// the topic the message was delivered on.
            /// </summary>
            internal StringPtr topic;
            /// <summary>
            /// The message payload. This will be payloadlen bytes long, and
            /// may be NULL if a zero length payload was sent.
            /// </summary>
            internal IntPtr payload;
            /// <summary>
            /// The length of the payload, in bytes.
            /// </summary>
            internal int payloadlen;
            /// <summary>
            /// The quality of service of the message, 0, 1, or 2.
            /// </summary>
            internal int qos;
            /// <summary>
            /// Set to true for stale retained messages.
            /// </summary>
            internal bool retain;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        internal struct libmosquitto_will
        {
            internal StringPtr topic;
            internal IntPtr payload;
            internal int payloadlen;
            internal int qos;
            internal bool retain;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        internal struct libmosquitto_auth
        {
            internal StringPtr username;
            internal StringPtr password;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        internal struct libmosquitto_tls
        {
            internal StringPtr cafile;
            internal StringPtr capath;
            internal StringPtr certfile;
            internal StringPtr keyfile;
            internal StringPtr ciphers;
            internal StringPtr tls_version;
            internal pw_callback pw_callback;
            internal int cert_reqs;
        };
        internal enum mqtt5_property
        {
            MQTT_PROP_PAYLOAD_FORMAT_INDICATOR = 1,     /* Byte :				PUBLISH, Will Properties */
            MQTT_PROP_MESSAGE_EXPIRY_INTERVAL = 2,      /* 4 byte int :			PUBLISH, Will Properties */
            MQTT_PROP_CONTENT_TYPE = 3,                 /* UTF-8 string :		PUBLISH, Will Properties */
            MQTT_PROP_RESPONSE_TOPIC = 8,               /* UTF-8 string :		PUBLISH, Will Properties */
            MQTT_PROP_CORRELATION_DATA = 9,             /* Binary Data :		PUBLISH, Will Properties */
            MQTT_PROP_SUBSCRIPTION_IDENTIFIER = 11,     /* Variable byte int :	PUBLISH, SUBSCRIBE */
            MQTT_PROP_SESSION_EXPIRY_INTERVAL = 17,     /* 4 byte int :			CONNECT, CONNACK, DISCONNECT */
            MQTT_PROP_ASSIGNED_CLIENT_IDENTIFIER = 18,  /* UTF-8 string :		CONNACK */
            MQTT_PROP_SERVER_KEEP_ALIVE = 19,           /* 2 byte int :			CONNACK */
            MQTT_PROP_AUTHENTICATION_METHOD = 21,       /* UTF-8 string :		CONNECT, CONNACK, AUTH */
            MQTT_PROP_AUTHENTICATION_DATA = 22,         /* Binary Data :		CONNECT, CONNACK, AUTH */
            MQTT_PROP_REQUEST_PROBLEM_INFORMATION = 23, /* Byte :				CONNECT */
            MQTT_PROP_WILL_DELAY_INTERVAL = 24,         /* 4 byte int :			Will properties */
            MQTT_PROP_REQUEST_RESPONSE_INFORMATION = 25,/* Byte :				CONNECT */
            MQTT_PROP_RESPONSE_INFORMATION = 26,        /* UTF-8 string :		CONNACK */
            MQTT_PROP_SERVER_REFERENCE = 28,            /* UTF-8 string :		CONNACK, DISCONNECT */
            MQTT_PROP_REASON_STRING = 31,               /* UTF-8 string :		All except Will properties */
            MQTT_PROP_RECEIVE_MAXIMUM = 33,             /* 2 byte int :			CONNECT, CONNACK */
            MQTT_PROP_TOPIC_ALIAS_MAXIMUM = 34,         /* 2 byte int :			CONNECT, CONNACK */
            MQTT_PROP_TOPIC_ALIAS = 35,                 /* 2 byte int :			PUBLISH */
            MQTT_PROP_MAXIMUM_QOS = 36,                 /* Byte :				CONNACK */
            MQTT_PROP_RETAIN_AVAILABLE = 37,            /* Byte :				CONNACK */
            MQTT_PROP_USER_PROPERTY = 38,               /* UTF-8 string pair :	All */
            MQTT_PROP_MAXIMUM_PACKET_SIZE = 39,         /* 4 byte int :			CONNECT, CONNACK */
            MQTT_PROP_WILDCARD_SUB_AVAILABLE = 40,      /* Byte :				CONNACK */
            MQTT_PROP_SUBSCRIPTION_ID_AVAILABLE = 41,   /* Byte :				CONNACK */
            MQTT_PROP_SHARED_SUB_AVAILABLE = 42,        /* Byte :				CONNACK */
        };
        internal enum mqtt5_property_type
        {
            MQTT_PROP_TYPE_BYTE = 1,
            MQTT_PROP_TYPE_INT16 = 2,
            MQTT_PROP_TYPE_INT32 = 3,
            MQTT_PROP_TYPE_VARINT = 4,
            MQTT_PROP_TYPE_BINARY = 5,
            MQTT_PROP_TYPE_STRING = 6,
            MQTT_PROP_TYPE_STRING_PAIR = 7
        };

        internal enum mqtt5_sub_options
        {
            MQTT_SUB_OPT_NO_LOCAL = 0x04,
            MQTT_SUB_OPT_RETAIN_AS_PUBLISHED = 0x08,
            MQTT_SUB_OPT_SEND_RETAIN_ALWAYS = 0x00,
            MQTT_SUB_OPT_SEND_RETAIN_NEW = 0x10,
            MQTT_SUB_OPT_SEND_RETAIN_NEVER = 0x20,
        };
        #endregion

        // Topic: Threads
        // 	libmosquitto provides thread safe operation, with the exception of
        // 	<see cref="mosquitto_lib_init"/> which is not thread safe.
        // 
        // 	If the library has been compiled without thread support it is *not*
        // 	guaranteed to be thread safe.
        // 
        // 	If your application uses threads you must use <see cref="mosquitto_threaded_set"/> to
        // 	tell the library this is the case, otherwise it makes some optimisations
        // 	for the single threaded case that may result in unexpected behaviour for
        // 	the multi threaded case.

        // **************************************************
        // Important note
        //
        // The following functions that deal with network operations will return
        // MOSQ_ERR_SUCCESS on success, but this does not mean that the operation has
        // taken place. An attempt will be made to write the network data, but if the
        // socket is not available for writing at that time then the packet will not be
        // sent. To ensure the packet is sent, call mosquitto_loop() (which must also
        // be called to process incoming network data).
        // This is especially important when disconnecting a client that has a will. If
        // the broker does not receive the DISCONNECT command, it will assume that the
        // client has disconnected unexpectedly and send the will.
        // 
        // mosquitto_connect()
        // mosquitto_disconnect()
        // mosquitto_subscribe()
        // mosquitto_unsubscribe()
        // mosquitto_publish()
        // **************************************************

        #region Library version, init, and cleanup
        /// <summary>
        /// Can be used to obtain version information for the mosquitto library.
        /// This allows the application to compare the library version against the
        /// version it was compiled against by using the LIBMOSQUITTO_MAJOR,
        /// LIBMOSQUITTO_MINOR and LIBMOSQUITTO_REVISION defines.
        /// </summary>
        /// <param name="major">an integer pointer. If not NULL, the major version of the library will be returned in this variable.</param>
        /// <param name="minor">an integer pointer. If not NULL, the minor version of the library will be returned in this variable.</param>
        /// <param name="revision">an integer pointer. If not NULL, the revision of the library will be returned in this variable.</param>
        /// <returns><list type="table">
        /// <item><term>LIBMOSQUITTO_VERSION_NUMBER</term><description>which is a unique number based on the major, minor and revision values.</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_lib_cleanup"/>, <see cref="mosquitto_lib_init"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int mosquitto_lib_version(ref int major, ref int minor, ref int revision);

        /// <summary>
        /// <para>
        /// Must be called before any other mosquitto functions.
        /// </para>
        /// <para>
        /// This function is *not* thread safe.
        /// </para>
        /// </summary>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_UNKNOWN</term><description>on Windows, if sockets couldn't be initialized.</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_lib_cleanup"/>, <see cref="mosquitto_lib_version"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_lib_init();

        /// <summary>
        /// Call to free resources associated with the library.
        /// </summary>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>always</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_lib_init"/>, <see cref="mosquitto_lib_version"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_lib_cleanup();
        #endregion

        #region Client creation, destruction, and reinitialisation
        /// <summary>
        /// Create a new mosquitto client instance.
        /// </summary>
        /// <param name="id">String to use as the client id. If NULL, a random client id will be generated. 
        /// If id is NULL, clean_session must be true.</param>
        /// <param name="clean_session">set to true to instruct the broker to clean all messages 
        /// and subscriptions on disconnect, false to instruct it to keep them. 
        /// See the man page mqtt(7) for more details. 
        /// Note that a client will never discard its own outgoing messages on disconnect. 
        /// Calling <see cref="mosquitto_connect"/> or <see cref="mosquitto_reconnect"/> will cause the messages to be resent.
        /// Use <see cref="mosquitto_reinitialise"/> to reset a client to its original state.
        /// Must be set to true if the id parameter is NULL.</param>
        /// <param name="obj">A user pointer that will be passed as an argument to any callbacks that are specified.</param>
        /// <returns>
        /// Pointer to a struct mosquitto on success.
        /// NULL on failure. Interrogate errno to determine the cause for the failure:
        /// <list type="bullet">
        /// <item><description>ENOMEM on out of memory.</description></item>
        /// <item><description>EINVAL on invalid input parameters.</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_reinitialise"/>, <see cref="mosquitto_destroy"/>, <see cref="mosquitto_user_data_set"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern MosquittoPtr mosquitto_new([MarshalAs(UnmanagedType.LPStr)] string id, bool clean_session, IntPtr obj);

        /// <summary>
        /// Use to free memory associated with a mosquitto client instance.
        /// </summary>
        /// <param name="mosq">a struct mosquitto pointer to free.</param>
        /// <remarks>
        /// See Also: <see cref="mosquitto_new"/>, <see cref="mosquitto_reinitialise"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mosquitto_destroy(MosquittoPtr mosq);

        /// <summary>
        /// This function allows an existing mosquitto client to be reused. Call on a
        /// mosquitto instance to close any open network connections, free memory
        /// and reinitialise the client with the new parameters. The end result is the
        /// same as the output of <see cref="mosquitto_new"/>.
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="id">string to use as the client id. If NULL, a random client id will be generated. 
        /// If id is NULL, clean_session must be true.</param>
        /// <param name="clean_session">set to true to instruct the broker to clean all messages 
        /// and subscriptions on disconnect, false to instruct it to keep them. 
        /// See the man page mqtt(7) for more details. 
        /// Must be set to true if the id parameter is NULL.</param>
        /// <param name="obj">A user pointer that will be passed as an argument to any callbacks that are specified.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>if an out of memory condition occurred.</description></item>
        /// <item><term>MOSQ_ERR_MALFORMED_UTF8</term><description>if the client id is not valid UTF-8.</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_new"/>, <see cref="mosquitto_destroy"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_reinitialise(MosquittoPtr mosq, [MarshalAs(UnmanagedType.LPStr)] string id, bool clean_session, IntPtr obj);
        #endregion

        #region Will
        /// <summary>
        /// <para>
        /// Configure will information for a mosquitto instance. By default, clients do
        /// not have a will.  This must be called before calling <see cref="mosquitto_connect"/>.
        /// </para>
        /// <para>
        /// It is valid to use this function for clients using all MQTT protocol versions.
        /// If you need to set MQTT v5 Will properties, use <see cref="mosquitto_will_set_v5"/> instead.
        /// </para>
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="topic">the topic on which to publish the will.</param>
        /// <param name="payloadlen">the size of the payload (bytes). Valid values are between 0 and 268,435,455.</param>
        /// <param name="payload">pointer to the data to send. If payloadlen > 0 this must be a valid memory location.</param>
        /// <param name="qos">integer value 0, 1 or 2 indicating the Quality of Service to be used for the will.</param>
        /// <param name="retain">set to true to make the will a retained message.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>if an out of memory condition occurred.</description></item>
        /// <item><term>MOSQ_ERR_PAYLOAD_SIZE</term><description>if payloadlen is too large.</description></item>
        /// <item><term>MOSQ_ERR_MALFORMED_UTF8</term><description>if the topic is not valid UTF-8.</description></item>
        /// </list></returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_will_set(MosquittoPtr mosq, [MarshalAs(UnmanagedType.LPStr)] string topic, int payloadlen, byte[] payload, int qos, bool retain);

        /// <summary>
        /// <para>
        /// Configure will information for a mosquitto instance, with attached
        /// properties. By default, clients do not have a will.  This must be called
        /// before calling <see cref="mosquitto_connect"/>.
        /// </para>
        /// <para>
        /// If the mosquitto instance `mosq` is using MQTT v5, the `properties` argument
        /// will be applied to the Will. For MQTT v3.1.1 and below, the `properties`
        /// argument will be ignored.
        /// </para>
        /// <para>
        /// Set your client to use MQTT v5 immediately after it is created:
        /// 
        /// <c>mosquitto_int_option(mosq, MOSQ_OPT_PROTOCOL_VERSION, MQTT_PROTOCOL_V5);</c>
        /// </para>
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="topic">the topic on which to publish the will.</param>
        /// <param name="payloadlen">the size of the payload (bytes). Valid values are between 0 and 268,435,455.</param>
        /// <param name="payload">pointer to the data to send. If payloadlen > 0 this must be a valid memory location.</param>
        /// <param name="qos">integer value 0, 1 or 2 indicating the Quality of Service to be used for the will.</param>
        /// <param name="retain">set to true to make the will a retained message.</param>
        /// <param name="properties">list of MQTT 5 properties. Can be NULL. 
        /// On success only, the property list becomes the property of libmosquitto once this function is called 
        /// and will be freed by the library. The property list must be freed by the application on error.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>if an out of memory condition occurred.</description></item>
        /// <item><term>MOSQ_ERR_PAYLOAD_SIZE</term><description>if payloadlen is too large.</description></item>
        /// <item><term>MOSQ_ERR_MALFORMED_UTF8</term><description>if the topic is not valid UTF-8.</description></item>
        /// <item><term>MOSQ_ERR_NOT_SUPPORTED</term><description>if properties is not NULL and the client is not using MQTT v5</description></item>
        /// <item><term>MOSQ_ERR_PROTOCOL</term><description>if a property is invalid for use with wills.</description></item>
        /// <item><term>MOSQ_ERR_DUPLICATE_PROPERTY</term><description>if a property is duplicated where it is forbidden.</description></item>
        /// </list></returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_will_set_v5(MosquittoPtr mosq, [MarshalAs(UnmanagedType.LPStr)] string topic, int payloadlen, byte[] payload, int qos, bool retain, MosquittoPropertyPtr properties);

        /// <summary>
        /// Remove a previously configured will. This must be called before calling
        /// <see cref="mosquitto_connect"/>.
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <returns><list type="table">
        /// <param name="MOSQ_ERR_SUCCESS">on success.</param>
        /// <param name="MOSQ_ERR_INVAL">if the input parameters were invalid.</param>
        /// </list></returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_will_clear(MosquittoPtr mosq);
        #endregion

        #region Username and password
        /// <summary>
        /// <para>
        /// Configure username and password for a mosquitto instance. By default, no
        /// username or password will be sent. For v3.1 and v3.1.1 clients, if username
        /// is NULL, the password argument is ignored.
        /// </para>
        /// <para>
        /// This is must be called before calling <see cref="mosquitto_connect"/>.
        /// </para>
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="username">the username to send as a string, or NULL to disable authentication.</param>
        /// <param name="password">the password to send as a string. Set to NULL when username is valid in order to send just a username.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>if an out of memory condition occurred.</description></item>
        /// </list></returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_username_pw_set(MosquittoPtr mosq, [MarshalAs(UnmanagedType.LPStr)] string username, [MarshalAs(UnmanagedType.LPStr)] string password);
        #endregion

        #region Connecting, reconnecting, disconnecting
        /// <summary>
        /// <para>
        /// Connect to an MQTT broker.
        /// </para>
        /// <para>
        /// It is valid to use this function for clients using all MQTT protocol versions.
        /// If you need to set MQTT v5 CONNECT properties, use <see cref="mosquitto_connect_bind_v5"/>
        /// instead.
        /// </para>
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="host">the hostname or ip address of the broker to connect to.</param>
        /// <param name="port">the network port to connect to. Usually 1883.</param>
        /// <param name="keepalive">the number of seconds after which the broker should send 
        /// a PING message to the client if no other messages have been exchanged in that time.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term>
        /// <description>if the input parameters were invalid, which could be any of:        
        /// <list type="bullet">
        ///     <item><description>mosq == NULL</description></item>
        ///     <item><description>host == NULL</description></item>
        ///     <item><description>port &lt; 0</description></item>
        ///     <item><description>keepalive &lt; 5</description></item>
        /// </list>
        /// </description></item>
        /// <item><term>MOSQ_ERR_ERRNO</term><description>if a system call returned an error. The variable errno contains the error code, even on Windows.
        /// Use strerror_r() where available or FormatMessage() on Windows.</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_connect_bind"/>, <see cref="mosquitto_connect_async"/>, <see cref="mosquitto_reconnect"/>, <see cref="mosquitto_disconnect"/>, <see cref="mosquitto_tls_set"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_connect(MosquittoPtr mosq, [MarshalAs(UnmanagedType.LPStr)] string host, int port, int keepalive);

        /// <summary>
        /// Connect to an MQTT broker. This extends the functionality of
        /// <see cref="mosquitto_connect"/> by adding the bind_address parameter. Use this function
        /// if you need to restrict network communication over a particular interface.
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="host">the hostname or ip address of the broker to connect to.</param>
        /// <param name="port">the network port to connect to. Usually 1883.</param>
        /// <param name="keepalive">the number of seconds after which the broker should send a PING message to the client if no other messages have been exchanged in that time.</param>
        /// <param name="bind_address">the hostname or ip address of the local network interface to bind to. If you do not want to bind to a specific interface, set this to NULL.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description> on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_ERRNO</term><description>if a system call returned an error. The variable errno contains the error code, even on Windows.
        /// Use strerror_r() where available or FormatMessage() on Windows.</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_connect"/>, <see cref="mosquitto_connect_async"/>, <see cref="mosquitto_connect_bind_async"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_connect_bind(MosquittoPtr mosq, [MarshalAs(UnmanagedType.LPStr)] string host, int port, int keepalive, [MarshalAs(UnmanagedType.LPStr)] string bind_address);

        /// <summary>
        /// <para>
        /// Connect to an MQTT broker. This extends the functionality of
        /// <see cref="mosquitto_connect"/> by adding the bind_address parameter and MQTT v5
        /// properties. Use this function if you need to restrict network communication
        /// over a particular interface.
        /// </para>
        /// <para>
        /// Use e.g. <see cref="mosquitto_property_add_string"/> and similar to create a list of
        /// properties, then attach them to this publish. Properties need freeing with
        /// <see cref="mosquitto_property_free_all"/>.
        /// </para>
        /// <para>
        /// If the mosquitto instance `mosq` is using MQTT v5, the `properties` argument
        /// will be applied to the CONNECT message. For MQTT v3.1.1 and below, the
        /// `properties` argument will be ignored.
        /// </para>
        /// <para>
        /// Set your client to use MQTT v5 immediately after it is created:
        /// 
        /// <c>mosquitto_int_option(mosq, MOSQ_OPT_PROTOCOL_VERSION, MQTT_PROTOCOL_V5);</c>
        /// </para>
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="host">the hostname or ip address of the broker to connect to.</param>
        /// <param name="port">the network port to connect to. Usually 1883.</param>
        /// <param name="keepalive">the number of seconds after which the broker should send a PING message to the client if no other messages have been exchanged in that time.</param>
        /// <param name="bind_address">the hostname or ip address of the local network interface to bind to. If you do not want to bind to a specific interface, set this to NULL.</param>
        /// <param name="properties">the MQTT 5 properties for the connect (not for the Will).</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid, which could be any of: 
        /// <list type="bullet">
        ///     <item><description>mosq == NULL</description></item>
        ///     <item><description>host == NULL</description></item>
        ///     <item><description>port &lt; 0</description></item>
        ///     <item><description>keepalive &lt; 5</description></item>
        /// </list></description></item>
        /// <item><term>MOSQ_ERR_ERRNO</term><description>if a system call returned an error. The variable errno contains the error code, even on Windows. 
        /// Use strerror_r() where available or FormatMessage() on Windows.</description></item>
        /// <item><term>MOSQ_ERR_DUPLICATE_PROPERTY</term><description>if a property is duplicated where it is forbidden.</description></item>
        /// <item><term>MOSQ_ERR_PROTOCOL</term><description>if any property is invalid for use with CONNECT.</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_connect"/>, <see cref="mosquitto_connect_async"/>, <see cref="mosquitto_connect_bind_async"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_connect_bind_v5(MosquittoPtr mosq, [MarshalAs(UnmanagedType.LPStr)] string host, int port, int keepalive, [MarshalAs(UnmanagedType.LPStr)] string bind_address, MosquittoPropertyPtr properties);

        /// <summary>
        /// <para>
        /// Connect to an MQTT broker. This is a non-blocking call. If you use
        /// <see cref="mosquitto_connect_async"/> your client must use the threaded interface
        /// <see cref="mosquitto_loop_start"/>. If you need to use <see cref="mosquitto_loop"/>, you must use
        /// <see cref="mosquitto_connect"/> to connect the client.
        /// </para>
        /// <para>
        /// May be called before or after <see cref="mosquitto_loop_start"/>.
        /// </para>
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="host">the hostname or ip address of the broker to connect to.</param>
        /// <param name="port">the network port to connect to. Usually 1883.</param>
        /// <param name="keepalive">the number of seconds after which the broker should send a PING message 
        /// to the client if no other messages have been exchanged in that time.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_ERRNO</term><description>if a system call returned an error. The variable errno contains the error code, even on Windows.
        /// Use strerror_r() where available or FormatMessage() on Windows.</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_connect_bind_async"/>, <see cref="mosquitto_connect"/>, <see cref="mosquitto_reconnect"/>, <see cref="mosquitto_disconnect"/>, <see cref="mosquitto_tls_set"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_connect_async(MosquittoPtr mosq, [MarshalAs(UnmanagedType.LPStr)] string host, int port, int keepalive);

        /// <summary>
        /// <para>
        /// Connect to an MQTT broker. This is a non-blocking call. If you use
        /// <see cref="mosquitto_connect_bind_async"/> your client must use the threaded interface
        /// <see cref="mosquitto_loop_start"/>. If you need to use <see cref="mosquitto_loop"/>, you must use
        /// <see cref="mosquitto_connect"/> to connect the client.
        /// </para>
        /// <para>
        /// This extends the functionality of <see cref="mosquitto_connect_async"/> by adding the
        /// bind_address parameter. Use this function if you need to restrict network
        /// communication over a particular interface.
        /// </para>
        /// <para>
        /// May be called before or after <see cref="mosquitto_loop_start"/>.
        /// </para>
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="host">the hostname or ip address of the broker to connect to.</param>
        /// <param name="port">the network port to connect to. Usually 1883.</param>
        /// <param name="keepalive">the number of seconds after which the broker should send a PING message 
        /// to the client if no other messages have been exchanged in that time.</param>
        /// <param name="bind_address">the hostname or ip address of the local network interface to bind to. 
        /// If you do not want to bind to a specific interface, set this to NULL.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid, which could be any of:
        /// <list type="bullet">
        ///     <item><description>mosq == NULL</description></item>
        ///     <item><description>host == NULL</description></item>
        ///     <item><description>port &lt; 0</description></item>
        ///     <item><description>keepalive &lt; 5</description></item>
        /// </list>
        /// </description></item>
        /// <item><term>MOSQ_ERR_ERRNO</term><description>if a system call returned an error. The variable errno contains the error code, even on Windows.
        /// Use strerror_r() where available or FormatMessage() on Windows.</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_connect_async"/>, <see cref="mosquitto_connect"/>, <see cref="mosquitto_connect_bind"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_connect_bind_async(MosquittoPtr mosq, [MarshalAs(UnmanagedType.LPStr)] string host, int port, int keepalive, [MarshalAs(UnmanagedType.LPStr)] string bind_address);

        /// <summary>
        /// <para>
        /// Connect to an MQTT broker.
        /// </para>
        /// <para>
        /// If you set `host` to `example.com`, then this call will attempt to retrieve
        /// the DNS SRV record for `_secure-mqtt._tcp.example.com` or
        /// `_mqtt._tcp.example.com` to discover which actual host to connect to.
        /// </para>
        /// <para>
        /// DNS SRV support is not usually compiled in to libmosquitto, use of this call
        /// is not recommended.
        /// </para>
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="host">the hostname to search for an SRV record.</param>
        /// <param name="keepalive">the number of seconds after which the broker should send a PING message 
        /// to the client if no other messages have been exchanged in that time.</param>
        /// <param name="bind_address">the hostname or ip address of the local network interface to bind to. 
        /// If you do not want to bind to a specific interface, set this to NULL.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid, which could be any of:
        /// <list type="bullet">
        ///     <item><description>mosq == NULL</description></item>
        ///     <item><description>host == NULL</description></item>
        ///     <item><description>port &lt; 0</description></item>
        ///     <item><description>keepalive &lt; 5</description></item>
        /// </list>
        /// </description></item>
        /// <item><term>MOSQ_ERR_ERRNO</term><description>if a system call returned an error. The variable errno contains the error code, even on Windows.
        /// Use strerror_r() where available or FormatMessage() on Windows.</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_connect_async"/>, <see cref="mosquitto_connect"/>, <see cref="mosquitto_connect_bind"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_connect_srv(MosquittoPtr mosq, [MarshalAs(UnmanagedType.LPStr)] string host, int keepalive, [MarshalAs(UnmanagedType.LPStr)] string bind_address);

        /// <summary>
        /// <para>
        /// Reconnect to a broker.
        /// </para>
        /// <para>
        /// This function provides an easy way of reconnecting to a broker after a
        /// connection has been lost. It uses the values that were provided in the
        /// <see cref="mosquitto_connect"/> call. It must not be called before
        /// <see cref="mosquitto_connect"/>.
        /// </para>
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>if an out of memory condition occurred.</description></item>
        /// <item><term>MOSQ_ERR_ERRNO</term><description>if a system call returned an error. The variable errno contains the error code, even on Windows.
        /// Use strerror_r() where available or FormatMessage() on Windows.</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_connect"/>, <see cref="mosquitto_disconnect"/>, <see cref="mosquitto_reconnect_async"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_reconnect(MosquittoPtr mosq);

        /// <summary>
        /// <para>
        /// Reconnect to a broker. Non blocking version of <see cref="mosquitto_reconnect"/>.
        /// </para>
        /// <para>
        /// This function provides an easy way of reconnecting to a broker after a
        /// connection has been lost. It uses the values that were provided in the
        /// <see cref="mosquitto_connect"/> or <see cref="mosquitto_connect_async"/> calls. It must not be
        /// called before <see cref="mosquitto_connect"/>.
        /// </para>
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>if an out of memory condition occurred.</description></item>
        /// <item><term>MOSQ_ERR_ERRNO</term><description>if a system call returned an error. The variable errno contains the error code, even on Windows.
        /// Use strerror_r() where available or FormatMessage() on Windows.</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_connect"/>, <see cref="mosquitto_disconnect"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_reconnect_async(MosquittoPtr mosq);

        /// <summary>
        /// <para>
        /// Disconnect from the broker.
        /// </para>
        /// <para>
        /// It is valid to use this function for clients using all MQTT protocol versions.
        /// If you need to set MQTT v5 DISCONNECT properties, use
        /// <see cref="mosquitto_disconnect_v5"/> instead.
        /// </para>
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_NO_CONN</term><description>if the client isn't connected to a broker.</description></item>
        /// </list></returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_disconnect(MosquittoPtr mosq);

        /// <summary>
        /// <para>
        /// Disconnect from the broker, with attached MQTT properties.
        /// </para>
        /// <para>
        /// Use e.g. <see cref="mosquitto_property_add_string"/> and similar to create a list of
        /// properties, then attach them to this publish. Properties need freeing with
        /// <see cref="mosquitto_property_free_all"/>.
        /// </para>
        /// <para>
        /// If the mosquitto instance `mosq` is using MQTT v5, the `properties` argument
        /// will be applied to the DISCONNECT message. For MQTT v3.1.1 and below, the
        /// `properties` argument will be ignored.
        /// </para>
        /// <para>
        /// Set your client to use MQTT v5 immediately after it is created:
        /// 
        /// <c>mosquitto_int_option(mosq, MOSQ_OPT_PROTOCOL_VERSION, MQTT_PROTOCOL_V5);</c>
        /// </para>
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="reason_code">the disconnect reason code.</param>
        /// <param name="properties">a valid mosquitto_property list, or NULL.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_NO_CONN</term><description>if the client isn't connected to a broker.</description></item>
        /// <item><term>MOSQ_ERR_DUPLICATE_PROPERTY</term><description>if a property is duplicated where it is forbidden.</description></item>
        /// <item><term>MOSQ_ERR_PROTOCOL</term><description>if any property is invalid for use with DISCONNECT.</description></item>
        /// </list></returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_disconnect_v5(MosquittoPtr mosq, int reason_code, MosquittoPropertyPtr properties);
        #endregion

        #region Publishing, subscribing, unsubscribing
        /// <summary>
        /// <para>
        /// Publish a message on a given topic.
        /// </para>
        /// <para>
        /// It is valid to use this function for clients using all MQTT protocol versions.
        /// If you need to set MQTT v5 PUBLISH properties, use <see cref="mosquitto_publish_v5"/>
        /// instead.
        /// </para>
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="mid">pointer to an int. If not NULL, the function will set this to the message id of this particular message. 
        /// This can be then used with the publish callback to determine when the message has been sent. 
        /// Note that although the MQTT protocol doesn't use message ids for messages with QoS=0, 
        /// libmosquitto assigns them message ids so they can be tracked with this parameter.</param>
        /// <param name="topic">null terminated string of the topic to publish to.</param>
        /// <param name="payloadlen">the size of the payload (bytes). Valid values are between 0 and 268,435,455.</param>
        /// <param name="payload">pointer to the data to send. If payloadlen > 0 this must be a valid memory location.</param>
        /// <param name="qos">integer value 0, 1 or 2 indicating the Quality of Service to be used for the message.</param>
        /// <param name="retain">set to true to make the message retained.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>if an out of memory condition occurred.</description></item>
        /// <item><term>MOSQ_ERR_NO_CONN</term><description>if the client isn't connected to a broker.</description></item>
        /// <item><term>MOSQ_ERR_PROTOCOL</term><description>if there is a protocol error communicating with the broker.</description></item>
        /// <item><term>MOSQ_ERR_PAYLOAD_SIZE</term><description>if payloadlen is too large.</description></item>
        /// <item><term>MOSQ_ERR_MALFORMED_UTF8</term><description>if the topic is not valid UTF-8</description></item>
        /// <item><term>MOSQ_ERR_QOS_NOT_SUPPORTED</term><description>if the QoS is greater than that supported by the broker.</description></item>
        /// <item><term>MOSQ_ERR_OVERSIZE_PACKET</term><description>if the resulting packet would be larger than
        /// supported by the broker.</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_max_inflight_messages_set"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_publish(MosquittoPtr mosq, ref int mid, [MarshalAs(UnmanagedType.LPStr)] string topic, int payloadlen, byte[] payload, int qos, bool retain);


        /// <summary>
        /// <para>
		/// Publish a message on a given topic, with attached MQTT properties.
		/// </para>
        /// <para>
		/// Use e.g. <see cref="mosquitto_property_add_string"/> and similar to create a list of
		/// properties, then attach them to this publish. Properties need freeing with
		/// <see cref="mosquitto_property_free_all"/>.
		/// </para>
        /// <para>
		/// If the mosquitto instance `mosq` is using MQTT v5, the `properties` argument
		/// will be applied to the PUBLISH message. For MQTT v3.1.1 and below, the
		/// `properties` argument will be ignored.
		/// </para>
        /// <para>
		/// Set your client to use MQTT v5 immediately after it is created:
		/// 
		/// <c>mosquitto_int_option(mosq, MOSQ_OPT_PROTOCOL_VERSION, MQTT_PROTOCOL_V5);</c>
        /// </para>
        /// </summary>
		/// <param name="mosq">a valid mosquitto instance.</param>
		/// <param name="mid">pointer to an int. If not NULL, the function will set this to the message id of this particular message. 
        /// This can be then used with the publish callback to determine when the message has been sent. 
        /// Note that although the MQTT protocol doesn't use message ids for messages with QoS=0, 
        /// libmosquitto assigns them message ids so they can be tracked with this parameter.</param>
		/// <param name="topic">null terminated string of the topic to publish to.</param>
		/// <param name="payloadlen">the size of the payload (bytes). Valid values are between 0 and 268,435,455.</param>
		/// <param name="payload">pointer to the data to send. If payloadlen > 0 this must be a valid memory location.</param>
		/// <param name="qos">integer value 0, 1 or 2 indicating the Quality of Service to be used for the message.</param>
		/// <param name="retain">set to true to make the message retained.</param>
		/// <param name="properties">a valid mosquitto_property list, or NULL.</param>
		/// <returns><list type="table">
		/// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
		/// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
		/// <item><term>MOSQ_ERR_NOMEM</term><description>if an out of memory condition occurred.</description></item>
		/// <item><term>MOSQ_ERR_NO_CONN</term><description>if the client isn't connected to a broker.</description></item>
		/// <item><term>MOSQ_ERR_PROTOCOL</term><description>if there is a protocol error communicating with the broker.</description></item>
		/// <item><term>MOSQ_ERR_PAYLOAD_SIZE</term><description>if payloadlen is too large.</description></item>
		/// <item><term>MOSQ_ERR_MALFORMED_UTF8</term><description>if the topic is not valid UTF-8</description></item>
		/// <item><term>MOSQ_ERR_DUPLICATE_PROPERTY</term><description>if a property is duplicated where it is forbidden.</description></item>
		/// <item><term>MOSQ_ERR_PROTOCOL</term><description>if any property is invalid for use with PUBLISH.</description></item>
		/// <item><term>MOSQ_ERR_QOS_NOT_SUPPORTED</term><description>if the QoS is greater than that supported by the broker.</description></item>
		/// <item><term>MOSQ_ERR_OVERSIZE_PACKET</term><description>if the resulting packet would be larger than
		/// supported by the broker.</description></item>
		/// </list></returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_publish_v5(
        MosquittoPtr mosq,
        ref int mid,
        [MarshalAs(UnmanagedType.LPStr)] string topic,
        int payloadlen,
        byte[] payload,
        int qos,
        bool retain,
        MosquittoPropertyPtr properties);


        /// <summary>
        /// <para>
        /// Subscribe to a topic.
        /// </para>
        /// <para>
        /// It is valid to use this function for clients using all MQTT protocol versions.
        /// If you need to set MQTT v5 SUBSCRIBE properties, use <see cref="mosquitto_subscribe_v5"/>
        /// instead.
        /// </para>
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="mid">a pointer to an int. If not NULL, the function will set this to the message id of this particular message. 
        /// This can be then used with the subscribe callback to determine when the message has been sent.</param>
        /// <param name="sub">the subscription pattern.</param>
        /// <param name="qos">the requested Quality of Service for this subscription.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>if an out of memory condition occurred.</description></item>
        /// <item><term>MOSQ_ERR_NO_CONN</term><description>if the client isn't connected to a broker.</description></item>
        /// <item><term>MOSQ_ERR_MALFORMED_UTF8</term><description>if the topic is not valid UTF-8</description></item>
        /// <item><term>MOSQ_ERR_OVERSIZE_PACKET</term><description>if the resulting packet would be larger than supported by the broker.</description></item>
        /// </list></returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_subscribe(MosquittoPtr mosq, ref int mid, [MarshalAs(UnmanagedType.LPStr)] string sub, int qos);

        /// <summary>
        /// <para>
        /// Subscribe to a topic, with attached MQTT properties.
        /// </para>
        /// <para>
        /// Use e.g. <see cref="mosquitto_property_add_string"/> and similar to create a list of
        /// properties, then attach them to this publish. Properties need freeing with
        /// <see cref="mosquitto_property_free_all"/>.
        /// </para>
        /// <para>
        /// If the mosquitto instance `mosq` is using MQTT v5, the `properties` argument
        /// will be applied to the PUBLISH message. For MQTT v3.1.1 and below, the
        /// `properties` argument will be ignored.
        /// </para>
        /// <para>
        /// Set your client to use MQTT v5 immediately after it is created:
        /// 
        /// <c>mosquitto_int_option(mosq, MOSQ_OPT_PROTOCOL_VERSION, MQTT_PROTOCOL_V5);</c>
        /// </para>
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="mid">a pointer to an int. If not NULL, the function will set this to the message id of this particular message. 
        /// This can be then used with the subscribe callback to determine when the message has been sent.</param>
        /// <param name="sub">the subscription pattern.</param>
        /// <param name="qos">the requested Quality of Service for this subscription.</param>
        /// <param name="options">options to apply to this subscription, OR'd together. 
        /// Set to 0 to use the default options, otherwise choose from list of <see cref="mqtt5_sub_options"/></param>
        /// <param name="properties">a valid mosquitto_property list, or NULL.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>if an out of memory condition occurred.</description></item>
        /// <item><term>MOSQ_ERR_NO_CONN</term><description>if the client isn't connected to a broker.</description></item>
        /// <item><term>MOSQ_ERR_MALFORMED_UTF8</term><description>if the topic is not valid UTF-8</description></item>
        /// <item><term>MOSQ_ERR_DUPLICATE_PROPERTY</term><description>if a property is duplicated where it is forbidden.</description></item>
        /// <item><term>MOSQ_ERR_PROTOCOL</term><description>if any property is invalid for use with SUBSCRIBE.</description></item>
        /// <item><term>MOSQ_ERR_OVERSIZE_PACKET</term><description>if the resulting packet would be larger than supported by the broker.</description></item>
        /// </list></returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_subscribe_v5(MosquittoPtr mosq, ref int mid, [MarshalAs(UnmanagedType.LPStr)] string sub, int qos, int options, MosquittoPropertyPtr properties);

        /// <summary>
        /// Subscribe to multiple topics.
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="mid">a pointer to an int. If not NULL, the function will set this to the message id of this particular message. 
        /// This can be then used with the subscribe callback to determine when the message has been sent.</param>
        /// <param name="sub_count">the count of subscriptions to be made</param>
        /// <param name="sub">array of sub_count pointers, each pointing to a subscription string. 
        /// The "char *const *const" datatype ensures that neither the array of pointers nor the strings that they point to are mutable. 
        /// If you aren't familiar with this, just think of it as a safer "char **", equivalent to "const char *" for a simple string pointer.</param>
        /// <param name="qos">the requested Quality of Service for each subscription.</param>
        /// <param name="options">options to apply to this subscription, OR'd together. This argument is not used for MQTT v3 susbcriptions. 
        /// Set to 0 to use the default options, otherwise choose from list of <see cref="mqtt5_sub_options"/></param>
        /// <param name="properties">a valid mosquitto_property list, or NULL. Only used with MQTT v5 clients.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>if an out of memory condition occurred.</description></item>
        /// <item><term>MOSQ_ERR_NO_CONN</term><description>if the client isn't connected to a broker.</description></item>
        /// <item><term>MOSQ_ERR_MALFORMED_UTF8</term><description>if a topic is not valid UTF-8</description></item>
        /// <item><term>MOSQ_ERR_OVERSIZE_PACKET</term><description>if the resulting packet would be larger than supported by the broker.</description></item>
        /// </list></returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_subscribe_multiple(MosquittoPtr mosq, ref int mid, int sub_count, [In, Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] sub, int qos, int options, MosquittoPropertyPtr properties);

        /// <summary>
        /// Unsubscribe from a topic.
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="mid">a pointer to an int. If not NULL, the function will set this to the message id of this particular message. 
        /// This can be then used with the unsubscribe callback to determine when the message has been sent.</param>
        /// <param name="sub">the unsubscription pattern.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>if an out of memory condition occurred.</description></item>
        /// <item><term>MOSQ_ERR_NO_CONN</term><description>if the client isn't connected to a broker.</description></item>
        /// <item><term>MOSQ_ERR_MALFORMED_UTF8</term><description>if the topic is not valid UTF-8</description></item>
        /// <item><term>MOSQ_ERR_OVERSIZE_PACKET</term><description>if the resulting packet would be larger than supported by the broker.</description></item>
        /// </list></returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_unsubscribe(MosquittoPtr mosq, ref int mid, [MarshalAs(UnmanagedType.LPStr)] string sub);

        /// <summary>
        /// <para>
        /// Unsubscribe from a topic, with attached MQTT properties.
        /// </para>
        /// <para>
        /// It is valid to use this function for clients using all MQTT protocol versions.
        /// If you need to set MQTT v5 UNSUBSCRIBE properties, use
        /// <see cref="mosquitto_unsubscribe_v5"/> instead.
        /// </para>
        /// <para>
        /// Use e.g. <see cref="mosquitto_property_add_string"/> and similar to create a list of
        /// properties, then attach them to this publish. Properties need freeing with
        /// <see cref="mosquitto_property_free_all"/>.
        /// </para>
        /// <para>
        /// If the mosquitto instance `mosq` is using MQTT v5, the `properties` argument
        /// will be applied to the PUBLISH message. For MQTT v3.1.1 and below, the
        /// `properties` argument will be ignored.
        /// </para>
        /// <para>
        /// Set your client to use MQTT v5 immediately after it is created:
        /// 
        /// <c>mosquitto_int_option(mosq, MOSQ_OPT_PROTOCOL_VERSION, MQTT_PROTOCOL_V5);</c>
        /// </para>
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="mid">a pointer to an int. If not NULL, the function will set this to the message id of this particular message. 
        /// This can be then used with the unsubscribe callback to determine when the message has been sent.</param>
        /// <param name="sub">the unsubscription pattern.</param>
        /// <param name="properties">a valid mosquitto_property list, or NULL. Only used with MQTT v5 clients.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>if an out of memory condition occurred.</description></item>
        /// <item><term>MOSQ_ERR_NO_CONN</term><description>if the client isn't connected to a broker.</description></item>
        /// <item><term>MOSQ_ERR_MALFORMED_UTF8</term><description>if the topic is not valid UTF-8</description></item>
        /// <item><term>MOSQ_ERR_DUPLICATE_PROPERTY</term><description>if a property is duplicated where it is forbidden.</description></item>
        /// <item><term>MOSQ_ERR_PROTOCOL</term><description>if any property is invalid for use with UNSUBSCRIBE.</description></item>
        /// <item><term>MOSQ_ERR_OVERSIZE_PACKET</term><description>if the resulting packet would be larger than supported by the broker.</description></item>
        /// </list></returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_unsubscribe_v5(MosquittoPtr mosq, ref int mid, [MarshalAs(UnmanagedType.LPStr)] string sub, MosquittoPropertyPtr properties);

        /// <summary>
        /// Unsubscribe from multiple topics.
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="mid">a pointer to an int. If not NULL, the function will set this to the message id of this particular message. 
        /// This can be then used with the subscribe callback to determine when the message has been sent.</param>
        /// <param name="sub_count">the count of unsubscriptions to be made</param>
        /// <param name="sub">array of sub_count pointers, each pointing to an unsubscription string. 
        /// The "char *const *const" datatype ensures that neither the array of pointers nor the strings that they point to are mutable. 
        /// If you aren't familiar with this, just think of it as a safer "char **", equivalent to "const char *" for a simple string pointer.</param>
        /// <param name="properties">a valid mosquitto_property list, or NULL. Only used with MQTT v5 clients.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>if an out of memory condition occurred.</description></item>
        /// <item><term>MOSQ_ERR_NO_CONN</term><description>if the client isn't connected to a broker.</description></item>
        /// <item><term>MOSQ_ERR_MALFORMED_UTF8</term><description>if a topic is not valid UTF-8</description></item>
        /// <item><term>MOSQ_ERR_OVERSIZE_PACKET</term><description>if the resulting packet would be larger than supported by the broker.</description></item>
        /// </list></returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_unsubscribe_multiple(MosquittoPtr mosq, ref int mid, int sub_count, [In, Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] sub, MosquittoPropertyPtr properties);
        #endregion

        #region Struct mosquitto_message helper functions
        /// <summary>
        /// Copy the contents of a mosquitto message to another message.
        /// Useful for preserving a message received in the on_message() callback.
        /// </summary>
        /// <param name="dst">a pointer to a valid mosquitto_message struct to copy to.</param>
        /// <param name="src">a pointer to a valid mosquitto_message struct to copy from.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>if an out of memory condition occurred.</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_message_free"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_message_copy(ref mosquitto_message dst, ref mosquitto_message src);

        /// <summary>
        /// Completely free a mosquitto_message struct.
        /// </summary>
        /// <param name="message">pointer to a mosquitto_message pointer to free.</param>
        /// <remarks>
        /// See Also: <see cref="mosquitto_message_copy"/>, <see cref="mosquitto_message_free_contents"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mosquitto_message_free(ref MosquittoMessagePtr message);

        /// <summary>
        /// Free a mosquitto_message struct contents, leaving the struct unaffected.
        /// </summary>
        /// <param name="message">pointer to a mosquitto_message struct to free its contents.</param>
        /// <remarks>
        /// See Also: <see cref="mosquitto_message_copy"/>, <see cref="mosquitto_message_free"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mosquitto_message_free_contents(ref mosquitto_message message);
        #endregion

        #region Network loop (managed by libmosquitto)
        // The internal network loop must be called at a regular interval. The two
        // recommended approaches are to use either <mosquitto_loop_forever> or
        // <mosquitto_loop_start>. <mosquitto_loop_forever> is a blocking call and is
        // suitable for the situation where you only want to handle incoming messages
        // in callbacks. <mosquitto_loop_start> is a non-blocking call, it creates a
        // separate thread to run the loop for you. Use this function when you have
        // other tasks you need to run at the same time as the MQTT client, e.g.
        // reading data from a sensor.

        /// <summary>
        /// <para>
        /// This function call loop() for you in an infinite blocking loop. It is useful
        /// for the case where you only want to run the MQTT client loop in your
        /// program.
        /// </para>
        /// <para>
        /// It handles reconnecting in case server connection is lost. If you call
        /// mosquitto_disconnect() in a callback it will return.
        /// </para>
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="timeout">Maximum number of milliseconds to wait for network activity in the select() call before timing out. 
        /// Set to 0 for instant return.  Set negative to use the default of 1000ms.</param>
        /// <param name="max_packets">this parameter is currently unused and should be set to 1 for future compatibility.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>if an out of memory condition occurred.</description></item>
        /// <item><term>MOSQ_ERR_NO_CONN</term><description>if the client isn't connected to a broker.</description></item>
        /// <item><term>MOSQ_ERR_CONN_LOST</term><description>if the connection to the broker was lost.</description></item>
        /// <item><term>MOSQ_ERR_PROTOCOL</term><description>if there is a protocol error communicating with the broker.</description></item>
        /// <item><term>MOSQ_ERR_ERRNO</term><description>if a system call returned an error. The variable errno contains the error code, even on Windows. 
        /// Use strerror_r() where available or FormatMessage() on Windows.</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_loop"/>, <see cref="mosquitto_loop_start"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_loop_forever(MosquittoPtr mosq, int timeout, int max_packets);

        /// <summary>
        /// This is part of the threaded client interface. Call this once to start a new
        /// thread to process network traffic. This provides an alternative to
        /// repeatedly calling <see cref="mosquitto_loop"/> yourself.
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_NOT_SUPPORTED</term><description>if thread support is not available.</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_connect_async"/>, <see cref="mosquitto_loop"/>, <see cref="mosquitto_loop_forever"/>, <see cref="mosquitto_loop_stop"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_loop_start(MosquittoPtr mosq);

        /// <summary>
        /// This is part of the threaded client interface. Call this once to stop the
        /// network thread previously created with <see cref="mosquitto_loop_start"/>. This call
        /// will block until the network thread finishes. For the network thread to end,
        /// you must have previously called <see cref="mosquitto_disconnect"/> or have set the force
        /// parameter to true.
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="force">set to true to force thread cancellation. If false, <see cref="mosquitto_disconnect"/> must have already been called.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_NOT_SUPPORTED</term><description>if thread support is not available.</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_loop"/>, <see cref="mosquitto_loop_start"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_loop_stop(MosquittoPtr mosq, bool force);

        /// <summary>
        /// <para>
        /// The main network loop for the client. This must be called frequently
        /// to keep communications between the client and broker working. This is
        /// carried out by <see cref="mosquitto_loop_forever"/> and <see cref="mosquitto_loop_start"/>, which
        /// are the recommended ways of handling the network loop. You may also use this
        /// function if you wish. It must not be called inside a callback.
        /// </para>
        /// <para>
        /// If incoming data is present it will then be processed. Outgoing commands,
        /// from e.g.  <see cref="mosquitto_publish"/>, are normally sent immediately that their
        /// function is called, but this is not always possible. <see cref="mosquitto_loop"/> will
        /// also attempt to send any remaining outgoing messages, which also includes
        /// commands that are part of the flow for messages with QoS>0.
        /// </para>
        /// <para>
        /// This calls select() to monitor the client network socket. If you want to
        /// integrate mosquitto client operation with your own select() call, use
        /// <see cref="mosquitto_socket"/>, <see cref="mosquitto_loop_read"/>, <see cref="mosquitto_loop_write"/> and
        /// <see cref="mosquitto_loop_misc"/>.
        /// </para>
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="timeout">Maximum number of milliseconds to wait for network activity in the select() call before timing out. 
        /// Set to 0 for instant return.  Set negative to use the default of 1000ms.</param>
        /// <param name="max_packets">this parameter is currently unused and should be set to 1 for future compatibility.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>if an out of memory condition occurred.</description></item>
        /// <item><term>MOSQ_ERR_NO_CONN</term><description>if the client isn't connected to a broker.</description></item>
        /// <item><term>MOSQ_ERR_CONN_LOST</term><description>if the connection to the broker was lost.</description></item>
        /// <item><term>MOSQ_ERR_PROTOCOL</term><description>if there is a protocol error communicating with the broker.</description></item>
        /// <item><term>MOSQ_ERR_ERRNO</term><description>if a system call returned an error. The variable errno contains the error code, even on Windows. 
        /// Use strerror_r() where available or FormatMessage() on Windows.</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_loop_forever"/>, <see cref="mosquitto_loop_start"/>, <see cref="mosquitto_loop_stop"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_loop(MosquittoPtr mosq, int timeout, int max_packets);
        #endregion

        #region Network loop (for use in other event loops)
        /// <summary>
        /// Carry out network read operations.
        /// This should only be used if you are not using mosquitto_loop() and are
        /// monitoring the client network socket for activity yourself.
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="max_packets">this parameter is currently unused and should be set to 1 for future compatibility.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>if an out of memory condition occurred.</description></item>
        /// <item><term>MOSQ_ERR_NO_CONN</term><description>if the client isn't connected to a broker.</description></item>
        /// <item><term>MOSQ_ERR_CONN_LOST</term><description>if the connection to the broker was lost.</description></item>
        /// <item><term>MOSQ_ERR_PROTOCOL</term><description>if there is a protocol error communicating with the broker.</description></item>
        /// <item><term>MOSQ_ERR_ERRNO</term><description>if a system call returned an error. The variable errno contains the error code, even on Windows. 
        /// Use strerror_r() where available or FormatMessage() on Windows.</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_socket"/>, <see cref="mosquitto_loop_write"/>, <see cref="mosquitto_loop_misc"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_loop_read(MosquittoPtr mosq, int max_packets);

        /// <summary>
        /// Carry out network write operations.
        /// This should only be used if you are not using mosquitto_loop() and are
        /// monitoring the client network socket for activity yourself.
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="max_packets">this parameter is currently unused and should be set to 1 for future compatibility.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>if an out of memory condition occurred.</description></item>
        /// <item><term>MOSQ_ERR_NO_CONN</term><description>if the client isn't connected to a broker.</description></item>
        /// <item><term>MOSQ_ERR_CONN_LOST</term><description>if the connection to the broker was lost.</description></item>
        /// <item><term>MOSQ_ERR_PROTOCOL</term><description>if there is a protocol error communicating with the broker.</description></item>
        /// <item><term>MOSQ_ERR_ERRNO</term><description>if a system call returned an error. The variable errno contains the error code, even on Windows. 
        /// Use strerror_r() where available or FormatMessage() on Windows.</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_socket"/>, <see cref="mosquitto_loop_read"/>, <see cref="mosquitto_loop_misc"/>, <see cref="mosquitto_want_write"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_loop_write(MosquittoPtr mosq, int max_packets);

        /// <summary>
        /// <para>
        /// Carry out miscellaneous operations required as part of the network loop.
        /// This should only be used if you are not using mosquitto_loop() and are
        /// monitoring the client network socket for activity yourself.
        /// </para>
        /// <para>
        /// This function deals with handling PINGs and checking whether messages need
        /// to be retried, so should be called fairly frequently, around once per second
        /// is sufficient.
        /// </para>
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_NO_CONN</term><description>if the client isn't connected to a broker.</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_socket"/>, <see cref="mosquitto_loop_read"/>, <see cref="mosquitto_loop_write"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_loop_misc(MosquittoPtr mosq);
        #endregion

        #region Network loop (helper functions)
        /// <summary>
        /// Return the socket handle for a mosquitto instance. Useful if you want to
        /// include a mosquitto client in your own select() calls.
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <returns>
        /// 	The socket for the mosquitto client or -1 on failure.
        /// </returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_socket(MosquittoPtr mosq);

        /// <summary>
        /// Returns true if there is data ready to be written on the socket.
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <remarks>
        /// See Also: <see cref="mosquitto_socket"/>, <see cref="mosquitto_loop_read"/>, <see cref="mosquitto_loop_write"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool mosquitto_want_write(MosquittoPtr mosq);

        /// <summary>
        /// <para>
        /// Used to tell the library that your application is using threads, but not
        /// using <see cref="mosquitto_loop_start"/>. The library operates slightly differently when
        /// not in threaded mode in order to simplify its operation. If you are managing
        /// your own threads and do not use this function you will experience crashes
        /// due to race conditions.
        /// </para>
        /// <para>
        /// When using <see cref="mosquitto_loop_start"/>, this is set automatically.
        /// </para>
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="threaded">true if your application is using threads, false otherwise.</param>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_threaded_set(MosquittoPtr mosq, bool threaded);
        #endregion

        #region Client options
        /// <summary>
        /// <para>
        /// Used to set options for the client.
        /// </para>
        /// <para>
        /// This function is deprecated, the replacement <see cref="mosquitto_int_option"/>,
        /// <see cref="mosquitto_string_option"/> and <see cref="mosquitto_void_option"/> functions should
        /// be used instead.
        /// </para>
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="option">the option to set.</param>
        /// <param name="value">the option specific value.</param>
        /// <remarks>
        /// Options: 
        /// <list type="table">
        /// <item>
        /// <term>MOSQ_OPT_PROTOCOL_VERSION</term>
        /// <description>Value must be an int, set to either MQTT_PROTOCOL_V31 or MQTT_PROTOCOL_V311. 
        /// Must be set before the client connects. Defaults to MQTT_PROTOCOL_V31.</description>
        /// </item>
        /// <item>
        /// <term>MOSQ_OPT_SSL_CTX</term>
        /// <description>Pass an openssl SSL_CTX to be used when creating
        /// TLS connections rather than libmosquitto creating its own.
        /// This must be called before connecting to have any effect.
        /// If you use this option, the onus is on you to ensure that
        /// you are using secure settings.
        /// Setting to NULL means that libmosquitto will use its own SSL_CTX
        /// if TLS is to be used.
        /// This option is only available for openssl 1.1.0 and higher.</description>
        /// </item>
        /// <item>
        /// <term>MOSQ_OPT_SSL_CTX_WITH_DEFAULTS</term>
        /// <description>Value must be an int set to 1 or 0.
        /// If set to 1, then the user specified SSL_CTX passed in using
        /// MOSQ_OPT_SSL_CTX will have the default options applied to it.
        /// This means that you only need to change the values that are
        /// relevant to you. If you use this option then you must configure
        /// the TLS options as normal, i.e. you should use
        /// <see cref="mosquitto_tls_set"/> to configure the cafile/capath as a minimum.
        /// This option is only available for openssl 1.1.0 and higher.</description>
        /// </item>
        /// </list>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_opts_set(MosquittoPtr mosq, mosq_opt_t option, IntPtr value);

        /// <summary>
        /// Used to set integer options for the client.
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="option">the option to set.</param>
        /// <param name="value">the option specific value.</param>
        /// <remarks>
        /// Options: 
        /// <list type="table">
        /// <item>
        /// <term>MOSQ_OPT_TCP_NODELAY</term>
        /// <description>Set to 1 to disable Nagle's algorithm on client
        /// sockets. This has the effect of reducing latency of individual
        /// messages at the potential cost of increasing the number of
        /// packets being sent.
        /// Defaults to 0, which means Nagle remains enabled.</description>
        /// </item>
        /// <item>
        /// <term>MOSQ_OPT_PROTOCOL_VERSION</term>
        /// <description>Value must be set to either MQTT_PROTOCOL_V31,
        /// MQTT_PROTOCOL_V311, or MQTT_PROTOCOL_V5. Must be set before the
        /// client connects.  Defaults to MQTT_PROTOCOL_V311.</description>
        /// </item>
        /// <item>
        /// <term>MOSQ_OPT_RECEIVE_MAXIMUM</term>
        /// <description>Value can be set between 1 and 65535 inclusive,
        /// and represents the maximum number of incoming QoS 1 and QoS 2
        /// messages that this client wants to process at once. Defaults to
        /// 20. This option is not valid for MQTT v3.1 or v3.1.1 clients.
        /// Note that if the MQTT_PROP_RECEIVE_MAXIMUM property is in the
        /// proplist passed to mosquitto_connect_v5(), then that property
        /// will override this option. Using this option is the recommended
        /// method however.</description>
        /// </item>
        /// <item>
        /// <term>MOSQ_OPT_SEND_MAXIMUM</term>
        /// <description>Value can be set between 1 and 65535 inclusive,
        /// and represents the maximum number of outgoing QoS 1 and QoS 2
        /// messages that this client will attempt to have "in flight" at
        /// once. Defaults to 20.
        /// This option is not valid for MQTT v3.1 or v3.1.1 clients.
        /// Note that if the broker being connected to sends a
        /// MQTT_PROP_RECEIVE_MAXIMUM property that has a lower value than
        /// this option, then the broker provided value will be used.</description>
        /// </item>
        /// <item>
        /// <term>MOSQ_OPT_SSL_CTX_WITH_DEFAULTS</term>
        /// <description>If value is set to a non zero value,
        /// then the user specified SSL_CTX passed in using MOSQ_OPT_SSL_CTX
        /// will have the default options applied to it. This means that
        /// you only need to change the values that are relevant to you.
        /// If you use this option then you must configure the TLS options
        /// as normal, i.e.  you should use <see cref="mosquitto_tls_set"/> to
        /// configure the cafile/capath as a minimum.
        /// This option is only available for openssl 1.1.0 and higher.</description>
        /// </item>
        /// <item>
        /// <term>MOSQ_OPT_TLS_OCSP_REQUIRED</term>
        /// <description>Set whether OCSP checking on TLS
        /// connections is required. Set to 1 to enable checking,
        /// or 0 (the default) for no checking.</description>
        /// </item>
        /// <item>
        /// <term>MOSQ_OPT_TLS_USE_OS_CERTS</term>
        /// <description>Set to 1 to instruct the client to load and
        /// trust OS provided CA certificates for use with TLS connections.
        /// Set to 0 (the default) to only use manually specified CA certs.</description>
        /// </item>
        /// </list>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_int_option(MosquittoPtr mosq, mosq_opt_t option, int value);


        /// <summary>
        /// Used to set string options for the client.
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="option">the option to set.</param>
        /// <param name="value">the option specific value.</param>
        /// <remarks>
        /// Options: 
        /// <list type="table">
        /// <item>
        /// <term>MOSQ_OPT_TLS_ENGINE</term>
        /// <description>Configure the client for TLS Engine support.
        /// Pass a TLS Engine ID to be used when creating TLS
        /// connections. Must be set before <see cref="mosquitto_connect"/>.
        /// Must be a valid engine, and note that the string will not be used
        /// until a connection attempt is made so this function will return
        /// success even if an invalid engine string is passed.</description>
        /// </item>
        /// <item>
        /// <term>MOSQ_OPT_TLS_KEYFORM</term>
        /// <description>Configure the client to treat the keyfile
        /// differently depending on its type.  Must be set
        /// before <see cref="mosquitto_connect"/>.
        /// Set as either "pem" or "engine", to determine from where the
        /// private key for a TLS connection will be obtained. Defaults to
        /// "pem", a normal private key file.</description>
        /// </item>
        /// <item>
        /// <term>MOSQ_OPT_TLS_KPASS_SHA1</term>
        /// <description>Where the TLS Engine requires the use of
        /// a password to be accessed, this option allows a hex encoded
        /// SHA1 hash of the private key password to be passed to the
        /// engine directly. Must be set before <see cref="mosquitto_connect"/>.</description>
        /// </item>
        /// <item>
        /// <term>MOSQ_OPT_TLS_ALPN</term>
        /// <description>If the broker being connected to has multiple
        /// services available on a single TLS port, such as both MQTT
        /// and WebSockets, use this option to configure the ALPN
        /// option for the connection.</description>
        /// </item>
        /// <item>
        /// <term>MOSQ_OPT_BIND_ADDRESS</term>
        /// <description>Set the hostname or ip address of the local network
        /// interface to bind to when connecting.</description>
        /// </item>
        /// </list>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_string_option(MosquittoPtr mosq, mosq_opt_t option, [MarshalAs(UnmanagedType.LPStr)] string value);


        /// <summary>
        /// Used to set void* options for the client.
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="option">the option to set.</param>
        /// <param name="value">the option specific value.</param>
        /// <remarks>
        /// Options: 
        /// <list type="table">
        /// <item>
        /// <term>MOSQ_OPT_SSL_CTX</term>
        /// <description>Pass an openssl SSL_CTX to be used when creating TLS
        /// connections rather than libmosquitto creating its own.  This must
        /// be called before connecting to have any effect. If you use this
        /// option, the onus is on you to ensure that you are using secure settings.
        /// Setting to NULL means that libmosquitto will use its own SSL_CTX if TLS is to be used.
        /// This option is only available for openssl 1.1.0 and higher.</description>
        /// </item>
        /// </list>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_void_option(MosquittoPtr mosq, mosq_opt_t option, IntPtr value);

        /// <summary>
        /// <para>
        /// Control the behaviour of the client when it has unexpectedly disconnected in
        /// <see cref="mosquitto_loop_forever"/> or after <see cref="mosquitto_loop_start"/>. The default
        /// behaviour if this function is not used is to repeatedly attempt to reconnect
        /// with a delay of 1 second until the connection succeeds.
        /// </para>
        /// <para>
        /// Use reconnect_delay parameter to change the delay between successive
        /// reconnection attempts. You may also enable exponential backoff of the time
        /// between reconnections by setting reconnect_exponential_backoff to true and
        /// set an upper bound on the delay with reconnect_delay_max.
        /// </para>
        /// <para>
        /// Example 1:
        /// <code>
        /// 	delay=2, delay_max=10, exponential_backoff=False
        /// 	Delays would be: 2, 4, 6, 8, 10, 10, ...
        /// </code>
        /// </para>
        /// <para>
        /// Example 2:
        /// <code>
        /// 	delay=3, delay_max=30, exponential_backoff=True
        /// 	Delays would be: 3, 6, 12, 24, 30, 30, ...
        /// </code>
        /// </para>
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="reconnect_delay">the number of seconds to wait between reconnects.</param>
        /// <param name="reconnect_delay_max">the maximum number of seconds to wait between reconnects.</param>
        /// <param name="reconnect_exponential_backoff">use exponential backoff between reconnect attempts. Set to true to enable exponential backoff.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// </list></returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_reconnect_delay_set(MosquittoPtr mosq, uint reconnect_delay, uint reconnect_delay_max, bool reconnect_exponential_backoff);

        /// <summary>
        /// <para>
        /// This function is deprected. Use the <see cref="mosquitto_int_option"/> function with the
        /// MOSQ_OPT_SEND_MAXIMUM option instead.
        /// </para>
        /// <para>
        /// Set the number of QoS 1 and 2 messages that can be "in flight" at one time.
        /// An in flight message is part way through its delivery flow. Attempts to send
        /// further messages with <see cref="mosquitto_publish"/> will result in the messages being
        /// queued until the number of in flight messages reduces.
        /// </para>
        /// <para>
        /// A higher number here results in greater message throughput, but if set
        /// higher than the maximum in flight messages on the broker may lead to
        /// delays in the messages being acknowledged.
        /// </para>
        /// <para>
        /// Set to 0 for no maximum.
        /// </para>
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="max_inflight_messages">the maximum number of inflight messages. Defaults to 20.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// </list></returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_max_inflight_messages_set(MosquittoPtr mosq, uint max_inflight_messages);

        /// <summary>
        /// This function now has no effect.
        /// </summary>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mosquitto_message_retry_set(MosquittoPtr mosq, uint message_retry);

        /// <summary>
        /// When <see cref="mosquitto_new"/> is called, the pointer given as the "obj" parameter
        /// will be passed to the callbacks as user data. The <see cref="mosquitto_user_data_set"/>
        /// function allows this obj parameter to be updated at any time. This function
        /// will not modify the memory pointed to by the current user data pointer. If
        /// it is dynamically allocated memory you must free it yourself.
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="obj">A user pointer that will be passed as an argument to any callbacks that are specified.</param>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mosquitto_user_data_set(MosquittoPtr mosq, IntPtr obj);

        /// <summary>
        /// Retrieve the "userdata" variable for a mosquitto client.
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <returns>
        /// 	A pointer to the userdata member variable.
        /// </returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr mosquitto_userdata(MosquittoPtr mosq);
        #endregion

        #region TLS support
        /// <summary>
        /// <para>
        /// Configure the client for certificate based SSL/TLS support. Must be called
        /// before <see cref="mosquitto_connect"/>.
        /// Cannot be used in conjunction with <see cref="mosquitto_tls_psk_set"/>.
        /// </para>
        /// <para>
        /// Define the Certificate Authority certificates to be trusted (ie. the server
        /// certificate must be signed with one of these certificates) using cafile.
        /// </para>
        /// <para>
        /// If the server you are connecting to requires clients to provide a
        /// certificate, define certfile and keyfile with your client certificate and
        /// private key. If your private key is encrypted, provide a password callback
        /// function or you will have to enter the password at the command line.
        /// </para>
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="cafile">path to a file containing the PEM encoded trusted CA certificate files. 
        /// Either cafile or capath must not be NULL.</param>
        /// <param name="capath">path to a directory containing the PEM encoded trusted CA certificate files. 
        /// See mosquitto.conf for more details on configuring this directory. Either cafile or capath must not be NULL.</param>
        /// <param name="certfile">path to a file containing the PEM encoded certificate file for this client. 
        /// If NULL, keyfile must also be NULL and no client certificate will be used.</param>
        /// <param name="keyfile">path to a file containing the PEM encoded private key for this client. 
        /// If NULL, certfile must also be NULL and no client certificate will be used.</param>
        /// <param name="pw_callback">if keyfile is encrypted, set pw_callback to allow your client to pass the correct password for decryption. 
        /// If set to NULL, the password must be entered on the command line.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>if an out of memory condition occurred.</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_tls_opts_set"/>, <see cref="mosquitto_tls_psk_set"/>,
        /// 	<see cref="mosquitto_tls_insecure_set"/>, <see cref="mosquitto_userdata"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_tls_set(MosquittoPtr mosq,
            [MarshalAs(UnmanagedType.LPStr)] string cafile, [MarshalAs(UnmanagedType.LPStr)] string capath,
            [MarshalAs(UnmanagedType.LPStr)] string certfile, [MarshalAs(UnmanagedType.LPStr)] string keyfile,
            [MarshalAs(UnmanagedType.FunctionPtr)] pw_callback pw_callback);
        /// <summary>
        /// Your callback must write the password into "buf", which is "size" bytes long. 
        /// The return value must be the length of the password. 
        /// "mosq" will be set to the calling mosquitto instance. 
        /// The mosquitto userdata member variable can be retrieved using <see cref="mosquitto_userdata"/>.
        /// </summary>
        /// <param name="buf">Buffer into which the password should be written</param>
        /// <param name="size">size of buf</param>
        /// <param name="rwflag"></param>
        /// <param name="mosq"></param>
        /// <returns></returns>
        internal delegate int pw_callback(IntPtr buf, int size, int rwflag, MosquittoPtr mosq);

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
        /// Must be called before <see cref="mosquitto_connect"/>.
        /// </para>
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="value">if set to false, the default, certificate hostname checking is performed. If set to true, no hostname checking is performed and the connection is insecure.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_tls_set"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_tls_insecure_set(MosquittoPtr mosq, bool value);

        /// <summary>
        /// Set advanced SSL/TLS options. Must be called before <see cref="mosquitto_connect"/>.
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="cert_reqs">an integer defining the verification requirements the client will impose on the server. This can be one of:
        /// <list type="bullet">
        /// <item><term>SSL_VERIFY_NONE (0)</term><description>the server will not be verified in any way.</description></item>
        /// <item><term>SSL_VERIFY_PEER (1)</term><description>the server certificate will be verified 
        /// and the connection aborted if the verification fails.</description></item>
        /// </list>
        /// The default and recommended value is SSL_VERIFY_PEER. Using SSL_VERIFY_NONE provides no security.</param>
        /// <param name="tls_version">the version of the SSL/TLS protocol to use as a string. 
        /// If NULL, the default value is used. 
        /// The default value and the available values depend on the version of openssl that the library was compiled against. 
        /// For openssl >= 1.0.1, the available options are tlsv1.2, tlsv1.1 and tlsv1, with tlv1.2 as the default. 
        /// For openssl < 1.0.1, only tlsv1 is available.</param>
        /// <param name="ciphers">a string describing the ciphers available for use. 
        /// See the "openssl ciphers" tool for more information. 
        /// If NULL, the default ciphers will be used.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>if an out of memory condition occurred.</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_tls_set"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_tls_opts_set(MosquittoPtr mosq, int cert_reqs, [MarshalAs(UnmanagedType.LPStr)] string tls_version, [MarshalAs(UnmanagedType.LPStr)] string ciphers);

        /// <summary>
        /// <para>
        /// Configure the client for pre-shared-key based TLS support. Must be called
        /// before <see cref="mosquitto_connect"/>.
        /// </para>
        /// <para>
        /// Cannot be used in conjunction with <see cref="mosquitto_tls_set"/>.
        /// </para>
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="psk">the pre-shared-key in hex format with no leading "0x".</param>
        /// <param name="identity">the identity of this client. May be used as the username depending on the server settings.</param>
        /// <param name="ciphers">a string describing the PSK ciphers available for use. 
        /// See the "openssl ciphers" tool for more information. If NULL, the default ciphers will be used.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success.</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>if an out of memory condition occurred.</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_tls_set"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_tls_psk_set(MosquittoPtr mosq, [MarshalAs(UnmanagedType.LPStr)] string psk, [MarshalAs(UnmanagedType.LPStr)] string identity, [MarshalAs(UnmanagedType.LPStr)] string ciphers);


        /// <summary>
        /// Retrieve a pointer to the SSL structure used for TLS connections in this
        /// client. This can be used in e.g. the connect callback to carry out
        /// additional verification steps.
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance</param>
        /// <returns><list type="table">
        /// <item><term>A valid pointer to an openssl SSL structure</term><description>if the client is using TLS.</description></item>
        /// <item><term>NULL</term><description>if the client is not using TLS, or TLS support is not compiled in.</description></item>
        /// </list></returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr mosquitto_ssl_get(MosquittoPtr mosq);
        #endregion

        #region Callbacks
        /// <summary>
        /// Set the connect callback. This is called when the library receives a CONNACK
        /// message in response to a connection.
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="on_connect">pointer to a callback function of type <see cref="on_connect"/></param>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mosquitto_connect_callback_set(MosquittoPtr mosq, [MarshalAs(UnmanagedType.FunctionPtr)] on_connect on_connect);
        /// <summary>
        /// Callback for <see cref="mosquitto_connect_callback_set"/>
        /// </summary>
        /// <param name="mosq">the mosquitto instance making the callback.</param>
        /// <param name="obj">the user data provided in <see cref="mosquitto_new"/></param>
        /// <param name="rc">the return code of the connection response. 
        /// The values are defined by the MQTT protocol version in use. 
        /// For MQTT v5.0, look at section 3.2.2.2 Connect Reason code: https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html 
        /// For MQTT v3.1.1, look at section 3.2.2.3 Connect Return code: http://docs.oasis-open.org/mqtt/mqtt/v3.1.1/mqtt-v3.1.1.html
        /// </param>
        internal delegate void on_connect(MosquittoPtr posq, IntPtr obj, int rc);

        /// <summary>
        /// Set the connect callback. This is called when the library receives a CONNACK
        /// message in response to a connection.
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="on_connect">pointer to a callback function of type <see cref="on_connect_with_flags"/></param>
        /// </param>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mosquitto_connect_with_flags_callback_set(MosquittoPtr mosq, [MarshalAs(UnmanagedType.FunctionPtr)] on_connect_with_flags on_connect);
        /// <summary>
        /// Callback for <see cref="mosquitto_connect_with_flags_callback_set"/>
        /// </summary>
        /// <param name="mosq">the mosquitto instance making the callback.</param>
        /// <param name="obj">the user data provided in <see cref="mosquitto_new"/></param>
        /// <param name="rc">the return code of the connection response. 
        /// The values are defined by the MQTT protocol version in use. 
        /// For MQTT v5.0, look at section 3.2.2.2 Connect Reason code: https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html 
        /// For MQTT v3.1.1, look at section 3.2.2.3 Connect Return code: http://docs.oasis-open.org/mqtt/mqtt/v3.1.1/mqtt-v3.1.1.html
        /// </param>
        /// <param name="flags">the connect flags.</param>
        internal delegate void on_connect_with_flags(MosquittoPtr mosq, IntPtr obj, int rc, int flags);

        /// <summary>
        /// <para>
        /// Set the connect callback. This is called when the library receives a CONNACK
        /// message in response to a connection.
        /// </para>
        /// <para>
        /// It is valid to set this callback for all MQTT protocol versions. If it is
        /// used with MQTT clients that use MQTT v3.1.1 or earlier, then the `props`
        /// argument will always be NULL.
        /// </para>
        /// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="on_connect">pointer to a callback function of type <see cref="on_connect_v5"/></param>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mosquitto_connect_v5_callback_set(MosquittoPtr mosq, [MarshalAs(UnmanagedType.FunctionPtr)] on_connect_v5 on_connect);
        /// <summary>
        /// Callback for <see cref="mosquitto_connect_v5_callback_set"/>
        /// </summary>
        /// <param name="mosq">the mosquitto instance making the callback.</param>
        /// <param name="obj">the user data provided in <see cref="mosquitto_new"/></param>
        /// <param name="rc">the return code of the connection response. 
        /// The values are defined by the MQTT protocol version in use. 
        /// For MQTT v5.0, look at section 3.2.2.2 Connect Reason code: https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html 
        /// For MQTT v3.1.1, look at section 3.2.2.3 Connect Return code: http://docs.oasis-open.org/mqtt/mqtt/v3.1.1/mqtt-v3.1.1.html
        /// </param>
        /// <param name="flags">the connect flags.</param>
        /// <param name="props">list of MQTT 5 properties, or NULL</param>
        internal delegate void on_connect_v5(MosquittoPtr mosq, IntPtr obj, int rc, int flags, MosquittoPropertyPtr props);

        /// <summary>
		/// Set the disconnect callback. This is called when the broker has received the
		/// DISCONNECT command and has disconnected the client.
		/// </summary>
		/// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="on_disconnect">pointer to a callback function of type <see cref="on_disconnect"/></param>
        /// </param>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mosquitto_disconnect_callback_set(MosquittoPtr mosq, [MarshalAs(UnmanagedType.FunctionPtr)] on_disconnect on_disconnect);
        /// <summary>
        /// Callback for <see cref="mosquitto_disconnect_callback_set"/>
        /// </summary>
		/// <param name="mosq">the mosquitto instance making the callback.</param>
		/// <param name="obj">the user data provided in <see cref="mosquitto_new"/></param>
		/// <param name="rc">integer value indicating the reason for the disconnect. 
        /// A value of 0 means the client has called <see cref="mosquitto_disconnect"/>. 
        /// Any other value indicates that the disconnect is unexpected.</param>
        internal delegate void on_disconnect(MosquittoPtr mosq, IntPtr obj, int rc);

        /// <summary>
        /// <para>
        /// Set the disconnect callback. This is called when the broker has received the
        /// DISCONNECT command and has disconnected the client.
        /// </para>
        /// <para>
        /// It is valid to set this callback for all MQTT protocol versions. If it is
        /// used with MQTT clients that use MQTT v3.1.1 or earlier, then the `props`
        /// argument will always be NULL.
        /// </para>
		/// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="on_disconnect">pointer to a callback function of type <see cref="on_disconnect_v5"/></param>
        /// </param>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mosquitto_disconnect_v5_callback_set(MosquittoPtr mosq, [MarshalAs(UnmanagedType.FunctionPtr)] on_disconnect_v5 on_disconnect);
        /// <summary>
        /// Callback for <see cref="mosquitto_disconnect_v5_callback_set"/>
        /// </summary>
        /// <param name="mosq">the mosquitto instance making the callback.</param>
        /// <param name="obj">the user data provided in <see cref="mosquitto_new"/></param>
        /// <param name="rc">integer value indicating the reason for the disconnect. 
        /// A value of 0 means the client has called <see cref="mosquitto_disconnect"/>. 
        /// Any other value indicates that the disconnect is unexpected.</param>
        /// <param name="props">list of MQTT 5 properties, or NULL</param>
        internal delegate void on_disconnect_v5(MosquittoPtr mosq, IntPtr obj, int rc, MosquittoPropertyPtr props);

        /// <summary>
        /// Set the publish callback. This is called when a message initiated with
		/// <see cref="mosquitto_publish"/> has been sent to the broker. "Sent" means different
		/// things depending on the QoS of the message:
        /// <list type="table">
		/// <item><term>QoS 0</term><description>The PUBLISH was passed to the local operating system for delivery,
		/// there is no guarantee that it was delivered to the remote broker.</description></item>
		/// <item><term>QoS 1</term><description>The PUBLISH was sent to the remote broker and the corresponding
		/// PUBACK was received by the library.</description></item>
		/// <item><term>QoS 2</term><description>The PUBLISH was sent to the remote broker and the corresponding
		/// PUBCOMP was received by the library.</description></item>
        /// </list>
		/// </summary>
		/// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="on_publish">pointer to a callback function of type <see cref="on_publish"/></param>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mosquitto_publish_callback_set(MosquittoPtr mosq, [MarshalAs(UnmanagedType.FunctionPtr)] on_publish on_publish);

        /// <summary>
        /// Callback for <see cref="mosquitto_publish_callback_set"/>
        /// </summary>
        /// <param name="mosq">the mosquitto instance making the callback.</param>
        /// <param name="obj">the user data provided in <see cref="mosquitto_new"/></param>
        /// <param name="mid">the message id of the sent message.</param>
        internal delegate void on_publish(MosquittoPtr mosq, IntPtr obj, int mid);

        /// <summary>
        /// <para>
        /// Set the publish callback. This is called when a message initiated with
        /// <see cref="mosquitto_publish"/> has been sent to the broker. This callback will be
        /// called both if the message is sent successfully, or if the broker responded
        /// with an error, which will be reflected in the reason_code parameter.
        /// "Sent" means different things depending on the QoS of the message:
        /// <list type="table">
        /// <item><term>QoS 0</term><description>The PUBLISH was passed to the local operating system for delivery,
		/// there is no guarantee that it was delivered to the remote broker.</description></item>
		/// <item><term>QoS 1</term><description>The PUBLISH was sent to the remote broker and the corresponding
		/// PUBACK was received by the library.</description></item>
		/// <item><term>QoS 2</term><description>The PUBLISH was sent to the remote broker and the corresponding
		/// PUBCOMP was received by the library.</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// It is valid to set this callback for all MQTT protocol versions. If it is
        /// used with MQTT clients that use MQTT v3.1.1 or earlier, then the `props`
        /// argument will always be NULL.
        /// </para>
		/// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="on_publish">pointer to a callback function of type <see cref="on_publish_v5"/></param>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mosquitto_publish_v5_callback_set(MosquittoPtr mosq, [MarshalAs(UnmanagedType.FunctionPtr)] on_publish_v5 on_publish);

        /// <summary>
        /// Callback for <see cref="mosquitto_publish_v5_callback_set"/>
        /// </summary>
        /// <param name="mosq">the mosquitto instance making the callback.</param>
        /// <param name="obj">the user data provided in <see cref="mosquitto_new"/></param>
        /// <param name="mid">the message id of the sent message.</param>
        /// <param name="reason_code">the MQTT 5 reason code</param>
        /// <param name="props">list of MQTT 5 properties, or NULL</param>
        internal delegate void on_publish_v5(MosquittoPtr mosq, IntPtr obj, int mid, int reason_code, MosquittoPropertyPtr props);

        /// <summary>
		/// Set the message callback. This is called when a message is received from the
		/// broker and the required QoS flow has completed.
		/// </summary>
		/// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="on_message">pointer to a callback function of type <see cref="on_message"/></param>
		/// <remarks>
		/// See Also: <see cref="mosquitto_message_copy"/>
		/// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mosquitto_message_callback_set(MosquittoPtr mosq, [MarshalAs(UnmanagedType.FunctionPtr)] on_message on_message);

        /// <summary>
        /// Callback for <see cref="mosquitto_message_callback_set"/>
        /// </summary>
		/// <param name="mosq">the mosquitto instance making the callback.</param>
		/// <param name="obj">the user data provided in <see cref="mosquitto_new"/></param>
		/// <param name="message">the message data. 
        /// This variable and associated memory will be freed by the library after the callback completes. 
        /// The client should make copies of any of the data it requires.</param>
        internal delegate void on_message(MosquittoPtr mosq, IntPtr obj, ref mosquitto_message message);

        /// <summary>
        /// <para>
        /// Set the message callback. This is called when a message is received from the
        /// broker and the required QoS flow has completed.
        /// </para>
        /// <para>
        /// It is valid to set this callback for all MQTT protocol versions. If it is
        /// used with MQTT clients that use MQTT v3.1.1 or earlier, then the `props`
        /// argument will always be NULL.
        /// </para>
		/// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="on_message">pointer to a callback function of type <see cref="on_message_v5"/></param>
        /// <remarks>
        /// See Also: <see cref="mosquitto_message_copy"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mosquitto_message_v5_callback_set(MosquittoPtr mosq, [MarshalAs(UnmanagedType.FunctionPtr)] on_message_v5 on_message);

        /// <summary>
        /// Callback for <see cref="mosquitto_message_callback_set"/>
        /// </summary>
        /// <param name="mosq">the mosquitto instance making the callback.</param>
        /// <param name="obj">the user data provided in <see cref="mosquitto_new"/></param>
        /// <param name="message">the message data. 
        /// This variable and associated memory will be freed by the library after the callback completes. 
        /// The client should make copies of any of the data it requires.</param>
        /// <param name="props">list of MQTT 5 properties, or NULL</param>
        internal delegate void on_message_v5(MosquittoPtr mosq, IntPtr obj, ref mosquitto_message message, MosquittoPropertyPtr props);

        /// <summary>
		/// Set the subscribe callback. This is called when the library receives a
		/// SUBACK message in response to a SUBSCRIBE.
		/// </summary>
		/// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="on_subscribe">pointer to a callback function of type <see cref="on_subscribe"/></param>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mosquitto_subscribe_callback_set(MosquittoPtr mosq, [MarshalAs(UnmanagedType.FunctionPtr)] on_subscribe on_subscribe);

        /// <summary>
        /// Callback for <see cref="mosquitto_subscribe_callback_set"/>
        /// </summary>
		/// <param name="mosq">the mosquitto instance making the callback.</param>
		/// <param name="obj">the user data provided in <see cref="mosquitto_new"/></param>
		/// <param name="mid">the message id of the subscribe message.</param>
		/// <param name="qos_count">the number of granted subscriptions (size of granted_qos).</param>
		/// <param name="granted_qos">an array of integers indicating the granted QoS for each of the subscriptions.</param>
        internal delegate void on_subscribe(MosquittoPtr mosq, IntPtr obj, int mid, int qos_count, IntPtr granted_qos);

        /// <summary>
        /// <para>
        /// Set the subscribe callback. This is called when the library receives a
        /// SUBACK message in response to a SUBSCRIBE.
        /// </para>
        /// <para>
        /// It is valid to set this callback for all MQTT protocol versions. If it is
        /// used with MQTT clients that use MQTT v3.1.1 or earlier, then the `props`
        /// argument will always be NULL.
        /// </para>
		/// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="on_subscribe">pointer to a callback function of type <see cref="on_subscribe_v5"/></param>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mosquitto_subscribe_v5_callback_set(MosquittoPtr mosq, [MarshalAs(UnmanagedType.FunctionPtr)] on_subscribe_v5 on_subscribe);

        /// <summary>
        /// Callback for <see cref="mosquitto_subscribe_v5_callback_set"/>
        /// </summary>
        /// <param name="mosq">the mosquitto instance making the callback.</param>
        /// <param name="obj">the user data provided in <see cref="mosquitto_new"/></param>
        /// <param name="mid">the message id of the subscribe message.</param>
        /// <param name="qos_count">the number of granted subscriptions (size of granted_qos).</param>
        /// <param name="granted_qos">an array of integers indicating the granted QoS for each of the subscriptions.</param>
        /// <param name="props">list of MQTT 5 properties, or NULL</param>
        internal delegate void on_subscribe_v5(MosquittoPtr mosq, IntPtr obj, int mid, int qos_count, IntArrayPtr granted_qos, MosquittoPropertyPtr props);

        /// <summary>
		/// Set the unsubscribe callback. This is called when the library receives a
		/// UNSUBACK message in response to an UNSUBSCRIBE.
		/// </summary>
		/// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="on_unsubscribe">pointer to a callback function of type <see cref="on_unsubscribe"/></param>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mosquitto_unsubscribe_callback_set(MosquittoPtr mosq, [MarshalAs(UnmanagedType.FunctionPtr)] on_unsubscribe on_unsubscribe);

        /// <summary>
        /// Callback for <see cref="mosquitto_unsubscribe_callback_set"/>
        /// </summary>
        /// <param name="mosq">the mosquitto instance making the callback.</param>
        /// <param name="obj">the user data provided in <see cref="mosquitto_new"/></param>
        /// <param name="mid">the message id of the unsubscribe message.</param>
        internal delegate void on_unsubscribe(MosquittoPtr mosq, IntPtr obj, int mid);

        /// <summary>
        /// <para>
		/// Set the unsubscribe callback. This is called when the library receives a
		/// UNSUBACK message in response to an UNSUBSCRIBE.
        /// </para>
        /// <para>
		/// It is valid to set this callback for all MQTT protocol versions. If it is
		/// used with MQTT clients that use MQTT v3.1.1 or earlier, then the `props`
		/// argument will always be NULL.
        /// </para>
		/// </summary>
		/// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="on_unsubscribe">pointer to a callback function of type <see cref="on_unsubscribe_v5"/></param>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mosquitto_unsubscribe_v5_callback_set(MosquittoPtr mosq, [MarshalAs(UnmanagedType.FunctionPtr)] on_unsubscribe_v5 on_unsubscribe);

        /// <summary>
        /// Callback for <see cref="mosquitto_unsubscribe_callback_set"/>
        /// </summary>
		/// <param name="mosq">the mosquitto instance making the callback.</param>
		/// <param name="obj">the user data provided in <see cref="mosquitto_new"/></param>
		/// <param name="mid">the message id of the unsubscribe message.</param>
		/// <param name="props">list of MQTT 5 properties, or NULL</param>
        internal delegate void on_unsubscribe_v5(MosquittoPtr mosq, IntPtr obj, int mid, MosquittoPropertyPtr props);

        /// <summary>
		/// Set the logging callback. This should be used if you want event logging
		/// information from the client library.
        /// </summary>
		/// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="on_log">pointer to a callback function of type <see cref="on_log"/></param>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mosquitto_log_callback_set(MosquittoPtr mosq, [MarshalAs(UnmanagedType.FunctionPtr)] on_log on_log);

        /// <summary>
        /// Callback for <see cref="mosquitto_log_callback_set"/>
        /// </summary>
		/// <param name="mosq">the mosquitto instance making the callback.</param>
		/// <param name="obj">the user data provided in <see cref="mosquitto_new"/></param>
		/// <param name="level">the log message level from the values: 
        /// <list type="bullet">
        ///     <item><description>MOSQ_LOG_INFO</description></item>
        ///     <item><description>MOSQ_LOG_NOTICE</description></item>
        ///     <item><description>MOSQ_LOG_WARNING</description></item>
        ///     <item><description>MOSQ_LOG_ERR</description></item>
        ///     <item><description>MOSQ_LOG_DEBUG</description></item>
        /// </list>
        /// </param>
		/// <param name="str">the message string.</param>
        internal delegate void on_log(MosquittoPtr mosq, IntPtr obj, int level, [MarshalAs(UnmanagedType.LPStr)] string str);
        #endregion

        #region SOCKS5 proxy functions
        /// <summary>
        /// Configure the client to use a SOCKS5 proxy when connecting. Must be called
        /// before connecting. "None" and "username/password" authentication is
        /// supported.
		/// </summary>
        /// <param name="mosq">a valid mosquitto instance.</param>
        /// <param name="host">the SOCKS5 proxy host to connect to.</param>
        /// <param name="port">the SOCKS5 proxy port to use.</param>
        /// <param name="username">if not NULL, use this username when authenticating with the proxy.</param>
        /// <param name="password">if not NULL and username is not NULL, use this password when authenticating with the proxy.</param>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_socks5_set(MosquittoPtr mosq, [MarshalAs(UnmanagedType.LPStr)] string host, int port, [MarshalAs(UnmanagedType.LPStr)] string username, [MarshalAs(UnmanagedType.LPStr)] string password);
        #endregion

        #region Utility functions
        /// <summary>
        /// Call to obtain a const string description of a mosquitto error number.
		/// </summary>
        /// <param name="mosq_errno">a mosquitto error number.</param>
        /// <returns>
        /// 	A constant string describing the error.
        /// </returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        internal static extern string mosquitto_strerror(int mosq_errno);

        /// <summary>
        /// Call to obtain a const string description of an MQTT connection result.
		/// </summary>
        /// <param name="connack_code">an MQTT connection result.</param>
        /// <returns>
        /// 	A constant string describing the result.
        /// </returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        internal static extern string mosquitto_connack_string(int connack_code);

        /// <summary>
        /// Call to obtain a const string description of an MQTT reason code.
		/// </summary>
        /// <param name="reason_code">an MQTT reason code.</param>
        /// <returns>
        /// 	A constant string describing the reason.
        /// </returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        internal static extern string mosquitto_reason_string(int reason_code);

        /// <summary>
        /// Take a string input representing an MQTT command and convert it to the
        /// libmosquitto integer representation.
		/// </summary>
        /// <param name="str">the string to parse.</param>
        /// <param name="cmd">pointer to an int, for the result.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>on an invalid input.</description></item>
        /// </list></returns>
        /// <remarks>Example: 
        /// <code>
        /// mosquitto_string_to_command("CONNECT", &cmd);
        /// // cmd == CMD_CONNECT
        /// </code>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_string_to_command([MarshalAs(UnmanagedType.LPStr)] string str, ref int cmd);

        /// <summary>
        /// <para>
        /// Tokenise a topic or subscription string into an array of strings
        /// representing the topic hierarchy.
        /// </para>
        /// <para>
        /// For example:
        /// <code>
        /// subtopic: "a/deep/topic/hierarchy"
        /// </code>
        /// Would result in:
        /// <code>
        /// topics[0] = "a"
        /// topics[1] = "deep"
        /// topics[2] = "topic"
        /// topics[3] = "hierarchy"
        /// </code>
        /// and:
        /// <code>
        /// subtopic: "/a/deep/topic/hierarchy/"
        /// </code>
        /// Would result in:
        /// <code>
        /// topics[0] = NULL
        /// topics[1] = "a"
        /// topics[2] = "deep"
        /// topics[3] = "topic"
        /// topics[4] = "hierarchy"
        /// </code>
        /// </para>
		/// </summary>
        /// <param name="subtopic">the subscription/topic to tokenise</param>
        /// <param name="topics">a pointer to store the array of strings</param>
        /// <param name="count">an int pointer to store the number of items in the topics array.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>if an out of memory condition occurred.</description></item>
        /// <item><term>MOSQ_ERR_MALFORMED_UTF8</term><description>if the topic is not valid UTF-8</description></item>
        /// </list></returns>
        /// <remarks>Example: 
        /// <code><![CDATA[
        /// char **topics;
        /// int topic_count;
        /// int i;
        /// 
        /// mosquitto_sub_topic_tokenise("$SYS/broker/uptime", &topics, &topic_count);
        /// 
        /// for(i=0; i<token_count; i++){
        ///     printf("%d: %s\n", i, topics[i]);
        /// }
        /// ]]>
        /// </code>
        /// </remarks>
        /// <remarks>
        /// See Also: <see cref="mosquitto_sub_topic_tokens_free"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_sub_topic_tokenise([MarshalAs(UnmanagedType.LPStr)] string subtopic, ref StringArrayPtr topics, ref int count);

        /// <summary>
        /// Free memory that was allocated in <see cref="mosquitto_sub_topic_tokenise"/>.
		/// </summary>
        /// <param name="topics">pointer to string array.</param>
        /// <param name="count">count of items in string array.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_sub_topic_tokenise"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_sub_topic_tokens_free(ref StringArrayPtr topics, int count);

        /// <summary>
        /// <para>
        /// Check whether a topic matches a subscription.
        /// </para>
        /// <para>
        /// For example:
        /// </para>
        /// <para>
        /// <c>foo/bar</c> would match the subscription <c>foo/#</c> or <c>+/bar</c>
        /// </para>
        /// <para>
        /// <c>non/matching</c> would not match the subscription <c>non/+/+</c>
        /// </para>
		/// </summary>
        /// <param name="sub">subscription string to check topic against.</param>
        /// <param name="topic">topic to check.</param>
        /// <param name="result">bool pointer to hold result. Will be set to true if the topic matches the subscription.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>if an out of memory condition occurred.</description></item>
        /// </list></returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_topic_matches_sub([MarshalAs(UnmanagedType.LPStr)] string sub, [MarshalAs(UnmanagedType.LPStr)] string topic, ref bool result);


        /// <summary>
        /// <para>
        /// Check whether a topic matches a subscription.
        /// </para>
        /// <para>
        /// For example:
        /// </para>
        /// <para>
        /// <c>foo/bar</c> would match the subscription <c>foo/#</c> or <c>+/bar</c>
        /// </para>
        /// <para>
        /// <c>non/matching</c> would not match the subscription <c>non/+/+</c>
        /// </para>
		/// </summary>
        /// <param name="sub">subscription string to check topic against.</param>
        /// <param name="sublen">length in bytes of sub string</param>
        /// <param name="topic">topic to check.</param>
        /// <param name="topiclen">length in bytes of topic string</param>
        /// <param name="result">bool pointer to hold result. Will be set to true if the topic matches the subscription.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the input parameters were invalid.</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>if an out of memory condition occurred.</description></item>
        /// </list></returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_topic_matches_sub2([MarshalAs(UnmanagedType.LPStr)] string sub, SizeT sublen, [MarshalAs(UnmanagedType.LPStr)] string topic, SizeT topiclen, ref bool result);

        /// <summary>
        /// <para>
        /// Check whether a topic to be used for publishing is valid.
        /// </para>
        /// <para>
        /// This searches for + or # in a topic and checks its length.
        /// </para>
        /// <para>
        /// This check is already carried out in <see cref="mosquitto_publish"/> and
        /// <see cref="mosquitto_will_set"/>, there is no need to call it directly before them. It
        /// may be useful if you wish to check the validity of a topic in advance of
        /// making a connection for example.
        /// </para>
		/// </summary>
        /// <param name="topic">the topic to check</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>for a valid topic</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the topic contains a + or a #, or if it is too long.</description></item>
        /// <item><term>MOSQ_ERR_MALFORMED_UTF8</term><description>if topic is not valid UTF-8</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_sub_topic_check"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_pub_topic_check([MarshalAs(UnmanagedType.LPStr)] string topic);

        /// <summary>
        /// <para>
        /// Check whether a topic to be used for publishing is valid.
        /// </para>
        /// <para>
        /// This searches for + or # in a topic and checks its length.
        /// </para>
        /// <para>
        /// This check is already carried out in <see cref="mosquitto_publish"/> and
        /// <see cref="mosquitto_will_set"/>, there is no need to call it directly before them. It
        /// may be useful if you wish to check the validity of a topic in advance of
        /// making a connection for example.
        /// </para>
		/// </summary>
        /// <param name="topic">the topic to check</param>
        /// <param name="topiclen">length of the topic in bytes</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>for a valid topic</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the topic contains a + or a #, or if it is too long.</description></item>
        /// <item><term>MOSQ_ERR_MALFORMED_UTF8</term><description>if topic is not valid UTF-8</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_sub_topic_check"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_pub_topic_check2([MarshalAs(UnmanagedType.LPStr)] string topic, SizeT topiclen);

        /// <summary>
        /// <para>
        /// Check whether a topic to be used for subscribing is valid.
        /// </para>
        /// <para>
        /// This searches for + or # in a topic and checks that they aren't in invalid
        /// positions, such as with foo/#/bar, foo/+bar or foo/bar#, and checks its
        /// length.
        /// </para>
        /// <para>
        /// This check is already carried out in <see cref="mosquitto_subscribe"/> and
        /// <see cref="mosquitto_unsubscribe"/>, there is no need to call it directly before them.
        /// It may be useful if you wish to check the validity of a topic in advance of
        /// making a connection for example.
        /// </para>
		/// </summary>
        /// <param name="topic">the topic to check</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>for a valid topic</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the topic contains a + or a # that is in an invalid position, or if it is too long.</description></item>
        /// <item><term>MOSQ_ERR_MALFORMED_UTF8</term><description>if topic is not valid UTF-8</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_sub_topic_check"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_sub_topic_check([MarshalAs(UnmanagedType.LPStr)] string topic);

        /// <summary>
        /// <para>
        /// Check whether a topic to be used for subscribing is valid.
        /// </para>
        /// <para>
        /// This searches for + or # in a topic and checks that they aren't in invalid
        /// positions, such as with foo/#/bar, foo/+bar or foo/bar#, and checks its
        /// length.
        /// </para>
        /// <para>
        /// This check is already carried out in <see cref="mosquitto_subscribe"/> and
        /// <see cref="mosquitto_unsubscribe"/>, there is no need to call it directly before them.
        /// It may be useful if you wish to check the validity of a topic in advance of
        /// making a connection for example.
        /// </para>
		/// </summary>
        /// <param name="topic">the topic to check</param>
        /// <param name="topiclen">the length in bytes of the topic</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>for a valid topic</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the topic contains a + or a # that is in an invalid position, or if it is too long.</description></item>
        /// <item><term>MOSQ_ERR_MALFORMED_UTF8</term><description>if topic is not valid UTF-8</description></item>
        /// </list></returns>
        /// <remarks>
        /// See Also: <see cref="mosquitto_sub_topic_check"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_sub_topic_check2([MarshalAs(UnmanagedType.LPStr)] string topic, SizeT topiclen);


        /// <summary>
        /// Helper function to validate whether a UTF-8 string is valid, according to
        /// the UTF-8 spec and the MQTT additions.
		/// </summary>
        /// <param name="str">a string to check</param>
        /// <param name="len">the length of the string in bytes</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if str is NULL or len<0 or len>65536</description></item>
        /// <item><term>MOSQ_ERR_MALFORMED_UTF8</term><description>if str is not valid UTF-8</description></item>
        /// </list></returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_validate_utf8([MarshalAs(UnmanagedType.LPStr)] string str, int len);
        #endregion

        #region One line client helper functions

        /// <summary>
        /// <para>
        /// Helper function to make subscribing to a topic and retrieving some messages
        /// very straightforward.
        /// </para>
        /// <para>
        /// This connects to a broker, subscribes to a topic, waits for msg_count
        /// messages to be received, then returns after disconnecting cleanly.
        /// </para>
		/// </summary>
        /// <param name="messages">pointer to a "struct mosquitto_message *". The received messages will be returned here. On error, this will be set to NULL.</param>
        /// <param name="msg_count">the number of messages to retrieve.</param>
        /// <param name="want_retained">if set to true, stale retained messages will be treated as normal messages with regards to msg_count. If set to false, they will be ignored.</param>
        /// <param name="topic">the subscription topic to use (wildcards are allowed).</param>
        /// <param name="qos">the qos to use for the subscription.</param>
        /// <param name="host">the broker to connect to.</param>
        /// <param name="port">the network port the broker is listening on.</param>
        /// <param name="client_id">the client id to use, or NULL if a random client id should be generated.</param>
        /// <param name="keepalive">the MQTT keepalive value.</param>
        /// <param name="clean_session">the MQTT clean session flag.</param>
        /// <param name="username">the username string, or NULL for no username authentication.</param>
        /// <param name="password">the password string, or NULL for an empty password.</param>
        /// <param name="will">a libmosquitto_will struct containing will information, or NULL for no will.</param>
        /// <param name="tls">a libmosquitto_tls struct containing TLS related parameters, or NULL for no use of TLS.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success</description></item>
        /// <item><term>Greater than 0</term><description>on error.</description></item>
        /// </list></returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_subscribe_simple(
            ref MosquittoMessagePtr messages,
            int msg_count,
            bool want_retained,
            [MarshalAs(UnmanagedType.LPStr)] string topic,
            int qos,
            [MarshalAs(UnmanagedType.LPStr)] string host,
            int port,
            [MarshalAs(UnmanagedType.LPStr)] string client_id,
            int keepalive,
            bool clean_session,
            [MarshalAs(UnmanagedType.LPStr)] string username,
            [MarshalAs(UnmanagedType.LPStr)] string password,
            ref libmosquitto_will will,
            ref libmosquitto_tls tls);


        /// <summary>
        /// <para>
        /// Helper function to make subscribing to a topic and processing some messages
        /// very straightforward.
        /// </para>
        /// <para>
        /// This connects to a broker, subscribes to a topic, then passes received
        /// messages to a user provided callback. If the callback returns a 1, it then
        /// disconnects cleanly and returns.
        /// </para>
		/// </summary>
        /// <param name="callback">a callback function in the following form: 
        /// <c>int callback(MosquittoPtr mosq, void *obj, const struct mosquitto_message *message)</c>
        /// Note that this is the same as the normal on_message callback, except that it returns an int.</param>
        /// <param name="userdata">user provided pointer that will be passed to the callback.</param>
        /// <param name="topic">the subscription topic to use (wildcards are allowed).</param>
        /// <param name="qos">the qos to use for the subscription.</param>
        /// <param name="host">the broker to connect to.</param>
        /// <param name="port">the network port the broker is listening on.</param>
        /// <param name="client_id">the client id to use, or NULL if a random client id should be generated.</param>
        /// <param name="keepalive">the MQTT keepalive value.</param>
        /// <param name="clean_session">the MQTT clean session flag.</param>
        /// <param name="username">the username string, or NULL for no username authentication.</param>
        /// <param name="password">the password string, or NULL for an empty password.</param>
        /// <param name="will">a libmosquitto_will struct containing will information, or NULL for no will.</param>
        /// <param name="tls">a libmosquitto_tls struct containing TLS related parameters, or NULL for no use of TLS.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success</description></item>
        /// <item><term>Greater than 0</term><description>on error.</description></item>
        /// </list></returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_subscribe_callback(
            subscribe_callback callback,
            IntPtr userdata,
            [MarshalAs(UnmanagedType.LPStr)] string topic,
            int qos,
            [MarshalAs(UnmanagedType.LPStr)] string host,
            int port,
            [MarshalAs(UnmanagedType.LPStr)] string client_id,
            int keepalive,
            bool clean_session,
            [MarshalAs(UnmanagedType.LPStr)] string username,
            [MarshalAs(UnmanagedType.LPStr)] string password,
            ref libmosquitto_will will,
            ref libmosquitto_tls tls);
        /// <inheritdoc cref="mosquitto_subscribe_callback"/>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_subscribe_callback(
            subscribe_callback callback,
            IntPtr userdata,
            [MarshalAs(UnmanagedType.LPStr)] string topic,
            int qos,
            [MarshalAs(UnmanagedType.LPStr)] string host,
            int port,
            [MarshalAs(UnmanagedType.LPStr)] string client_id,
            int keepalive,
            bool clean_session,
            [MarshalAs(UnmanagedType.LPStr)] string username,
            [MarshalAs(UnmanagedType.LPStr)] string password,
            IntPtr will, // to allow passing null
            ref libmosquitto_tls tls);
        /// <inheritdoc cref="mosquitto_subscribe_callback"/>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_subscribe_callback(
            subscribe_callback callback,
            IntPtr userdata,
            [MarshalAs(UnmanagedType.LPStr)] string topic,
            int qos,
            [MarshalAs(UnmanagedType.LPStr)] string host,
            int port,
            [MarshalAs(UnmanagedType.LPStr)] string client_id,
            int keepalive,
            bool clean_session,
            [MarshalAs(UnmanagedType.LPStr)] string username,
            [MarshalAs(UnmanagedType.LPStr)] string password,
            ref libmosquitto_will will,
            IntPtr tls); // to allow passing null
        /// <inheritdoc cref="mosquitto_subscribe_callback"/>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_subscribe_callback(
            subscribe_callback callback,
            IntPtr userdata,
            [MarshalAs(UnmanagedType.LPStr)] string topic,
            int qos,
            [MarshalAs(UnmanagedType.LPStr)] string host,
            int port,
            [MarshalAs(UnmanagedType.LPStr)] string client_id,
            int keepalive,
            bool clean_session,
            [MarshalAs(UnmanagedType.LPStr)] string username,
            [MarshalAs(UnmanagedType.LPStr)] string password,
            IntPtr will, // to allow passing null
            IntPtr tls); // to allow passing null

        /// <summary>
        /// Callback for <see cref="mosquitto_subscribe_callback"/>
        /// </summary>
        /// <returns>
        /// If this callback returns 1, mosquitto will disconnect cleanly and exit
        /// </returns>
        /// <inheritdoc cref="on_message"/>
        internal delegate int subscribe_callback(MosquittoPtr mosq, IntPtr obj, ref mosquitto_message message);
        #endregion

        #region Properties

        /// <summary>
        /// <para>
        /// Add a new byte property to a property list.
        /// </para>
        /// <para>
        /// If *proplist == NULL, a new list will be created, otherwise the new property
        /// will be appended to the list.
        /// </para>
		/// </summary>
        /// <param name="proplist">pointer to mosquitto_property pointer, the list of properties</param>
        /// <param name="identifier">property identifier (e.g. MQTT_PROP_PAYLOAD_FORMAT_INDICATOR)</param>
        /// <param name="value">integer value for the new property</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if identifier is invalid, or if proplist is NULL</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>on out of memory</description></item>
        /// </list></returns>
        /// <remarks>Example: 
        /// <code><![CDATA[
        /// mosquitto_property *proplist = NULL;
        /// mosquitto_property_add_byte(&proplist, MQTT_PROP_PAYLOAD_FORMAT_INDICATOR, 1);
        /// ]]></code>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_property_add_byte(ref MosquittoPropertyPtr proplist, mqtt5_property identifier, byte value);

        /// <summary>
        /// <para>
        /// Add a new int16 property to a property list.
        /// </para>
        /// <para>
        /// If *proplist == NULL, a new list will be created, otherwise the new property
        /// will be appended to the list.
        /// </para>
		/// </summary>
        /// <param name="proplist">pointer to mosquitto_property pointer, the list of properties</param>
        /// <param name="identifier">property identifier (e.g. MQTT_PROP_RECEIVE_MAXIMUM)</param>
        /// <param name="value">integer value for the new property</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if identifier is invalid, or if proplist is NULL</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>on out of memory</description></item>
        /// </list></returns>
        /// <remarks>Example: 
        /// <code><![CDATA[
        /// mosquitto_property *proplist = NULL;
        /// mosquitto_property_add_int16(&proplist, MQTT_PROP_RECEIVE_MAXIMUM, 1000);
        /// ]]></code>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_property_add_int16(ref MosquittoPropertyPtr proplist, mqtt5_property identifier, ushort value);

        /// <summary>
        /// <para>
        /// Add a new int32 property to a property list.
        /// </para>
        /// <para>
        /// If *proplist == NULL, a new list will be created, otherwise the new property
        /// will be appended to the list.
        /// </para>
        /// </summary>
        /// <param name="proplist">pointer to mosquitto_property pointer, the list of properties</param>
        /// <param name="identifier">property identifier (e.g. MQTT_PROP_MESSAGE_EXPIRY_INTERVAL)</param>
        /// <param name="value">integer value for the new property</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if identifier is invalid, or if proplist is NULL</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>on out of memory</description></item>
        /// </list></returns>
        /// <remarks>Example: 
        /// <code><![CDATA[
        /// mosquitto_property *proplist = NULL;
        /// mosquitto_property_add_int32(&proplist, MQTT_PROP_MESSAGE_EXPIRY_INTERVAL, 86400);
        /// ]]></code>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_property_add_int32(ref MosquittoPropertyPtr proplist, mqtt5_property identifier, uint value);

        /// <summary>
        /// <para>
        /// Add a new varint property to a property list.
        /// </para>
        /// <para>
        /// If *proplist == NULL, a new list will be created, otherwise the new property
        /// will be appended to the list.
        /// </para>
        /// </summary>
        /// <param name="proplist">pointer to mosquitto_property pointer, the list of properties</param>
        /// <param name="identifier">property identifier (e.g. MQTT_PROP_SUBSCRIPTION_IDENTIFIER)</param>
        /// <param name="value">integer value for the new property</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if identifier is invalid, or if proplist is NULL</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>on out of memory</description></item>
        /// </list></returns>
        /// <remarks>Example: 
        /// <code><![CDATA[
        /// mosquitto_property *proplist = NULL;
        /// mosquitto_property_add_varint(&proplist, MQTT_PROP_SUBSCRIPTION_IDENTIFIER, 1);
        /// ]]></code>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_property_add_varint(ref MosquittoPropertyPtr proplist, mqtt5_property identifier, uint value);

        /// <summary>
        /// <para>
        /// Add a new binary property to a property list.
        /// </para>
        /// <para>
        /// If *proplist == NULL, a new list will be created, otherwise the new property
        /// will be appended to the list.
        /// </para>
        /// </summary>
        /// <param name="proplist">pointer to mosquitto_property pointer, the list of properties</param>
        /// <param name="identifier">property identifier (e.g. MQTT_PROP_PAYLOAD_FORMAT_INDICATOR)</param>
        /// <param name="value">pointer to the property data</param>
        /// <param name="len">length of property data in bytes</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if identifier is invalid, or if proplist is NULL</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>on out of memory</description></item>
        /// </list></returns>
        /// <remarks>Example: 
        /// <code><![CDATA[
        /// mosquitto_property *proplist = NULL;
        /// mosquitto_property_add_binary(&proplist, MQTT_PROP_AUTHENTICATION_DATA, auth_data, auth_data_len);
        /// ]]></code>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_property_add_binary(ref MosquittoPropertyPtr proplist, mqtt5_property identifier, byte[] value, ushort len);

        /// <summary>
        /// <para>
        /// Add a new string property to a property list.
        /// </para>
        /// <para>
        /// If *proplist == NULL, a new list will be created, otherwise the new property
        /// will be appended to the list.
        /// </para>
        /// </summary>
        /// <param name="proplist">pointer to mosquitto_property pointer, the list of properties</param>
        /// <param name="identifier">property identifier (e.g. MQTT_PROP_CONTENT_TYPE)</param>
        /// <param name="value">string value for the new property, must be UTF-8 and zero terminated</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if identifier is invalid, if value is NULL, or if proplist is NULL</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>on out of memory</description></item>
        /// <item><term>MOSQ_ERR_MALFORMED_UTF8</term><description>value is not valid UTF-8.</description></item>
        /// </list></returns>
        /// <remarks>Example: 
        /// <code><![CDATA[
        /// mosquitto_property *proplist = NULL;
        /// mosquitto_property_add_string(&proplist, MQTT_PROP_CONTENT_TYPE, "application/json");
        /// ]]></code>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_property_add_string(ref MosquittoPropertyPtr proplist, mqtt5_property identifier, [MarshalAs(UnmanagedType.LPStr)] string value);

        /// <summary>
        /// <para>
        /// Add a new string pair property to a property list.
        /// </para>
        /// <para>
        /// If *proplist == NULL, a new list will be created, otherwise the new property
        /// will be appended to the list.
        /// </para>
        /// </summary>
        /// <param name="proplist">pointer to mosquitto_property pointer, the list of properties</param>
        /// <param name="identifier">property identifier (e.g. MQTT_PROP_USER_PROPERTY)</param>
        /// <param name="name">string name for the new property, must be UTF-8 and zero terminated</param>
        /// <param name="value">string value for the new property, must be UTF-8 and zero terminated</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if identifier is invalid, if name or value is NULL, or if proplist is NULL</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>on out of memory</description></item>
        /// <item><term>MOSQ_ERR_MALFORMED_UTF8</term><description>if name or value are not valid UTF-8.</description></item>
        /// </list></returns>
        /// <remarks>Example: 
        /// <code><![CDATA[
        /// mosquitto_property *proplist = NULL;
        /// mosquitto_property_add_string_pair(&proplist, MQTT_PROP_USER_PROPERTY, "client", "mosquitto_pub");
        /// ]]></code>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_property_add_string_pair(ref MosquittoPropertyPtr proplist, mqtt5_property identifier, [MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPStr)] string value);


        /// <summary>
        /// Return the property identifier for a single property.
        /// </summary>
        /// <param name="property">pointer to a valid mosquitto_property pointer.</param>
        /// <returns>
        ///A valid property identifier on success
        /// 0 on error
        /// </returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_property_identifier(MosquittoPropertyPtr property);


        /// <summary>
        /// Return the next property in a property list. Use to iterate over a property
        /// list, e.g.:
        /// 
        /// <code><![CDATA[
        /// for(prop = proplist; prop != NULL; prop = mosquitto_property_next(prop)){
        /// 	if(mosquitto_property_identifier(prop) == MQTT_PROP_CONTENT_TYPE){
        /// 		...
        /// 	}
        /// }
        /// ]]></code>
        /// </summary>
        /// <param name="proplist">pointer to mosquitto_property pointer, the list of properties</param>
        /// <returns>
        /// 	Pointer to the next item in the list
        /// 	NULL, if proplist is NULL, or if there are no more items in the list.
        /// </returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern MosquittoPropertyPtr mosquitto_property_next(MosquittoPropertyPtr proplist);


        /// <summary>
        /// <para>
        /// Attempt to read a byte property matching an identifier, from a property list
        /// or single property. This function can search for multiple entries of the
        /// same identifier by using the returned value and skip_first. Note however
        /// that it is forbidden for most properties to be duplicated.
        /// </para>
        /// <para>
        /// If the property is not found, *value will not be modified, so it is safe to
        /// pass a variable with a default value to be potentially overwritten:
        /// 
        /// <code><![CDATA[
        /// ushort keepalive = 60; // default value
        /// // Get value from property list, or keep default if not found.
        /// mosquitto_property_read_int16(proplist, MQTT_PROP_SERVER_KEEP_ALIVE, &keepalive, false);
        /// ]]></code>
        /// </para>
        /// </summary>
        /// <param name="proplist">mosquitto_property pointer, the list of properties or single property</param>
        /// <param name="identifier">property identifier (e.g. MQTT_PROP_PAYLOAD_FORMAT_INDICATOR)</param>
        /// <param name="value">pointer to store the value, or NULL if the value is not required.</param>
        /// <param name="skip_first">boolean that indicates whether the first item in the list should be ignored or not. Should usually be set to false.</param>
        /// <returns>
        /// <para>
        /// 	A valid property pointer if the property is found
        /// </para>
        /// <para>
        /// 	NULL, if the property is not found, or proplist is NULL.
        /// </para>
        /// </returns>
        /// <remarks>Example: 
        /// <code><![CDATA[
        /// 	// proplist is obtained from a callback
        /// 	mosquitto_property *prop;
        /// 	prop = mosquitto_property_read_byte(proplist, identifier, &value, false);
        /// 	while(prop){
        /// 		printf("value: %s\n", value);
        /// 		prop = mosquitto_property_read_byte(prop, identifier, &value);
        /// 	}
        /// ]]></code>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern MosquittoPropertyPtr mosquitto_property_read_byte(
            MosquittoPropertyPtr proplist,
            mqtt5_property identifier,
            ref byte value,
            bool skip_first);

        /// <summary>
        /// Read an int16 property value from a property.
        /// </summary>
        /// <param name="proplist">property to read</param>
        /// <param name="identifier">property identifier (e.g. MQTT_PROP_PAYLOAD_FORMAT_INDICATOR)</param>
        /// <param name="value">pointer to store the value, or NULL if the value is not required.</param>
        /// <param name="skip_first">boolean that indicates whether the first item in the list should be ignored or not. Should usually be set to false.</param>
        /// <returns>
        /// <para>
        /// 	A valid property pointer if the property is found
        /// </para>
        /// <para>
        /// 	NULL, if the property is not found, or proplist is NULL.
        /// </para>
        /// </returns>
        /// <remarks>
        /// Example: <see cref="mosquitto_property_read_byte"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern MosquittoPropertyPtr mosquitto_property_read_int16(
            MosquittoPropertyPtr proplist,
            mqtt5_property identifier,
            ref ushort value,
            bool skip_first);

        /// <summary>
        /// Read an int32 property value from a property.
        /// </summary>
        /// <param name="property">pointer to mosquitto_property pointer, the list of properties</param>
        /// <param name="identifier">property identifier (e.g. MQTT_PROP_PAYLOAD_FORMAT_INDICATOR)</param>
        /// <param name="value">pointer to store the value, or NULL if the value is not required.</param>
        /// <param name="skip_first">boolean that indicates whether the first item in the list should be ignored or not. Should usually be set to false.</param>
        /// <returns>
        /// <para>
        /// 	A valid property pointer if the property is found
        /// </para>
        /// <para>
        /// 	NULL, if the property is not found, or proplist is NULL.
        /// </para>
        /// </returns>
        /// <remarks>
        /// Example: <see cref="mosquitto_property_read_byte"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern MosquittoPropertyPtr mosquitto_property_read_int32(
            MosquittoPropertyPtr proplist,
            mqtt5_property identifier,
            ref uint value,
            bool skip_first);

        /// <summary>
        /// Read a varint property value from a property.
        /// </summary>
        /// <param name="proplist">property to read</param>
        /// <param name="identifier">property identifier (e.g. MQTT_PROP_PAYLOAD_FORMAT_INDICATOR)</param>
        /// <param name="value">pointer to store the value, or NULL if the value is not required.</param>
        /// <param name="skip_first">boolean that indicates whether the first item in the list should be ignored or not. Should usually be set to false.</param>
        /// <returns>
        /// <para>
        /// 	A valid property pointer if the property is found
        /// </para>
        /// <para>
        /// 	NULL, if the property is not found, or proplist is NULL.
        /// </para>
        /// </returns>
        /// <remarks>
        /// Example: <see cref="mosquitto_property_read_byte"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern MosquittoPropertyPtr mosquitto_property_read_varint(
            MosquittoPropertyPtr proplist,
            mqtt5_property identifier,
            ref uint value,
            bool skip_first);

        /// <summary>
        /// <para>
        /// Read a binary property value from a property.
        /// </para>
        /// <para>
        /// On success, value must be free()'d by the application.
        /// </para>
        /// </summary>
        /// <param name="proplist">property to read</param>
        /// <param name="identifier">property identifier (e.g. MQTT_PROP_PAYLOAD_FORMAT_INDICATOR)</param>
        /// <param name="value">pointer to store the value, or NULL if the value is not required.</param>
        /// <param name="skip_first">boolean that indicates whether the first item in the list should be ignored or not. Should usually be set to false.</param>
        /// <returns>
        /// <para>
        /// 	A valid property pointer if the property is found
        /// </para>
        /// <para>
        /// 	NULL, if the property is not found, or proplist is NULL, or if an out of memory condition occurred.
        /// </para>
        /// </returns>
        /// <remarks>
        /// Example: <see cref="mosquitto_property_read_byte"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern MosquittoPropertyPtr mosquitto_property_read_binary(
            MosquittoPropertyPtr proplist,
            mqtt5_property identifier,
            ref IntPtr value,
            ref ushort len,
            bool skip_first);

        /// <summary>
        /// <para>
        /// Read a string property value from a property.
        /// </para>
        /// <para>
        /// On success, value must be free()'d by the application.
        /// </para>
        /// </summary>
        /// <param name="proplist">property to read</param>
        /// <param name="identifier">property identifier (e.g. MQTT_PROP_PAYLOAD_FORMAT_INDICATOR)</param>
        /// <param name="value">pointer to char *, for the property data to be stored in, or NULL if the value is not required.</param>
        /// <param name="skip_first">boolean that indicates whether the first item in the list should be ignored or not. Should usually be set to false.</param>
        /// <returns>
        /// <para>
        /// 	A valid property pointer if the property is found
        /// </para>
        /// <para>
        /// 	NULL, if the property is not found, or proplist is NULL, or if an out of memory condition occurred.
        /// </para>
        /// </returns>
        /// <remarks>
        /// Example: <see cref="mosquitto_property_read_byte"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern MosquittoPropertyPtr mosquitto_property_read_string(
            MosquittoPropertyPtr proplist,
            mqtt5_property identifier,
            ref IntPtr value,
            bool skip_first);

        /// <summary>
        /// <para>
        /// Read a string pair property value pair from a property.
        /// </para>
        /// <para>
        /// On success, name and value must be free()'d by the application.
        /// </para>
        /// </summary>
        /// <param name="proplist">property to read</param>
        /// <param name="identifier">property identifier (e.g. MQTT_PROP_PAYLOAD_FORMAT_INDICATOR)</param>
        /// <param name="name">pointer to char* for the name property data to be stored in, or NULL if the name is not required.</param>
        /// <param name="value">pointer to char*, for the property data to be stored in, or NULL if the value is not required.</param>
        /// <param name="skip_first">boolean that indicates whether the first item in the list should be ignored or not. Should usually be set to false.</param>
        /// <returns>
        /// <para>
        /// 	A valid property pointer if the property is found
        /// </para>
        /// <para>
        /// 	NULL, if the property is not found, or proplist is NULL, or if an out of memory condition occurred.
        /// </para>
        /// </returns>
        /// <remarks>
        /// Example: <see cref="mosquitto_property_read_byte"/>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern MosquittoPropertyPtr mosquitto_property_read_string_pair(
            MosquittoPropertyPtr proplist,
            mqtt5_property identifier,
            ref IntPtr name,
            ref IntPtr value,
            bool skip_first);

        /// <summary>
        /// Free all properties from a list of properties. Frees the list and sets *properties to NULL.
        /// </summary>
        /// <param name="properties">list of properties to free</param>
        /// <remarks>Example: 
        /// <code><![CDATA[
        /// mosquitto_properties *properties = NULL;
        /// // Add properties
        /// mosquitto_property_free_all(&properties);
        /// ]]></code>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mosquitto_property_free_all(ref MosquittoPropertyPtr properties);

        /// <summary>
        /// <param name="dest">pointer for new property list</param>
        /// <param name="src">property list</param>
        /// </summary>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on successful copy</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if dest is NULL</description></item>
        /// <item><term>MOSQ_ERR_NOMEM</term><description>on out of memory (dest will be set to NULL)</description></item>
        /// </list></returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_property_copy_all(ref MosquittoPropertyPtr dest, MosquittoPropertyPtr src);

        /// <summary>
        /// Check whether a property identifier is valid for the given command.
        /// </summary>
        /// <param name="command">MQTT command (e.g. CMD_CONNECT)</param>
        /// <param name="identifier">MQTT property (e.g. MQTT_PROP_USER_PROPERTY)</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>if the identifier is valid for command</description></item>
        /// <item><term>MOSQ_ERR_PROTOCOL</term><description>if the identifier is not valid for use with command.</description></item>
        /// </list></returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_property_check_command(int command, mqtt5_property identifier);


        /// <summary>
        /// <para>
        /// Check whether a list of properties are valid for a particular command,
        /// whether there are duplicates, and whether the values are valid where
        /// possible.
        /// </para>
        /// <para>
        /// Note that this function is used internally in the library whenever
        /// properties are passed to it, so in basic use this is not needed, but should
        /// be helpful to check property lists *before* the point of using them.
        /// </para>
        /// </summary>
        /// <param name="command">MQTT command (e.g. CMD_CONNECT)</param>
        /// <param name="properties">list of MQTT properties to check.</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>if all properties are valid</description></item>
        /// <item><term>MOSQ_ERR_DUPLICATE_PROPERTY</term><description>if a property is duplicated where it is forbidden.</description></item>
        /// <item><term>MOSQ_ERR_PROTOCOL</term><description>if any property is invalid</description></item>
        /// </list></returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t mosquitto_property_check_all(int command, MosquittoPropertyPtr properties);

        /// <summary>
        /// Return the property name as a string for a property identifier.
        /// The property name is as defined in the MQTT specification, with - as a
        /// separator, for example: payload-format-indicator.
        /// </summary>
        /// <param name="identifier">valid MQTT property identifier integer</param>
        /// <returns>
        /// <para>
        /// A const string to the property name on success
        /// </para>
        /// <para>
        /// NULL on failure
        /// </para>
        /// </returns>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        internal static extern string mosquitto_property_identifier_to_string(mqtt5_property identifier);


        /// <summary> 
        /// Parse a property name string and convert to a property identifier and data type.
        /// The property name is as defined in the MQTT specification, with - as a
        /// separator, for example: payload-format-indicator.
        /// </summary>
        /// <param name="propname">the string to parse</param>
        /// <param name="identifier">pointer to an int to receive the property identifier</param>
        /// <param name="type">pointer to an int to receive the property type</param>
        /// <returns><list type="table">
        /// <item><term>MOSQ_ERR_SUCCESS</term><description>on success</description></item>
        /// <item><term>MOSQ_ERR_INVAL</term><description>if the string does not match a property</description></item>
        /// </list></returns>
        /// <remarks>Example: 
        /// <code><![CDATA[
        /// 	mosquitto_string_to_property_info("response-topic", &id, &type);
        /// 	// id == MQTT_PROP_RESPONSE_TOPIC
        /// 	// type == MQTT_PROP_TYPE_STRING
        /// ]]></code>
        /// </remarks>
        [DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern mosq_err_t mosquitto_string_to_property_info([MarshalAs(UnmanagedType.LPStr)] string propname, ref mqtt5_property identifier, ref mqtt5_property_type type);
        #endregion


        #region OpenSSL_Crypto
        [DllImport(cryptoNativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t OPENSSL_init_crypto(ulong opts, IntPtr settings);
        #endregion

        #region OpenSSL_SSL
        [DllImport(sslNativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern mosq_err_t OPENSSL_init_ssl(ulong opts, IntPtr settings);
        #endregion

        #region Memory
        /// <summary>
        /// <para>
        /// In C# 6, the NativeMemory class exposes these methods, but in earlier versions we need a slim native library to wrap our low level memory functions.
        /// </para>
        /// <para>
        /// native_free calls the C function 'free'
        /// </para>
        /// </summary>
        /// <param name="ptr"></param>
        [DllImport(memoryNativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void native_free(IntPtr ptr);

        /// <summary>
        /// <para>
        /// In C# 6, the NativeMemory class exposes these methods, but in earlier versions we need a slim native library to wrap our low level memory functions.
        /// </para>
        /// <para>
        /// native_malloc calls the C function 'malloc'. Allocates 'size' bytes worth of memory and returns a pointer to the allocated memory.
        /// </para>
        /// </summary>
        /// <param name="size">Size of memory to allocate</param>
        /// <returns>Pointer to the allocated memory</returns>
        [DllImport(memoryNativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr native_malloc(SizeT size);

        /// <summary>
        /// <para>
        /// In C# 6, the NativeMemory class exposes these methods, but in earlier versions we need a slim native library to wrap our low level memory functions.
        /// </para>
        /// <para>
        /// native_calloc calls the C function 'calloc'. Allocates and clears 'count' * 'size' bytes worth of memory and returns a pointer to the allocated memory.
        /// </para>
        /// </summary>
        /// <param name="int">Size of memory to allocate</param>
        /// <param name="size">Size of memory to allocate</param>
        /// <returns>Pointer to the allocated memory</returns>
        [DllImport(memoryNativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr native_calloc(SizeT count, SizeT size);

        #endregion
    }

}