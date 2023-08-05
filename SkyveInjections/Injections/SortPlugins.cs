using ColossalFramework.PlatformServices;

using KianCommons;

using SkyveShared;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using PluginInfo = ColossalFramework.Plugins.PluginManager.PluginInfo;

namespace SkyveInjections.Injections;
/// <summary>
/// sort plugins and excludes excluded plugins
/// </summary>
public static class SortPlugins
{
	public static void Sort(Dictionary<string, PluginInfo> plugins)
	{
		try
		{
			Log.Debug("Sorting assemblies ...", true);

			var config = ModConfig.Deserialize();
			var dictionary = config.GetModsInfo();

			var list = plugins
				.Where(pair => !CMPatchHelpers.IsDirectoryExcluded(pair.Key))
				.OrderByDescending(x => dictionary.TryGetValue(x.Value.modPath, out var info) ? info.LoadOrder : 0)
				.ToList();

			plugins.Clear();

			foreach (var pair in list)
			{
				plugins.Add(pair.Key, pair.Value);
			}

			ReplaceAssembies.Init(plugins.Values.ToArray());

#if DEBUG
			Log.Debug("\n=========================== plugins.Values: =======================", false);
			foreach (var p in plugins.Values)
			{
				var dllFiles = Directory.GetFiles(p.modPath, "*.dll", SearchOption.AllDirectories);
				// exclude assets.
				if (!dllFiles.IsNullorEmpty())
				{
					var dlls = string.Join(", ", dllFiles);
					Log.Debug(
						$"loadOrder={(dictionary.TryGetValue(p.modPath, out var info) ? info.LoadOrder : 0)} path={p.modPath} dlls={{{dlls}}}"
						, false);
				}
			}
			Log.Debug("\n=========================== END plugins.Values =====================\n", false);
#endif
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
		}
	}
}