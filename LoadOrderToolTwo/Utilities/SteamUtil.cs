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
	private const string DLC_CACHE_FILE = "SteamDlcsCache.json";
	private static readonly CSCache _csCache;

	private static readonly SteamItemProcessor _workshopItemProcessor;
	private static readonly SteamUserProcessor _steamUserProcessor;

	public static List<SteamDlc> Dlcs { get; private set; }

	public static event Action? DLCsLoaded;

	public static event Action? WorkshopItemsLoaded;
	public static event Action? SteamUsersLoaded;

	static SteamUtil()
	{
		_csCache = CSCache.Deserialize();

		ISave.Load(out List<SteamDlc>? cache, DLC_CACHE_FILE);

		Dlcs = cache ?? new();

		_workshopItemProcessor = new();
		_steamUserProcessor = new();

		_workshopItemProcessor.ItemsLoaded += _workshopItemProcessor_ItemsLoaded;
		_steamUserProcessor.ItemsLoaded += _steamUserProcessor_ItemsLoaded;
	}

	public static SteamUser? GetUser(ulong steamId)
	{
		if (steamId == 0)
		{
			return null;
		}

		return _steamUserProcessor.Get(steamId, false).Result;
	}

	public static SteamWorkshopItem? GetItem(ulong steamId)
	{
		if (steamId == 0)
		{
			return null;
		}

		return _workshopItemProcessor.Get(steamId, false).Result;
	}

	public static async Task<SteamUser?> GetUserAsync(ulong steamId)
	{
		if (steamId == 0)
		{
			return null;
		}

		return await _steamUserProcessor.Get(steamId, true);
	}

	public static async Task<SteamWorkshopItem?> GetItemAsync(ulong steamId)
	{
		if (steamId == 0)
		{
			return null;
		}

		return await _workshopItemProcessor.Get(steamId, true);
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
		//package.WorkshopInfo = steamWorkshopItem;

		//	package.Status = ModsUtil.GetStatus(package, out var reason);
		//	package.StatusReason = reason;

		//if (!cache)
		//		CentralManager.InformationUpdate(package);
		
	}

	public static int GetScore(IPackage package)
	{
		var upvotes = package.PositiveVotes;
		var downvotes = (package.NegativeVotes / 10) + package.Reports;

		if (upvotes + downvotes < 5)
		{
			return -1;
		}

		var subscribersFactor = (-Math.Min(100000, package.Subscriptions) / 2000) - 10;
		var goal = (1.472 * (Math.Pow(subscribersFactor, 2) + (Math.Pow(subscribersFactor, 3) / 100))) - 120;

		if (!package.IsMod)
		{
			goal /= 3.5;
		}

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

	private static void _workshopItemProcessor_ItemsLoaded()
	{
		WorkshopItemsLoaded?.Invoke();
		WorkshopItemsLoaded = null;
	}

	private static void _steamUserProcessor_ItemsLoaded()
	{
		SteamUsersLoaded?.Invoke();
		SteamUsersLoaded = null;
	}

	public static bool IsSteamAvailable()
	{
		return LocationManager.FileExists(LocationManager.SteamPathWithExe);
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

	internal static async Task<Dictionary<ulong, SteamUser>> GetSteamUsersAsync(List<ulong> steamId64s)
	{
		var idString = string.Join(",", steamId64s);
		var url = $"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={KEYS.STEAM_API_KEY}&steamids={idString}";

		steamId64s.Remove(0);

		if (steamId64s.Count == 0)
		{
			return new();
		}

		try
		{
			using var httpClient = new HttpClient();
			var httpResponse = await httpClient.GetAsync(url);

			if (httpResponse.IsSuccessStatusCode)
			{
				var response = await httpResponse.Content.ReadAsStringAsync();

				return JsonConvert.DeserializeObject<SteamUserRootResponse>(response)?.response.players.Select(x => new SteamUser(x)).ToDictionary(x => x.SteamId) ?? new();
			}

			Log.Error("failed to get steam author data: " + httpResponse.RequestMessage);
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "Failed to get steam author information");
		}

		return new();
	}

	internal static async Task<Dictionary<ulong, SteamWorkshopItem>> GetWorkshopInfoAsync(List<ulong> ids)
	{
		var url = $"https://api.steampowered.com/IPublishedFileService/GetDetails/v1/?key={KEYS.STEAM_API_KEY}&includetags=true&includechildren=true&includevotes=true";

		ids.Remove(0);

		if (ids.Count == 0)
		{
			return new();
		}

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
		var url = $"https://api.steampowered.com/IPublishedFileService/QueryFiles/v1/?key={KEYS.STEAM_API_KEY}&query_type={(int)order}&numperpage=100&return_vote_data=true&appid=255710&match_all_tags=true&return_tags=true&return_children=true&return_details=true";

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

					_workshopItemProcessor.AddToCache(newData.Item2);

					if (data.Count == newData.Item1)
					{
						return data;
					}

					page++;
				}
			}

			var httpResponse = await httpClient.GetAsync(url + "&page=0");

			var returnedData = (await ConvertPublishedFileResponse(httpResponse)).Item2;

			_workshopItemProcessor.AddToCache(returnedData);

			return returnedData;
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

			var data = info?.response?.publishedfiledetails
				.Select(x => new SteamWorkshopItem(x))
				.ToList() ?? new();

			_steamUserProcessor.AddRange(data.Select(x => x.AuthorID));

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

	public static async void LoadDlcs()
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
	}
}