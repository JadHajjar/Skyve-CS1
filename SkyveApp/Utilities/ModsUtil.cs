using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Enums;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Domain.Utilities;
using SkyveApp.Services;
using SkyveApp.Services.Interfaces;
using SkyveApp.Utilities.IO;

using System;
using System.IO;
using System.Linq;

namespace SkyveApp.Utilities;
internal class ModsUtil : IModUtil
{
	private readonly CachedSaveLibrary<CachedModInclusion, Mod, bool> _includedLibrary = new();
	private readonly CachedSaveLibrary<CachedModEnabled, Mod, bool> _enabledLibrary = new();

	private readonly IContentManager _contentManager;
	private readonly IModLogicManager _modLogicManager;
	private readonly IColossalOrderUtil _colossalOrderUtil;
	private readonly ISettings _settings;
	private readonly ILogger _logger;
	private readonly INotifier _notifier;

	public ModsUtil(IContentManager contentManager, IModLogicManager modLogicManager, ISettings settings, ILogger logger, INotifier notifier, IColossalOrderUtil colossalOrderUtil)
	{
		_contentManager = contentManager;
		_modLogicManager = modLogicManager;
		_colossalOrderUtil = colossalOrderUtil;
		_settings = settings;
		_logger = logger;
		_notifier = notifier;
	}

	public Mod? GetMod(Package package)
	{
		if (IsValidModFolder(package.Folder, out var dllPath, out var version))
		{
			return new Mod(package, dllPath!, version!);
		}

		return null;
	}

	private bool IsValidModFolder(string dir, out string? dllPath, out Version? version)
	{
		try
		{
			var files = Directory.GetFiles(dir, "*.dll", SearchOption.AllDirectories);

			if (files != null && files.Length > 0)
			{
				if (CrossIO.CurrentPlatform is Platform.MacOSX)
				{
					return MacAssemblyUtil.FindImplementation(files, out dllPath, out version);
				}

				return Program.Services.GetService<AssemblyUtil>().FindImplementation(files, out dllPath, out version);
			}
		}
		catch { }

		dllPath = null;
		version = null;
		return false;
	}

	public void SavePendingValues()
	{
		if (_notifier.ApplyingProfile || _notifier.BulkUpdating)
		{
			return;
		}

#if DEBUG
		_logger.Debug("Saving pending mod values:\r\n" +
			$"_includedLibrary {_includedLibrary._dictionary.Count}\r\n" +
			$"_enabledLibrary {_enabledLibrary._dictionary.Count}");
#endif
		var saveSettings = _enabledLibrary.Any();

		_includedLibrary.Save();
		_enabledLibrary.Save();

		if (saveSettings)
		{
			_colossalOrderUtil.SaveSettings();
		}
	}

	public bool IsIncluded(Mod mod)
	{
		return _includedLibrary.GetValue(mod, out var included) ? included : IsLocallyIncluded(mod);
	}

	public bool IsEnabled(Mod mod)
	{
		return _enabledLibrary.GetValue(mod, out var enabled) ? enabled : IsLocallyEnabled(mod);
	}

	public bool IsLocallyIncluded(Mod mod)
	{
		return !CrossIO.FileExists(CrossIO.Combine(mod.Folder, ContentUtil.EXCLUDED_FILE_NAME));
	}

	public bool IsLocallyEnabled(Mod mod)
	{
		return _colossalOrderUtil.IsEnabled(mod);
	}

	public void SetIncluded(Mod mod, bool value)
	{
		if (!value && _modLogicManager.IsRequired(mod))
		{
			value = true;
		}

		if (_notifier.ApplyingProfile || _notifier.BulkUpdating)
		{
#if DEBUG
			_logger.Debug($"Delaying inclusion ({value}) for mod: {mod} (currently {IsLocallyIncluded(mod)}) ({mod.Folder})");
#endif
			_includedLibrary.SetValue(mod, value);
		}
		else
		{
			SetLocallyIncluded(mod, value);
		}

		if (_settings.SessionSettings.UserSettings.LinkModAssets && mod.Package.Assets != null)
		{
			Program.Services.GetService<IContentUtil>().SetBulkIncluded(mod.Package.Assets, value);
		}

		if (!_settings.SessionSettings.UserSettings.AdvancedIncludeEnable)
		{
			SetEnabled(mod, value);
			return;
		}

		_notifier.OnInclusionUpdated();
		_notifier.TriggerAutoSave();
	}

	public void SetLocallyIncluded(Mod mod, bool value)
	{
		try
		{
#if DEBUG
			_logger.Debug($"Applying Inclusion status ({value}) for mod: {mod} ({mod.Folder})");
#endif
			if ((value || _modLogicManager.IsRequired(mod)) && !_modLogicManager.IsForbidden(mod))
			{
#if DEBUG
				_logger.Debug($"Deleting the file ({CrossIO.Combine(mod.Folder, ContentUtil.EXCLUDED_FILE_NAME)})");
#endif
				CrossIO.DeleteFile(CrossIO.Combine(mod.Folder, ContentUtil.EXCLUDED_FILE_NAME));
			}
			else
			{
#if DEBUG
				_logger.Debug($"Creating the file ({CrossIO.Combine(mod.Folder, ContentUtil.EXCLUDED_FILE_NAME)})");
#endif
				File.WriteAllBytes(CrossIO.Combine(mod.Folder, ContentUtil.EXCLUDED_FILE_NAME), new byte[0]);
			}
		}
		catch (Exception ex)
		{
			_logger.Exception(ex, $"Failed to set 'Included' status ({value}) for {mod}");
		}
	}

	public void SetEnabled(Mod mod, bool value, bool save = true)
	{
		if (_notifier.ApplyingProfile || _notifier.BulkUpdating)
		{
			_enabledLibrary.SetValue(mod, value);
		}
		else
		{
			SetLocallyEnabled(mod, value, save);
		}

		_notifier.OnInclusionUpdated();
		_notifier.TriggerAutoSave();
	}

	public void SetLocallyEnabled(Mod mod, bool value, bool save)
	{
		try
		{
			if (_modLogicManager.IsRequired(mod))
			{
				value = true;
			}
			else if (_modLogicManager.IsForbidden(mod))
			{
				value = false;
			}

			_colossalOrderUtil.SetEnabled(mod, value);

			if (save)
			{
				_colossalOrderUtil.SaveSettings();
			}
		}
		catch (Exception ex)
		{
			_logger.Exception(ex, $"Failed to set 'Enabled' status ({value}) for {mod}");
		}
	}

	public DownloadStatus GetStatus(IPackage mod, out string reason)
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

	public Mod FindMod(string? folder)
	{
		return _contentManager.Mods.FirstOrDefault(x => x.Folder.Equals(folder, StringComparison.OrdinalIgnoreCase));
	}
}
