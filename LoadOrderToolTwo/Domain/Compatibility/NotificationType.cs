namespace LoadOrderToolTwo.Domain.Compatibility;

public enum NotificationType // second hex is the id group of the notification, 0 means no notification | first hex is an id
{
	None = 0x00,
	Info = 0x10,

	MissingDependency = 0x01,
	Caution = 0x11,
	Warning = 0x21,

	AttentionRequired = 0x02,
	Unsubscribe = 0x12,
	Switch = 0x22,
}
