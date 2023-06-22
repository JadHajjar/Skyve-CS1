using System.Collections.Generic;

namespace SkyveApp.Domain.Systems;
public interface IModUtil
{
	bool IsIncluded(IMod asset);
	bool IsEnabled(IMod asset);
	void SetIncluded(IMod asset, bool value);
	void SetEnabled(IMod asset, bool value);
	void SaveChanges();
	IMod GetModByFolder(string? folder);
	IEnumerable<IMod> GetMod(ILocalPackage package);
}
