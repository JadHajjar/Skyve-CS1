using System.Collections.Generic;

namespace SkyveApp.Domain.Systems;
public interface IModUtil
{
	bool IsIncluded(IMod mod);
	bool IsEnabled(IMod mod);
	void SetIncluded(IMod mod, bool value);
	void SetEnabled(IMod mod, bool value);
	void SaveChanges();
	IMod GetModByFolder(string? folder);
	IEnumerable<IMod> GetMod(ILocalPackage package);
}
