using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.PlatformServices;

using KianCommons;

using SkyveInjections;

using SkyveMod.Util;

using SkyveShared;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

using static KianCommons.ReflectionHelpers;

namespace SkyveInjections;
public enum DownloadStatus
{
	DownloadOK,
	OutOfDate,
	CatalogOutOfDate,
	NotDownloaded,
	PartiallyDownloaded,
	Gone,
}

public class DoNothingComponent : MonoBehaviour
{
	void Awake()
	{
		UnityEngine.Debug.Log("TestComponent.Awake() was called");
	}

	void Start()
	{
		UnityEngine.Debug.Log("TestComponent.Start() was called");
	}
}


#if !NO_CO_STEAM_API
public class MassSubscribe : MonoBehaviour
{
	public enum StateT
	{
		None = 0,
		SubSent = 1,
		Subbed = 2,
		Failed = 3,
	}

	public class ItemT
	{
		public ItemT(ulong id)
		{
			PublishedFileId = new PublishedFileId(id);
			State = StateT.None;
			LastEventTime = default;
			RequestCount = 0;
		}

		public PublishedFileId PublishedFileId;
		public StateT State;
		public DateTime LastEventTime;
		public int RequestCount;

		public void Subscribe()
		{
			RequestCount++;
			LastEventTime = DateTime.Now;
			if (PublishedFileId == default || PublishedFileId == PublishedFileId.invalid)
			{
				State = StateT.Failed;
			}
			PlatformService.workshop.Subscribe(PublishedFileId);
			State = StateT.SubSent;
		}
	}

	DateTime LastEventTime = default;

	public List<ItemT> Items = new List<ItemT>();
	public IEnumerable<ItemT> RemainingItems => Items.Where(item => item.State <= StateT.SubSent);

	void Awake()
	{
		LogCalled();
	}

	void Start()
	{
		try
		{
			LogCalled();
			SteamUtilities.GetMassSub(out var filePath);
			List<ulong> ids;
			if (filePath.IsNullorEmpty())
			{
				var path = Path.Combine(DataLocation.localApplicationData, "Skyve");
				ids = UGCListTransfer.GetList(out _);
			}
			else
			{
				ids = UGCListTransfer.GetListFromFile(filePath, out _);
			}

			var subscriedItems = PlatformService.workshop.GetSubscribedItems();
			foreach (var id in ids)
			{
				if (!subscriedItems.Any(item => item.AsUInt64 == id))
				{
					Items.Add(new ItemT(id));
				}
			}
			RemainingCount = Items.Count();
			StartSubToAll();
			StartUpdateUI();
		}
		catch (Exception ex) { ex.Log(); }
	}

	public Coroutine StartSubToAll()
	{
		return StartCoroutine(SubToAllCoroutine());
	}

	private IEnumerator SubToAllCoroutine()
	{
		LogCalled();
		while (!SteamUtilities.SteamInitialized)
		{
			yield return new WaitForSeconds(0.1f);
		}

		PlatformService.workshop.eventWorkshopSubscriptionChanged += Workshop_eventWorkshopSubscriptionChanged;

		Log.Info("entering loop");
		for (; ; )
		{
			var counter = 0;
			foreach (var item in Items)
			{
				yield return new WaitForSeconds(0.05f);
				item.Subscribe();
				counter++;
				if (counter % 100 == 0)
				{
					yield return 0;
				}
			}

			RemainingCount = RemainingItems.Count();
			if (RemainingCount == 0)
			{
				break;
			}

			yield return new WaitForSeconds(1);
			yield return new WaitForSeconds(0.01f * RemainingCount);
		}
		Log.Info("all items subscribed!");
		System.Diagnostics.Process.GetCurrentProcess().Kill();
	}

	private void Workshop_eventWorkshopSubscriptionChanged(PublishedFileId fileID, bool subscribed)
	{
		try
		{
			LastEventTime = DateTime.Now;
			var item = Items.Find(item => item.PublishedFileId == fileID);
			if (item == null)
			{
				return;
			}
			else if (subscribed)
			{
				item.State = StateT.Subbed;
			}
			else
			{
				item.Subscribe();
			}
		}
		catch (Exception ex) { ex.Log(); }
	}

	public int RemainingCount;
	public Coroutine StartUpdateUI()
	{
		return StartCoroutine(UpdateUICoroutine());
	}

	private IEnumerator UpdateUICoroutine()
	{
		while (true)
		{
			RemainingCount = RemainingItems.Count();
			if (RemainingCount == 0)
			{
				Log.Info("all items subscribed!");
				System.Diagnostics.Process.GetCurrentProcess().Kill();
			}
			yield return new WaitForSeconds(0.5f);
		}
	}

	void OnGUI()
	{
		GUILayout.Label($"{Items.Count - RemainingCount}/{Items.Count} of assets are subscribed");
		if (GUILayout.Button("Terminate Now"))
		{
			System.Diagnostics.Process.GetCurrentProcess().Kill();
		}
	}
}

public class MassUnSubscribe : MonoBehaviour
{
	public enum StateT
	{
		None = 0,
		UnSubSent = 1,
		UnSubbed = 2,
		Failed = 3,
	}

	public class ItemT
	{
		public ItemT(ulong id)
		{
			PublishedFileId = new PublishedFileId(id);
			State = StateT.None;
			LastEventTime = default;
			RequestCount = 0;
		}

		public PublishedFileId PublishedFileId;
		public StateT State;
		public DateTime LastEventTime;
		public int RequestCount;

		public void UnSubscribe()
		{
			RequestCount++;
			LastEventTime = DateTime.Now;
			if (PublishedFileId == default || PublishedFileId == PublishedFileId.invalid)
			{
				State = StateT.Failed;
			}
			PlatformService.workshop.Unsubscribe(PublishedFileId);
			State = StateT.UnSubSent;
			Log.Called("unsubscribe request sent for: " + PublishedFileId);
		}
	}

	DateTime LastEventTime = default;

	public List<ItemT> Items = new List<ItemT>();
	public IEnumerable<ItemT> RemainingItems => Items.Where(item => item.State <= StateT.UnSubSent);

	void Awake()
	{
		LogCalled();
	}

	void Start()
	{
		try
		{
			LogCalled();
			SteamUtilities.GetMassUnSub(out var filePath);
			List<ulong> ids;
			bool missing;
			if (filePath.IsNullorEmpty())
			{
				var path = Path.Combine(DataLocation.localApplicationData, "Skyve");
				ids = UGCListTransfer.GetList(out missing);
				if (missing)
				{
					ids.AddRange(SteamUtilities.GetMissingItems().Select(item => item.AsUInt64));
					UGCListTransfer.SendList(ids, false); // replace missing with actual ids.
				}
			}
			else
			{
				ids = UGCListTransfer.GetListFromFile(filePath, out missing);
				if (missing)
				{
					ids.AddRange(SteamUtilities.GetMissingItems().Select(item => item.AsUInt64));
					UGCListTransfer.SendList(ids, false); // replace missing with actual ids.
				}
			}

			var subscriedItems = PlatformService.workshop.GetSubscribedItems();
			foreach (var id in ids)
			{
				if (subscriedItems.Any(item => item.AsUInt64 == id))
				{
					Items.Add(new ItemT(id));
				}
			}
			RemainingCount = Items.Count();
			StartUnSubToAll();
			StartUpdateUI();
		}
		catch (Exception ex) { ex.Log(); }
	}

	public Coroutine StartUnSubToAll()
	{
		return StartCoroutine(UnSubToAllCoroutine());
	}

	private IEnumerator UnSubToAllCoroutine()
	{
		LogCalled();
		while (!SteamUtilities.SteamInitialized)
		{
			yield return new WaitForSeconds(0.1f);
		}

		PlatformService.workshop.eventWorkshopSubscriptionChanged += Workshop_eventWorkshopSubscriptionChanged;
		Log.Info("entering loop");
		for (; ; )
		{
			var counter = 0;
			foreach (var item in Items)
			{
				item.UnSubscribe();
				counter++;
				if (counter % 100 == 0)
				{
					yield return 0;
				}
			}

			RemainingCount = RemainingItems.Count();
			if (RemainingCount == 0)
			{
				break;
			}

			yield return new WaitForSeconds(1);
			yield return new WaitForSeconds(0.01f * RemainingCount);
		}
		Log.Info("all items unsubscribed!");
		System.Diagnostics.Process.GetCurrentProcess().Kill();
	}

	private void Workshop_eventWorkshopSubscriptionChanged(PublishedFileId fileID, bool subscribed)
	{
		try
		{
			Log.Called(fileID, subscribed);
			LastEventTime = DateTime.Now;
			var item = Items.Find(item => item.PublishedFileId == fileID);
			if (item == null)
			{
				return;
			}
			else if (!subscribed)
			{
				item.State = StateT.UnSubbed;
			}
			else
			{
				item.UnSubscribe();
			}
		}
		catch (Exception ex) { ex.Log(); }
	}

	public int RemainingCount;
	public Coroutine StartUpdateUI()
	{
		return StartCoroutine(UpdateUICoroutine());
	}

	private IEnumerator UpdateUICoroutine()
	{
		while (true)
		{
			RemainingCount = RemainingItems.Count();
			if (RemainingCount == 0)
			{
				Log.Info("all items unsubscribed!");
				System.Diagnostics.Process.GetCurrentProcess().Kill();
			}
			yield return new WaitForSeconds(0.5f);
		}
	}


	void OnGUI()
	{
		GUILayout.Label($"{Items.Count - RemainingCount}/{Items.Count} of assets are unsubscribed");
		if (GUILayout.Button("Terminate Now"))
		{
			System.Diagnostics.Process.GetCurrentProcess().Kill();
		}
	}
}
#endif

public static class SubscriptionManager
{
	/// <returns>true, to avoid loading intro</returns>
	public static bool PostBootAction()
	{
		Log.Called();

		try
		{
			File.WriteAllLines(Path.Combine(SharedUtil.LocalLOMData, "SubscriptionList.txt"), PlatformService.workshop.GetSubscribedItems().Select(x => x.AsUInt64.ToString()).ToArray());
		}
		catch (Exception ex) { Log.Exception(ex); }

		try
		{
			var blacklists = BlackListTransfer.GetList(out _);

			foreach (var blacklist in blacklists)
			{
				PlatformService.workshop.Unsubscribe(new PublishedFileId(blacklist));
			}
		}
		catch { }

		if (SteamUtilities.GetStub())
		{
			SubscriptionUtil.Run();

			System.Diagnostics.Process.GetCurrentProcess().Kill();

			return true;
		}
		else if (SteamUtilities.GetMassSub(out _))
		{
			new GameObject().AddComponent<Camera>();
			//new GameObject("base").AddComponent<Example>();
#if !NO_CO_STEAM_API
			new GameObject("mass subscribe go").AddComponent<MassSubscribe>();
#endif
			return true;
		}
		else if (SteamUtilities.GetMassUnSub(out _))
		{
			new GameObject().AddComponent<Camera>();
			//new GameObject("base").AddComponent<Example>();
#if !NO_CO_STEAM_API
			new GameObject("mass unsubscribe go").AddComponent<MassUnSubscribe>();
#endif
			return true;
		}
		else
		{
			return false.LogRet(ThisMethod);
		}
	}

	public static void DoNothing()
	{
		new GameObject().AddComponent<Camera>();
		new GameObject("nop go").AddComponent<DoNothingComponent>();
	}
}

public static class CMPatchHelpers
{
	private static readonly SubscriptionTransfer subscriptionTransfer;
	private static readonly List<ulong> subscribedItems;
	private static readonly Dictionary<string, ModConfig.ModInfo> modConfig;

	static CMPatchHelpers()
	{
		modConfig = ModConfig.Deserialize().GetModsInfo();

		var filePath = Path.Combine(DataLocation.localApplicationData, Path.Combine("Skyve", "SubscriptionTransfer.xml"));

		subscriptionTransfer = File.Exists(filePath) ? SharedUtil.Deserialize<SubscriptionTransfer>(filePath) ?? new() : new();
		subscribedItems = PlatformService.workshop.GetSubscribedItems().Select(x => x.AsUInt64).ToList();
	}

	public static bool IsDirectoryExcluded(string path)
	{
		var workshopId = Path.GetDirectoryName(path) == "255710" && ulong.TryParse(Path.GetFileName(path), out var id) ? id : 0;

		return
			path.IsNullOrWhiteSpace() ||
			path[0] == '_' ||
			(workshopId > 0 && subscribedItems.Count > 0 && ((!subscribedItems.Contains(workshopId) && !(subscriptionTransfer.SubscribeTo?.Contains(workshopId) ?? false)) ||
			(subscriptionTransfer.UnsubscribingFrom?.Contains(workshopId) ?? false))) ||
			(modConfig.TryGetValue(path, out var info) && info.Excluded);
	}

	public static bool IsIDExcluded(PublishedFileId id)
	{
		var path = PlatformService.workshop.GetSubscribedItemPath(id);
		return IsDirectoryExcluded(path);
	}

	public static string CheckFiles(string path)
	{
		EnsureIncludedOrExcludedFiles(path);
		return path;
	}

	public static void EnsureIncludedOrExcludedFiles(string path)
	{
		try
		{
			foreach (var file in Directory.GetFiles(path, "*", searchOption: SearchOption.AllDirectories))
			{
				EnsureFile(file);
			}
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
		}
	}

	public static void EnsureFile(string fullFilePath)
	{
		if (string.IsNullOrEmpty(fullFilePath))
		{
			return;
		}

		var included = SteamUtilities.ToIncludedPath(fullFilePath);
		var excluded = SteamUtilities.ToExcludedPath2(fullFilePath);
		if (File.Exists(included) && File.Exists(excluded))
		{
			File.Delete(excluded);
			File.Move(included, excluded);
			fullFilePath = excluded;
		}
	}
}

public static class SteamUtilities
{
	public static bool Initialized { get; private set; } = false;
	public static bool sman = Environment.GetCommandLineArgs().Any(_arg => _arg == "-sman");
	public static bool GetStub()
	{
		return ParseCommandLine("stub", out _);
	}
	public static bool GetMassSub(out string filePath)
	{
		return ParseCommandLine("subscribe", out filePath);
	}

	public static bool GetMassUnSub(out string filePath)
	{
		return ParseCommandLine("unsubscribe", out filePath);
	}


	// steam manager is already initialized at this point.

	/// <summary>
	/// if no match was found, value=null and returns false.
	/// if a match is found, value="" or string after --prototype= and returns true.
	/// </summary>
	public static bool ParseCommandLine(string prototypes, out string value)
	{
		foreach (var arg in Environment.GetCommandLineArgs())
		{
			foreach (var prototype in prototypes.Split("|"))
			{
				if (MatchCommandLineArg(arg, prototype, out var val))
				{
					value = val;
					return true;
				}
			}
		}
		value = null;
		return false;
	}

	/// <summary>
	/// matches one arg with one prototype
	/// </summary>
	public static bool MatchCommandLineArg(string arg, string prototype, out string value)
	{
		if (arg == "-" + prototype)
		{
			value = "";
			return true;
		}
		else if (arg.StartsWith($"--{prototype}="))
		{
			var i = prototype.Length + 3;
			if (arg.Length > i)
			{
				value = arg.Substring(i);
			}
			else
			{
				value = "";
			}

			return true;
		}
		else
		{
			value = null;
			return false;
		}
	}

	public static void RegisterEvents()
	{
		if (Initialized)
		{
			return;
		}

		Initialized = true; // used to check if patch loader is effective.
		PlatformService.eventSteamControllerInit += SteamUtilities.OnInitSteamController;
		PlatformService.eventGameOverlayActivated += SteamUtilities.OnGameOverlayActivated;
		PlatformService.workshop.eventSubmitItemUpdate += SteamUtilities.OnSubmitItemUpdate;
		PlatformService.workshop.eventWorkshopItemInstalled += SteamUtilities.OnWorkshopItemInstalled;
		PlatformService.workshop.eventWorkshopSubscriptionChanged -= SteamUtilities.OnWorkshopSubscriptionChanged;
		PlatformService.workshop.eventWorkshopSubscriptionChanged += SteamUtilities.OnWorkshopSubscriptionChanged;
		PlatformService.workshop.eventUGCQueryCompleted += SteamUtilities.OnUGCQueryCompleted;
		AppDomain.CurrentDomain.UnhandledException += SteamUtilities.CurrentDomain_UnhandledException;
	}
	public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
	{
		var ex = (Exception)args.ExceptionObject;
		ex.Log(showInPannel: false);
	}

	public static bool firstTime = true;
	public static bool SteamInitialized { get; private set; } = false;
	public static void OnInitSteamController()
	{
		if (!firstTime)
		{
			return;
		}

		firstTime = false;

		var items = PlatformService.workshop.GetSubscribedItems();
		Log.Info("Subscribed Items are: " + items.ToSTR());

		EnsureIncludedOrExcludedAllFast();
		//if (Config.DeleteUnsubscribedItemsOnLoad)
		//    DeleteUnsubbed();
		SteamInitialized = true;
	}

	public static bool IsCloudEnabled()
	{
		var ret = PlatformService.cloud?.enabled ?? false;
		Log.Info("Cloud.enabled=" + ret);
		if (!ret)
		{
			Log.Info("Skipping cloud packages.");
		}
		return ret;
	}

	/// <summary>
	/// returns a list of items that are subscribed but not downloaded (excluding deleted items).
	/// </summary>
	/// <returns></returns>
	public static IEnumerable<PublishedFileId> GetMissingItems()
	{
		foreach (var id in PlatformService.workshop.GetSubscribedItems())
		{
			var path = PlatformService.workshop.GetSubscribedItemPath(id);
			if (!path.IsNullOrWhiteSpace() && !Directory.Exists(path) && !Directory.Exists(ToExcludedPath2(path)))
			{
				yield return id;
			}
		}
	}

	static DirectoryInfo GetWSPath()
	{
		foreach (var id in PlatformService.workshop.GetSubscribedItems())
		{
			var path = PlatformService.workshop.GetSubscribedItemPath(id);
			if (!path.IsNullOrWhiteSpace())
			{
				return new DirectoryInfo(path).Parent;
			}
		}
		return default;
	}

	//public static void OnRequestItemDetailsClicked() {
	//    Log.Debug("RequestItemDetails pressed");
	//    foreach (var id in PlatformService.workshop.GetSubscribedItems())
	//        PlatformService.workshop.RequestItemDetails(id).LogRet($"RequestItemDetails({id.AsUInt64})");
	//    //var id = new PublishedFileId(2040656402ul);
	//    //PlatformService.workshop.RequestItemDetails(id).LogRet($"RequestItemDetails({id.AsUInt64})");
	//}
	//public static void OnQueryItemsClicked() {
	//    Log.Debug("QueryItems pressed");
	//    PlatformService.workshop.QueryItems().LogRet($"QueryItems()"); ;
	//}

	private static void OnSubmitItemUpdate(SubmitItemUpdateResult result, bool ioError)
	{
		Log.Debug($"PlatformService.workshop.eventSubmitItemUpdate(result:{result.result}, {ioError})");
	}


	private static void OnGameOverlayActivated(bool active)
	{
		Log.Debug($"PlatformService.workshop.eventGameOverlayActivated({active})");
	}

	private static void OnWorkshopItemInstalled(PublishedFileId id)
	{
		Log.Debug($"PlatformService.workshop.eventWorkshopItemInstalled({id.AsUInt64})");
	}

	private static void OnWorkshopSubscriptionChanged(PublishedFileId id, bool subscribed)
	{
		Log.Debug($"PlatformService.workshop.eventWorkshopSubscriptionChanged({id.AsUInt64}, {subscribed})");
	}

	private static void OnUGCQueryCompleted(UGCDetails result, bool ioError)
	{
		// called after QueryItems
		Log.Debug($"OnUGCQueryCompleted(" +
			$"result:{result.ToSTR()}, " +
			$"ioError:{ioError})");
	}

	public static string Class(ref this UGCDetails ugc)
	{
		var tags = ugc.tags.Split(",").Select(tag => tag.Trim().ToLower()).ToArray();
		if (tags.Contains("mod"))
		{
			return "Mod";
		}
		else if (tags.Length > 0)
		{
			return "Asset";
		}
		else
		{
			return "subscribed item";
		}
	}

	public static string ToSTR(this ref UGCDetails result)
	{
		return $"UGCDetails({result.result} {result.publishedFileId.AsUInt64})";
	}

	public static string ToSTR2(this ref UGCDetails result)
	{
#pragma warning disable
		string m =
			$"UGCDetails:\n" +
			$"    publishedFileId:{result.publishedFileId}\n" +
			$"    title:{result.title}\n" +
			$"    creator: {result.creatorID} : {result.creatorID.ToAuthorName()}\n" +
			$"    result:{result.result}\n" +
			$"    timeUpdated:{result.timeUpdated} : {result.timeUpdated.ToLocalTime()}\n" +
			$"    timeCreated:{result.timeCreated} : {result.timeCreated.ToLocalTime()}\n" +
			$"    fileSize:{result.fileSize}\n" +
			$"    tags:{result.tags}\n";

#pragma warning enable
		return m;
	}

	//code copied from package entry
	public static DateTime GetLocalTimeCreated(string modPath)
	{
		DateTime dateTime = DateTime.MinValue;
		foreach (string path in Directory.GetFiles(modPath, "*", searchOption: SearchOption.AllDirectories))
		{
			DateTime creationTimeUtc = File.GetCreationTimeUtc(path);
			if (creationTimeUtc > dateTime)
			{
				dateTime = creationTimeUtc;
			}
		}
		return dateTime;
	}

	//code copied from package entry
	public static DateTime GetLocalTimeUpdated(string modPath)
	{
		DateTime dateTime = DateTime.MinValue;
		if (Directory.Exists(modPath))
		{
			foreach (string path in Directory.GetFiles(modPath, "*", searchOption: SearchOption.AllDirectories))
			{
				if (Path.GetFileName(path) != EXCLUDED_FILE_NAME)
				{
					DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(path);
					if (lastWriteTimeUtc > dateTime)
					{
						dateTime = lastWriteTimeUtc;
					}
				}
			}
		}
		return dateTime;
	}

	public static DateTime kEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

	public static DateTime ToUTCTime(this uint time) => kEpoch.AddSeconds(time);
	public static DateTime ToLocalTime(this uint time) => kEpoch.AddSeconds(time).ToLocalTime();

	public static string ToAuthorName(this UserID userID) => new Friend(userID).personaName;

	static string STR(DateTime time)
	{
		var local = time.ToLocalTime().ToString();
		var utc = time.ToUniversalTime().ToShortTimeString();
		return $"{local} (UTC {utc})";
	}

	public static DownloadStatus IsUGCUpToDate(UGCDetails det, out string reason)
	{
		Assertion.Assert(det.publishedFileId != PublishedFileId.invalid, "invalid id");
		Assertion.Assert(det.publishedFileId.AsUInt64 != 0, "id 0");
		if (det.title.IsNullOrWhiteSpace())
		{
			reason = "could not get steam details (removed from workshop?)";
			return DownloadStatus.Gone;
		}
		string path = PlatformService.workshop.GetSubscribedItemPath(det.publishedFileId);
		if (path.IsNullOrWhiteSpace())
		{
			reason = "could not get item path (removed from workshop?)";
			return DownloadStatus.Gone;
		}

		string localPath = GetFinalPath(path);
		if (localPath == null)
		{
			reason = $"{det.Class()} is not downloaded. path does not exits: " +
				PlatformService.workshop.GetSubscribedItemPath(det.publishedFileId);
			return DownloadStatus.NotDownloaded;
		}

		var updatedServer = det.timeUpdated.ToUTCTime();
		var updatedLocal = GetLocalTimeUpdated(localPath).ToUniversalTime();
		var sizeServer = det.fileSize;
		var localSize = GetTotalSize(localPath);

		if (localSize == 0)
		{
			reason = $"{det.Class()} is not downloaded (empty folder '{localPath}'): " +
				PlatformService.workshop.GetSubscribedItemPath(det.publishedFileId);
			return DownloadStatus.NotDownloaded;
		}

		if (updatedLocal == DateTime.MinValue)
		{
			reason = $"Error getting local time at {localPath}";
			return DownloadStatus.NotDownloaded;
		}
		else if (updatedLocal < updatedServer)
		{
			bool sure =
				localSize < sizeServer ||
				updatedLocal < updatedServer.AddHours(-24);
			string be = sure ? "is" : "may be";

			PublishedFileId CR = new(2881031511);
			if (det.publishedFileId == CR)
			{
				reason = $"Compatibility report Catalog {be} out of date.\n\t" +
				$"server-time={STR(updatedServer)} |  local-time={STR(updatedLocal)}";
				return DownloadStatus.CatalogOutOfDate;
			}
			else
			{
				reason = $"{det.Class()} {be} out of date.\n\t" +
					$"server-time={STR(updatedServer)} |  local-time={STR(updatedLocal)}";
				return DownloadStatus.OutOfDate;
			}
		}

		if (localSize < sizeServer) // could be bigger if user has its own files in there.
		{
			reason = $"{det.Class()} download is incomplete. server-size={sizeServer}) local-size={localSize})";
			return DownloadStatus.PartiallyDownloaded;
		}

		reason = null;
		return DownloadStatus.DownloadOK;
	}

	public static long GetTotalSize(string path)
	{
		var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
		return files.Sum(_f => new FileInfo(_f).Length);
	}


	#region included excluded
	public static string GetFinalPath(string includedPath)
	{
		if (includedPath.IsNullorEmpty())
			return null;
		if (Directory.Exists(includedPath))
			return includedPath;

		string excludedPath = ToExcludedPath(includedPath);
		if (Directory.Exists(excludedPath))
			return excludedPath;

		return null;
	}

	/// <summary>
	/// puts _ behind directory 
	/// prints error if it already has _ or if path is empty.
	/// </summary>
	public static string ToExcludedPath(string includedPath)
	{
		string p1 = Path.GetDirectoryName(includedPath);
		string p2 = Path.GetFileName(includedPath);
		if (string.IsNullOrEmpty(p1) || string.IsNullOrEmpty(p2))
		{
			Log.Error(ThisMethod +
				$"includedPath={includedPath}\n" +
				$"p1={p1}\n" +
				$"p2={p2}\n");
		}
		if (p2.StartsWith("_"))
		{
			Log.Error($"includedPath={includedPath} should not start with _");
			return includedPath;
		}
		p2 = "_" + p2;
		return Path.Combine(p1, p2);
	}

	/// <summary>
	/// puts _ behind directory if it does not have _ already
	/// </summary>
	public static string ToExcludedPath2(string fullPath)
	{
		Assertion.Assert(!fullPath.IsNullorEmpty(), "fullPath=" + fullPath);
		string parent = Path.GetDirectoryName(fullPath);
		string file = Path.GetFileName(fullPath);
		if (!file.StartsWith("_"))
			file = "_" + file;
		return Path.Combine(parent, file);
	}

	public static bool IsPathIncluded(string fullPath)
	{
		Assertion.Assert(!fullPath.IsNullorEmpty(), "fullPath=" + fullPath);
		return Path.GetFileName(fullPath).StartsWith("_");
	}
	public static string ToIncludedPath(string fullPath)
	{
		Assertion.Assert(!fullPath.IsNullorEmpty(), "fullPath=" + fullPath);
		string parent = Path.GetDirectoryName(fullPath);
		string file = Path.GetFileName(fullPath);
		if (file.StartsWith("_"))
			file = file.Substring(1); //drop _
		return Path.Combine(parent, file);
	}


	public static bool IsExcludedMod(PublishedFileId publishedFileId)
	{
		try
		{
			var path = PlatformService.workshop.GetSubscribedItemPath(publishedFileId);
			return CMPatchHelpers.IsDirectoryExcluded(path);
		}
		catch (Exception ex)
		{
			ex.Log("publishedFileId=" + publishedFileId);
			throw;
		}
	}

	public static void EnsureIncludedOrExcluded(PublishedFileId id)
	{
		try
		{
			string path1 = PlatformService.workshop.GetSubscribedItemPath(id);
			if (path1.IsNullorEmpty())
			{
				Log.Warning($"item {id} does not have path");
				return;
			}

			string path2 = ToExcludedPath(path1);
			if (Directory.Exists(path1) && Directory.Exists(path2))
			{
				Directory.Delete(path2, true);
				Directory.Move(path1, path2);
			}
		}
		catch (Exception ex)
		{
			Log.Exception(ex, $"EnsureIncludedOrExcluded({id})", showInPanel: false);
		}
	}


	//public static void EnsureAll() {
	//    Log.Called();
	//    var items = PlatformService.workshop.GetSubscribedItems();
	//    foreach (var id in items) {
	//        EnsureIncludedOrExcluded(id);
	//        PlatformService.workshop.RequestItemDetails(id)
	//            .LogRet($"RequestItemDetails({id.AsUInt64})");
	//    }
	//}

	public static void EnsureIncludedOrExcludedAllFast()
	{
		var dir = GetWSPath();
		if (dir != default)
			EnsureIncludedOrExcludedAllFast(dir);
	}

	/// <summary>
	/// I think GetFiles/GetDirectories is cached and therefore the normal EnsureIncludedOrExcluded should work fast
	/// but just in case it doesn't for some OS then I should use this method
	/// </summary>
	public static void EnsureIncludedOrExcludedAllFast(DirectoryInfo RootDir)
	{
		try
		{
			Log.Called(RootDir);
			foreach (var path in Directory.GetDirectories(RootDir.FullName))
			{
				var dirName = Path.GetFileName(path);
				if (dirName.StartsWith("_"))
				{
					Exclude(path);
				}
			}
		}
		catch (Exception ex)
		{
			Log.Exception(ex, $"EnsureIncludedOrExcluded({RootDir})", showInPanel: false);
		}
		Log.Succeeded();
	}

	public static bool Exclude(string path)
	{
		try
		{
			string excludedPath = ToExcludedPath2(path);
			string includedPath = ToIncludedPath(path);
			if (Directory.Exists(excludedPath))
			{
				if (Directory.Exists(includedPath))
				{
					Directory.Delete(excludedPath, true);
				}
				else
				{
					Directory.Move(excludedPath, includedPath);
				}
			}
			else if (!Directory.Exists(includedPath))
			{
				return false;
			}
			Touch(Path.Combine(includedPath, EXCLUDED_FILE_NAME));
			return true;
		}
		catch (Exception ex) { ex.Log(); }
		return false;
	}


	public const string EXCLUDED_FILE_NAME = ".excluded";
	public static void Touch(string path)
	{
		if (!File.Exists(path))
		{
			File.CreateText(path).Dispose();
		}
	}

	#endregion

	public static void DeleteUnsubbed()
	{
		Log.Info("DeleteUnsubbed called ...");
		var items = PlatformService.workshop.GetSubscribedItems();
		if (items == null || items.Length == 0)
			return;

		var path = PlatformService.workshop.GetSubscribedItemPath(items[0]);
		path = Path.GetDirectoryName(path);

		foreach (var dir in Directory.GetDirectories(path))
		{
			ulong id;
			string strID = Path.GetFileName(dir);
			if (strID.StartsWith("_"))
				strID = strID.Substring(1);
			if (!ulong.TryParse(strID, out id))
				continue;
			bool deleted = !items.Any(item => item.AsUInt64 == id);
			if (deleted)
			{
				Log.Warning("unsubbed mod will be deleted: " + dir);
				Directory.Delete(dir, true);
			}
		}
		Log.Succeeded();
	}

	public static DirectoryInfo FindWSDir()
	{
		foreach (var id in PlatformService.workshop.GetSubscribedItems())
		{
			var path = PlatformService.workshop.GetSubscribedItemPath(id);
			if (path != null)
			{
				return new DirectoryInfo(path).Parent;
			}
		}
		return null;
	}
}
