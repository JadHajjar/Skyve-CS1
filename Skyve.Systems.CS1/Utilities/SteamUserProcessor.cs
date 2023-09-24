using Extensions;

using Skyve.Domain;
using Skyve.Domain.CS1.Steam;
using Skyve.Domain.Systems;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skyve.Systems.CS1.Utilities;
internal class SteamUserProcessor : PeriodicProcessor<ulong, SteamUser>
{
	public const string STEAM_USER_CACHE_FILE = "SteamUsersCache.json";

	public SteamUserProcessor() : base(200, 5000, GetCachedInfo())
	{
		MaxCacheTime = TimeSpan.FromDays(7);
	}

	protected override bool CanProcess()
	{
		return ConnectionHandler.IsConnected;
	}

	protected override async Task<(Dictionary<ulong, SteamUser>, bool)> ProcessItems(List<ulong> entities)
	{
		var failed = false;

		try
		{
			var results = await SteamUtil.GetSteamUsersAsync(entities);

			failed = results.Count == 0;

			return (results, failed);
		}
		catch
		{
			failed = true;
			throw;
		}
		finally
		{
			if (!failed)
			{
				ServiceCenter.Get<INotifier>().OnWorkshopUsersInfoLoaded();
			}
		}
	}

	protected override void CacheItems(Dictionary<ulong, SteamUser> results)
	{
		try
		{
			ISave.Save(results, STEAM_USER_CACHE_FILE);
		}
		catch { }
	}

	private static Dictionary<ulong, SteamUser>? GetCachedInfo()
	{
		try
		{
			var path = ISave.GetPath(STEAM_USER_CACHE_FILE);

			ISave.Load(out Dictionary<ulong, SteamUser>? dic, STEAM_USER_CACHE_FILE);

			return dic;
		}
		catch
		{
			return null;
		}
	}
}
