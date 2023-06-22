using Extensions;

using Newtonsoft.Json;

using SkyveApp.Domain.Interfaces;
using SkyveApp.Domain.Steam;
using SkyveApp.Services;
using SkyveApp.Services.Interfaces;
using SkyveApp.Utilities.IO;

using SkyveShared;

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

using static System.Net.WebRequestMethods;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SkyveApp.Utilities;

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

	public static void Download(IEnumerable<IPackage> packages)
	{
		var currentPath = Program.Services.GetService<IOUtil>().ToRealPath(Path.GetDirectoryName(Program.CurrentDirectory));

		if (packages.Any(x => x.Folder.PathEquals(currentPath)))
		{
			if (MessagePrompt.Show(Locale.LOTWillRestart, PromptButtons.OKCancel, PromptIcons.Info, Program.MainForm) == DialogResult.Cancel)
			{
				return;
			}

			Program.Services.GetService<IOUtil>().WaitForUpdate();

			Application.Exit();
		}

		Download(packages.Select(x => x.SteamId));
	}

	public static void Download(IEnumerable<ulong> ids)
	{
		try
		{
			foreach (var id in ids.Reverse().Chunk(100))
			{
				var steamArguments = new StringBuilder("steam://open/console");

				if (CrossIO.CurrentPlatform is not Platform.Windows)
				{
					steamArguments.Append(" \"");
				}

				foreach (var item in id)
				{
					steamArguments.AppendFormat(" +workshop_download_item 255710 {0}", item);
				}

				if (CrossIO.CurrentPlatform is not Platform.Windows)
				{
					steamArguments.Append("\"");
				}

				ExecuteSteam(steamArguments.ToString());

				Thread.Sleep(250);
			}

			ExecuteSteam("steam://open/downloads");
		}
		catch (Exception) { }
	}

	public static bool IsDlcInstalledLocally(uint dlcId)
	{
		return _csCache?.Dlcs?.Contains(dlcId) ?? false;
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
		return CrossIO.FileExists(Program.Services.GetService<ILocationManager>().SteamPathWithExe);
	}

	public static void ExecuteSteam(string args)
	{
		var file = Program.Services.GetService<ILocationManager>().SteamPathWithExe;

		if (CrossIO.CurrentPlatform is Platform.Windows)
		{
			var process = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(file));

			if (process.Length == 0)
			{
				Notification.Create(Locale.SteamNotOpenTo, null, PromptIcons.Info, null).Show(Program.MainForm, 10);
			}
		}

		Program.Services.GetService<IOUtil>().Execute(file, args);
	}

	internal static async Task<Dictionary<ulong, SteamUser>> GetSteamUsersAsync(List<ulong> steamId64s)
	{
		steamId64s.RemoveAll(x => x == 0);

		if (steamId64s.Count == 0)
		{
			return new();
		}

		try
		{
			var idString = string.Join(",", steamId64s.Distinct());

			var result = await ApiUtil.Get<SteamUserRootResponse>($"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/",
				("key", KEYS.STEAM_API_KEY),
				("steamids", idString));

			return result?.response.players.Select(x => new SteamUser(x)).ToDictionary(x => x.SteamId) ?? new();
		}
		catch (Exception ex)
		{
			Program.Services.GetService<ILogger>().Exception(ex, "Failed to get steam author information");
		}

		return new();
	}

	internal static async Task<Dictionary<ulong, SteamWorkshopItem>> GetWorkshopInfoAsync(List<ulong> ids)
	{
		ids.RemoveAll(x => x == 0);

		if (ids.Count == 0)
		{
			return new();
		}

		var query = new List<(string, object)>
		{
			("key", KEYS.STEAM_API_KEY),
			("includetags", true),
			("includechildren", true),
			("includevotes", true),
		};

		for (var i = 0; i < ids.Count; i++)
		{
			query.Add(($"publishedfileids[{i}]", ids[i]));
		}

		return (await ConvertPublishedFileResponse("https://api.steampowered.com/IPublishedFileService/GetDetails/v1/", query)).Item2;
	}

	internal static async Task<Dictionary<ulong, SteamWorkshopItem>> GetWorkshopItemsByUserAsync(ulong userId, bool all = false)
	{
		if (userId == 0)
		{
			return new();
		}

		var url = "https://api.steampowered.com/IPublishedFileService/GetUserFiles/v1/";
		var query = new (string, object)[]
		{
			("key", KEYS.STEAM_API_KEY),
			("steamid", userId),
			("appid", 255710),
			("file_type", 0),
			("numperpage", 100),
			("return_vote_data", true),
			("return_tags", true),
			("return_children", true),
		};

		var data = new Dictionary<ulong, SteamWorkshopItem>();
		var page = 1;

		while (true)
		{
			var newData = await ConvertPublishedFileResponse(url, query.Concat(new (string, object)[]
			{
				("page", page)
			}));

			data.AddRange(newData.Item2);

			_workshopItemProcessor.AddToCache(newData.Item2);

			if (!all || data.Count == newData.Item1)
			{
				return data;
			}

			page++;
		}
	}

	public static async Task<Dictionary<ulong, SteamWorkshopItem>> QueryFilesAsync(SteamQueryOrder order, string? query = null, string[]? requiredTags = null, string[]? excludedTags = null, (DateTime, DateTime)? dateRange = null, bool all = false)
	{
		var url = $"https://api.steampowered.com/IPublishedFileService/QueryFiles/v1/";
		var queryItems = new List<(string, object)>
		{
			("key", KEYS.STEAM_API_KEY),
			("query_type", (int)order),
			("numperpage", 100),
			("appid", 255710),
			("match_all_tags", true),
			("return_vote_data", true),
			("return_tags", true),
			("return_children", true),
			("return_details", true),
		};

		if (!string.IsNullOrWhiteSpace(query))
		{
			queryItems.Add(("search_text", query!));
		}

		if (requiredTags?.Any() ?? false)
		{
			for (var i = 0; i < requiredTags.Length; i++)
			{
				queryItems.Add(($"requiredtags[{i}]", requiredTags[i]!));
			}
		}

		if (excludedTags?.Any() ?? false)
		{
			for (var i = 0; i < excludedTags.Length; i++)
			{
				queryItems.Add(($"excludedtags[{i}]", excludedTags[i]!));
			}
		}

		if (dateRange is not null)
		{
			queryItems.Add(($"date_range_updated[0]", dateRange.Value.Item1));
			queryItems.Add(($"date_range_updated[1]", dateRange.Value.Item2));
		}

		var data = new Dictionary<ulong, SteamWorkshopItem>();
		var page = 0;

		while (true)
		{
			var newData = await ConvertPublishedFileResponse(url, queryItems.Concat(new (string, object)[]
			{
				("page", page)
			}));

			data.AddRange(newData.Item2);

			_workshopItemProcessor.AddToCache(newData.Item2);

			if (!all || data.Count == newData.Item1)
			{
				return data;
			}

			page++;
		}
	}

	private static async Task<(int, Dictionary<ulong, SteamWorkshopItem>)> ConvertPublishedFileResponse(string url, IEnumerable<(string, object)> query)
	{
		try
		{
			var info = await ApiUtil.Get<SteamFileServiceInfo>(url, query.ToArray());

			var data = info?.response?.publishedfiledetails
				.Select(x => new SteamWorkshopItem(x))
				.ToList() ?? new();

			_steamUserProcessor.AddRange(data.Select(x => x.AuthorID));

			return (info?.response?.total ?? 0, data.ToDictionary(x => x.SteamId));
		}
		catch (Exception ex)
		{
			Program.Services.GetService<ILogger>().Error("failed to get steam data: " + ex.Message);
		}

		return (0, new());
	}

	public static async Task<Dictionary<string, SteamAppInfo>> GetSteamAppInfoAsync(uint steamId)
	{
		try
		{
			var url = $"https://store.steampowered.com/api/appdetails";

			return await ApiUtil.Get<Dictionary<string, SteamAppInfo>>(url, ("appids", steamId)) ?? new();
		}
		catch (Exception ex) { Program.Services.GetService<ILogger>().Exception(ex, "Failed to get the steam information for appid " + steamId); }

		return new();
	}

	public static async void LoadDlcs()
	{
		Program.Services.GetService<ILogger>().Info($"Loading DLCs..");

		var dlcs = await GetSteamAppInfoAsync(255710);

		if (!dlcs.ContainsKey("255710"))
		{
			Program.Services.GetService<ILogger>().Info($"Failed to load DLCs, steam info returned invalid content..");
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

		Program.Services.GetService<ILogger>().Info($"DLCs ({newDlcs.Count}) loaded..");

		ISave.Save(Dlcs = newDlcs, DLC_CACHE_FILE);

		DLCsLoaded?.Invoke();

		Program.Services.GetService<IAssetUtil>().SetAvailableDlcs(Dlcs.Select(x => x.Id));

		foreach (var dlc in Dlcs)
		{
			await Program.Services.GetService<IImageManager>().Ensure(dlc.ThumbnailUrl, false, $"{dlc.Id}.png", false);

			DLCsLoaded?.Invoke();
		}
	}

	internal static void ClearCache()
	{
		_workshopItemProcessor.Clear();
		_steamUserProcessor.Clear();

		try
		{ CrossIO.DeleteFile(ISave.GetPath(DLC_CACHE_FILE)); }
		catch (Exception ex) { Program.Services.GetService<ILogger>().Exception(ex, "Failed to clear DLC_CACHE_FILE"); }

		try
		{ CrossIO.DeleteFile(ISave.GetPath(SteamUserProcessor.STEAM_USER_CACHE_FILE)); }
		catch (Exception ex) { Program.Services.GetService<ILogger>().Exception(ex, "Failed to clear STEAM_USER_CACHE_FILE"); }

		try
		{ CrossIO.DeleteFile(ISave.GetPath(SteamItemProcessor.STEAM_CACHE_FILE)); }
		catch (Exception ex) { Program.Services.GetService<ILogger>().Exception(ex, "Failed to clear STEAM_CACHE_FILE"); }
	}
}