using ColossalFramework.Plugins;

using HarmonyLib;

using KianCommons;

using SkyveMod.Util;

using System;
using System.Collections.Generic;

using static ColossalFramework.Plugins.PluginManager;
using static KianCommons.ReflectionHelpers;

namespace SkyveMod.Patches.HotReload;
[HarmonyPatch(typeof(PluginManager), "RemovePluginAtPath")]
public static class RemovePluginAtPathPatch
{
	/// <summary>
	/// pluginInfo.name that is being removed. the name of the containing directory.
	/// (different than IUserMod.Name).
	/// </summary>        
	public static string name;

	static void Prefix(string path, Dictionary<string, PluginInfo> ___m_Plugins)
	{
		try
		{
			LogCalled(path);
			if (___m_Plugins.TryGetValue(path, out var p) && p.isEnabled)
			{
				name = p.GetUserModName();
			}
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
		}
	}

	static void Finalizer(Exception __exception)
	{
		LogCalled();
		name = null;
		if (__exception != null)
		{
			Log.Exception(__exception);
		}
	}
}
