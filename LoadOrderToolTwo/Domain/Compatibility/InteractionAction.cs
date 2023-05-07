namespace LoadOrderToolTwo.Domain.Compatibility;

public enum InteractionAction
{
	[CRN(NotificationType.None)] None = 0,
	[CRN(NotificationType.AttentionRequired)] RequiresConfiguration,
	[CRN(NotificationType.AttentionRequired)] SelectOne,
	[CRN(NotificationType.Unsubscribe)] Unsubscribe,
	[CRN(NotificationType.Switch)] Switch,
}
