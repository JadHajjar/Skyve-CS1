using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Domain.Systems;
public interface IBulkUtil
{
	void SetBulkEnabled(IEnumerable<IMod> mods, bool value);
	void SetBulkIncluded(IEnumerable<IPackage> packages, bool value);
}
