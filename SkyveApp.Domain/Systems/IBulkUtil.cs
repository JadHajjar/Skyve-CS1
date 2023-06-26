using System.Collections.Generic;

namespace SkyveApp.Domain.Systems;
public interface IBulkUtil
{
	void SetBulkEnabled(IEnumerable<IMod> mods, bool value);
	void SetBulkIncluded(IEnumerable<ILocalPackage> packages, bool value);
}
