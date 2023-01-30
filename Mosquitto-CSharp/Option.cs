
namespace Mosquitto
{
    /// <summary> 
    /// Client options.
    /// See <see cref="ClientBase.SetOption"/>.
    /// </summary>
    public enum Option
    {
        ProtocolVersion = Native.mosq_opt_t.MOSQ_OPT_PROTOCOL_VERSION,
        SslCtx = Native.mosq_opt_t.MOSQ_OPT_SSL_CTX,
        SslCtxWithDefaults = Native.mosq_opt_t.MOSQ_OPT_SSL_CTX_WITH_DEFAULTS,
        ReceiveMaximum = Native.mosq_opt_t.MOSQ_OPT_RECEIVE_MAXIMUM,
        SendMaximum = Native.mosq_opt_t.MOSQ_OPT_SEND_MAXIMUM,
        TlsKeyform = Native.mosq_opt_t.MOSQ_OPT_TLS_KEYFORM,
        TlsEngine = Native.mosq_opt_t.MOSQ_OPT_TLS_ENGINE,
        TlsEngineKpassSha1 = Native.mosq_opt_t.MOSQ_OPT_TLS_ENGINE_KPASS_SHA1,
        TlsOcspRequired = Native.mosq_opt_t.MOSQ_OPT_TLS_OCSP_REQUIRED,
        TlsAlpn = Native.mosq_opt_t.MOSQ_OPT_TLS_ALPN,
        TcpNodelay = Native.mosq_opt_t.MOSQ_OPT_TCP_NODELAY,
        BindAddress = Native.mosq_opt_t.MOSQ_OPT_BIND_ADDRESS,
        TlsUseOsCerts = Native.mosq_opt_t.MOSQ_OPT_TLS_USE_OS_CERTS,
    }
}
