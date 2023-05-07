namespace LoadOrderToolTwo.Domain.Compatibility;

public enum InteractionType
{
	[CRN(NotificationType.Switch)] Successor,
	[CRN(NotificationType.None)] Alternative,
	[CRN(NotificationType.Caution)] SameFunctionality,
	[CRN(NotificationType.AttentionRequired)] Incompatible,
	[CRN(NotificationType.Info)] RequirementAlternative,
	[CRN(NotificationType.Warning)] CausesIssues,
	[CRN(NotificationType.AttentionRequired)] Required,
}
