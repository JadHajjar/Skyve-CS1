using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.Domain.Utilities;
using LoadOrderToolTwo.Utilities.IO;
using LoadOrderToolTwo.Utilities.Managers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LoadOrderToolTwo.Utilities;
internal static class ModsUtil
{
	private static readonly CachedSaveLibrary<CachedModInclusion, Mod, bool> _includedLibrary = new();
	private static readonly CachedSaveLibrary<CachedModEnabled, Mod, bool> _enabledLibrary = new();

	static ModsUtil()
	{
		CitiesManager.MonitorTick += CitiesManager_MonitorTick;
	}

	private static void CitiesManager_MonitorTick(bool isAvailable, bool isRunning)
	{
		if (!isRunning && !ProfileManager.ApplyingProfile && (_includedLibrary.Any() || _enabledLibrary.Any()))
		{
			SavePendingValues();
		}
	}

	public static Mod? GetMod(Package package)
	{
		if (IsValidModFolder(package.Folder, out var dllPath, out var version))
		{
			return new Mod(package, dllPath!, version!);
		}

		return null;
	}

	private static bool IsValidModFolder(string dir, out string? dllPath, out Version? version)
	{
		try
		{
			var files = Directory.GetFiles(dir, "*.dll", SearchOption.AllDirectories);

			if (files != null && files.Length > 0)
			{
				return AssemblyUtil.FindImplementation(files, "ICities.IUserMod", out dllPath, out version);
			}
		}
		catch { }

		dllPath = null;
		version = null;
		return false;
	}

	internal static void SavePendingValues()
	{
		var saveSettings = _enabledLibrary.Any();

		_includedLibrary.Save();
		_enabledLibrary.Save();

		if (saveSettings)
		{
			ColossalOrderUtil.SaveSettings();
		}
	}

	internal static bool IsIncluded(Mod mod)
	{
		return _includedLibrary.GetValue(mod, out var included) ? included : IsLocallyIncluded(mod);
	}

	internal static bool IsEnabled(Mod mod)
	{
		return _enabledLibrary.GetValue(mod, out var enabled) ? enabled : IsLocallyEnabled(mod);
	}

	internal static bool IsLocallyIncluded(Mod mod)
	{
		return !File.Exists(Path.Combine(mod.Folder, ContentUtil.EXCLUDED_FILE_NAME));
	}

	internal static bool IsLocallyEnabled(Mod mod)
	{
		return ColossalOrderUtil.IsEnabled(mod);
	}

	internal static void SetIncluded(Mod mod, bool value)
	{
		if (ProfileManager.ApplyingProfile || CitiesManager.IsRunning())
		{
			_includedLibrary.SetValue(mod, value);
		}
		else
		{
			SetLocallyIncluded(mod, value);
		}

		if (!CentralManager.SessionSettings.AdvancedIncludeEnable)
		{
			SetEnabled(mod, value);
			return;
		}

		CentralManager.InformationUpdate(mod.Package);
		ProfileManager.TriggerAutoSave();
	}

	internal static void SetIncluded(IEnumerable<Mod> mods, bool value)
	{
		var list = mods.ToList();

		for (var i = 0; i < list.Count; i++)
		{
			_includedLibrary.SetValue(list[i], value);
			CentralManager.InformationUpdate(list[i]);
		}

		if (!CentralManager.SessionSettings.AdvancedIncludeEnable)
		{
			SetEnabled(list, value);
			return;
		}

		if (!ProfileManager.ApplyingProfile && !CitiesManager.IsRunning())
		{
			SavePendingValues();
		}

		ProfileManager.TriggerAutoSave();
	}

	internal static void SetLocallyIncluded(Mod mod, bool value)
	{
		if ((value || ModLogicManager.IsRequired(mod)) && !ModLogicManager.IsForbidden(mod))
		{
			File.Delete(Path.Combine(mod.Folder, ContentUtil.EXCLUDED_FILE_NAME));
		}
		else
		{
			File.WriteAllBytes(Path.Combine(mod.Folder, ContentUtil.EXCLUDED_FILE_NAME), new byte[0]);
		}

		if (CentralManager.SessionSettings.LinkModAssets && mod.Package.Assets != null)
		{
			foreach (var asset in mod.Package.Assets)
			{
				asset.IsIncluded = value;
			}
		}
	}

	internal static void SetEnabled(Mod mod, bool value, bool save = true)
	{
		if (ProfileManager.ApplyingProfile || CitiesManager.IsRunning())
		{
			_enabledLibrary.SetValue(mod, value);
		}
		else
		{
			SetLocallyEnabled(mod, value, save);
		}

		CentralManager.InformationUpdate(mod.Package);
		ProfileManager.TriggerAutoSave();
	}

	internal static void SetEnabled(IEnumerable<Mod> mods, bool value)
	{
		var list = mods.ToList();

		for (var i = 0; i < list.Count; i++)
		{
			_enabledLibrary.SetValue(list[i], value);

			CentralManager.InformationUpdate(list[i]);
		}

		if (!ProfileManager.ApplyingProfile && !CitiesManager.IsRunning())
		{
			SavePendingValues();
		}

		ProfileManager.TriggerAutoSave();
	}

	internal static void SetLocallyEnabled(Mod mod, bool value, bool save)
	{
		if (ModLogicManager.IsRequired(mod))
		{
			value = true;
		}
		else if (ModLogicManager.IsForbidden(mod))
		{
			value = false;
		}

		ColossalOrderUtil.SetEnabled(mod, value);

		if (save)
		{
			ColossalOrderUtil.SaveSettings();
		}
	}

	public static DownloadStatus GetStatus(IPackage mod, out string reason)
	{
		if (!mod.Workshop)
		{
			reason = Locale.ModIsLocal;
			return DownloadStatus.OK;
		}

		if (mod.RemovedFromSteam)
		{
			reason = Locale.ModIsRemoved;
			return DownloadStatus.Removed;
		}

		if (!mod.SteamInfoLoaded)
		{
			reason = Locale.ModIsUnknown;
			return DownloadStatus.Unknown;
		}

		if (!Directory.Exists(mod.Folder))
		{
			reason = Locale.ModIsNotDownloaded;
			return DownloadStatus.NotDownloaded;
		}

		var updatedServer = mod.ServerTime;
		var updatedLocal = mod.LocalTime;
		var sizeServer = mod.ServerSize;
		var localSize = ContentUtil.GetTotalSize(mod.Folder);

		if (updatedLocal < updatedServer)
		{
			var certain =
				localSize < sizeServer ||
				updatedLocal < updatedServer.AddHours(-24);

			reason = certain ? Locale.ModIsOutOfDate : Locale.ModIsMaybeOutOfDate;
			reason += $"\r\n{Locale.Server}: {updatedServer:g} | {Locale.Local}: {updatedLocal:g}";
			return DownloadStatus.OutOfDate;
		}

		if (localSize < sizeServer)
		{
			reason = Locale.ModIsIncomplete;
			reason += $"\r\n{Locale.Server}: {sizeServer.SizeString()} | {Locale.Local}: {localSize.SizeString()}";
			return DownloadStatus.PartiallyDownloaded;
		}

		reason = Locale.ModIsUpToDate;
		return DownloadStatus.OK;
	}

	public static IEnumerable<IGrouping<string, Mod>> GetDuplicateMods()
	{
		return CentralManager.Mods
			.Where(x => x.IsIncluded)
			.GroupBy(x => Path.GetFileName(x.FileName))
			.Where(x => x.Count() > 1);
	}

	internal static Mod FindMod(string? folder)
	{
		return CentralManager.Mods.FirstOrDefault(x => x.Folder.Equals(folder, StringComparison.InvariantCultureIgnoreCase));
	}

	internal static Mod FindMod(ulong steamID)
	{
		return CentralManager.Mods.FirstOrDefault(x => x.SteamId == steamID);
	}

	internal static string RemoveVersionText(this string name)
	{
		return Regex.Replace(name, @"v?\d+\.\d+(\.\d+)*(-[\d\w]+)*", x => string.Empty, RegexOptions.IgnoreCase)
			.RemoveDoubleSpaces();
	}
}
