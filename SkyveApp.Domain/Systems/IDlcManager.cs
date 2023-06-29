using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Domain.Systems;
public interface IDlcManager
{
	bool IsDlcExcluded(uint id);
	bool IsDlcAvailable(uint dlcId);
	void SetAvailableDlcs(IEnumerable<uint> enumerable);
}
