using Extensions;
using LoadOrderToolTwo.Domain.Steam;
using LoadOrderToolTwo.Domain.Steam.Markdown;
using LoadOrderToolTwo.Utilities.Managers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadOrderToolTwo.Utilities;
internal class SteamUserProcessor : PeriodicProcessor<ulong, SteamUser>
{
	private const string STEAM_USER_CACHE_FILE = "SteamUsersCache.json";

	public SteamUserProcessor() : base(200, 5000, GetCachedInfo())
	{

	}

	protected override bool CanProcess()
	{
		return ConnectionHandler.IsConnected;
	}

	protected override async Task<Dictionary<ulong, SteamUser>> ProcessItems(List<ulong> entities)
	{
		try
		{
			return await SteamUtil.GetSteamUsersAsync(entities); 
		}
		finally
		{
			CentralManager.OnWorkshopInfoUpdated();
		}
	}

	protected override void CacheItems(Dictionary<ulong, SteamUser> results)
	{
		try
		{ ISave.Save(results, STEAM_USER_CACHE_FILE); }
		catch { }
	}

	private static Dictionary<ulong, SteamUser>? GetCachedInfo()
	{
		try
		{
			var path = ISave.GetPath(STEAM_USER_CACHE_FILE);

			if (DateTime.Now - File.GetLastWriteTime(path) > TimeSpan.FromDays(7) && ConnectionHandler.IsConnected)
			{
				return null;
			}

			ISave.Load(out Dictionary<ulong, SteamUser>? dic, STEAM_USER_CACHE_FILE);

			return dic;
		}
		catch { return null; }
	}
}
