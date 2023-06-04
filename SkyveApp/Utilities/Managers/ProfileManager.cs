using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Compatibility.Enums;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Domain.Utilities;

using SkyveShared;

using SlickControls;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SkyveApp.Utilities.Managers;
public static class ProfileManager
{
	internal const string LOCAL_APP_DATA_PATH = "%LOCALAPPDATA%";
	internal const string CITIES_PATH = "%CITIES%";
	internal const string WS_CONTENT_PATH = "%WORKSHOP%";

	private static readonly List<Profile> _profiles;
	private static bool disableAutoSave;
	private static readonly FileSystemWatcher? _watcher;

	public static bool ApplyingProfile { get; private set; }
	public static Profile CurrentProfile { get; private set; }
	public static IEnumerable<Profile> Profiles
	{
		get
		{
			yield return Profile.TemporaryProfile;

			List<Profile> profiles;

			lock (_profiles)
			{
				profiles = new(_profiles);
			}

			foreach (var profile in profiles)
			{
				yield return profile;
			}
		}
	}

	public static bool ProfilesLoaded { get; private set; }

	public static event Action<Profile>? ProfileChanged;

	public static event Action? ProfileUpdated;

	static ProfileManager()
	{
		try
		{
			var current = LoadCurrentProfile();

			if (current != null)
			{
				_profiles = new() { current };
				CurrentProfile = current;
			}
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "Failed to load the current profile");
		}

		_profiles ??= new();

		CurrentProfile ??= Profile.TemporaryProfile;

		if (Directory.Exists(LocationManager.SkyveAppDataPath))
		{
			Directory.CreateDirectory(LocationManager.SkyveProfilesAppDataPath);

			_watcher = new FileSystemWatcher
			{
				Path = LocationManager.SkyveProfilesAppDataPath,
				NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
				Filter = "*.json"
			};
		}

		if (!CommandUtil.NoWindow)
		{
			new BackgroundAction(ConvertLegacyProfiles).Run();
			new BackgroundAction(LoadAllProfiles).Run();
		}
	}

	private static Profile? LoadCurrentProfile()
	{
		if (CentralManager.SessionSettings.CurrentProfile is null or "" && CommandUtil.PreSelectedProfile is null or "")
		{
			return null;
		}

		var profile = LocationManager.Combine(LocationManager.SkyveProfilesAppDataPath, (CommandUtil.PreSelectedProfile ?? CentralManager.SessionSettings.CurrentProfile) + ".json");

		if (!ExtensionClass.FileExists(profile))
		{
			return null;
		}

		var newProfile = Newtonsoft.Json.JsonConvert.DeserializeObject<Profile>(File.ReadAllText(profile));

		if (newProfile != null)
		{
			newProfile.Name = Path.GetFileNameWithoutExtension(profile);
			newProfile.LastEditDate = File.GetLastWriteTime(profile);
			newProfile.DateCreated = File.GetCreationTime(profile);

			if (newProfile.LastUsed == DateTime.MinValue)
			{
				newProfile.LastUsed = newProfile.LastEditDate;
			}

			return newProfile;
		}
		else
		{
			Log.Error($"Could not load the profile: '{profile}'");
		}

		return null;
	}

	public static void ConvertLegacyProfiles()
	{
		if (!CentralManager.SessionSettings.FirstTimeSetupCompleted)
		{
			return;
		}

		Log.Info("Checking for Legacy profiles");

		var legacyProfilePath = LocationManager.Combine(LocationManager.AppDataPath, "LoadOrder", "LOMProfiles");

		if (!Directory.Exists(legacyProfilePath))
		{
			return;
		}

		Log.Info("Checking for Legacy profiles");

		foreach (var profile in Directory.EnumerateFiles(legacyProfilePath, "*.xml"))
		{
			var newName = LocationManager.Combine(LocationManager.SkyveProfilesAppDataPath, $"{Path.GetFileNameWithoutExtension(profile)}.json");

			if (ExtensionClass.FileExists(newName))
			{
				Log.Info($"Profile '{newName}' already exists, skipping..");
				continue;
			}

			Log.Info($"Converting profile '{newName}'..");

			ConvertLegacyProfile(profile);
		}
	}

	internal static Profile? ConvertLegacyProfile(string profilePath, bool removeLegacyFile = true)
	{
		var legacyProfilePath = LocationManager.Combine(LocationManager.AppDataPath, "LoadOrder", "LOMProfiles");
		var legacyProfile = LoadOrderTool.Legacy.LoadOrderProfile.Deserialize(profilePath);
		var newProfile = legacyProfile?.ToLot2Profile(Path.GetFileNameWithoutExtension(profilePath));

		if (newProfile != null)
		{
			newProfile.LastEditDate = File.GetLastWriteTime(profilePath);
			newProfile.DateCreated = File.GetCreationTime(profilePath);
			newProfile.LastUsed = newProfile.LastEditDate;

			lock (_profiles)
			{
				_profiles.Add(newProfile);
			}

			Save(newProfile, true);

			if (removeLegacyFile)
			{
				Directory.CreateDirectory(LocationManager.Combine(legacyProfilePath, "Legacy"));

				File.Move(profilePath, LocationManager.Combine(legacyProfilePath, "Legacy", Path.GetFileName(profilePath)));
			}
		}
		else
		{
			Log.Error($"Could not load the profile: '{profilePath}'");
		}

		return newProfile;
	}

	private static void CentralManager_ContentLoaded()
	{
		new BackgroundAction(() =>
		{
			List<Profile> profiles;

			lock (_profiles)
			{
				profiles = new(_profiles);
			}

			foreach (var profile in profiles)
			{
				profile.IsMissingItems = profile.Mods.Any(x => GetMod(x) is null) || profile.Assets.Any(x => GetAsset(x) is null);
#if DEBUG
				if (profile.IsMissingItems)
				{
					Log.Debug($"Missing items in the profile: {profile}\r\n" +
						profile.Mods.Where(x => GetMod(x) is null).ListStrings(x => $"{x.Name} ({ToLocalPath(x.RelativePath)})", "\r\n") + "\r\n" +
						profile.Assets.Where(x => GetAsset(x) is null).ListStrings(x => $"{x.Name} ({ToLocalPath(x.RelativePath)})", "\r\n"));
				}
#endif
			}

			ProfileUpdated?.Invoke();
		}).Run();
	}

	internal static void MergeProfile(Profile profile)
	{
		new BackgroundAction("Applying profile", apply).Run();

		void apply()
		{
			try
			{
				var unprocessedMods = CentralManager.Mods.ToList();
				var unprocessedAssets = CentralManager.Assets.ToList();
				var missingMods = new List<Profile.Mod>();
				var missingAssets = new List<Profile.Asset>();

				ApplyingProfile = true;

				foreach (var mod in profile.Mods)
				{
					var localMod = GetMod(mod);

					if (localMod != null)
					{
						localMod.IsIncluded = true;
						localMod.IsEnabled |= mod.Enabled;

						unprocessedMods.Remove(localMod);
					}
					else
					{
						missingMods.Add(mod);
					}
				}

				foreach (var asset in profile.Assets)
				{
					var localAsset = GetAsset(asset);

					if (localAsset != null)
					{
						localAsset.IsIncluded = true;

						unprocessedAssets.Remove(localAsset);
					}
					else
					{
						missingAssets.Add(asset);
					}
				}

				if ((missingMods.Count > 0 || missingAssets.Count > 0) && Program.MainForm is not null)
				{
					UserInterface.Panels.PC_MissingPackages.PromptMissingPackages(Program.MainForm, missingMods, missingAssets);
				}

				ApplyingProfile = false;
				disableAutoSave = true;

				ModsUtil.SavePendingValues();
				AssetsUtil.SaveChanges();

				disableAutoSave = false;

				ProfileChanged?.Invoke(CurrentProfile);

				TriggerAutoSave();
			}
			catch (Exception ex)
			{
				MessagePrompt.Show(ex, "Failed to merge your profiles", form: Program.MainForm);
			}
			finally
			{
				ApplyingProfile = false;
				disableAutoSave = false;
			}
		}
	}

	internal static void ExcludeProfile(Profile profile)
	{
		new BackgroundAction("Applying profile", apply).Run();

		void apply()
		{
			try
			{
				var unprocessedMods = CentralManager.Mods.ToList();
				var unprocessedAssets = CentralManager.Assets.ToList();
				var missingMods = new List<Profile.Mod>();
				var missingAssets = new List<Profile.Asset>();

				ApplyingProfile = true;

				foreach (var mod in profile.Mods)
				{
					var localMod = GetMod(mod);

					if (localMod != null)
					{
						localMod.IsIncluded = false;
						localMod.IsEnabled = false;
					}
				}

				foreach (var asset in profile.Assets)
				{
					var localAsset = GetAsset(asset);

					if (localAsset != null)
					{
						localAsset.IsIncluded = false;
					}
				}

				ApplyingProfile = false;
				disableAutoSave = true;

				ModsUtil.SavePendingValues();
				AssetsUtil.SaveChanges();

				disableAutoSave = false;

				ProfileChanged?.Invoke(CurrentProfile);

				TriggerAutoSave();
			}
			catch (Exception ex)
			{
				MessagePrompt.Show(ex, "Failed to merge your profiles", form: Program.MainForm);
			}
			finally
			{
				ApplyingProfile = false;
				disableAutoSave = false;
			}
		}
	}

	internal static void DeleteProfile(Profile profile)
	{
		ExtensionClass.DeleteFile(LocationManager.Combine(LocationManager.SkyveProfilesAppDataPath, $"{profile.Name}.json"));

		lock (_profiles)
		{
			_profiles.Remove(profile);
		}

		if (profile == CurrentProfile)
		{
			SetProfile(Profile.TemporaryProfile);
		}

		ProfileUpdated?.Invoke();
	}

	internal static void SetProfile(Profile profile)
	{
		CurrentProfile = profile;

		if (profile.Temporary)
		{
			ProfileChanged?.Invoke(profile);

			CentralManager.SessionSettings.CurrentProfile = null;
			CentralManager.SessionSettings.Save();

			return;
		}

		if (Program.MainForm is null)
		{
			apply();
		}
		else
		{
			new BackgroundAction("Applying profile", apply).Run();
		}

		void apply()
		{
			try
			{
				var unprocessedMods = CentralManager.Mods.ToList();
				var unprocessedAssets = CentralManager.Assets.ToList();
				var missingMods = new List<Profile.Mod>();
				var missingAssets = new List<Profile.Asset>();

				ApplyingProfile = true;

				foreach (var mod in profile.Mods)
				{
					var localMod = GetMod(mod);

					if (localMod != null)
					{
						localMod.IsIncluded = true;
						localMod.IsEnabled = mod.Enabled;

						unprocessedMods.Remove(localMod);
					}
					else if (!CompatibilityManager.IsBlacklisted(mod))
					{
						missingMods.Add(mod);
					}
				}

				foreach (var asset in profile.Assets)
				{
					var localAsset = GetAsset(asset);

					if (localAsset != null)
					{
						localAsset.IsIncluded = true;

						unprocessedAssets.Remove(localAsset);
					}
					else if (!CompatibilityManager.IsBlacklisted(asset))
					{
						missingAssets.Add(asset);
					}
				}

				foreach (var mod in unprocessedMods)
				{
					mod.IsIncluded = false;
					mod.IsEnabled = false;
				}

				foreach (var asset in unprocessedAssets)
				{
					asset.IsIncluded = false;
				}

#if DEBUG
				Log.Debug($"unprocessedMods: {unprocessedMods.Count}\r\n" +
					$"unprocessedAssets: {unprocessedAssets.Count}\r\n" +
					$"missingMods: {missingMods.Count}\r\n" +
					$"missingAssets: {missingAssets.Count}");
#endif

				if ((missingMods.Count > 0 || missingAssets.Count > 0) && Program.MainForm is not null)
				{
					UserInterface.Panels.PC_MissingPackages.PromptMissingPackages(Program.MainForm, missingMods, missingAssets);
				}

				AssetsUtil.SetDlcsExcluded(profile.ExcludedDLCs.ToArray());

				ApplyingProfile = false;
				disableAutoSave = true;

				ModsUtil.SavePendingValues();
				AssetsUtil.SaveChanges();

				profile.LastUsed = DateTime.Now;
				Save(profile);

				ProfileChanged?.Invoke(profile);

				try
				{ SaveLsmSettings(profile); }
				catch (Exception ex) { Log.Exception(ex, "Failed to apply the LSM settings for profile " + profile.Name); }

				if (!CommandUtil.NoWindow)
				{
					CentralManager.SessionSettings.CurrentProfile = profile.Name;
					CentralManager.SessionSettings.Save();
				}

				disableAutoSave = false;
			}
			catch (Exception ex)
			{
				MessagePrompt.Show(ex, "Failed to merge your profiles", form: Program.MainForm);

				ProfileChanged?.Invoke(profile);
			}
			finally
			{
				ApplyingProfile = false;
				disableAutoSave = false;
			}
		}
	}

	internal static void TriggerAutoSave()
	{
		if (CurrentProfile.AutoSave && !disableAutoSave && !ApplyingProfile && CentralManager.IsContentLoaded && !ContentUtil.BulkUpdating)
		{
			CurrentProfile.Save();
		}
	}

	private static void LoadAllProfiles()
	{
		try
		{
			foreach (var profile in Directory.EnumerateFiles(LocationManager.SkyveProfilesAppDataPath, "*.json"))
			{
				if (Path.GetFileNameWithoutExtension(profile).Equals(CentralManager.SessionSettings.CurrentProfile, StringComparison.CurrentCultureIgnoreCase))
				{
					continue;
				}

				var newProfile = Newtonsoft.Json.JsonConvert.DeserializeObject<Profile>(File.ReadAllText(profile));

				if (newProfile != null)
				{
					newProfile.Name = Path.GetFileNameWithoutExtension(profile);
					newProfile.LastEditDate = File.GetLastWriteTime(profile);
					newProfile.DateCreated = File.GetCreationTime(profile);

					if (newProfile.LastUsed == DateTime.MinValue)
					{
						newProfile.LastUsed = newProfile.LastEditDate;
					}

					lock (_profiles)
					{
						_profiles.Add(newProfile);
					}
				}
				else
				{
					Log.Error($"Could not load the profile: '{profile}'");
				}
			}
		}
		catch { }

		if (CentralManager.IsContentLoaded)
		{
			CentralManager_ContentLoaded();
		}

		CentralManager.ContentLoaded += CentralManager_ContentLoaded;

		ProfilesLoaded = true;

		ProfileUpdated?.Invoke();

		if (_watcher is not null)
		{
			_watcher.Changed += new FileSystemEventHandler(FileChanged);
			_watcher.Created += new FileSystemEventHandler(FileChanged);
			_watcher.Deleted += new FileSystemEventHandler(FileChanged);

			try
			{ _watcher.EnableRaisingEvents = true; }
			catch (Exception ex) { Log.Exception(ex, $"Failed to start profile watcher ({LocationManager.SkyveProfilesAppDataPath})"); }
		}
	}

	private static void FileChanged(object sender, FileSystemEventArgs e)
	{
		try
		{
			if (!Path.GetExtension(e.FullPath).Equals(".json", StringComparison.CurrentCultureIgnoreCase))
			{
				return;
			}

			if (!ExtensionClass.FileExists(e.FullPath))
			{
				lock (_profiles)
				{
					var profile = _profiles.FirstOrDefault(x => x.Name?.Equals(Path.GetFileNameWithoutExtension(e.FullPath), StringComparison.OrdinalIgnoreCase) ?? false);

					if (profile != null)
					{
						_profiles.Remove(profile);
					}
				}

				ProfileUpdated?.Invoke();

				return;
			}

			var newProfile = Newtonsoft.Json.JsonConvert.DeserializeObject<Profile>(File.ReadAllText(e.FullPath));

			if (newProfile != null)
			{
				newProfile.Name = Path.GetFileNameWithoutExtension(e.FullPath);
				newProfile.LastEditDate = File.GetLastWriteTime(e.FullPath);
				newProfile.DateCreated = File.GetCreationTime(e.FullPath);

				if (newProfile.LastUsed == DateTime.MinValue)
				{
					newProfile.LastUsed = newProfile.LastEditDate;
				}

				lock (_profiles)
				{
					var currentProfile = _profiles.FirstOrDefault(x => x.Name?.Equals(Path.GetFileNameWithoutExtension(e.FullPath), StringComparison.OrdinalIgnoreCase) ?? false);

					_profiles.Remove(currentProfile);

					_profiles.Add(newProfile);
				}

				ProfileUpdated?.Invoke();
			}
			else
			{
				Log.Error($"Could not load the profile: '{e.FullPath}'");
			}
		}
		catch (Exception ex) { Log.Exception(ex, "Failed to refresh changes to profiles"); }
	}

	public static void GatherInformation(Profile? profile)
	{
		if (profile == null || profile.Temporary || !CentralManager.IsContentLoaded)
		{
			return;
		}

		profile.Assets = CentralManager.Assets.Where(x => x.IsIncluded).Select(x => new Profile.Asset(x)).ToList();
		profile.Mods = CentralManager.Mods.Where(x => x.IsIncluded).Select(x => new Profile.Mod(x)).ToList();
		profile.ExcludedDLCs = SkyveConfig.Deserialize()?.RemovedDLCs.ToList() ?? new();
	}

	public static bool Save(Profile? profile, bool forced = false)
	{
		if (profile == null || (!forced && (profile.Temporary || !CentralManager.IsContentLoaded)))
		{
			return false;
		}

		try
		{
			if (_watcher is not null)
			{
				_watcher.EnableRaisingEvents = false;
			}

			Directory.CreateDirectory(LocationManager.SkyveProfilesAppDataPath);

			File.WriteAllText(
				LocationManager.Combine(LocationManager.SkyveProfilesAppDataPath, $"{profile.Name}.json"),
				Newtonsoft.Json.JsonConvert.SerializeObject(profile, Newtonsoft.Json.Formatting.Indented));

			profile.IsMissingItems = profile.Mods.Any(x => GetMod(x) is null) || profile.Assets.Any(x => GetAsset(x) is null);
#if DEBUG
			if (profile.IsMissingItems)
			{
				Log.Debug($"Missing items in the profile: {profile}\r\n" +
					profile.Mods.Where(x => GetMod(x) is null).ListStrings(x => $"{x.Name} ({ToLocalPath(x.RelativePath)})", "\r\n") + "\r\n" +
					profile.Assets.Where(x => GetAsset(x) is null).ListStrings(x => $"{x.Name} ({ToLocalPath(x.RelativePath)})", "\r\n"));
			}
#endif

			return true;
		}
		catch (Exception ex)
		{
			Log.Exception(ex, $"Failed to save profile ({profile.Name}) to {LocationManager.Combine(LocationManager.SkyveProfilesAppDataPath, $"{profile.Name}.json")}");
		}
		finally
		{
			if (_watcher is not null)
			{
				_watcher.EnableRaisingEvents = true;
			}
		}

		return false;
	}

	internal static Mod GetMod(Profile.Mod mod)
	{
		return ModsUtil.FindMod(ToLocalPath(mod.RelativePath));
	}

	internal static Asset GetAsset(Profile.Asset asset)
	{
		return AssetsUtil.GetAsset(ToLocalPath(asset.RelativePath));
	}

	internal static string ToRelativePath(string? localPath)
	{
		if (localPath is null or "")
		{
			return string.Empty;
		}

		return localPath
			.Replace(LocationManager.AppDataPath, LOCAL_APP_DATA_PATH)
			.Replace(LocationManager.GamePath, CITIES_PATH)
			.Replace(LocationManager.WorkshopContentPath, WS_CONTENT_PATH)
			.FormatPath();
	}

	internal static string ToLocalPath(string? relativePath)
	{
		if (relativePath is null or "")
		{
			return string.Empty;
		}

		return relativePath
			.Replace(LOCAL_APP_DATA_PATH, LocationManager.AppDataPath)
			.Replace(CITIES_PATH, LocationManager.GamePath)
			.Replace(WS_CONTENT_PATH, LocationManager.WorkshopContentPath)
			.FormatPath();
	}

	internal static bool RenameProfile(Profile profile, string text)
	{
		if (profile == null || profile.Temporary)
		{
			return false;
		}

		text = text.EscapeFileName();

		var newName = LocationManager.Combine(LocationManager.SkyveProfilesAppDataPath, $"{text}.json");
		var oldName = LocationManager.Combine(LocationManager.SkyveProfilesAppDataPath, $"{profile.Name}.json");

		try
		{
			if (newName == oldName)
			{
				return true;
			}

			if (ExtensionClass.FileExists(newName))
			{
				return false;
			}

			if (ExtensionClass.FileExists(oldName))
			{
				File.Move(oldName, newName);

				profile.Name = text;
			}
			else
			{
				profile.Name = text;

				Save(profile);
			}
		}
		finally
		{
			CentralManager.SessionSettings.CurrentProfile = text;
			CentralManager.SessionSettings.Save();
		}

		return true;
	}

	internal static string? GetNewProfileName()
	{
		var startName = LocationManager.Combine(LocationManager.SkyveProfilesAppDataPath, "New Profile.json");

		// Check if the file with the proposed name already exists
		if (ExtensionClass.FileExists(startName))
		{
			var extension = ".json";
			var nameWithoutExtension = LocationManager.Combine(LocationManager.SkyveProfilesAppDataPath, "New Profile");
			var counter = 1;

			// Loop until a valid file name is found
			while (ExtensionClass.FileExists(startName))
			{
				// Generate the new file name with the counter appended
				startName = $"{nameWithoutExtension} ({counter}){extension}";

				// Increment the counter
				counter++;
			}
		}

		// Return the valid file name
		return Path.GetFileNameWithoutExtension(startName);
	}

	internal static DynamicIcon GetIcon(this IProfile profile)
	{
		if (profile.Temporary)
		{
			return "I_TempProfile";
		}

		return profile.Usage switch
		{
			PackageUsage.CityBuilding => "I_City",
			PackageUsage.AssetCreation => "I_Tools",
			_ => "I_ProfileSettings"
		};
	}

	internal static List<Package> GetInvalidPackages(PackageUsage usage)
	{
		if ((int)usage == -1)
		{
			return new();
		}

		return CentralManager.Packages.AllWhere(x =>
		{
			var cr = x.GetCompatibilityInfo().Data;

			if (cr is null)
			{
				return false;
			}

			if (cr.Package.Usage.HasFlag(usage))
			{
				return false;
			}

			return x.IsIncluded;
		});
	}

	public static void SaveLsmSettings(Profile profile)
	{
		var current = LsmSettingsFile.Deserialize();

		if (current == null)
		{
			return;
		}

		current.loadEnabled = profile.LsmSettings.LoadEnabled;
		current.loadUsed = profile.LsmSettings.LoadUsed;
		current.skipFile = profile.LsmSettings.SkipFile;
		current.skipPrefabs = profile.LsmSettings.UseSkipFile;

		current.SyncAndSerialize();
	}

	internal static void AddProfile(Profile newProfile)
	{
		lock (_profiles)
		{
			_profiles.Add(newProfile);
		}

		ProfileUpdated?.Invoke();
	}

	internal static Profile? ImportProfile(string obj)
	{
		if (_watcher is not null)
		{
			_watcher.EnableRaisingEvents = false;
		}

		var newPath = LocationManager.Combine(LocationManager.SkyveProfilesAppDataPath, Path.GetFileName(obj));

		File.Move(obj, newPath);

		if (_watcher is not null)
		{
			_watcher.EnableRaisingEvents = true;
		}

		var newProfile = Newtonsoft.Json.JsonConvert.DeserializeObject<Profile>(File.ReadAllText(newPath));

		if (newProfile != null)
		{
			newProfile.Name = Path.GetFileNameWithoutExtension(newPath);
			newProfile.LastEditDate = File.GetLastWriteTime(newPath);
			newProfile.DateCreated = File.GetCreationTime(newPath);

			if (newProfile.LastUsed == DateTime.MinValue)
			{
				newProfile.LastUsed = newProfile.LastEditDate;
			}

			lock (_profiles)
			{
				var currentProfile = _profiles.FirstOrDefault(x => x.Name?.Equals(Path.GetFileNameWithoutExtension(newPath), StringComparison.OrdinalIgnoreCase) ?? false);

				_profiles.Remove(currentProfile);

				_profiles.Add(newProfile);
			}

			ProfileUpdated?.Invoke();

			return newProfile;
		}
		else
		{
			Log.Error($"Could not load the profile: '{obj}' / '{newPath}'");
		}

		return null;
	}

	internal static void SetIncludedForAll<T>(T item, bool value) where T : IPackage
	{
		try
		{
			if (_watcher is not null)
			{
				_watcher.EnableRaisingEvents = false;
			}

			if (item is Mod mod)
			{
				var profileMod = new Profile.Mod(mod);

				foreach (var profile in Profiles.Skip(1))
				{
					SetIncludedFor(value, profileMod, profile);
				}
			}
			else if (item is Asset asset)
			{
				var profileAsset = new Profile.Asset(asset);

				foreach (var profile in Profiles.Skip(1))
				{
					SetIncludedFor(value, profileAsset, profile);
				}
			}
			else if (item is Package package)
			{
				var profileMod = package.Mod is null ? null : new Profile.Mod(package.Mod);
				var assets = package.Assets?.Select(x => new Profile.Asset(x)).ToList() ?? new();

				foreach (var profile in Profiles.Skip(1))
				{
					SetIncludedFor(value, profileMod, assets, profile);
				}
			}
		}
		catch (Exception ex) { Log.Exception(ex, $"Failed to apply included status '{value}' to package: '{item}'"); }
		finally
		{
			if (_watcher is not null)
			{
				_watcher.EnableRaisingEvents = true;
			}
		}
	}

	private static void SetIncludedFor(bool value, Profile.Mod? profileMod, List<Profile.Asset> assets, Profile profile)
	{
		if (value)
		{
			if (profileMod is not null)
			{
				if (!profile.Mods.Any(x => x.RelativePath?.Equals(profileMod.RelativePath, StringComparison.OrdinalIgnoreCase) ?? false))
				{
					profile.Mods.Add(profileMod);
				}
			}

			if (assets.Count > 0)
			{
				var assetsToAdd = new List<Profile.Asset>(assets);

				foreach (var pa in profile.Assets)
				{
					foreach (var profileAsset in assetsToAdd)
					{
						if (pa.RelativePath?.Equals(profileAsset.RelativePath, StringComparison.OrdinalIgnoreCase) ?? false)
						{
							assetsToAdd.Remove(profileAsset);
							break;
						}
					}
				}

				profile.Assets.AddRange(assetsToAdd);
			}
		}
		else
		{
			if (profileMod is not null)
			{
				profile.Mods.RemoveAll(x => x.RelativePath?.Equals(profileMod.RelativePath, StringComparison.OrdinalIgnoreCase) ?? false);
			}

			if (assets.Count > 0)
			{
				profile.Assets.RemoveAll(x => assets.Any(profileAsset => x.RelativePath?.Equals(profileAsset.RelativePath, StringComparison.OrdinalIgnoreCase) ?? false));
			}
		}

		Save(profile);
	}

	private static void SetIncludedFor(bool value, Profile.Asset profileAsset, Profile profile)
	{
		if (value)
		{
			if (!profile.Assets.Any(x => x.RelativePath?.Equals(profileAsset.RelativePath, StringComparison.OrdinalIgnoreCase) ?? false))
			{
				profile.Assets.Add(profileAsset);
			}
		}
		else
		{
			profile.Assets.RemoveAll(x => x.RelativePath?.Equals(profileAsset.RelativePath, StringComparison.OrdinalIgnoreCase) ?? false);
		}

		Save(profile);
	}

	private static void SetIncludedFor(bool value, Profile.Mod profileMod, Profile profile)
	{
		if (value)
		{
			if (!profile.Mods.Any(x => x.RelativePath?.Equals(profileMod.RelativePath, StringComparison.OrdinalIgnoreCase) ?? false))
			{
				profile.Mods.Add(profileMod);
			}
		}
		else
		{
			profile.Mods.RemoveAll(x => x.RelativePath?.Equals(profileMod.RelativePath, StringComparison.OrdinalIgnoreCase) ?? false);
		}

		Save(profile);
	}

	internal static bool IsPackageIncludedInProfile(IPackage ipackage, Profile profile)
	{
		if (ipackage is Package package)
		{
			var profileMod = package.Mod is null ? null : new Profile.Mod(package.Mod);
			var assets = package.Assets?.Select(x => new Profile.Asset(x)).ToList() ?? new();

			if (profileMod is not null)
			{
				if (!profile.Mods.Any(x => x.RelativePath?.Equals(profileMod.RelativePath, StringComparison.OrdinalIgnoreCase) ?? false))
				{
					return false;
				}
			}

			if (assets.Count > 0)
			{
				if (!assets.All(profileAsset => profile.Assets.Any(x => x.RelativePath?.Equals(profileAsset.RelativePath, StringComparison.OrdinalIgnoreCase) ?? false)))
				{
					return false;
				}
			}
		}
		else
		{
			if (ipackage.IsMod)
			{
				return profile.Mods.Any(x => x.SteamId == ipackage.SteamId);
			}

			return profile.Assets.Any(x => x.SteamId == ipackage.SteamId);
		}

		return true;
	}

	internal static void SetIncludedFor(IPackage ipackage, Profile profile, bool value)
	{
		if (ipackage is Package package)
		{
			var profileMod = package.Mod is null ? null : new Profile.Mod(package.Mod);
			var assets = package.Assets?.Select(x => new Profile.Asset(x)).ToList() ?? new();

			SetIncludedFor(value, profileMod, assets, profile);
		}
		else
		{
			var profileMod = profile.Mods.FirstOrDefault(x => x.SteamId == ipackage.SteamId) ?? new Profile.Mod(ipackage);

			SetIncludedFor(value, profileMod, new(), profile);
		}
	}

	internal static string GetFileName(Profile profile)
	{
		return LocationManager.Combine(LocationManager.SkyveProfilesAppDataPath, $"{profile.Name}.json");
	}

	internal static void CreateShortcut(Profile item)
	{
		try
		{
			var launch = MessagePrompt.Show(Locale.AskToLaunchGameForShortcut, PromptButtons.YesNo, PromptIcons.Question) == DialogResult.Yes;

			ExtensionClass.CreateShortcut(LocationManager.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), item.Name + ".lnk")
				, Program.ExecutablePath
				, (launch ? "-launch " : "") + $"-profile {item.Name}");
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "Failed to create shortcut");
		}
	}

	internal static async Task Share(Profile item)
	{
		await SkyveApiUtil.SaveUserProfile(new()
		{
			AuthorId = SteamUtil.GetLoggedInSteamId(),
			Banner = item.BannerBytes,
			Color = item.Color?.ToArgb(),
			Name = item.Name,
			Contents = item.Assets.Concat(item.Mods).Select(x => x.AsProfileContent()).ToArray()
		});
	}
}
