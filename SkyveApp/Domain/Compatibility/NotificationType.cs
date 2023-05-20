namespace SkyveApp.Domain.Compatibility;

public enum NotificationType
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
