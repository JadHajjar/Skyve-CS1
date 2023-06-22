using System;
using System.Collections.Generic;

namespace SkyveApp.Domain.Systems;
public interface ICompatibilityManager
{
	event Action? ReportProcessed;

	ICompatibilityInfo GetCompatibilityInfo(IPackage package, bool noCache = false);
	bool IsBlacklisted(IPackage package);
	bool IsSnoozed(ICompatibilityItem reportItem);
	void LoadCachedData();
	void ResetCache();
	void ResetSnoozes();
	void ToggleSnoozed(ICompatibilityItem reportItem);
}
