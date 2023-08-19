using Extensions;

using Skyve.Domain;
using Skyve.Domain.Systems;
using Skyve.Systems.CS1.Utilities;

using SkyveShared;

using System.Collections.Generic;
using System.Linq;

namespace Skyve.Systems.CS1.Systems;
internal class VersionUpdateService : IVersionUpdateService
{
	private readonly ISettings _settings;

	public VersionUpdateService(ISettings settings)
	{
		_settings = settings;
	}

	public void Run(List<ILocalPackageWithContents> content)
	{
		if (_settings.SessionSettings.LastVersioningNumber < 1)
		{
			var oldConfig = SkyveConfigOld.Deserialize();

			if (oldConfig is null)
			{
				return;
			}

			ServiceCenter.Get<IAssetUtil, AssetsUtil>()
				.SetExcludedAssets(oldConfig.Assets.Select(x => x.Path ?? string.Empty).WhereNotEmpty());

			ServiceCenter.Get<IDlcManager>()
				.SetExcludedDlcs(oldConfig.RemovedDLCs);

			var excludedPackages = new List<IMod>();

			foreach (var item in content)
			{
				if (CrossIO.FileExists(CrossIO.Combine(item.Folder, ".excluded")))
				{
					try
					{ CrossIO.DeleteFile(CrossIO.Combine(item.Folder, ".excluded"), true); }
					catch { }

					if (item.Mod is not null)
					{
						excludedPackages.Add(item.Mod);
					}
				}
			}

			ServiceCenter.Get<IBulkUtil>().SetBulkIncluded(excludedPackages, false);

			SteamUtil.ClearCache();

			_settings.SessionSettings.LastVersioningNumber = 1;
			_settings.SessionSettings.Save();
		}
	}
}
