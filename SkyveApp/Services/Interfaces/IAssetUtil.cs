using SkyveApp.Domain;
using SkyveApp.Domain.Interfaces;

using SkyveShared;

using System.Collections.Generic;

namespace SkyveApp.Services.Interfaces;

public interface IAssetUtil
{
	Dictionary<string, CSCache.Asset> AssetInfoCache { get; }
	HashSet<string> ExcludedHashSet { get; }

	void BuildAssetIndex();
	IEnumerable<string> GetAllFindItTags();
	Asset GetAsset(string v);
	IEnumerable<Asset> GetAssets(Package package, bool withSubDirectories = true);
	IEnumerable<string> GetFindItTags(IPackage package);
	bool IsDlcExcluded(uint dlc);
	bool IsIncluded(Asset asset);
	void RemoveFindItTag(Asset asset, string tag);
	void SaveChanges();
	void SetAvailableDlcs(IEnumerable<uint> dlcs);
	void SetDlcExcluded(uint dlc, bool excluded);
	void SetDlcsExcluded(uint[] dlc);
	void SetFindItTag(IPackage package, string tag);
	void SetIncluded(Asset asset, bool value);
}
