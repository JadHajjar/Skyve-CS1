namespace SkyveApp.Domain.Enums;

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
