namespace LoadOrderToolTwo.Domain.Compatibility;

public enum StatusType
{
	[CRN(NotificationType.None, false)] None = 0,
	[CRN(NotificationType.Caution, false)] NotReviewed,
	[CRN(NotificationType.Warning)] Deprecated,
	[CRN(NotificationType.Warning)] Reupload,
	[CRN(NotificationType.Warning)] SavesCantLoadWithout,
	[CRN(NotificationType.Info)] TestVersion,
	[CRN(NotificationType.None)] DependencyMod,
	[CRN(NotificationType.None)] SourceCodeAvailable,
	[CRN(NotificationType.Warning, false)] SourceCodeNotAvailable,
	[CRN(NotificationType.Warning)] CausesIssues,
	[CRN(NotificationType.Caution, false)] MusicCanBeCopyrighted,
	[CRN(NotificationType.None)] MusicIsCopyrightFree,
	[CRN(NotificationType.Caution, false)] IncompleteDescription,
}
