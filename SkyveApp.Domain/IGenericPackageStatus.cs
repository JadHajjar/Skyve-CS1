using SkyveApp.Domain.Enums;

namespace SkyveApp.Domain;

public interface IGenericPackageStatus
{
	StatusAction Action { get; set; }
	ulong[]? Packages { get; set; }
	string? Note { get; set; }
	NotificationType Notification { get; }
	int IntType { get; set; }
	string LocaleKey { get; }
}
