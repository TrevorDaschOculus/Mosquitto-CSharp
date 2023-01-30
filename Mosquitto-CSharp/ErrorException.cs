using System;

namespace Mosquitto
{
    public class ErrorException : Exception
    {
        public readonly Error Error;

        public ErrorException(Error error)
        {
            this.Error = error;
        }

        public override string Message => $"Mosquitto Error: {Error}";
    }
}