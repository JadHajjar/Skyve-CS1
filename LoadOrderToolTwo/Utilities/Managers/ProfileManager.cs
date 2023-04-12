using Extensions;

using LoadOrderShared;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.Domain.Utilities;

using SlickControls;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using static LoadOrderToolTwo.UserInterface.Generic.ThreeOptionToggle;

namespace LoadOrderToolTwo.Utilities.Managers;
public static class ProfileManager
{
	private const string LOCAL_APP_DATA_PATH = "%LOCALAPPDATA%";
	private const string CITIES_PATH = "%CITIES%";
	private const string WS_CONTENT_PATH = "%WORKSHOP%";
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

	public static event Action<Profile>? ProfileChanged;

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

		if (Directory.Exists(LocationManager.LotAppDataPath))
		{
			Directory.CreateDirectory(LocationManager.LotProfilesAppDataPath);

			_watcher = new FileSystemWatcher
			{
				Path = LocationManager.LotProfilesAppDataPath,
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

		var profile = LocationManager.Combine(LocationManager.LotProfilesAppDataPath, (CommandUtil.PreSelectedProfile ?? CentralManager.SessionSettings.CurrentProfile) + ".json");

		if (!File.Exists(profile))
		{
			return null;
		}

		var newProfile = Newtonsoft.Json.JsonConvert.DeserializeObject<Profile>(File.ReadAllText(profile));

		if (newProfile != null)
		{
			newProfile.Name = Path.GetFileNameWithoutExtension(profile);
			newProfile.LastEditDate = File.GetLastWriteTime(profile);
			newProfile.DateCreated = File.GetCreationTime(profile);

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
			var newName = LocationManager.Combine(LocationManager.LotProfilesAppDataPath, $"{Path.GetFileNameWithoutExtension(profile)}.json");

			if (File.Exists(newName))
			{
				Log.Info($"Profile '{newName}' already exists, skipping..");
				continue;
			}

			Log.Info($"Converting profile '{newName}'..");

			var legacyProfile = LoadOrderTool.Legacy.LoadOrderProfile.Deserialize(profile);
			var newProfile = legacyProfile?.ToLot2Profile(Path.GetFileNameWithoutExtension(profile));

			if (newProfile != null)
			{
				newProfile.LastEditDate = File.GetLastWriteTime(profile);
				newProfile.DateCreated = File.GetCreationTime(profile);

				lock (_profiles)
				{
					_profiles.Add(newProfile);
				}

				Save(newProfile, true);

				Directory.CreateDirectory(LocationManager.Combine(legacyProfilePath, "Legacy"));

				File.Move(profile, LocationManager.Combine(legacyProfilePath, "Legacy", Path.GetFileName(profile)));
			}
			else
			{
				Log.Error($"Could not load the profile: '{profile}'");
			}
		}
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
			}
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
		ExtensionClass.DeleteFile(LocationManager.Combine(LocationManager.LotProfilesAppDataPath, $"{profile.Name}.json"));

		if (profile == CurrentProfile)
		{
			SetProfile(Profile.TemporaryProfile);
		}
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

				foreach (var mod in unprocessedMods)
				{
					mod.IsIncluded = false;
					mod.IsEnabled = false;
				}

				foreach (var asset in unprocessedAssets)
				{
					asset.IsIncluded = false;
				}

				if ((missingMods.Count > 0 || missingAssets.Count > 0) && Program.MainForm is not null)
				{
					UserInterface.Panels.PC_MissingPackages.PromptMissingPackages(Program.MainForm, missingMods, missingAssets);
				}

				AssetsUtil.SetDlcsExcluded(profile.ExcludedDLCs.ToArray());

				ApplyingProfile = false;
				disableAutoSave = true;

				ModsUtil.SavePendingValues();
				AssetsUtil.SaveChanges();

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
		if (CurrentProfile.AutoSave && !disableAutoSave && !ApplyingProfile && CentralManager.IsContentLoaded)
		{
			CurrentProfile.Save();
		}
	}

	private static void LoadAllProfiles()
	{
		try
		{
			foreach (var profile in Directory.EnumerateFiles(LocationManager.LotProfilesAppDataPath, "*.json"))
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

		if (_watcher is not null)
		{
			_watcher.Changed += new FileSystemEventHandler(FileChanged);
			_watcher.Created += new FileSystemEventHandler(FileChanged);
			_watcher.Deleted += new FileSystemEventHandler(FileChanged);

			_watcher.EnableRaisingEvents = true;
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

			if (!File.Exists(e.FullPath))
			{
				lock (_profiles)
				{
					var profile = _profiles.FirstOrDefault(x => x.Name?.Equals(Path.GetFileNameWithoutExtension(e.FullPath), StringComparison.OrdinalIgnoreCase) ?? false);

					if (profile != null)
					{
						_profiles.Remove(profile);
					}
				}

				return;
			}

			var newProfile = Newtonsoft.Json.JsonConvert.DeserializeObject<Profile>(File.ReadAllText(e.FullPath));

			if (newProfile != null)
			{
				newProfile.Name = Path.GetFileNameWithoutExtension(e.FullPath);
				newProfile.LastEditDate = File.GetLastWriteTime(e.FullPath);
				newProfile.DateCreated = File.GetCreationTime(e.FullPath);

				lock (_profiles)
				{
					var currentProfile = _profiles.FirstOrDefault(x => x.Name?.Equals(Path.GetFileNameWithoutExtension(e.FullPath), StringComparison.OrdinalIgnoreCase) ?? false);

					_profiles.Remove(currentProfile);

					_profiles.Add(newProfile);
				}
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
		profile.ExcludedDLCs = LoadOrderConfig.Deserialize()?.RemovedDLCs.ToList() ?? new();
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

			Directory.CreateDirectory(LocationManager.LotProfilesAppDataPath);

			File.WriteAllText(
				LocationManager.Combine(LocationManager.LotProfilesAppDataPath, $"{profile.Name}.json"),
				Newtonsoft.Json.JsonConvert.SerializeObject(profile, Newtonsoft.Json.Formatting.Indented));

			return true;
		}
		catch (Exception ex)
		{
			Log.Exception(ex, $"Failed to save profile ({profile.Name}) to {LocationManager.Combine(LocationManager.LotProfilesAppDataPath, $"{profile.Name}.json")}");
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

	internal static string? ToRelativePath(string? localPath)
	{
		if (string.IsNullOrEmpty(localPath))
		{
			return string.Empty;
		}

		return LocationManager.FormatPath(localPath?
			.Replace(LocationManager.AppDataPath, LOCAL_APP_DATA_PATH)
			.Replace(LocationManager.GamePath, CITIES_PATH)
			.Replace(LocationManager.WorkshopContentPath, WS_CONTENT_PATH) ?? string.Empty);
	}

	internal static string? ToLocalPath(string? relativePath)
	{
		if (string.IsNullOrEmpty(relativePath))
		{
			return string.Empty;
		}

		return LocationManager.FormatPath(relativePath?
			.Replace(LOCAL_APP_DATA_PATH, LocationManager.AppDataPath)
			.Replace(CITIES_PATH, LocationManager.GamePath)
			.Replace(WS_CONTENT_PATH, LocationManager.WorkshopContentPath) ?? string.Empty);
	}

	internal static bool RenameProfile(Profile profile, string text)
	{
		if (profile == null || profile.Temporary)
		{
			return false;
		}

		text = text.EscapeFileName();

		var newName = LocationManager.Combine(LocationManager.LotProfilesAppDataPath, $"{text}.json");
		var oldName = LocationManager.Combine(LocationManager.LotProfilesAppDataPath, $"{profile.Name}.json");

		try
		{
			if (newName == oldName)
			{
				return true;
			}

			if (File.Exists(newName))
			{
				return false;
			}

			if (File.Exists(oldName))
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
		var startName = LocationManager.Combine(LocationManager.LotProfilesAppDataPath, "New Profile.json");

		// Check if the file with the proposed name already exists
		if (File.Exists(startName))
		{
			var extension = ".json";
			var nameWithoutExtension = LocationManager.Combine(LocationManager.LotProfilesAppDataPath, "New Profile");
			var counter = 1;

			// Loop until a valid file name is found
			while (File.Exists(startName))
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

	internal static System.Drawing.Bitmap GetIcon(this Profile profile)
	{
		if (profile.Temporary)
		{
			return Properties.Resources.I_TempProfile;
		}
		else if (profile.ForAssetEditor)
		{
			return Properties.Resources.I_Tools;
		}
		else if (profile.ForGameplay)
		{
			return Properties.Resources.I_City;
		}
		else
		{
			return Properties.Resources.I_ProfileSettings;
		}
	}

	internal static List<Package> GetInvalidPackages(bool gameplay, bool editor)
	{
		if (gameplay)
		{
			return new List<Package>(CentralManager.Packages.Where(x => x.IsIncluded && x.ForAssetEditor == true));
		}

		if (editor)
		{
			return new List<Package>(CentralManager.Packages.Where(x => x.IsIncluded && x.ForNormalGame == true));
		}

		return new();
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
	}

	internal static Profile? ImportProfile(string obj)
	{
		if (_watcher is not null)
		{
			_watcher.EnableRaisingEvents = false;
		}

		var newPath = LocationManager.Combine(LocationManager.LotProfilesAppDataPath, Path.GetFileName(obj));

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

			lock (_profiles)
			{
				var currentProfile = _profiles.FirstOrDefault(x => x.Name?.Equals(Path.GetFileNameWithoutExtension(newPath), StringComparison.OrdinalIgnoreCase) ?? false);

				_profiles.Remove(currentProfile);

				_profiles.Add(newProfile);
			}

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

	internal static bool IsPackageIncludedInProfile(Package package, Profile profile)
	{
		var profileMod = package.Mod is null ? null : new Profile.Mod(package.Mod);
		var assets = package.Assets?.Select(x => new Profile.Asset(x)).ToList() ?? new();

		if (profileMod is not null)
		{
			if (!profile.Mods.Any(x => x.RelativePath?.Equals(profileMod.RelativePath, StringComparison.OrdinalIgnoreCase) ?? false))
				return false;
		}

		if (assets.Count > 0)
		{
			if (!assets.All(profileAsset => profile.Assets.Any(x => x.RelativePath?.Equals(profileAsset.RelativePath, StringComparison.OrdinalIgnoreCase) ?? false)))
				return false;
		}

		return true;
	}

	internal static void SetIncludedFor(Package package, Profile profile, bool value)
	{
		var profileMod = package.Mod is null ? null : new Profile.Mod(package.Mod);
		var assets = package.Assets?.Select(x => new Profile.Asset(x)).ToList() ?? new();

		SetIncludedFor(value, profileMod, assets, profile);

	}
}
