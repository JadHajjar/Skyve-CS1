using System;
using SkyveApp.Domain.Compatibility.Enums;

namespace SkyveApp.Domain.Compatibility;

public interface IPackageStatus<TType> : IGenericPackageStatus where TType : struct, Enum
{
	TType Type { get; set; }
}

public interface IGenericPackageStatus
{
	StatusAction Action { get; set; }
	ulong[]? Packages { get; set; }
	string? Note { get; set; }
#if !API
	NotificationType Notification { get; }
	int IntType { get; set; }
#endif
}
