namespace Mosquitto
{
    public enum PublishFailedReasonV5
	{
		Success = 0,
		Unspecified = 128,
		ImplementationSpecific = 131,
		NotAuthorized = 135,
		TopicNameInvalid = 144,
		PacketIdInUse = 145,
		PacketTooLarge = 149,
		QuotaExceeded = 151,
    }
}
