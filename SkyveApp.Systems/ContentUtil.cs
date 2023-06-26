using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Enums;
using SkyveApp.Domain.Systems;

using System;
using System.Collections.Generic;
using System.Linq;

namespace SkyveApp.Systems;
public class ContentUtil : IContentUtil
{
	private readonly IServiceProvider _serviceProvider;
	private readonly IModUtil _modUtil;
	private readonly IAssetUtil _assetUtil;
	private readonly IBulkUtil _bulkUtil;
	private readonly ILocale _locale;
	private readonly IContentManager _contentManager;
	private readonly IPackageUtil _packageUtil;

	public ContentUtil(IServiceProvider serviceProvider, IModUtil modUtil, IAssetUtil assetUtil, IBulkUtil bulkUtil, ILocale locale, IPackageUtil packageUtil, IContentManager contentManager)
	{
		_serviceProvider = serviceProvider;
		_modUtil = modUtil;
		_assetUtil = assetUtil;
		_bulkUtil = bulkUtil;
		_locale = locale;
		_packageUtil = packageUtil;
		_contentManager = contentManager;
	}

	public bool IsIncluded(ILocalPackage localPackage)
	{
		return IsIncluded(localPackage, out _);
	}

	public bool IsIncluded(ILocalPackage localPackage, out bool partiallyIncluded)
	{
		if (localPackage is ILocalPackageWithContents packageWithContents)
		{
			var included = false;
			var excluded = false;

			if (packageWithContents.Mod is not null)
			{
				if (IsIncluded(packageWithContents.Mod, out _))
				{
					included = true;
				}
				else
				{
					excluded = true;
				}
			}

			foreach (var item in packageWithContents.Assets)
			{
				if (IsIncluded(item, out _))
				{
					included = true;
				}
				else
				{
					excluded = true;
				}

				if (included && excluded)
				{
					partiallyIncluded = true;

					return true;
				}
			}

			partiallyIncluded = false;

			return included;
		}

		partiallyIncluded = false;

		return localPackage is IMod mod ? _modUtil.IsIncluded(mod) : localPackage is IAsset asset && _assetUtil.IsIncluded(asset);
	}

	public bool IsEnabled(ILocalPackage package)
	{
		if (package is IMod mod)
		{
			return _modUtil.IsEnabled(mod);
		}

		if (package is ILocalPackageWithContents packageWithContents && packageWithContents.Mod is not null)
		{
			return _modUtil.IsEnabled(packageWithContents.Mod);
		}

		return true;
	}

	public bool IsIncludedAndEnabled(ILocalPackage package)
	{
		return IsIncluded(package) && IsEnabled(package);
	}

	public void SetIncluded(ILocalPackage localPackage, bool value)
	{
		if (localPackage is ILocalPackageWithContents)
		{
			_bulkUtil.SetBulkIncluded(new[] { localPackage }, value);
		}

		if (localPackage is IMod mod)
		{
			_modUtil.SetIncluded(mod, value);
		}

		if (localPackage is IAsset asset)
		{
			_assetUtil.SetIncluded(asset, value);
		}
	}

	public DownloadStatus GetStatus(IPackage mod, out string reason)
	{
		var workshopInfo = mod.GetWorkshopInfo();

		if (workshopInfo is null)
		{
			if (mod.LocalPackage?.IsLocal ?? false)
			{
				reason = string.Empty;
				return DownloadStatus.None;
			}

			reason = string.Empty;
			return DownloadStatus.None;
		}

		if (workshopInfo.Removed)
		{
			reason = _locale.Get("PackageIsRemoved").Format(_packageUtil.CleanName(mod));
			return DownloadStatus.Removed;
		}

		if (workshopInfo.ServerTime == default)
		{
			reason = _locale.Get("PackageIsUnknown").Format(_packageUtil.CleanName(mod));
			return DownloadStatus.Unknown;
		}

		var updatedServer = workshopInfo.ServerTime;
		var updatedLocal = mod.LocalPackage?.LocalTime ?? DateTime.MinValue;
		var sizeServer = workshopInfo.ServerSize;
		var localSize = mod.LocalPackage?.LocalSize ?? 0;

		if (updatedLocal < updatedServer)
		{
			var certain = updatedLocal < updatedServer.AddHours(-24);

			reason = certain
				? _locale.Get("PackageIsOutOfDate").Format(_packageUtil.CleanName(mod), (updatedServer - updatedLocal).ToReadableString(true))
				: _locale.Get("PackageIsMaybeOutOfDate").Format(_packageUtil.CleanName(mod), updatedServer.ToLocalTime().ToRelatedString(true));
			return DownloadStatus.OutOfDate;
		}

		if (localSize < sizeServer && sizeServer > 0)
		{
			reason = _locale.Get("PackageIsIncomplete").Format(_packageUtil.CleanName(mod), localSize.SizeString(), sizeServer.SizeString());
			return DownloadStatus.PartiallyDownloaded;
		}

		reason = string.Empty;
		return DownloadStatus.OK;
	}

	public IEnumerable<ILocalPackage> GetPackagesThatReference(IPackage package, bool withExcluded = false)
	{
		var compatibilityUtil = _serviceProvider.GetService<ICompatibilityManager>();
		var packages = withExcluded || _serviceProvider.GetService<ISettings>().UserSettings.ShowAllReferencedPackages
			? _contentManager.Packages.ToList()
			: _contentManager.Packages.AllWhere(IsIncluded);

		foreach (var localPackage in packages)
		{
			foreach (var requirement in localPackage.Requirements)
			{
				if (compatibilityUtil.GetFinalSuccessor(requirement)?.Id == package.Id)
				{
					yield return localPackage;
				}
			}
		}
	}
}
