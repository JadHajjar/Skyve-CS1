using Extensions;

using Skyve.Domain;
using Skyve.Domain.CS1.Steam;
using Skyve.Domain.Systems;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Skyve.Systems.CS1.Utilities;

internal class SteamItemProcessor : PeriodicProcessor<ulong, SteamWorkshopInfo>
{
	public const string STEAM_CACHE_FILE = "SteamModsCache.json";
	private readonly SaveHandler _saveHandler;

	public SteamItemProcessor(SaveHandler saveHandler) : base(200, 5000, 0, GetCachedInfo(saveHandler))
	{
		_saveHandler = saveHandler;
		MaxCacheTime = TimeSpan.FromHours(1);
	}

	protected override bool CanProcess()
	{
		return ConnectionHandler.IsConnected;
	}

	protected override async Task<(ConcurrentDictionary<ulong, SteamWorkshopInfo>, bool)> ProcessItems(List<ulong> entities)
	{
		var failed = false;

		try
		{
			var results = await SteamUtil.GetWorkshopInfoAsync(entities);

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
				ServiceCenter.Get<INotifier>().OnWorkshopInfoUpdated();
			}
		}
	}

	protected override void CacheItems(ConcurrentDictionary<ulong, SteamWorkshopInfo> results)
	{
		try
		{
			_saveHandler.Save(results, STEAM_CACHE_FILE);
		}
		catch { }
	}

	private static ConcurrentDictionary<ulong, SteamWorkshopInfo>? GetCachedInfo(SaveHandler saveHandler)
	{
		try
		{
			var path = saveHandler.GetPath(STEAM_CACHE_FILE);

			if (DateTime.Now - File.GetLastWriteTime(path) > TimeSpan.FromDays(7) && ConnectionHandler.IsConnected)
			{
				return null;
			}

			saveHandler.Load(out ConcurrentDictionary<ulong, SteamWorkshopInfo>? dic, STEAM_CACHE_FILE);

			return dic;
		}
		catch
		{
			return null;
		}
	}
}
