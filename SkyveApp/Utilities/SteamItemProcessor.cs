using Extensions;

using SkyveApp.Domain.Steam;
using SkyveApp.Utilities.Managers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SkyveApp.Utilities;
internal class SteamItemProcessor : PeriodicProcessor<ulong, SteamWorkshopItem>
{
	internal const string STEAM_CACHE_FILE = "SteamModsCache.json";

	public SteamItemProcessor() : base(200, 5000, GetCachedInfo())
	{

	}

	protected override bool CanProcess()
	{
		return ConnectionHandler.IsConnected;
	}

	protected override async Task<Dictionary<ulong, SteamWorkshopItem>> ProcessItems(List<ulong> entities)
	{
		try
		{
			return await SteamUtil.GetWorkshopInfoAsync(entities);
		}
		finally
		{
			CentralManager.OnWorkshopInfoUpdated();
		}
	}

	protected override void CacheItems(Dictionary<ulong, SteamWorkshopItem> results)
	{
		try
		{ ISave.Save(results, STEAM_CACHE_FILE); }
		catch { }
	}

	private static Dictionary<ulong, SteamWorkshopItem>? GetCachedInfo()
	{
		try
		{
			var path = ISave.GetPath(STEAM_CACHE_FILE);

			if (DateTime.Now - File.GetLastWriteTime(path) > TimeSpan.FromDays(7) && ConnectionHandler.IsConnected)
			{
				return null;
			}

			ISave.Load(out Dictionary<ulong, SteamWorkshopItem>? dic, STEAM_CACHE_FILE);

			return dic;
		}
		catch { return null; }
	}
}
