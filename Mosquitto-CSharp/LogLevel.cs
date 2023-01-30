
using System;

namespace Mosquitto
{
    [Flags]
    public enum LogLevel : uint
    {
        None = Native.MOSQ_LOG_NONE,
        Info = Native.MOSQ_LOG_INFO,
        Notice = Native.MOSQ_LOG_NOTICE,
        Warning = Native.MOSQ_LOG_WARNING,
        Error = Native.MOSQ_LOG_ERR,
        Debug = Native.MOSQ_LOG_DEBUG,
        Subscribe = Native.MOSQ_LOG_SUBSCRIBE,
        Unsubscribe = Native.MOSQ_LOG_UNSUBSCRIBE,
        Websockets = Native.MOSQ_LOG_WEBSOCKETS,
        Internal = Native.MOSQ_LOG_INTERNAL,
        All = Native.MOSQ_LOG_ALL
    }
}