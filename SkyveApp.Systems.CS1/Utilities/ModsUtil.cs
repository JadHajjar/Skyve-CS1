﻿using Extensions;

using Newtonsoft.Json.Linq;

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
	private readonly Dictionary<string, ModConfig.ModInfo> _modConfigInfo;

	private readonly ColossalOrderUtil _colossalOrderUtil;
	private readonly AssemblyUtil _assemblyUtil;
	private readonly MacAssemblyUtil _macAssemblyUtil;
	private readonly IModLogicManager _modLogicManager;
	private readonly INotifier _notifier;
	private readonly ISettings _settings;

	public ModsUtil(IModLogicManager modLogicManager, INotifier notifier, ColossalOrderUtil colossalOrderUtil, AssemblyUtil assemblyUtil, MacAssemblyUtil macAssemblyUtil, ISettings settings)
	{
		_assemblyUtil = assemblyUtil;
		_modLogicManager = modLogicManager;
		_colossalOrderUtil = colossalOrderUtil;
		_macAssemblyUtil = macAssemblyUtil;
		_notifier = notifier;
		_settings = settings;

		_notifier.CompatibilityDataLoaded += BuildLoadOrder;

		_config = ModConfig.Deserialize();
		_modConfigInfo = _config.GetModsInfo();
	}

	private void BuildLoadOrder()
	{
		var index = 1;
		var mods = ServiceCenter.Get<ILoadOrderHelper>().GetOrderedMods().Reverse();

		foreach (var mod in mods)
		{
			var modInfo = _modConfigInfo.TryGetValue(mod.Folder, out var info) ? info : new();

			modInfo.LoadOrder = index++;

			_modConfigInfo[mod.Folder] = modInfo;
		}

		SaveChanges();
	}

	public void SaveChanges()
	{
		if (_notifier.ApplyingPlayset || _notifier.BulkUpdating)
		{
			return;
		}

		_config.SetModsInfo(_modConfigInfo);
		_config.Serialize();

		_colossalOrderUtil.SaveSettings();
	}

	public bool IsIncluded(IMod mod)
	{
		return !(_modConfigInfo.TryGetValue(mod.Folder, out var info) && info.Excluded);
	}

	public bool IsEnabled(IMod mod)
	{
		return _colossalOrderUtil.IsEnabled(mod);
	}

	public void SetIncluded(IMod mod, bool value)
	{
		value = (value || _modLogicManager.IsRequired(mod, this)) && !_modLogicManager.IsForbidden(mod);

		var modInfo = _modConfigInfo.TryGetValue(mod.Folder, out var info) ? info : new();

		modInfo.Excluded = !value;

		_modConfigInfo[mod.Folder] = modInfo;

		if (!_settings.UserSettings.AdvancedIncludeEnable)
		{
			_colossalOrderUtil.SetEnabled(mod, value);
		}

		_notifier.OnInclusionUpdated();
		_notifier.TriggerAutoSave();

		SaveChanges();
	}

	public void SetEnabled(IMod mod, bool value)
	{
		value = (value || _modLogicManager.IsRequired(mod, this)) && !_modLogicManager.IsForbidden(mod);

		_colossalOrderUtil.SetEnabled(mod, value);

		_notifier.OnInclusionUpdated();
		_notifier.TriggerAutoSave();

		SaveChanges();
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