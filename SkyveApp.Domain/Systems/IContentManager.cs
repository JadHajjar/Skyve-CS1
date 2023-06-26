using System.Collections.Generic;

namespace SkyveApp.Domain.Systems;
public interface IContentManager
{
	IEnumerable<IAsset> Assets { get; }
	IEnumerable<IMod> Mods { get; }
	IEnumerable<ILocalPackage> Packages { get; }

	ILocalPackage? GetPackageById(ulong id);
	void AddPackage(ILocalPackage package);
	void HandleNewPackage(ILocalPackage package);
	void RemovePackage(ILocalPackage package);
	void SetPackages(List<ILocalPackage> content);
	void DeleteAll(IEnumerable<ulong> ids);
	void DeleteAll(string folder);
	void MoveToLocalFolder(IPackage package);
	bool IsDlcAvailable(uint dlcId);
}
