using Extensions;

using Skyve.Domain;
using Skyve.Domain.CS1.Steam;
using Skyve.Domain.CS1.Utilities;
using Skyve.Domain.Enums;
using Skyve.Domain.Systems;
using Skyve.Systems.CS1.Utilities;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skyve.Systems.CS1.Systems;
internal class WorkshopService : IWorkshopService
{
	public void CleanDownload(List<ILocalPackageWithContents> packages)
	{
		PackageWatcher.Pause();
		foreach (var item in packages)
		{
			try
			{
				CrossIO.DeleteFolder(item.Folder);
			}
			catch (Exception ex)
			{
				ServiceCenter.Get<ILogger>().Exception(ex, $"Failed to delete the folder '{item.Folder}'");
			}
		}
		PackageWatcher.Resume();

		SteamUtil.Download(packages);
	}

	public void ClearCache()
	{
		SteamUtil.ClearCache();
	}

	public IEnumerable<IWorkshopInfo> GetAllPackages()
	{
		return SteamUtil.Packages;
	}

	public IWorkshopInfo? GetInfo(IPackageIdentity identity)
	{
		return SteamUtil.GetItem(identity?.Id ?? 0);
	}

	public async Task<IWorkshopInfo?> GetInfoAsync(IPackageIdentity identity)
	{
		return await SteamUtil.GetItemAsync(identity?.Id ?? 0);
	}

	public IPackage GetPackage(IPackageIdentity identity)
	{
		var info = identity is IWorkshopInfo inf ? inf : GetInfo(identity);

		if (info is not null)
		{
			return new WorkshopPackage(info);
		}

		return new GenericWorkshopPackage(identity);
	}

	public async Task<IPackage> GetPackageAsync(IPackageIdentity identity)
	{
		var info = await GetInfoAsync(identity);

		if (info is not null)
		{
			return new WorkshopPackage(info);
		}

		return new GenericWorkshopPackage(identity);
	}

	public IUser? GetUser(object userId)
	{
		return SteamUtil.GetUser(ulong.TryParse(userId?.ToString() ?? string.Empty, out var id) ? id : 0);
	}

	public async Task<IEnumerable<IWorkshopInfo>> GetWorkshopItemsByUserAsync(object userId)
	{
		return (await SteamUtil.GetWorkshopItemsByUserAsync(ulong.TryParse(userId?.ToString() ?? string.Empty, out var id) ? id : 0, true)).Values;
	}

	public async Task<IEnumerable<IWorkshopInfo>> QueryFilesAsync(PackageSorting sorting, string? query = null, string[]? requiredTags = null, string[]? excludedTags = null, (DateTime, DateTime)? dateRange = null, bool all = false)
	{
		var steamSorting = sorting switch
		{
			PackageSorting.UpdateTime => SteamQueryOrder.RankedByLastUpdatedDate,
			_ => SteamQueryOrder.RankedByTrend
		};

		return (await SteamUtil.QueryFilesAsync(steamSorting, query, requiredTags, excludedTags, dateRange, all)).Values;
	}
}
