using Extensions;

using LoadOrderShared;

using LoadOrderToolTwo.ColossalOrder;
using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Utilities;

using SlickControls;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LoadOrderToolTwo.Utilities.Managers;
public static class ProfileManager
{
	private const string LOCAL_APP_DATA_PATH = "%LOCALAPPDATA%";
	private const string CITIES_PATH = "%CITIES%";
	private const string WS_CONTENT_PATH = "%WORKSHOP%";
	private static List<Profile> _profiles;
	private static bool disableAutoSave;
	private static readonly FileSystemWatcher _watcher;

	public static bool ApplyingProfile { get; private set; }
	public static Profile CurrentProfile { get; private set; }
	public static IEnumerable<Profile> Profiles
	{
		get
		{
			yield return Profile.TemporaryProfile;

			foreach (var profile in _profiles)
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
			ConvertLegacyProfiles();
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "Failed to port LOT1 profiles");
		}

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

		_watcher = new FileSystemWatcher
		{
			Path = LocationManager.LotProfilesAppDataPath,
			NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
			Filter = "*.json"
		};

		if (!CommandUtil.NoWindow)
		{
			new BackgroundAction(LoadAllProfiles).Run();
		}
	}

	private static Profile? LoadCurrentProfile()
	{
		if (CentralManager.SessionSettings.CurrentProfile is null or "" && CommandUtil.PreSelectedProfile is null or "")
		{
			return null;
		}

		var profile = Path.Combine(LocationManager.LotProfilesAppDataPath, (CommandUtil.PreSelectedProfile ?? CentralManager.SessionSettings.CurrentProfile) + ".json");

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

	private static void ConvertLegacyProfiles()
	{
		try
		{
			var legacyProfilePath = Path.Combine(LocationManager.AppDataPath, "LoadOrder", "LOMProfiles");

			if (!Directory.Exists(legacyProfilePath))
			{
				return;
			}

			foreach (var profile in Directory.EnumerateFiles(legacyProfilePath, "*.xml"))
			{
				var newName = Path.Combine(LocationManager.LotProfilesAppDataPath, $"{Path.GetFileNameWithoutExtension(profile)}.json");

				if (File.Exists(newName))
				{
					continue;
				}

				var legacyProfile = LoadOrderTool.Legacy.LoadOrderProfile.Deserialize(profile);
				var newProfile = legacyProfile?.ToLot2Profile(Path.GetFileNameWithoutExtension(profile));

				if (newProfile != null)
				{
					Save(newProfile, true);

					Directory.CreateDirectory(Path.Combine(legacyProfilePath, "Legacy"));

					File.Move(profile, Path.Combine(legacyProfilePath, "Legacy", Path.GetFileName(profile)));
				}
				else
				{
					Log.Error($"Could not load the profile: '{profile}'");
				}
			}
		}
		catch { }
	}

	private static void CentralManager_ContentLoaded()
	{
		new BackgroundAction(() =>
		{
			foreach (var profile in _profiles)
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
		File.Delete(Path.Combine(LocationManager.LotProfilesAppDataPath, $"{profile.Name}.json"));

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
		if (CurrentProfile.AutoSave && !disableAutoSave && !ApplyingProfile && !CentralManager.IsContentLoaded)
		{
			CurrentProfile.Save();
		}
	}

	private static void LoadAllProfiles()
	{
		var profiles = new List<Profile>(_profiles);

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

					profiles.Add(newProfile);
				}
				else
				{
					Log.Error($"Could not load the profile: '{profile}'");
				}
			}
		}
		catch { }

		_profiles = profiles;

		if (CentralManager.IsContentLoaded)
		{
			CentralManager_ContentLoaded();
		}

		CentralManager.ContentLoaded += CentralManager_ContentLoaded;

		_watcher.Changed += new FileSystemEventHandler(FileChanged);
		_watcher.Created += new FileSystemEventHandler(FileChanged);
		_watcher.Deleted += new FileSystemEventHandler(FileChanged);

		_watcher.EnableRaisingEvents = true;
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
				var profile = _profiles.FirstOrDefault(x => x.Name?.Equals(Path.GetFileNameWithoutExtension(e.FullPath), StringComparison.OrdinalIgnoreCase) ?? false);

				if (profile != null)
				{
					_profiles.Remove(profile);
				}

				return;
			}

			var newProfile = Newtonsoft.Json.JsonConvert.DeserializeObject<Profile>(File.ReadAllText(e.FullPath));

			if (newProfile != null)
			{
				newProfile.Name = Path.GetFileNameWithoutExtension(e.FullPath);
				newProfile.LastEditDate = File.GetLastWriteTime(e.FullPath);
				newProfile.DateCreated = File.GetCreationTime(e.FullPath);

				var currentProfile = _profiles.FirstOrDefault(x => x.Name?.Equals(Path.GetFileNameWithoutExtension(e.FullPath), StringComparison.OrdinalIgnoreCase) ?? false);

				_profiles.Remove(currentProfile);

				_profiles.Add(newProfile);
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
			_watcher.EnableRaisingEvents = false;

			Directory.CreateDirectory(LocationManager.LotProfilesAppDataPath);

			File.WriteAllText(
				Path.Combine(LocationManager.LotProfilesAppDataPath, $"{profile.Name}.json"),
				Newtonsoft.Json.JsonConvert.SerializeObject(profile, Newtonsoft.Json.Formatting.Indented));

			return true;
		}
		catch (Exception ex)
		{
			Log.Exception(ex, $"Failed to save profile ({profile.Name}) to {Path.Combine(LocationManager.LotProfilesAppDataPath, $"{profile.Name}.json")}"); 
		}
		finally
		{
			_watcher.EnableRaisingEvents = true;
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
		return localPath?
			.Replace(LocationManager.AppDataPath, LOCAL_APP_DATA_PATH)
			.Replace(LocationManager.GamePath, CITIES_PATH)
			.Replace(LocationManager.WorkshopContentPath, WS_CONTENT_PATH);
	}

	internal static string? ToLocalPath(string? relativePath)
	{
		return relativePath?
			.Replace(LOCAL_APP_DATA_PATH, LocationManager.AppDataPath)
			.Replace(CITIES_PATH, LocationManager.GamePath)
			.Replace(WS_CONTENT_PATH, LocationManager.WorkshopContentPath);
	}

	internal static bool RenameProfile(Profile profile, string text)
	{
		if (profile == null || profile.Temporary)
		{
			return false;
		}

		text = text.EscapeFileName();

		var newName = Path.Combine(LocationManager.LotProfilesAppDataPath, $"{text}.json");
		var oldName = Path.Combine(LocationManager.LotProfilesAppDataPath, $"{profile.Name}.json");

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
		var startName = Path.Combine(LocationManager.LotProfilesAppDataPath, "New Profile.json");

		// Check if the file with the proposed name already exists
		if (File.Exists(startName))
		{
			var extension = ".json";
			var nameWithoutExtension = Path.Combine(LocationManager.LotProfilesAppDataPath, "New Profile");
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
		_profiles.Add(newProfile);
	}
}
