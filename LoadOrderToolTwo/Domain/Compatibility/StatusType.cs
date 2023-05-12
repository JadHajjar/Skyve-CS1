namespace LoadOrderToolTwo.Domain.Compatibility;

public enum StatusType
{
	[CRN(NotificationType.None, false)] None = 0,
	[CRN(NotificationType.Warning)] Deprecated,
	[CRN(NotificationType.Warning)] Reupload,
	[CRN(NotificationType.Warning)] CausesIssues,
	[CRN(NotificationType.Warning)] SavesCantLoadWithoutIt,
	[CRN(NotificationType.Info)] TestVersion,
	[CRN(NotificationType.None)] DependencyMod,
	[CRN(NotificationType.Warning)] SourceCodeNotAvailable,
	[CRN(NotificationType.Caution)] MusicCanBeCopyrighted,
	[CRN(NotificationType.Caution)] IncompleteDescription,
}
