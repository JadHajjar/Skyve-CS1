namespace LoadOrderToolTwo.Domain.Compatibility;

public enum NotificationType // second hex is the id group of the notification, 0 means no notification | first hex is an id
{
	None,
	Info,

	MissingDependency,
	Caution,
	Warning,

	AttentionRequired,
	Exclude,
	Unsubscribe,
	Switch,
}
