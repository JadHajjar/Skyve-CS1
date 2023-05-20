namespace SkyveApp.Domain.Compatibility;

public enum NotificationType
{
	None,
	Info,

	Caution,
	MissingDependency,
	Warning,

	AttentionRequired,
	Exclude,
	Unsubscribe,
	Switch,
}
