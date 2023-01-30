using System;

namespace Mosquitto
{
    public class ConnectFailedException : Exception
    {
        public readonly ConnectFailedReason Reason;

        public ConnectFailedException(ConnectFailedReason reason)
        {
            Reason = reason;
        }

        public override string Message => $"Mosquitto ConnectFailedReason: {Reason}";
    }
}