using ColossalFramework.PlatformServices;

using KianCommons;
using KianCommons.Plugins;

using SkyveMod.Settings;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

using UnityEngine;

using SteamUtilities = Injections.SkyveInjections.SteamUtilities;

namespace SkyveMod.Util;
extern alias Injections;
public class CheckSubsUtil : MonoBehaviour
{
	static GameObject go_;
	public static CheckSubsUtil Ensure()
	{
		return Instance;
	}

	public static CheckSubsUtil Instance
	{
		get
		{
			if (!go_)
			{
				go_ = new GameObject(nameof(CheckSubsUtil), typeof(CheckSubsUtil));
			}
			return go_.GetComponent<CheckSubsUtil>();
		}
	}

	//public static void EnsureAll() {
	//    Instance.EnsureIncludedOrExcluded();
	//    //Instance.RequestItemDetails();
	//}

	public Coroutine EnsureIncludedOrExcluded()
	{
		return StartCoroutine(EnsureIncludedOrExcludedCoroutine());
	}

	public IEnumerator EnsureIncludedOrExcludedCoroutine()
	{
		Log.Called();
		var items = PlatformService.workshop.GetSubscribedItems();
		var counter = 0;
		foreach (var id in items)
		{
			SteamUtilities.EnsureIncludedOrExcluded(id);
			if (counter >= 100)
			{
				counter = 0;
				yield return 0;
			}
		}
	}

	//public Coroutine RequestItemDetails() => StartCoroutine(RequiestItemDetailsCoroutine());
	//public IEnumerator RequiestItemDetailsCoroutine() {
	//    Log.DisplayMesage($"Checking all items ...");
	//    RegisterEvents();
	//    var items = PlatformService.workshop.GetSubscribedItems();
	//    int counter = 0;
	//    foreach (var id in items) {
	//        PlatformService.workshop.RequestItemDetails(id)
	//            .LogRet($"RequestItemDetails({id.AsUInt64})");
	//        if (counter >= 100) {
	//            counter = 0;
	//            yield return 0;
	//        }
	//    }
	//}

#if !NO_CO_STEAM_API
	public Coroutine UnsubDepricated()
	{
		return StartCoroutine(UnsubDepricatedCoroutine());
	}

	public IEnumerator UnsubDepricatedCoroutine()
	{
		Log.DisplayMesage($"Unsubscribing from deprecated items ...");

		var items = PlatformService.workshop.GetSubscribedItems();
		if (items == null || items.Length == 0)
		{
			yield break;
		}

		var counter = 0;
		var nUnsubbed = 0;
		foreach (var item in items)
		{
			var path = PlatformService.workshop.GetSubscribedItemPath(item);
			if (path == null)
			{
				Log.DisplayWarning($"Deprecated item will be unsubbed: {item}");
				PlatformService.workshop.Unsubscribe(item);
				nUnsubbed++;
			}

			if (counter >= 100)
			{
				counter = 0;
				yield return 0;
			}
		}

		Log.DisplayMesage($"Unsubscribing from {nUnsubbed} deprecated items.");
	}
#endif

	public static List<PublishedFileId> GetBrokenDownloads()
	{
		var ids = new List<PublishedFileId>();
		foreach (var item in ContentManagerUtil.ModEntries)
		{
			var det = item.workshopDetails;
			var id = det.publishedFileId;
			if (id == PublishedFileId.invalid || id.AsUInt64 == 0)
			{
				continue;
			}

			if (id.AsUInt64 is PatchLoaderStatus.PatchLoaderWorkshopId or
				LoadOrderUtil.WSId)
			{
				continue; // cannot resub to patch loader or LOM.
			}

			var status = SteamUtilities.IsUGCUpToDate(det, out _);
			if (status != DownloadStatus.DownloadOK)
			{
				Log.Debug($"redownloading {item.entryName} with status:{status}", false);
				ids.Add(id);
			}
		}
		var missing = SteamUtilities.GetMissingItems().ToArray();
		Log.Debug("missing items = " + missing.ToSTR(), false);
		ids.AddRange(missing);
		return ids;
	}

	public static void ResubcribeExternally()
	{
		Log.Called();
		try
		{
			var ids = GetBrokenDownloads();
			Injections.SkyveShared.UGCListTransfer.SendList(
				ids: ids.Select(id => id.AsUInt64),
				missing: false);

			var modPath = PluginUtil.GetSkyveMod().modPath;
			Process.Start("CMD.exe", $"/c \"{modPath}/resub.bat\"");
			Process.GetCurrentProcess().Kill();
		}
		catch (Exception ex)
		{
			ex.Log();
		}
	}

	public static Process Execute(string dir, string exeFile, string args)
	{
		try
		{
			var startInfo = new ProcessStartInfo
			{
				WorkingDirectory = dir,
				FileName = exeFile,
				Arguments = args,
				WindowStyle = ProcessWindowStyle.Normal,
				UseShellExecute = true,
				CreateNoWindow = false,
			};
			Log.Info($"Executing ...\n" +
				$"\tWorkingDirectory={dir}\n" +
				$"\tFileName={exeFile}\n" +
				$"\tArguments={args}");
			var process = new Process { StartInfo = startInfo };
			process.Start();
			process.OutputDataReceived += (_, e) => Log.Info(e.Data);
			process.ErrorDataReceived += (_, e) => Log.Warning(e.Data);
			process.Exited += (_, e) => Log.Info("process exited with code " + process.ExitCode);
			return process;
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
			return null;
		}
	}

	public static void ReDownload(string steamPath)
	{
		try
		{
			var ids = GetBrokenDownloads();
			ReDownload(ids.Select(id => id.AsUInt64), new FileInfo(steamPath));
		}
		catch (Exception ex) { ex.Log(); }
	}

	public static void ReDownload(IEnumerable<ulong> ids, FileInfo steam)
	{
		try
		{
			Log.Called(ids, steam);
			Assertion.Assert(steam.Exists);
			var steamDir = steam.DirectoryName;
			var steamExe = steam.Name;
			void ExecuteSteam(string args)
			{
				Execute(steamDir, steamExe, args).WaitForExit();
				Thread.Sleep(30);
			}

			ExecuteSteam("steam://open/console"); // so that user can see what is happening.
			Thread.Sleep(100); // wait until steam is ready.

			ExecuteSteam($"+workshop_download_item 255710 {ids.FirstOrDefault()}");
			Thread.Sleep(100);

			foreach (var id in ids)
			{
				ExecuteSteam($"+workshop_download_item 255710 {id}");
			}

			ExecuteSteam("steam://open/downloads");
		}
		catch (Exception ex) { ex.Log(); }
	}

	public Coroutine DeleteUnsubbed()
	{
		return StartCoroutine(DeleteUnsubbedCoroutine());
	}

	public IEnumerator DeleteUnsubbedCoroutine()
	{
		Log.DisplayMesage($"Deleting unsubscribed items.");
		var items = PlatformService.workshop.GetSubscribedItems();
		if (items == null || items.Length == 0)
		{
			yield break;
		}

		var path = PlatformService.workshop.GetSubscribedItemPath(items[0]);
		path = Path.GetDirectoryName(path);

		var counter = 0;
		var n = 0;
		foreach (var dir in Directory.GetDirectories(path))
		{
			var strID = Path.GetFileName(dir);
			if (strID.StartsWith("_"))
			{
				strID = strID.Substring(1);
			}

			if (!ulong.TryParse(strID, out var id))
			{
				continue;
			}

			var deleted = !items.Any(item => item.AsUInt64 == id);
			if (deleted)
			{
				Log.DisplayWarning("unsubbed item will be deleted: " + dir);
				Directory.Delete(dir, true);
				n++;
			}

			if (counter >= 100)
			{
				counter = 0;
				yield return 0;
			}
		}

		Log.DisplayMesage($"Deleted {n} unsubscribed items.");
	}

	//public Coroutine Resubscribe(PublishedFileId id) => StartCoroutine(ResubscribeCoroutine(id));
	//public IEnumerator ResubscribeCoroutine(PublishedFileId id) {
	//    Log.Called(id);
	//    if (id != PublishedFileId.invalid) {
	//        try { PlatformService.workshop.Unsubscribe(id); } catch(Exception ex) { ex.Log(); }

	//        yield return new WaitForSeconds(3);

	//        try {
	//            //string path = PlatformService.workshop.GetSubscribedItemPath(id);
	//            //if (Directory.Exists(path))
	//            //    Directory.Delete(path, true);
	//        } catch(Exception ex) { ex.Log(); }

	//        try { PlatformService.workshop.Subscribe(id); } catch (Exception ex) { ex.Log(); }
	//    }
	//}

	public static bool IsWorkshop(string path)
	{
		return path != null && path.Contains(ConfigUtil.Config.WorkShopContentPath);
	}

	public static PublishedFileId GetID1(string dir)
	{
		if (dir != null && dir.Contains(ConfigUtil.Config.WorkShopContentPath))
		{
			var dirName = new DirectoryInfo(dir).Name;
			if (ulong.TryParse(dirName, out var id))
			{
				return new PublishedFileId(id);
			}
		}
		return PublishedFileId.invalid;
	}
	public static PublishedFileId GetID2(string file)
	{
		return GetID1(new FileInfo(file).DirectoryName);
	}
}