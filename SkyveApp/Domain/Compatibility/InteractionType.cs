namespace SkyveApp.Domain.Compatibility;

public enum InteractionType
{
	[CRN(NotificationType.None, false)]
	None = 0,

	[CRN(NotificationType.Switch, new[] { StatusAction.Switch, StatusAction.NoAction })]
	Successor = 1,

	[CRN(NotificationType.None, new[] { StatusAction.SelectOne, StatusAction.Switch, StatusAction.NoAction })]
	Alternative = 2,

	[CRN(NotificationType.Caution, new[] { StatusAction.SelectOne, StatusAction.Switch, StatusAction.UnsubscribeThis, StatusAction.UnsubscribeOther, StatusAction.ExcludeThis, StatusAction.ExcludeOther, StatusAction.RequiresConfiguration, StatusAction.NoAction })]
	SameFunctionality = 3,

	[CRN(NotificationType.AttentionRequired, new[] { StatusAction.SelectOne, StatusAction.Switch, StatusAction.UnsubscribeThis, StatusAction.UnsubscribeOther, StatusAction.ExcludeThis, StatusAction.ExcludeOther, StatusAction.RequiresConfiguration, StatusAction.NoAction })]
	IncompatibleWith = 4,

	[CRN(NotificationType.Info, new[] { StatusAction.NoAction })]
	RequirementAlternative = 5,

	[CRN(NotificationType.Caution, new[] { StatusAction.SelectOne, StatusAction.Switch, StatusAction.UnsubscribeThis, StatusAction.UnsubscribeOther, StatusAction.ExcludeThis, StatusAction.ExcludeOther, StatusAction.RequiresConfiguration, StatusAction.NoAction })]
	CausesIssuesWith = 6,

	[CRN(NotificationType.MissingDependency, new[] { StatusAction.SubscribeToPackages, StatusAction.NoAction })]
	RequiredPackages = 7,

	[CRN(NotificationType.None, new[] { StatusAction.SubscribeToPackages, StatusAction.NoAction })]
	OptionalPackages = 8,

	/***********************************/

	[CRN(NotificationType.Switch, false)]
	SucceededBy,

	[CRN(NotificationType.AttentionRequired, false)]
	Identical
}
