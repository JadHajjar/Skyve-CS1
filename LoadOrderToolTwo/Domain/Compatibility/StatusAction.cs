namespace LoadOrderToolTwo.Domain.Compatibility;

public enum StatusAction
{
	[CRN(NotificationType.None)]
	NoAction = 0,

	[CRN(NotificationType.None)]
	SubscribeToPackages = 1,

	[CRN(NotificationType.AttentionRequired)]
	RequiresConfiguration = 2,

	[CRN(NotificationType.None)]
	SelectOne = 3,

	[CRN(NotificationType.Unsubscribe)]
	UnsubscribeThis = 4,

	[CRN(NotificationType.Unsubscribe)]
	UnsubscribeOther = 5,

	[CRN(NotificationType.Switch)]
	Switch = 6,

	[CRN(NotificationType.AttentionRequired)]
	ExcludeThis = 7,

	[CRN(NotificationType.AttentionRequired)]
	ExcludeOther = 8,
}
