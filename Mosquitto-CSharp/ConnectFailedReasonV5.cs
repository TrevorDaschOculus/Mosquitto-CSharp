
namespace Mosquitto
{
    public enum ConnectFailedReasonV5
    {
		Success = 0,
		Unspecified = 128,
		MalformedPacket = 129,
		ImplementationSpecific = 131,
		UnsupportedProtocolVersion = 132,
		ClientidNotValid = 133,
		BadUsernameOrPassword = 134,
		NotAuthorized = 135,
		ServerUnavailable = 136,
		ServerBusy = 137,
		Banned = 138,
		BadAuthenticationMethod = 140,
		TopicNameInvalid = 144,
		PacketTooLarge = 149,
		PayloadFormatInvalid = 153,
		RetainNotSupported = 154,
		QosNotSupported = 155,
		UseAnotherServer = 156,
		ServerMoved = 157,
		ConnectionRateExceeded = 159,
	}
}