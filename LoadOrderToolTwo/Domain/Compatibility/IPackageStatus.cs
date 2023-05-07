namespace LoadOrderToolTwo.Domain.Compatibility;

public interface IPackageStatus
{
	InteractionAction Action { get; }
	ulong[]? Packages { get; }
	string? Note { get; }
#if !API
	NotificationType Notification { get; }
#endif
}
