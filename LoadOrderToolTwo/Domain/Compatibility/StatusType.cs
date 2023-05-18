namespace LoadOrderToolTwo.Domain.Compatibility;

public enum StatusType
{
	[CRN(NotificationType.None, false)]
	None = 0,

	[CRN(NotificationType.Caution, new[] { StatusAction.UnsubscribeThis, StatusAction.ExcludeThis, StatusAction.Switch, StatusAction.SubscribeToPackages, StatusAction.NoAction })]
	Deprecated = 1,

	[CRN(NotificationType.Warning, new[] { StatusAction.UnsubscribeThis, StatusAction.ExcludeThis, StatusAction.Switch, StatusAction.SubscribeToPackages, StatusAction.NoAction })]
	Reupload = 2,

	[CRN(NotificationType.Warning, new[] { StatusAction.UnsubscribeThis, StatusAction.ExcludeThis, StatusAction.RequiresConfiguration, StatusAction.NoAction })]
	CausesIssues = 3,

	[CRN(NotificationType.Warning, new[] { StatusAction.UnsubscribeThis, StatusAction.ExcludeThis, StatusAction.Switch, StatusAction.RequiresConfiguration, StatusAction.NoAction })]
	SavesCantLoadWithoutIt = 4,

	[CRN(NotificationType.Info, new[] { StatusAction.NoAction, StatusAction.UnsubscribeThis, StatusAction.ExcludeThis, StatusAction.Switch })]
	TestVersion = 5,

	[CRN(NotificationType.None, new[] { StatusAction.NoAction })]
	DependencyMod = 6,

	[CRN(NotificationType.Warning, false)]
	SourceCodeNotAvailable = 7,

	[CRN(NotificationType.Caution, new[] { StatusAction.NoAction })]
	MusicCanBeCopyrighted = 8,

	[CRN(NotificationType.Caution, false)]
	IncompleteDescription = 9,

	/********************************/

	[CRN(NotificationType.MissingDependency, false)]
	MissingDlc,

	[CRN(NotificationType.Unsubscribe, false)]
	Succeeded
}
