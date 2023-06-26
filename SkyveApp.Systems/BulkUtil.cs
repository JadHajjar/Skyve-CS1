using SkyveApp.Domain;
using SkyveApp.Domain.Systems;

using System.Collections.Generic;
using System.Linq;

namespace SkyveApp.Systems;
internal class BulkUtil : IBulkUtil
{
	private readonly INotifier _notifier;
	private readonly IModUtil _modUtil;
	private readonly IAssetUtil _assetUtil;

	public BulkUtil(INotifier notifier, IModUtil modUtil, IAssetUtil assetUtil)
	{
		_notifier = notifier;
		_modUtil = modUtil;
		_assetUtil = assetUtil;
	}

	public void SetBulkIncluded(IEnumerable<ILocalPackage> packages, bool value)
	{
		var packageList = packages.ToList();

		if (packageList.Count == 0)
		{
			return;
		}

		if (packageList[0] is ILocalPackageWithContents)
		{
			packageList = packageList.SelectMany(getPackageContents).ToList();
		}

		if (packageList.Count == 0)
		{
			return;
		}

		_notifier.BulkUpdating = true;

		foreach (var package in packageList)
		{
			if (package is IMod mod)
			{
				_modUtil.SetIncluded(mod, value);
			}
			else if (package is IAsset asset)
			{
				_assetUtil.SetIncluded(asset, value);
			}
		}

		_notifier.BulkUpdating = false;

		_notifier.OnInformationUpdated();
		_modUtil.SaveChanges();
		_assetUtil.SaveChanges();
		_notifier.TriggerAutoSave();

		static IEnumerable<ILocalPackage> getPackageContents(ILocalPackage package)
		{
			var packageContainer = (package as ILocalPackageWithContents)!;

			if (packageContainer.Mod is not null)
			{
				yield return packageContainer.Mod;
			}

			for (var i = 0; i < packageContainer.Assets.Length; i++)
			{
				yield return packageContainer.Assets[i];
			}
		}
	}

	public void SetBulkEnabled(IEnumerable<IMod> mods, bool value)
	{
		_notifier.BulkUpdating = true;

		var modList = mods.ToList();

		if (modList.Count == 0)
		{
			_notifier.BulkUpdating = false;
			return;
		}

		for (var i = 1; i < modList.Count; i++)
		{
			_modUtil.SetEnabled(modList[i], value);
		}

		_notifier.BulkUpdating = false;

		_modUtil.SetEnabled(modList[0], value);
	}
}
