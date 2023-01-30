namespace Mosquitto
{
    /// <summary>
    /// See http://docs.oasis-open.org/mqtt/mqtt/v3.1.1/mqtt-v3.1.1.html
    /// Section 3.2.2.3
    /// 
    /// In order to differentiate from errors, we add 128 to the response
    /// codes returned from the server
    /// </summary>
    public enum ConnectFailedReason
    {
        ConnectionAccepted = 0,
        Unspecified = 128, // This is not in the mosquitto spec, added for convenience
        UnacceptableProtocol = Unspecified + 1,
        IdentifierRejected = Unspecified + 2,
        ServerUnavailable = Unspecified + 3,
        BadUserNameOrPassword = Unspecified + 4,
        NotAuthorized = Unspecified + 5
    }
}
