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
	IEnumerable<Mod> Mods { get; }
	IEnumerable<Package> Packages { get; }

	void AddPackage(Package package);
	void HandleNewPackage(Package package);
	void RemovePackage(Package package);
	Package? GetPackage(ulong steamId);
	void SetPackages(List<Package> content);
}
