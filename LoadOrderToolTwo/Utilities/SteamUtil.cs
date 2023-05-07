using Extensions;

using LoadOrderShared;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.Domain.Steam;
using LoadOrderToolTwo.Utilities.IO;
using LoadOrderToolTwo.Utilities.Managers;

using Newtonsoft.Json;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LoadOrderToolTwo.Utilities;

public static class SteamUtil
{
	private const string STEAM_CACHE_FILE = "SteamModsCache.json";
	private const string DLC_CACHE_FILE = "SteamDlcsCache.json";
	private static readonly CSCache _csCache;
	private static readonly Dictionary<string, SteamUserEntry> _usersCache = new();

	public static List<SteamDlc> Dlcs { get; private set; }

	public static event Action? DLCsLoaded;

	static SteamUtil()
	{
		_csCache = CSCache.Deserialize();

		ISave.Load(out List<SteamDlc>? cache, DLC_CACHE_FILE);

		Dlcs = cache ?? new();
	}

	public static bool IsSteamAvailable()
	{
		return LocationManager.FileExists(LocationManager.SteamPathWithExe);
	}

	private static void SaveCache(Dictionary<ulong, SteamWorkshopItem> list)
	{
		var cache = GetCachedInfo() ?? new();

		foreach (var item in list)
		{
			cache[item.Key] = item.Value;
		}

		ISave.Save(cache, STEAM_CACHE_FILE);
	}

	internal static Dictionary<ulong, SteamWorkshopItem>? GetCachedInfo()
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

	public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunkSize)
	{
		while (source.Any())
		{
			yield return source.Take(chunkSize);
			source = source.Skip(chunkSize);
		}
	}

	public static async Task<Dictionary<T1, T2>> ConvertInChunks<T1, T2>(IEnumerable<T1> mainList, int chunkSize, Func<List<T1>, Task<Dictionary<T1, T2>>> converter)
	{
		var chunks = mainList.Chunk(chunkSize); // Split mainList into chunks of chunkSize
		var tasks = new List<Task<Dictionary<T1, T2>>>(); // A list of tasks to convert the chunks
		var results = new Dictionary<T1, T2>(); // A dictionary to store the results

		foreach (var chunk in chunks)
		{
			tasks.Add(converter(chunk.ToList())); // Convert the current chunk and add the task to the list
		}

		await Task.WhenAll(tasks); // Wait for all tasks to complete

		foreach (var task in tasks)
		{
			var chunkResults = await task; // Get the results of the completed task

			foreach (var kvp in chunkResults)
			{
				results[kvp.Key] = kvp.Value; // Add the results to the dictionary
			}
		}

		return results;
	}

	public static void ExecuteSteam(string args)
	{
		var file = LocationManager.SteamPathWithExe;

		if (LocationManager.Platform is Platform.Windows)
		{
			var process = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(file));

			if (process.Length == 0)
			{
				Notification.Create(Locale.SteamNotOpenTo, null, PromptIcons.Info, null).Show(Program.MainForm, 10);
			}
		}

		IOUtil.Execute(file, args);
	}

	public static async Task<Dictionary<string, SteamUserEntry>> GetSteamUsers(List<string> steamId64s)
	{
		var idString = string.Join(",", steamId64s);
		var url = $"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={KEYS.STEAM_API_KEY}&steamids={idString}";

		if (steamId64s.Count == 0)
			return new();

		try
		{
			using var httpClient = new HttpClient();
			var httpResponse = await httpClient.GetAsync(url);

			if (httpResponse.IsSuccessStatusCode)
			{
				var response = await httpResponse.Content.ReadAsStringAsync();

				return JsonConvert.DeserializeObject<SteamUserRootResponse>(response)?.response.players.ToDictionary(x => x.steamid) ?? new();
			}

			Log.Error("failed to get steam author data: " + httpResponse.RequestMessage);
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "Failed to get steam author information");
		}

		return new();
	}

	public static async Task<Dictionary<ulong, SteamWorkshopItem>> GetWorkshopInfoAsync(ulong[] ids)
	{
		await QueryFilesAsync(SteamQueryOrder.RankedByLastUpdatedDate, requiredTags: new[] { "Mod" });

		var results = await ConvertInChunks(ids, 200, GetWorkshopInfoImplementationAsync);

		SaveCache(results);

		return results;
	}

	private static async Task<Dictionary<ulong, SteamWorkshopItem>> GetWorkshopInfoImplementationAsync(List<ulong> ids)
	{
		var url = $"https://api.steampowered.com/IPublishedFileService/GetDetails/v1/?key={KEYS.STEAM_API_KEY}&includetags=true&includechildren=true&includevotes=true";

		ids.Remove(0);

		if (ids.Count == 0)
			return new();

		for (var i = 0; i < ids.Count; i++)
		{
			url += $"&publishedfileids[{i}]={ids[i]}";
		}

		try
		{
			using var httpClient = new HttpClient();
			var httpResponse = await httpClient.GetAsync(url);

			return (await ConvertPublishedFileResponse(httpResponse)).Item2;
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "Failed to get steam information");
		}

		return new();
	}

	public static async Task<Dictionary<ulong, SteamWorkshopItem>> QueryFilesAsync(SteamQueryOrder order, string? query = null, string[]? requiredTags = null, string[]? excludedTags = null, (DateTime, DateTime)? dateRange = null, bool all = false)
	{
		var url = $"https://api.steampowered.com/IPublishedFileService/QueryFiles/v1/?key={KEYS.STEAM_API_KEY}&query_type={(int)order}&numperpage=100&creator_appid=255710&appid=255710&match_all_tags=true&return_tags=true&return_children=true&return_details=true";

		if (!string.IsNullOrWhiteSpace(query))
		{
			url += $"&search_text={System.Net.WebUtility.UrlEncode(query)}";
		}

		if (requiredTags?.Any() ?? false)
		{
			for (var i = 0; i < requiredTags.Length; i++)
			{
				url += $"&requiredtags%5B{i}%5D={System.Net.WebUtility.UrlEncode(requiredTags[i])}";
			}
		}

		if (excludedTags?.Any() ?? false)
		{
			for (var i = 0; i < excludedTags.Length; i++)
			{
				url += $"&excludedtags%5B{i}%5D={System.Net.WebUtility.UrlEncode(excludedTags[i])}";
			}
		}

		if (dateRange is not null)
		{
			url += $"&date_range_updated%5B0%5D={dateRange?.Item1}&date_range_updated%5B1%5D={dateRange?.Item2}";
		}

		try
		{
			using var httpClient = new HttpClient();

			if (all)
			{
				var data = new Dictionary<ulong, SteamWorkshopItem>();
				var page = 1;

				while (true)
				{
					var response = await httpClient.GetAsync(url + "&page=" + page);
					var newData = await ConvertPublishedFileResponse(response);

					data.AddRange(newData.Item2);

					if (data.Count == newData.Item1)
						return data;

					page++;
				}
			}

			var httpResponse = await httpClient.GetAsync(url + "&page=0");

			return (await ConvertPublishedFileResponse(httpResponse)).Item2;
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "Failed to get steam information");
		}

		return new();
	}

	private static async Task<(int, Dictionary<ulong, SteamWorkshopItem>)> ConvertPublishedFileResponse(HttpResponseMessage httpResponse)
	{
		if (httpResponse.IsSuccessStatusCode)
		{
			var response = await httpResponse.Content.ReadAsStringAsync();

			var info = JsonConvert.DeserializeObject<SteamFileServiceInfo>(response);

			var data= info?.response?.publishedfiledetails
				.Where(x => x.result is 1 or 17 or 86 or 9)
				.Select(x => new SteamWorkshopItem(x))
				.ToList() ?? new();

			var users = await ConvertInChunks(data.Select(x => x.AuthorID ?? string.Empty).Distinct().Where(x => !_usersCache.ContainsKey(x)), 100, GetSteamUsers);

			foreach (var item in users)
			{
				_usersCache[item.Key] = item.Value;
			}

			foreach (var item in data)
			{
				if (!string.IsNullOrEmpty(item.AuthorID) && _usersCache.ContainsKey(item.AuthorID!))
				{
					item.Author = new(_usersCache[item.AuthorID!]);
				}
			}

			return (info?.response?.total ?? 0, data.ToDictionary(x => ulong.Parse(x.PublishedFileID)));
		}

		Log.Error("failed to get steam data: " + httpResponse.RequestMessage);

		return (0, new());
	}

	public static async Task<Dictionary<string, SteamAppInfo>> GetSteamAppInfoAsync(uint steamId)
	{
		var url = $"https://store.steampowered.com/api/appdetails?appids={steamId}";

		try
		{
			using var httpClient = new HttpClient();
			var httpResponse = await httpClient.GetAsync(url);

			if (httpResponse.IsSuccessStatusCode)
			{
				var response = await httpResponse.Content.ReadAsStringAsync();

				return JsonConvert.DeserializeObject<Dictionary<string, SteamAppInfo>>(response);
			}
		}
		catch (Exception ex) { Log.Exception(ex, "Failed to get the steam information for appid " + steamId); }

		return new();
	}

	public static void ReDownload(params IPackage[] packages)
	{
		var currentPath = IOUtil.ToRealPath(Path.GetDirectoryName(Program.CurrentDirectory));

		if (packages.Any(x => x.Folder.PathEquals(currentPath)))
		{
			if (MessagePrompt.Show(Locale.LOTWillRestart, PromptButtons.OKCancel, PromptIcons.Info, Program.MainForm) == DialogResult.Cancel)
			{
				return;
			}

			IOUtil.WaitForUpdate();

			Application.Exit();
		}

		ReDownload(packages.Select(x => x.SteamId).ToArray());
	}

	public static void ReDownload(params ulong[] ids)
	{
		try
		{
			var steamArguments = new StringBuilder("steam://open/console");

			if (LocationManager.Platform is not Platform.Windows)
			{
				steamArguments.Append(" \"");
			}

			for (var i = 0; i < ids.Length; i++)
			{
				steamArguments.AppendFormat(" +workshop_download_item 255710 {0}", ids[i]);
			}

			if (LocationManager.Platform is not Platform.Windows)
			{
				steamArguments.Append("\"");
			}

			ExecuteSteam(steamArguments.ToString());

			Thread.Sleep(150);

			ExecuteSteam("steam://open/downloads");
		}
		catch (Exception) { }
	}

	public static bool IsDlcInstalledLocally(uint dlcId)
	{
		return _csCache?.Dlcs?.Contains(dlcId) ?? false;
	}

	public static void SetSteamInformation(this Package package, SteamWorkshopItem steamWorkshopItem, bool cache)
	{
		package.WorkshopInfo = steamWorkshopItem;

		if (!cache)
		{
			package.Status = ModsUtil.GetStatus(package, out var reason);
			package.StatusReason = reason;

			CentralManager.InformationUpdate(package);
		}
	}

	internal static void LoadDlcs()
	{
		new BackgroundAction("Loading DLCs", async () =>
		{
			Log.Info($"Loading DLCs..");

			var dlcs = await GetSteamAppInfoAsync(255710);

			if (!dlcs.ContainsKey("255710"))
			{
				Log.Info($"Failed to load DLCs, steam info returned invalid content..");
				return;
			}

			var newDlcs = new List<SteamDlc>(Dlcs);

			foreach (var dlc in dlcs["255710"].data.dlc.Where(x => !Dlcs.Any(y => y.Id == x)))
			{
				var data = await GetSteamAppInfoAsync(dlc);

				if (data.ContainsKey(dlc.ToString()))
				{
					var info = data[dlc.ToString()].data;

					newDlcs.Add(new SteamDlc
					{
						Id = dlc,
						Name = info.name,
						Description = info.short_description,
						ReleaseDate = DateTime.TryParseExact(info.release_date?.date, "dd MMM, yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt) ? dt : DateTime.MinValue
					});
				}
			}

			Log.Info($"DLCs ({newDlcs.Count}) loaded..");

			ISave.Save(Dlcs = newDlcs, DLC_CACHE_FILE);

			DLCsLoaded?.Invoke();

			AssetsUtil.SetAvailableDlcs(Dlcs.Select(x => x.Id));

			foreach (var dlc in Dlcs)
			{
				await ImageManager.Ensure(dlc.ThumbnailUrl, false, $"{dlc.Id}.png", false);

				DLCsLoaded?.Invoke();
			}
		}).Run();
	}

	public static int GetScore(IPackage package)
	{
		var upvotes = package.PositiveVotes;
		var downvotes = package.NegativeVotes / 10 + package.Reports;

		if (upvotes + downvotes < 5)
		{
			return -1;
		}

		var subscribersFactor = -Math.Min(100000, package.Subscriptions) / 2000 - 10;
		var goal = 1.472 * (Math.Pow(subscribersFactor, 2) + Math.Pow(subscribersFactor, 3) / 100) - 120;

		if (!package.IsMod)
			goal /= 3.5;

		return ((int)(100 * (upvotes - downvotes) / goal)).Between(0, 100);
	}

	public static ulong GetLoggedInSteamId()
	{
		try
		{
			using var steam = new SteamBridge();

			return steam.GetSteamId();
		}
		catch { return 0; }
	}
}