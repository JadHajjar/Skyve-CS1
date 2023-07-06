using SkyveApp.Domain;
using SkyveApp.Domain.Systems;
using SkyveApp.Systems.CS1.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Systems.CS1.Systems;
internal class DownloadService : IDownloadService
{
	public void Download(IEnumerable<IPackageIdentity> packageIds)
	{
		SteamUtil.Download(packageIds);
	}
}
