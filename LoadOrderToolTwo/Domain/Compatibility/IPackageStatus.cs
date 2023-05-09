using System;

namespace LoadOrderToolTwo.Domain.Compatibility;

public interface IPackageStatus<TType> : IGenericPackageStatus where TType : struct, Enum
{
	TType Type { get; } 
}

public interface IGenericPackageStatus 
{
	InteractionAction Action { get; }
	ulong[]? Packages { get; }
	string? Note { get; }
#if !API
	NotificationType Notification { get; }
#endif
}
