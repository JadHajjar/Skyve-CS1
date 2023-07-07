﻿using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.CS1;
using SkyveApp.Domain.CS1.Legacy;
using SkyveApp.Domain.CS1.Utilities;
using SkyveApp.Domain.Enums;
using SkyveApp.Domain.Systems;
using SkyveApp.Systems.CS1.Utilities;

using SkyveShared;

using SlickControls;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SkyveApp.Systems.CS1.Managers;
internal class PlaysetManager : IPlaysetManager
{
	private readonly List<ICustomPlayset> _profiles;
	private bool disableAutoSave;
	private readonly FileSystemWatcher? _watcher;

	public ICustomPlayset CurrentPlayset { get; private set; }
	public IEnumerable<ICustomPlayset> Playsets
	{
		get
		{
			yield return Playset.TemporaryPlayset;

			List<ICustomPlayset> profiles;

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

	public event PromptMissingItemsDelegate? PromptMissingItems;

	private readonly ILogger _logger;
	private readonly ILocationManager _locationManager;
	private readonly ISettings _settings;
	private readonly IPackageManager _packageManager;
	private readonly IPackageUtil _packageUtil;
	private readonly ICompatibilityManager _compatibilityManager;
	private readonly INotifier _notifier;
	private readonly IModUtil _modUtil;
	private readonly IAssetUtil _assetUtil;
	private readonly IDlcManager _dlcManager;

	public PlaysetManager(ILogger logger, ILocationManager locationManager, ISettings settings, IPackageManager packageManager, IPackageUtil packageUtil, ICompatibilityManager compatibilityManager, INotifier notifier, IModUtil modUtil, IAssetUtil assetUtil, IDlcManager dlcManager)
	{
		_logger = logger;
		_locationManager = locationManager;
		_settings = settings;
		_packageManager = packageManager;
		_packageUtil = packageUtil;
		_compatibilityManager = compatibilityManager;
		_notifier = notifier;
		_modUtil = modUtil;
		_assetUtil = assetUtil;
		_dlcManager = dlcManager;

		try
		{
			var current = LoadCurrentPlayset();

			if (current != null)
			{
				_profiles = new() { current };
				CurrentPlayset = current;
			}
		}
		catch (Exception ex)
		{
			_logger.Exception(ex, "Failed to load the current profile");
		}

		_profiles ??= new();

		CurrentPlayset ??= Playset.TemporaryPlayset;

		if (Directory.Exists(_locationManager.SkyveAppDataPath))
		{
			Directory.CreateDirectory(_locationManager.SkyvePlaysetsAppDataPath);

			_watcher = new FileSystemWatcher
			{
				Path = _locationManager.SkyvePlaysetsAppDataPath,
				NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
				Filter = "*.json"
			};
		}

		if (!CommandUtil.NoWindow)
		{
			new BackgroundAction(ConvertLegacyPlaysets).Run();
			new BackgroundAction(LoadAllPlaysets).Run();
		}

		_notifier.AutoSaveRequested += OnAutoSave;
	}

	private ICustomPlayset? LoadCurrentPlayset()
	{
		if (_settings.SessionSettings.CurrentPlayset is null or "" && CommandUtil.PreSelectedProfile is null or "")
		{
			return null;
		}

		var profile = CrossIO.Combine(_locationManager.SkyvePlaysetsAppDataPath, (CommandUtil.PreSelectedProfile ?? _settings.SessionSettings.CurrentPlayset) + ".json");

		if (!CrossIO.FileExists(profile))
		{
			return null;
		}

		var newPlayset = Newtonsoft.Json.JsonConvert.DeserializeObject<Playset>(File.ReadAllText(profile));

		if (newPlayset != null)
		{
			newPlayset.Name = Path.GetFileNameWithoutExtension(profile);
			newPlayset.LastEditDate = File.GetLastWriteTime(profile);
			newPlayset.DateCreated = File.GetCreationTime(profile);

			if (newPlayset.LastUsed == DateTime.MinValue)
			{
				newPlayset.LastUsed = newPlayset.LastEditDate;
			}

			return newPlayset;
		}
		else
		{
			_logger.Error($"Could not load the profile: '{profile}'");
		}

		return null;
	}

	public void ConvertLegacyPlaysets()
	{
		if (!_settings.SessionSettings.FirstTimeSetupCompleted)
		{
			return;
		}

		_logger.Info("Checking for Legacy profiles");

		var legacyPlaysetPath = CrossIO.Combine(_locationManager.AppDataPath, "LoadOrder", "LOMPlaysets");

		if (!Directory.Exists(legacyPlaysetPath))
		{
			return;
		}

		_logger.Info("Checking for Legacy profiles");

		foreach (var profile in Directory.EnumerateFiles(legacyPlaysetPath, "*.xml"))
		{
			var newName = CrossIO.Combine(_locationManager.SkyvePlaysetsAppDataPath, $"{Path.GetFileNameWithoutExtension(profile)}.json");

			if (CrossIO.FileExists(newName))
			{
				_logger.Info($"Playset '{newName}' already exists, skipping..");
				continue;
			}

			_logger.Info($"Converting profile '{newName}'..");

			ConvertLegacyPlayset(profile);
		}
	}

	public ICustomPlayset? ConvertLegacyPlayset(string profilePath, bool removeLegacyFile = true)
	{
		var legacyPlaysetPath = CrossIO.Combine(_locationManager.AppDataPath, "LoadOrder", "LOMPlaysets");
		var legacyPlayset = LoadOrderProfile.Deserialize(profilePath);
		var newPlayset = legacyPlayset?.ToSkyvePlayset(Path.GetFileNameWithoutExtension(profilePath));

		if (newPlayset != null)
		{
			newPlayset.LastEditDate = File.GetLastWriteTime(profilePath);
			newPlayset.DateCreated = File.GetCreationTime(profilePath);
			newPlayset.LastUsed = newPlayset.LastEditDate;

			lock (_profiles)
			{
				_profiles.Add(newPlayset);
			}

			Save(newPlayset, true);

			if (removeLegacyFile)
			{
				Directory.CreateDirectory(CrossIO.Combine(legacyPlaysetPath, "Legacy"));

				File.Move(profilePath, CrossIO.Combine(legacyPlaysetPath, "Legacy", Path.GetFileName(profilePath)));
			}
		}
		else
		{
			_logger.Error($"Could not load the profile: '{profilePath}'");
		}

		return newPlayset;
	}

	private void CentralManager_ContentLoaded()
	{
		new BackgroundAction(() =>
		{
			List<IPlayset> profiles;

			lock (_profiles)
			{
				profiles = new(_profiles);
			}

			foreach (Playset profile in profiles)
			{
				profile.IsMissingItems = profile.Mods.Any(x => GetMod(x) is null) || profile.Assets.Any(x => GetAsset(x) is null);
#if DEBUG
				if (profile.IsMissingItems)
				{
					_logger.Debug($"Missing items in the profile: {profile}\r\n" +
						profile.Mods.Where(x => GetMod(x) is null).ListStrings(x => $"{x.Name} ({_locationManager.ToLocalPath(x.RelativePath)})", "\r\n") + "\r\n" +
						profile.Assets.Where(x => GetAsset(x) is null).ListStrings(x => $"{x.Name} ({_locationManager.ToLocalPath(x.RelativePath)})", "\r\n"));
				}
#endif
			}

			_notifier.OnPlaysetUpdated();
		}).Run();
	}

	public void MergeIntoCurrentPlayset(IPlayset profile)
	{
		new BackgroundAction("Applying profile", apply).Run();

		void apply()
		{
			try
			{
				var unprocessedMods = _packageManager.Mods.ToList();
				var unprocessedAssets = _packageManager.Assets.ToList();
				var missingMods = new List<Playset.Mod>();
				var missingAssets = new List<Playset.Asset>();

				_notifier.ApplyingPlayset = true;

				foreach (var mod in (profile as Playset)!.Mods)
				{
					var localMod = GetMod(mod);

					if (localMod != null)
					{
						_modUtil.SetIncluded(localMod, true);
						_modUtil.SetEnabled(localMod, mod.Enabled);

						unprocessedMods.Remove(localMod);
					}
					else
					{
						missingMods.Add(mod);
					}
				}

				foreach (var asset in (profile as Playset)!.Assets)
				{
					var localAsset = GetAsset(asset);

					if (localAsset != null)
					{
						_assetUtil.SetIncluded(localAsset, true);

						unprocessedAssets.Remove(localAsset);
					}
					else
					{
						missingAssets.Add(asset);
					}
				}

				if ((missingMods.Count > 0 || missingAssets.Count > 0))
				{
					PromptMissingItems?.Invoke(this, missingMods.Concat(missingAssets));
				}

				_notifier.ApplyingPlayset = false;
				disableAutoSave = true;

				_modUtil.SaveChanges();
				_assetUtil.SaveChanges();

				disableAutoSave = false;

				_notifier.OnPlaysetChanged();

				OnAutoSave();
			}
			catch (Exception ex)
			{
				MessagePrompt.Show(ex, "Failed to merge your profiles", form: SystemsProgram.MainForm as SlickForm);
			}
			finally
			{
				_notifier.ApplyingPlayset = false;
				disableAutoSave = false;
			}
		}
	}

	public void ExcludeFromCurrentPlayset(IPlayset profile)
	{
		new BackgroundAction("Applying profile", apply).Run();

		void apply()
		{
			try
			{
				var unprocessedMods = _packageManager.Mods.ToList();
				var unprocessedAssets = _packageManager.Assets.ToList();
				var missingMods = new List<Playset.Mod>();
				var missingAssets = new List<Playset.Asset>();

				_notifier.ApplyingPlayset = true;

				foreach (var mod in (profile as Playset)!.Mods)
				{
					var localMod = GetMod(mod);

					if (localMod != null)
					{
						_modUtil.SetIncluded(localMod, false);
						_modUtil.SetEnabled(localMod, false);
					}
				}

				foreach (var asset in (profile as Playset)!.Assets)
				{
					var localAsset = GetAsset(asset);

					if (localAsset != null)
					{
						_assetUtil.SetIncluded(localAsset, false);
					}
				}

				_notifier.ApplyingPlayset = false;
				disableAutoSave = true;

				_modUtil.SaveChanges();
				_assetUtil.SaveChanges();

				disableAutoSave = false;

				_notifier.OnPlaysetChanged();

				OnAutoSave();
			}
			catch (Exception ex)
			{
				MessagePrompt.Show(ex, "Failed to exclude items from your profile", form: SystemsProgram.MainForm as SlickForm);
			}
			finally
			{
				_notifier.ApplyingPlayset = false;
				disableAutoSave = false;
			}
		}
	}

	public void DeletePlayset(ICustomPlayset profile)
	{
		CrossIO.DeleteFile(CrossIO.Combine(_locationManager.SkyvePlaysetsAppDataPath, $"{profile.Name}.json"));

		lock (_profiles)
		{
			_profiles.Remove(profile);
		}

		if (profile == CurrentPlayset)
		{
			SetCurrentPlayset(Playset.TemporaryPlayset);
		}

		_notifier.OnPlaysetUpdated();
	}

	public void SetCurrentPlayset(ICustomPlayset profile)
	{
		CurrentPlayset = profile;

		if (profile.Temporary)
		{
			_notifier.OnPlaysetChanged();

			_settings.SessionSettings.CurrentPlayset = null;
			_settings.SessionSettings.Save();

			return;
		}

		if (SystemsProgram.MainForm as SlickForm is null)
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
				var unprocessedMods = _packageManager.Mods.ToList();
				var unprocessedAssets = _packageManager.Assets.ToList();
				var missingMods = new List<Playset.Mod>();
				var missingAssets = new List<Playset.Asset>();

				_notifier.ApplyingPlayset = true;

				foreach (var mod in (profile as Playset)!.Mods)
				{
					var localMod = GetMod(mod);

					if (localMod != null)
					{
						_modUtil.SetIncluded(localMod, true);
						_modUtil.SetEnabled(localMod, mod.Enabled);

						unprocessedMods.Remove(localMod);
					}
					else if (!_compatibilityManager.IsBlacklisted(mod))
					{
						missingMods.Add(mod);
					}
				}

				foreach (var asset in (profile as Playset)!.Assets)
				{
					var localAsset = GetAsset(asset);

					if (localAsset != null)
					{
						_assetUtil.SetIncluded(localAsset, true);

						unprocessedAssets.Remove(localAsset);
					}
					else if (!_compatibilityManager.IsBlacklisted(asset))
					{
						missingAssets.Add(asset);
					}
				}

				foreach (var mod in unprocessedMods)
				{
					_modUtil.SetIncluded(mod, true);
					_modUtil.SetEnabled(mod, false);
				}

				foreach (var asset in unprocessedAssets)
				{
					_assetUtil.SetIncluded(asset, false);
				}

#if DEBUG
				_logger.Debug($"unprocessedMods: {unprocessedMods.Count}\r\n" +
					$"unprocessedAssets: {unprocessedAssets.Count}\r\n" +
					$"missingMods: {missingMods.Count}\r\n" +
					$"missingAssets: {missingAssets.Count}");
#endif

				if ((missingMods.Count > 0 || missingAssets.Count > 0))
				{
					PromptMissingItems?.Invoke(this, missingMods.Concat(missingAssets));
				}

				_dlcManager.SetExcludedDlcs((profile as Playset)!.ExcludedDLCs.ToArray());

				_notifier.ApplyingPlayset = false;
				disableAutoSave = true;

				_modUtil.SaveChanges();
				_assetUtil.SaveChanges();

				(profile as Playset)!.LastUsed = DateTime.Now;
				Save(profile);

				_notifier.OnPlaysetChanged();

				try
				{ SaveLsmSettings(profile); }
				catch (Exception ex) { _logger.Exception(ex, "Failed to apply the LSM settings for profile " + profile.Name); }

				if (!CommandUtil.NoWindow)
				{
					_settings.SessionSettings.CurrentPlayset = profile.Name;
					_settings.SessionSettings.Save();
				}

				disableAutoSave = false;
			}
			catch (Exception ex)
			{
				MessagePrompt.Show(ex, "Failed to apply your profile", form: SystemsProgram.MainForm as SlickForm);

				_notifier.OnPlaysetChanged();
			}
			finally
			{
				_notifier.ApplyingPlayset = false;
				disableAutoSave = false;
			}
		}
	}

	public void OnAutoSave()
	{
		if (!disableAutoSave && !_notifier.ApplyingPlayset && _notifier.IsContentLoaded && !_notifier.BulkUpdating)
		{
			var playset = (CurrentPlayset as Playset)!;

			if (playset.AutoSave)
			{
				playset.Save();
			}
			else if (!CurrentPlayset.Temporary)
			{
				playset.UnsavedChanges = true;

				Save(CurrentPlayset);
			}
		}
	}

	private void LoadAllPlaysets()
	{
		try
		{
			foreach (var profile in Directory.EnumerateFiles(_locationManager.SkyvePlaysetsAppDataPath, "*.json"))
			{
				if (Path.GetFileNameWithoutExtension(profile).Equals(_settings.SessionSettings.CurrentPlayset, StringComparison.CurrentCultureIgnoreCase))
				{
					continue;
				}

				var newPlayset = Newtonsoft.Json.JsonConvert.DeserializeObject<Playset>(File.ReadAllText(profile));

				if (newPlayset != null)
				{
					newPlayset.Name = Path.GetFileNameWithoutExtension(profile);
					newPlayset.LastEditDate = File.GetLastWriteTime(profile);
					newPlayset.DateCreated = File.GetCreationTime(profile);

					if (newPlayset.LastUsed == DateTime.MinValue)
					{
						newPlayset.LastUsed = newPlayset.LastEditDate;
					}

					lock (_profiles)
					{
						_profiles.Add(newPlayset);
					}
				}
				else
				{
					_logger.Error($"Could not load the profile: '{profile}'");
				}
			}
		}
		catch { }

		if (_notifier.IsContentLoaded)
		{
			CentralManager_ContentLoaded();
		}

		_notifier.ContentLoaded += CentralManager_ContentLoaded;

		_notifier.PlaysetsLoaded = true;

		_notifier.OnPlaysetUpdated();

		if (_watcher is not null)
		{
			_watcher.Changed += new FileSystemEventHandler(FileChanged);
			_watcher.Created += new FileSystemEventHandler(FileChanged);
			_watcher.Deleted += new FileSystemEventHandler(FileChanged);

			try
			{ _watcher.EnableRaisingEvents = true; }
			catch (Exception ex) { _logger.Exception(ex, $"Failed to start profile watcher ({_locationManager.SkyvePlaysetsAppDataPath})"); }
		}
	}

	private void FileChanged(object sender, FileSystemEventArgs e)
	{
		try
		{
			if (!Path.GetExtension(e.FullPath).Equals(".json", StringComparison.CurrentCultureIgnoreCase))
			{
				return;
			}

			if (!CrossIO.FileExists(e.FullPath))
			{
				lock (_profiles)
				{
					var profile = _profiles.FirstOrDefault(x => x.Name?.Equals(Path.GetFileNameWithoutExtension(e.FullPath), StringComparison.OrdinalIgnoreCase) ?? false);

					if (profile != null)
					{
						_profiles.Remove(profile);
					}
				}

				_notifier.OnPlaysetUpdated();

				return;
			}

			var newPlayset = Newtonsoft.Json.JsonConvert.DeserializeObject<Playset>(File.ReadAllText(e.FullPath));

			if (newPlayset != null)
			{
				newPlayset.Name = Path.GetFileNameWithoutExtension(e.FullPath);
				newPlayset.LastEditDate = File.GetLastWriteTime(e.FullPath);
				newPlayset.DateCreated = File.GetCreationTime(e.FullPath);

				if (newPlayset.LastUsed == DateTime.MinValue)
				{
					newPlayset.LastUsed = newPlayset.LastEditDate;
				}

				lock (_profiles)
				{
					var currentPlayset = _profiles.FirstOrDefault(x => x.Name?.Equals(Path.GetFileNameWithoutExtension(e.FullPath), StringComparison.OrdinalIgnoreCase) ?? false);

					_profiles.Remove(currentPlayset);

					_profiles.Add(newPlayset);
				}

				_notifier.OnPlaysetUpdated();
			}
			else
			{
				_logger.Error($"Could not load the profile: '{e.FullPath}'");
			}
		}
		catch (Exception ex) { _logger.Exception(ex, "Failed to refresh changes to profiles"); }
	}

	public void GatherInformation(IPlayset? profile)
	{
		if (profile is not Playset playset || profile.Temporary || !_notifier.IsContentLoaded)
		{
			return;
		}

		playset.Assets = _packageManager.Assets.Where(_assetUtil.IsIncluded).Select(x => new Playset.Asset(x)).ToList();
		playset.Mods = _packageManager.Mods.Where(_modUtil.IsIncluded).Select(x => new Playset.Mod(x)).ToList();
		playset.ExcludedDLCs = _dlcManager.GetExcludedDlcs();
	}

	public bool Save(IPlayset? profile, bool forced = false)
	{
		if (profile == null || (!forced && (profile.Temporary || !_notifier.IsContentLoaded)))
		{
			return false;
		}

		try
		{
			if (_watcher is not null)
			{
				_watcher.EnableRaisingEvents = false;
			}

			Directory.CreateDirectory(_locationManager.SkyvePlaysetsAppDataPath);

			File.WriteAllText(
				CrossIO.Combine(_locationManager.SkyvePlaysetsAppDataPath, $"{profile.Name}.json"),
				Newtonsoft.Json.JsonConvert.SerializeObject(profile, Newtonsoft.Json.Formatting.Indented));

			(profile as Playset)!.IsMissingItems = (profile as Playset)!.Mods.Any(x => GetMod(x) is null) || (profile as Playset)!.Assets.Any(x => GetAsset(x) is null);
#if DEBUG
			if ((profile as Playset)!.IsMissingItems)
			{
				_logger.Debug($"Missing items in the profile: {profile}\r\n" +
					(profile as Playset)!.Mods.Where(x => GetMod(x) is null).ListStrings(x => $"{x.Name} ({_locationManager.ToLocalPath(x.RelativePath)})", "\r\n") + "\r\n" +
					(profile as Playset)!.Assets.Where(x => GetAsset(x) is null).ListStrings(x => $"{x.Name} ({_locationManager.ToLocalPath(x.RelativePath)})", "\r\n"));
			}
#endif

			return true;
		}
		catch (Exception ex)
		{
			_logger.Exception(ex, $"Failed to save profile ({profile.Name}) to {CrossIO.Combine(_locationManager.SkyvePlaysetsAppDataPath, $"{profile.Name}.json")}");
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

	public IMod? GetMod(IPlaysetEntry mod)
	{
		var folder = _locationManager.ToLocalPath(mod.RelativePath);

		return _packageManager.Mods.FirstOrDefault(x => x.Folder.Equals(folder, StringComparison.OrdinalIgnoreCase));
	}

	public IAsset? GetAsset(IPlaysetEntry asset)
	{
		return _assetUtil.GetAssetByFile(_locationManager.ToLocalPath(asset.RelativePath));
	}

	public bool RenamePlayset(IPlayset profile, string text)
	{
		if (profile == null || profile.Temporary)
		{
			return false;
		}

		text = text.EscapeFileName();

		var newName = CrossIO.Combine(_locationManager.SkyvePlaysetsAppDataPath, $"{text}.json");
		var oldName = CrossIO.Combine(_locationManager.SkyvePlaysetsAppDataPath, $"{profile.Name}.json");

		try
		{
			if (newName == oldName)
			{
				return true;
			}

			if (CrossIO.FileExists(newName))
			{
				return false;
			}

			if (CrossIO.FileExists(oldName))
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
			_settings.SessionSettings.CurrentPlayset = text;
			_settings.SessionSettings.Save();
		}

		return true;
	}

	public string GetNewPlaysetName()
	{
		var startName = CrossIO.Combine(_locationManager.SkyvePlaysetsAppDataPath, "New Playset.json");

		// Check if the file with the proposed name already exists
		if (CrossIO.FileExists(startName))
		{
			var extension = ".json";
			var nameWithoutExtension = CrossIO.Combine(_locationManager.SkyvePlaysetsAppDataPath, "New Playset");
			var counter = 1;

			// Loop until a valid file name is found
			while (CrossIO.FileExists(startName))
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

	public List<ILocalPackageWithContents> GetInvalidPackages(PackageUsage usage)
	{
		if ((int)usage == -1)
		{
			return new();
		}

		return _packageManager.Packages.AllWhere(x =>
		{
			var cr = _compatibilityManager.GetPackageInfo(x);

			if (cr is null)
			{
				return false;
			}

			if (cr.Usage.HasFlag(usage))
			{
				return false;
			}

			return _packageUtil.IsIncluded(x, out var partial) || partial;
		});
	}

	public void SaveLsmSettings(IPlayset profile)
	{
		var current = LsmSettingsFile.Deserialize();

		if (current == null)
		{
			return;
		}

		current.loadEnabled = (profile as Playset)!.LsmSettings.LoadEnabled;
		current.loadUsed = (profile as Playset)!.LsmSettings.LoadUsed;
		current.skipFile = (profile as Playset)!.LsmSettings.SkipFile;
		current.skipPrefabs = (profile as Playset)!.LsmSettings.UseSkipFile;

		current.SyncAndSerialize();
	}

	public void AddPlayset(ICustomPlayset newPlayset)
	{
		lock (_profiles)
		{
			_profiles.Add(newPlayset);
		}

		_notifier.OnPlaysetUpdated();
	}

	public ICustomPlayset? ImportPlayset(string obj)
	{
		if (_watcher is not null)
		{
			_watcher.EnableRaisingEvents = false;
		}

		var newPath = CrossIO.Combine(_locationManager.SkyvePlaysetsAppDataPath, Path.GetFileName(obj));

		File.Move(obj, newPath);

		if (_watcher is not null)
		{
			_watcher.EnableRaisingEvents = true;
		}

		var newPlayset = Newtonsoft.Json.JsonConvert.DeserializeObject<Playset>(File.ReadAllText(newPath));

		if (newPlayset != null)
		{
			newPlayset.Name = Path.GetFileNameWithoutExtension(newPath);
			newPlayset.LastEditDate = File.GetLastWriteTime(newPath);
			newPlayset.DateCreated = File.GetCreationTime(newPath);

			if (newPlayset.LastUsed == DateTime.MinValue)
			{
				newPlayset.LastUsed = newPlayset.LastEditDate;
			}

			lock (_profiles)
			{
				var currentPlayset = _profiles.FirstOrDefault(x => x.Name?.Equals(Path.GetFileNameWithoutExtension(newPath), StringComparison.OrdinalIgnoreCase) ?? false);

				_profiles.Remove(currentPlayset);

				_profiles.Add(newPlayset);
			}

			_notifier.OnPlaysetUpdated();

			return newPlayset;
		}
		else
		{
			_logger.Error($"Could not load the profile: '{obj}' / '{newPath}'");
		}

		return null;
	}

	public void SetIncludedForAll(IPackage item, bool value)
	{
		try
		{
			if (_watcher is not null)
			{
				_watcher.EnableRaisingEvents = false;
			}

			if (item is Mod mod)
			{
				var profileMod = new Playset.Mod(mod);

				foreach (var profile in Playsets.Skip(1))
				{
					SetIncludedFor(value, profileMod, profile);
				}
			}
			else if (item is Asset asset)
			{
				var profileAsset = new Playset.Asset(asset);

				foreach (var profile in Playsets.Skip(1))
				{
					SetIncludedFor(value, profileAsset, profile);
				}
			}
			else if (item is Package package)
			{
				var profileMod = package.Mod is null ? null : new Playset.Mod(package.Mod);
				var assets = package.Assets?.Select(x => new Playset.Asset(x)).ToList() ?? new();

				foreach (var profile in Playsets.Skip(1))
				{
					SetIncludedFor(value, profileMod, assets, profile);
				}
			}
		}
		catch (Exception ex) { _logger.Exception(ex, $"Failed to apply included status '{value}' to package: '{item}'"); }
		finally
		{
			if (_watcher is not null)
			{
				_watcher.EnableRaisingEvents = true;
			}
		}
	}

	private void SetIncludedFor(bool value, Playset.Mod? profileMod, List<Playset.Asset> assets, IPlayset profile)
	{
		if (value)
		{
			if (profileMod is not null)
			{
				if (!(profile as Playset)!.Mods.Any(x => x.RelativePath?.Equals(profileMod.RelativePath, StringComparison.OrdinalIgnoreCase) ?? false))
				{
					(profile as Playset)!.Mods.Add(profileMod);
				}
			}

			if (assets.Count > 0)
			{
				var assetsToAdd = new List<Playset.Asset>(assets);

				foreach (var pa in (profile as Playset)!.Assets)
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

				(profile as Playset)!.Assets.AddRange(assetsToAdd);
			}
		}
		else
		{
			if (profileMod is not null)
			{
				(profile as Playset)!.Mods.RemoveAll(x => x.RelativePath?.Equals(profileMod.RelativePath, StringComparison.OrdinalIgnoreCase) ?? false);
			}

			if (assets.Count > 0)
			{
				(profile as Playset)!.Assets.RemoveAll(x => assets.Any(profileAsset => x.RelativePath?.Equals(profileAsset.RelativePath, StringComparison.OrdinalIgnoreCase) ?? false));
			}
		}

		Save(profile);
	}

	private void SetIncludedFor(bool value, Playset.Asset profileAsset, IPlayset profile)
	{
		if (value)
		{
			if (!(profile as Playset)!.Assets.Any(x => x.RelativePath?.Equals(profileAsset.RelativePath, StringComparison.OrdinalIgnoreCase) ?? false))
			{
				(profile as Playset)!.Assets.Add(profileAsset);
			}
		}
		else
		{
			(profile as Playset)!.Assets.RemoveAll(x => x.RelativePath?.Equals(profileAsset.RelativePath, StringComparison.OrdinalIgnoreCase) ?? false);
		}

		Save(profile);
	}

	private void SetIncludedFor(bool value, Playset.Mod profileMod, IPlayset profile)
	{
		if (value)
		{
			if (!(profile as Playset)!.Mods.Any(x => x.RelativePath?.Equals(profileMod.RelativePath, StringComparison.OrdinalIgnoreCase) ?? false))
			{
				(profile as Playset)!.Mods.Add(profileMod);
			}
		}
		else
		{
			(profile as Playset)!.Mods.RemoveAll(x => x.RelativePath?.Equals(profileMod.RelativePath, StringComparison.OrdinalIgnoreCase) ?? false);
		}

		Save(profile);
	}

	public bool IsPackageIncludedInPlayset(IPackage ipackage, IPlayset profile)
	{
		if (ipackage is Package package)
		{
			var profileMod = package.Mod is null ? null : new Playset.Mod(package.Mod);
			var assets = package.Assets?.Select(x => new Playset.Asset(x)).ToList() ?? new();

			if (profileMod is not null)
			{
				if (!(profile as Playset)!.Mods.Any(x => x.RelativePath?.Equals(profileMod.RelativePath, StringComparison.OrdinalIgnoreCase) ?? false))
				{
					return false;
				}
			}

			if (assets.Count > 0)
			{
				if (!assets.All(profileAsset => (profile as Playset)!.Assets.Any(x => x.RelativePath?.Equals(profileAsset.RelativePath, StringComparison.OrdinalIgnoreCase) ?? false)))
				{
					return false;
				}
			}
		}
		else
		{
			if (ipackage.IsMod)
			{
				return (profile as Playset)!.Mods.Any(x => x.Id == ipackage.Id);
			}

			return (profile as Playset)!.Assets.Any(x => x.Id == ipackage.Id);
		}

		return true;
	}

	public void SetIncludedFor(IPackage ipackage, IPlayset profile, bool value)
	{
		if (ipackage is Package package)
		{
			var profileMod = package.Mod is null ? null : new Playset.Mod(package.Mod);
			var assets = package.Assets?.Select(x => new Playset.Asset(x)).ToList() ?? new();

			SetIncludedFor(value, profileMod, assets, profile);
		}
		else
		{
			var profileMod = (profile as Playset)!.Mods.FirstOrDefault(x => x.Id == ipackage.Id) ?? new Playset.Mod(ipackage);

			SetIncludedFor(value, profileMod, new(), profile);
		}
	}

	public string GetFileName(IPlayset profile)
	{
		return CrossIO.Combine(_locationManager.SkyvePlaysetsAppDataPath, $"{profile.Name}.json");
	}

	public void CreateShortcut(IPlayset item)
	{
		try
		{
			var launch = MessagePrompt.Show(Locale.AskToLaunchGameForShortcut, PromptButtons.YesNo, PromptIcons.Question) == DialogResult.Yes;

			ExtensionClass.CreateShortcut(CrossIO.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), item.Name + ".lnk")
				, Application.ExecutablePath
				, (launch ? "-launch " : "") + $"-profile {item.Name}");
		}
		catch (Exception ex)
		{
			_logger.Exception(ex, "Failed to create shortcut");
		}
	}
}