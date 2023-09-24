using Skyve.Domain;
using Skyve.Domain.Systems;
using Skyve.Systems.CS1.Utilities;

using System.Collections.Generic;

namespace Skyve.Systems.CS1.Systems;
internal class DownloadService : IDownloadService
{
	public void Download(IEnumerable<IPackageIdentity> packageIds)
	{
		SteamUtil.Download(packageIds);
	}
}
