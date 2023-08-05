using SkyveApp.Domain;
using SkyveApp.Domain.CS1.Steam;
using SkyveApp.Domain.Enums;
using SkyveApp.Domain.Systems;
using SkyveApp.Systems.CS1.Utilities;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkyveApp.Systems.CS1.Systems;
internal class WorkshopService : IWorkshopService
{
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
		return SteamUtil.GetItem(identity.Id);
	}

	public async Task<IWorkshopInfo?> GetInfoAsync(IPackageIdentity identity)
	{
		return await SteamUtil.GetItemAsync(identity.Id);
	}

	public IPackage GetPackage(IPackageIdentity identity)
	{
		var info = GetInfo(identity);

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
