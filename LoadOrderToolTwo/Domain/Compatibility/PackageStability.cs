namespace LoadOrderToolTwo.Domain.Compatibility;

public enum PackageStability
{
	[CRN(NotificationType.Info, false)] NotReviewed,
	[CRN(NotificationType.None)] Stable,
	[CRN(NotificationType.Info)] NotEnoughInformation,
	[CRN(NotificationType.Warning)] HasIssues,
	[CRN(NotificationType.Unsubscribe)] Broken,
}
