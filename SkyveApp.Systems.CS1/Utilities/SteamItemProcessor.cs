using Extensions;

using SkyveApp.Domain.Steam;
using SkyveApp.Services;
using SkyveApp.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SkyveApp.Utilities;
public class SteamItemProcessor : PeriodicProcessor<ulong, SteamWorkshopInfo>
{
	public const string STEAM_CACHE_FILE = "SteamModsCache.json";

	public SteamItemProcessor() : base(200, 5000, GetCachedInfo())
	{

	}

	protected override bool CanProcess()
	{
		return ConnectionHandler.IsConnected;
	}

	protected override async Task<Dictionary<ulong, SteamWorkshopInfo>> ProcessItems(List<ulong> entities)
	{
		try
		{
			return await SteamUtil.GetWorkshopInfoAsync(entities);
		}
		finally
		{
			ServiceCenter.Get<INotifier>().OnWorkshopInfoUpdated();
		}
	}

	protected override void CacheItems(Dictionary<ulong, SteamWorkshopInfo> results)
	{
		try
		{ ISave.Save(results, STEAM_CACHE_FILE); }
		catch { }
	}

	private static Dictionary<ulong, SteamWorkshopInfo>? GetCachedInfo()
	{
		try
		{
			var path = ISave.GetPath(STEAM_CACHE_FILE);

			if (DateTime.Now - File.GetLastWriteTime(path) > TimeSpan.FromDays(7) && ConnectionHandler.IsConnected)
			{
				return null;
			}

			ISave.Load(out Dictionary<ulong, SteamWorkshopInfo>? dic, STEAM_CACHE_FILE);

			return dic;
		}
		catch { return null; }
	}
}
