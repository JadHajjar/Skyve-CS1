using SkyveApp.Domain;
using SkyveApp.Domain.CS1.Legacy;
using SkyveApp.Domain.Systems;
using SkyveApp.Systems.CS1.Utilities;

using SkyveShared;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Systems.CS1.Managers;
internal class DlcManager : IDlcManager
{
	private readonly DlcConfig _config;
	public IEnumerable<IDlcInfo> Dlcs => SteamUtil.Dlcs;

	public event Action? DlcsLoaded;

    public DlcManager()
	{
		_config = SkyveConfig.Deserialize() ?? new();
		SteamUtil.DLCsLoaded += DlcsLoaded;
    }

    public bool IsAvailable(uint dlcId)
	{
		return SteamUtil.IsDlcInstalledLocally(dlcId);
	}

	public bool IsIncluded(IDlcInfo dlc)
	{
		return SteamUtil.is
	}

	public void SetDlcsExcluded(uint[] dlcs)
	{
		_config.RemovedDLCs = dlcs;

		SaveChanges();
	}

	public void SetIncluded(IDlcInfo dlc, bool value)
	{

	}
}
