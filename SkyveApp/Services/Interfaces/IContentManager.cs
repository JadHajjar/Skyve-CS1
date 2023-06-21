using SkyveApp.Domain;
using SkyveApp.Domain.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Services.Interfaces;
public interface IContentManager
{
	IEnumerable<Asset> Assets { get; }
	bool IsContentLoaded { get; }
	IEnumerable<Mod> Mods { get; }
	IEnumerable<Package> Packages { get; }

	event Action? ContentLoaded;
	event Action? PackageInformationUpdated;
	event Action? PackageInclusionUpdated;

	void AddPackage(Package package);
	void AnalyzePackages(List<Package> content);
	Package? GetPackage(ulong steamId);
	void HandleNewPackage(Package package);
	void OnContentLoaded();
	void OnInclusionUpdated();
	void OnInformationUpdated();
	void RemovePackage(Package package);
}
