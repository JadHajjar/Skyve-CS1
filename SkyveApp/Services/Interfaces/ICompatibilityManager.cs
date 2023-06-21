using SkyveApp.Domain;
using SkyveApp.Domain.Compatibility;
using SkyveApp.Domain.Compatibility.Api;
using SkyveApp.Domain.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Services.Interfaces;
public interface ICompatibilityManager
{
	IndexedCompatibilityData CompatibilityData { get; }
	bool FirstLoadComplete { get; set; }
	Author User { get; }

	event Action? ReportProcessed;

	void CacheReport();
	void DoFirstCache(List<Package> packages);
	void DownloadData();
	CompatibilityInfo GetCompatibilityInfo(IPackage package, bool noCache = false);
	bool IsBlacklisted(IPackage package);
	bool IsSnoozed(ReportItem reportItem);
	void LoadCachedData();
	void ResetCache();
	void ResetSnoozes();
	void ToggleSnoozed(ReportItem reportItem);
}
