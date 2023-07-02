using System;
using System.Collections.Generic;

namespace SkyveApp.Domain.Systems;
public interface IDlcManager
{
	event Action DlcsLoaded;

	IEnumerable<IDlcInfo> Dlcs { get; }

	bool IsDlcExcluded(uint id);
	bool IsAvailable(uint dlcId);
	void SetAvailableDlcs(IEnumerable<uint> enumerable);
	void SetDlcsExcluded(uint[] uints);
	bool IsIncluded(IDlcInfo dlc);
	void SetIncluded(IDlcInfo dlc, bool value);
}
