using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.CS1;
using SkyveApp.Domain.CS1.Utilities;
using SkyveApp.Domain.Systems;
using SkyveApp.Systems.CS1.Utilities.IO;

using SkyveShared;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkyveApp.Systems.CS1.Utilities;
internal class ModsUtil : IModUtil
{
	private readonly ModConfig _config;
	private readonly CachedSaveLibrary<IMod, bool> _includedLibrary;
	private readonly CachedSaveLibrary<IMod, bool> _enabledLibrary;

	private readonly ColossalOrderUtil _colossalOrderUtil;
	private readonly AssemblyUtil _assemblyUtil;
	private readonly MacAssemblyUtil _macAssemblyUtil;
	private readonly IPackageManager _contentManager;
	private readonly IModLogicManager _modLogicManager;
	private readonly ISettings _settings;
	private readonly ILogger _logger;
	private readonly INotifier _notifier;

	public HashSet<string> ExcludedHashSet { get; }

	public ModsUtil(IPackageManager contentManager, IModLogicManager modLogicManager, ISettings settings, ILogger logger, INotifier notifier, ColossalOrderUtil colossalOrderUtil, AssemblyUtil assemblyUtil, MacAssemblyUtil macAssemblyUtil)
	{
		_assemblyUtil = assemblyUtil;
		_contentManager = contentManager;
		_modLogicManager = modLogicManager;
		_colossalOrderUtil = colossalOrderUtil;
		_macAssemblyUtil = macAssemblyUtil;
		_settings = settings;
		_logger = logger;
		_notifier = notifier;

		_config = ModConfig.Deserialize();
		_includedLibrary = new(IsLocallyIncluded, SetLocallyIncluded);
		_enabledLibrary = new(IsLocallyEnabled, SetLocallyEnabled);

		ExcludedHashSet = new(_config.ModsInfo.Where(x => x.Excluded).Select(x => x.Path ?? string.Empty), new PathEqualityComparer());
	}

	public void SaveChanges()
	{
		if (_notifier.ApplyingPlayset || _notifier.BulkUpdating)
		{
			return;
		}

#if DEBUG
		_logger.Debug("Saving pending mod values:\r\n" +
			$"_includedLibrary {_includedLibrary.Count}\r\n" +
			$"_enabledLibrary {_enabledLibrary.Count}");
#endif
		var saveSettings = _enabledLibrary.Any();

		_includedLibrary.Save();
		_enabledLibrary.Save();

		if (saveSettings)
		{
			_colossalOrderUtil.SaveSettings();
		}
	}

	public bool IsIncluded(IMod mod)
	{
		return _includedLibrary.GetValue(mod, out var included) ? included : IsLocallyIncluded(mod);
	}

	public bool IsEnabled(IMod mod)
	{
		return _enabledLibrary.GetValue(mod, out var enabled) ? enabled : IsLocallyEnabled(mod);
	}

	public bool IsLocallyIncluded(IMod mod)
	{
		return !ExcludedHashSet.Contains(mod.Folder);
	}

	public bool IsLocallyEnabled(IMod mod)
	{
		return _colossalOrderUtil.IsEnabled(mod);
	}

	public void SetIncluded(IMod mod, bool value)
	{
		if (!value && _modLogicManager.IsRequired(mod, this))
		{
			value = true;
		}

		if (_notifier.ApplyingPlayset || _notifier.BulkUpdating)
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

		_notifier.OnInclusionUpdated();
		_notifier.TriggerAutoSave();
	}

	public void SetLocallyIncluded(IMod mod, bool value)
	{
		try
		{
#if DEBUG
			_logger.Debug($"Applying Inclusion status ({value}) for mod: {mod} ({mod.Folder})");
#endif
			if ((value || _modLogicManager.IsRequired(mod, this)) && !_modLogicManager.IsForbidden(mod))
			{
				ExcludedHashSet.Remove(mod.Folder);
			}
			else
			{
				ExcludedHashSet.Add(mod.Folder);
			}

			SaveChanges();
		}
		catch (Exception ex)
		{
			_logger.Exception(ex, $"Failed to set 'Included' status ({value}) for {mod}");
		}
	}

	public void SetEnabled(IMod mod, bool value)
	{
		SetEnabled(mod, value, true);
	}

	public void SetEnabled(IMod mod, bool value, bool save)
	{
		if (_notifier.ApplyingPlayset || _notifier.BulkUpdating)
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

	public void SetLocallyEnabled(IMod mod, bool value)
	{
		SetLocallyEnabled(mod, value, false);
	}

	public void SetLocallyEnabled(IMod mod, bool value, bool save)
	{
		try
		{
			if (_modLogicManager.IsRequired(mod, this))
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

	public IMod? GetMod(ILocalPackageWithContents package)
	{
		return IsValidModFolder(package.Folder, out var dllPath, out var version) ? new Mod(package, dllPath!, version!) : (IMod?)null;
	}

	private bool IsValidModFolder(string dir, out string? dllPath, out Version? version)
	{
		try
		{
			var files = Directory.GetFiles(dir, "*.dll", SearchOption.AllDirectories);

			if (files != null && files.Length > 0)
			{
				return CrossIO.CurrentPlatform is Platform.MacOSX
					? _macAssemblyUtil.FindImplementation(files, out dllPath, out version)
					: _assemblyUtil.FindImplementation(files, out dllPath, out version);
			}
		}
		catch { }

		dllPath = null;
		version = null;
		return false;
	}
}
