using System.Collections.Generic;

namespace SkyveApp.Domain.Systems;
public interface IPlaysetManager
{
	IPlayset CurrentPlayset { get; }
	IEnumerable<IPlayset> Playsets { get; }

	string GetNewPlaysetName();
	void AddPlayset(IPlayset playset);
	void DeletePlayset(IPlayset playset);
	IPlayset? ImportPlayset(string obj);
	void ExcludeFromCurrentPlayset(IPlayset playset);
	void MergeIntoCurrentPlayset(IPlayset playset);
	void GatherInformation(IPlayset playset);
	IAsset GetAsset(ILocalPackageIdentity asset);
	IMod GetMod(ILocalPackageIdentity mod);
	bool IsPackageIncludedInPlayset(IPackage package, IPlayset playset);
	bool RenamePlayset(IPlayset playset, string newName);
	bool Save(IPlayset? playset, bool forced = false);
	void SetIncludedFor(IPackage package, IPlayset playset, bool value);
	void SetIncludedForAll(IPackage package, bool value);
	void SetCurrentPlayset(IPlayset playset);
}
