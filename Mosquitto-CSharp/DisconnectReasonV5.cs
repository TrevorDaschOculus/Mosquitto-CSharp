
namespace Mosquitto
{
	public enum DisconnectReasonV5
	{
		NormalDisconnection = 0,
		DisconnectWithWillMsg = 4,

		Unspecified = 128,
		MalformedPacket = 129,
		ProtocolError = 130,
		ImplementationSpecific = 131,
		NotAuthorized = 135,
		ServerBusy = 137,
		ServerShuttingDown = 139,
		KeepAliveTimeout = 141,
		SessionTakenOver = 142,
		TopicFilterInvalid = 143,
		TopicNameInvalid = 144,
		ReceiveMaximumExceeded = 147,
		TopicAliasInvalid = 148,
		PacketTooLarge = 149,
		MessageRateTooHigh = 150,
		QuotaExceeded = 151,
		AdministrativeAction = 152,
		PayloadFormatInvalid = 153,
		RetainNotSupported = 154,
		QosNotSupported = 155,
		UseAnotherServer = 156,
		ServerMoved = 157,
		SharedSubsNotSupported = 158,
		ConnectionRateExceeded = 159,
		MaximumConnectTime = 160,
		SubscriptionIdsNotSupported = 161,
		WildcardSubsNotSupported = 162,
	}
}