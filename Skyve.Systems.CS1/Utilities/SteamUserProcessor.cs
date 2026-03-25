using Extensions;

using Skyve.Domain;
using Skyve.Domain.CS1.Steam;
using Skyve.Domain.Systems;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skyve.Systems.CS1.Utilities;
internal class SteamUserProcessor : PeriodicProcessor<ulong, SteamUser>
{
	public const string STEAM_USER_CACHE_FILE = "SteamUsersCache.json";
	private readonly SaveHandler _saveHandler;

	public SteamUserProcessor(SaveHandler saveHandler) : base(200, 5000, 0, GetCachedInfo(saveHandler))
	{
		_saveHandler= saveHandler;
		MaxCacheTime = TimeSpan.FromDays(7);
	}

	protected override bool CanProcess()
	{
		return ConnectionHandler.IsConnected;
	}

	protected override async Task<(ConcurrentDictionary<ulong, SteamUser>, bool)> ProcessItems(List<ulong> entities)
	{
		var failed = false;

		try
		{
			var results = await SteamUtil.GetSteamUsersAsync(entities);

			failed = results.Count == 0;

			return (new(results), failed);
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

	protected override void CacheItems(ConcurrentDictionary<ulong, SteamUser> results)
	{
		try
		{
			_saveHandler.Save(results, STEAM_USER_CACHE_FILE);
		}
		catch { }
	}

	private static ConcurrentDictionary<ulong, SteamUser>? GetCachedInfo(SaveHandler saveHandler)
	{
		try
		{
			var path = saveHandler.GetPath(STEAM_USER_CACHE_FILE);

			saveHandler.Load(out ConcurrentDictionary<ulong, SteamUser>? dic, STEAM_USER_CACHE_FILE);

			return dic;
		}
		catch
		{
			return null;
		}
	}
}
