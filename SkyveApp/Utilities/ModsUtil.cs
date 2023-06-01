using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Compatibility.Enums;
using SkyveApp.Domain.Enums;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Domain.Utilities;
using SkyveApp.Utilities.IO;
using SkyveApp.Utilities.Managers;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SkyveApp.Utilities;
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
				if (LocationManager.Platform is Platform.MacOSX)
				{
					return MacAssemblyUtil.FindImplementation(files, out dllPath, out version);
				}

				return AssemblyUtil.FindImplementation(files, out dllPath, out version);
			}
		}
		catch { }

		dllPath = null;
		version = null;
		return false;
	}

	internal static void SavePendingValues()
	{
		if (ProfileManager.ApplyingProfile || ContentUtil.BulkUpdating)
		{
			return;
		}

#if DEBUG
		Log.Debug("Saving pending mod values:\r\n" +
			$"_includedLibrary {_includedLibrary._dictionary.Count}\r\n" +
			$"_enabledLibrary {_enabledLibrary._dictionary.Count}");
#endif
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
		return !ExtensionClass.FileExists(LocationManager.Combine(mod.Folder, ContentUtil.EXCLUDED_FILE_NAME));
	}

	internal static bool IsLocallyEnabled(Mod mod)
	{
		return ColossalOrderUtil.IsEnabled(mod);
	}

	internal static void SetIncluded(Mod mod, bool value)
	{
		if (!value && ModLogicManager.IsRequired(mod))
		{
			value = true;
		}

		if (ProfileManager.ApplyingProfile || ContentUtil.BulkUpdating || CitiesManager.IsRunning())
		{
#if DEBUG
			Log.Debug($"Delaying inclusion ({value}) for mod: {mod} (currently {IsLocallyIncluded(mod)}) ({mod.Folder})");
#endif
			_includedLibrary.SetValue(mod, value);
		}
		else
		{
			SetLocallyIncluded(mod, value);
		}

		if (CentralManager.SessionSettings.UserSettings.LinkModAssets && mod.Package.Assets != null)
		{
			foreach (var asset in mod.Package.Assets)
			{
				asset.IsIncluded = value;
			}
		}

		if (!CentralManager.SessionSettings.UserSettings.AdvancedIncludeEnable)
		{
			SetEnabled(mod, value);
			return;
		}

		CentralManager.OnInclusionUpdated();
		ProfileManager.TriggerAutoSave();
	}

	internal static void SetLocallyIncluded(Mod mod, bool value)
	{
		try
		{
#if DEBUG
			Log.Debug($"Applying Inclusion status ({value}) for mod: {mod} ({mod.Folder})");
#endif
			if ((value || ModLogicManager.IsRequired(mod)) && !ModLogicManager.IsForbidden(mod))
			{
#if DEBUG
				Log.Debug($"Deleting the file ({LocationManager.Combine(mod.Folder, ContentUtil.EXCLUDED_FILE_NAME)})");
#endif
				ExtensionClass.DeleteFile(LocationManager.Combine(mod.Folder, ContentUtil.EXCLUDED_FILE_NAME));
			}
			else
			{
#if DEBUG
				Log.Debug($"Creating the file ({LocationManager.Combine(mod.Folder, ContentUtil.EXCLUDED_FILE_NAME)})");
#endif
				File.WriteAllBytes(LocationManager.Combine(mod.Folder, ContentUtil.EXCLUDED_FILE_NAME), new byte[0]);
			}
		}
		catch (Exception ex)
		{
			Log.Exception(ex, $"Failed to set 'Included' status ({value}) for {mod}");
		}
	}

	internal static void SetEnabled(Mod mod, bool value, bool save = true)
	{
		if (ProfileManager.ApplyingProfile || ContentUtil.BulkUpdating || CitiesManager.IsRunning())
		{
			_enabledLibrary.SetValue(mod, value);
		}
		else
		{
			SetLocallyEnabled(mod, value, save);
		}

		CentralManager.OnInclusionUpdated();
		ProfileManager.TriggerAutoSave();
	}

	internal static void SetLocallyEnabled(Mod mod, bool value, bool save)
	{
		try
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
		catch (Exception ex)
		{
			Log.Exception(ex, $"Failed to set 'Enabled' status ({value}) for {mod}");
		}
	}

	public static DownloadStatus GetStatus(IPackage mod, out string reason)
	{
		if (mod.RemovedFromSteam)
		{
			reason = Locale.PackageIsRemoved.Format(mod.CleanName());
			return DownloadStatus.Removed;
		}

		if (mod.ServerTime == default)
		{
			if (!mod.Workshop)
			{
				reason = string.Empty;
				return DownloadStatus.None;
			}

			reason = Locale.PackageIsUnknown.Format(mod.CleanName());
			return DownloadStatus.Unknown;
		}

		//if (!Directory.Exists(mod.Folder))
		//{
		//	reason = Locale.PackageIsNotDownloaded.Format(mod.CleanName());
		//	return DownloadStatus.NotDownloaded;
		//}

		var updatedServer = mod.ServerTime;
		var updatedLocal = mod.Package?.LocalTime ?? DateTime.MinValue;
		var sizeServer = mod.ServerSize;
		var localSize = mod.Package?.FileSize ?? 0;

		if (updatedLocal < updatedServer)
		{
			var certain = updatedLocal < updatedServer.AddHours(-24);

			reason = certain
				? Locale.PackageIsOutOfDate.Format(mod.CleanName(), (updatedServer - updatedLocal).ToReadableString(true))
				: Locale.PackageIsMaybeOutOfDate.Format(mod.CleanName(), updatedServer.ToLocalTime().ToRelatedString(true));
			return DownloadStatus.OutOfDate;
		}

		if (localSize < sizeServer && sizeServer > 0)
		{
			reason = Locale.PackageIsIncomplete.Format(mod.CleanName(), localSize.SizeString(), sizeServer.SizeString());
			return DownloadStatus.PartiallyDownloaded;
		}

		reason = string.Empty;
		return DownloadStatus.OK;
	}

	internal static Mod FindMod(string? folder)
	{
		return CentralManager.Mods.FirstOrDefault(x => x.Folder.Equals(folder, StringComparison.OrdinalIgnoreCase));
	}

	internal static Mod FindMod(ulong steamID)
	{
		return CentralManager.Mods.FirstOrDefault(x => x.SteamId == steamID);
	}

	internal static string CleanName(this IPackage package)
	{
		if (package?.Name is null)
		{
			return string.Empty;
		}

		var text = package.Name.RegexRemove(@"(?<!Catalogue\s+)v?\d+\.\d+(\.\d+)*(-[\d\w]+)*");

		return text.RegexRemove(@"[\[\(](.+?)[\]\)]").RemoveDoubleSpaces().Trim('-', ']', '[', '(', ')', ' ');
	}

	internal static string CleanName(this IPackage package, out List<(Color Color, string Text)> tags)
	{
		tags = new();

		if (package?.Name is null)
		{
			return string.Empty;
		}

		var text = package.Name.RegexRemove(@"(?<!Catalogue\s+)v?\d+\.\d+(\.\d+)*(-[\d\w]+)*");
		var tagMatches = Regex.Matches(text, @"[\[\(](.+?)[\]\)]");

		text = text.RegexRemove(@"[\[\(](.+?)[\]\)]").RemoveDoubleSpaces().Trim('-', ']', '[', '(', ')', ' ');


		foreach (Match match in tagMatches)
		{
			var tagText = match.Groups[1].Value.Trim();

			if (!tags.Any(x => x.Text == tagText))
			{
				if (tagText.ToLower() is "stable" or "deprecated" or "obsolete" or "abandoned" or "broken")
				{ continue; }

				var color = tagText.ToLower() switch
				{
					"alpha" or "experimental" => Color.FromArgb(200, FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.RedColor)),
					"beta" or "test" or "testing" => Color.FromArgb(180, FormDesign.Design.YellowColor),
					_ => (Color?)null
				};

				tags.Add((color ?? FormDesign.Design.ButtonColor, color is null ? tagText : LocaleHelper.GetGlobalText(tagText).One.ToUpper()));
			}
		}

		if (package.Incompatible)
		{
			tags.Add((FormDesign.Design.RedColor, LocaleCR.Incompatible.One.ToUpper()));
		}
		else
		{
			var compatibility = package.GetCompatibilityInfo();

			if (compatibility.Data?.Package.Stability is PackageStability.Broken)
			{
				tags.Add((Color.FromArgb(225, FormDesign.Design.RedColor), LocaleCR.Broken.One.ToUpper()));
			}
		}

		return text;
	}

	internal static string GetVersionText(this string name)
	{
		var match = Regex.Match(name, @"v?(\d+\.\d+(\.\d+)*(-[\d\w]+)*)", RegexOptions.IgnoreCase);

		if (match.Success)
		{
			return "v" + match.Groups[1].Value;
		}

		return string.Empty;
	}
}
