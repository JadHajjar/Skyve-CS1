using ColossalFramework.IO;
using ColossalFramework.Packaging;
using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;

using HarmonyLib;

using SkyveMod.Settings.Tabs;

namespace SkyveMod.Patches.HotReload;

[HarmonyPatch(typeof(PluginManager), "OnFileWatcherEventChanged", new[] { typeof(object), typeof(ReporterEventArgs) })]
public class AutoReloadBlock1
{
	public static bool Prefix()
	{
		return DebugTab.HotReload;
	}
}

[HarmonyPatch(typeof(PluginManager), "OnFileWatcherEventCreated", new[] { typeof(object), typeof(ReporterEventArgs) })]
public class AutoReloadBlock2
{
	public static bool Prefix()
	{
		return DebugTab.HotReload;
	}
}

[HarmonyPatch(typeof(PackageManager), "OnFileWatcherEventChanged", new[] { typeof(object), typeof(ReporterEventArgs) })]
public class AutoReloadBlock3
{
	public static bool Prefix()
	{
		return DebugTab.HotReload;
	}
}

[HarmonyPatch(typeof(PackageManager), "OnFileWatcherEventCreated", new[] { typeof(object), typeof(ReporterEventArgs) })]
public class AutoReloadBlock4
{
	public static bool Prefix()
	{
		return DebugTab.HotReload;
	}
}

[HarmonyPatch(typeof(Workshop), "EventWorkshopItemInstalled", new[] { typeof(PublishedFileId) })]
public class AutoReloadBlock5
{
	public static bool Prefix()
	{
		return DebugTab.HotReload;
	}
}

[HarmonyPatch(typeof(Workshop), "EventWorkshopSubscriptionChanged", new[] { typeof(PublishedFileId), typeof(bool) })]
public class AutoReloadBlock6
{
	public static bool Prefix()
	{
		return DebugTab.HotReload;
	}
}