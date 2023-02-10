﻿namespace Mosquitto
{
    public enum Error
    {
        // Custom Cancelled error code if we force disconnect while connecting
        Cancelled = -10,

        AuthContinue = Native.mosq_err_t.MOSQ_ERR_AUTH_CONTINUE,
        NoSubscribers = Native.mosq_err_t.MOSQ_ERR_NO_SUBSCRIBERS,
        SubExists = Native.mosq_err_t.MOSQ_ERR_SUB_EXISTS,
        ConnPending = Native.mosq_err_t.MOSQ_ERR_CONN_PENDING,
        Success = Native.mosq_err_t.MOSQ_ERR_SUCCESS,
        Nomem = Native.mosq_err_t.MOSQ_ERR_NOMEM,
        Protocol = Native.mosq_err_t.MOSQ_ERR_PROTOCOL,
        Inval = Native.mosq_err_t.MOSQ_ERR_INVAL,
        NoConn = Native.mosq_err_t.MOSQ_ERR_NO_CONN,
        ConnRefused = Native.mosq_err_t.MOSQ_ERR_CONN_REFUSED,
        NotFound = Native.mosq_err_t.MOSQ_ERR_NOT_FOUND,
        ConnLost = Native.mosq_err_t.MOSQ_ERR_CONN_LOST,
        Tls = Native.mosq_err_t.MOSQ_ERR_TLS,
        PayloadSize = Native.mosq_err_t.MOSQ_ERR_PAYLOAD_SIZE,
        NotSupported = Native.mosq_err_t.MOSQ_ERR_NOT_SUPPORTED,
        Auth = Native.mosq_err_t.MOSQ_ERR_AUTH,
        AclDenied = Native.mosq_err_t.MOSQ_ERR_ACL_DENIED,
        Unknown = Native.mosq_err_t.MOSQ_ERR_UNKNOWN,
        Errno = Native.mosq_err_t.MOSQ_ERR_ERRNO,
        Eai = Native.mosq_err_t.MOSQ_ERR_EAI,
        Proxy = Native.mosq_err_t.MOSQ_ERR_PROXY,
        PluginDefer = Native.mosq_err_t.MOSQ_ERR_PLUGIN_DEFER,
        MalformedUtf8 = Native.mosq_err_t.MOSQ_ERR_MALFORMED_UTF8,
        Keepalive = Native.mosq_err_t.MOSQ_ERR_KEEPALIVE,
        Lookup = Native.mosq_err_t.MOSQ_ERR_LOOKUP,
        MalformedPacket = Native.mosq_err_t.MOSQ_ERR_MALFORMED_PACKET,
        DuplicateProperty = Native.mosq_err_t.MOSQ_ERR_DUPLICATE_PROPERTY,
        TlsHandshake = Native.mosq_err_t.MOSQ_ERR_TLS_HANDSHAKE,
        QosNotSupported = Native.mosq_err_t.MOSQ_ERR_QOS_NOT_SUPPORTED,
        OversizePacket = Native.mosq_err_t.MOSQ_ERR_OVERSIZE_PACKET,
        Ocsp = Native.mosq_err_t.MOSQ_ERR_OCSP,
        Timeout = Native.mosq_err_t.MOSQ_ERR_TIMEOUT,
        RetainNotSupported = Native.mosq_err_t.MOSQ_ERR_RETAIN_NOT_SUPPORTED,
        TopicAliasInvalid = Native.mosq_err_t.MOSQ_ERR_TOPIC_ALIAS_INVALID,
        AdministrativeAction = Native.mosq_err_t.MOSQ_ERR_ADMINISTRATIVE_ACTION,
        AlreadyExists = Native.mosq_err_t.MOSQ_ERR_ALREADY_EXISTS,
    }
}