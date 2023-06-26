using SkyveApp.Domain.Enums;

using System.Collections.Generic;

namespace SkyveApp.Domain.Systems;

public interface IContentUtilTemp
{
	List<ILocalPackage> LoadContents();
	void ContentUpdated(string path, bool builtIn, bool workshop, bool self);
	IEnumerable<ILocalPackage> GetReferencingPackage(ulong steamId, bool includedOnly);
	void RefreshPackage(ILocalPackage package, bool self);
	DownloadStatus GetStatus(IPackage mod, out string reason);
	void StartListeners();
}
