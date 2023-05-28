using ColossalFramework.IO;
using ColossalFramework.Packaging;
using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;

using HarmonyLib;

using System;

namespace SkyveMod.Patches.HotReload;

[HarmonyPatch(typeof(PluginManager), "OnFileWatcherEventChanged", new[] { typeof(object), typeof(ReporterEventArgs) })]
public class AutoReloadBlock1
{
	public static bool Prefix()
	{
		return false;
	}
}

[HarmonyPatch(typeof(PluginManager), "OnFileWatcherEventCreated", new[] { typeof(object), typeof(ReporterEventArgs) })]
public class AutoReloadBlock2
{
	public static bool Prefix()
	{
		return false;
	}
}

[HarmonyPatch(typeof(PackageManager), "OnFileWatcherEventChanged", new[] { typeof(object), typeof(ReporterEventArgs) })]
public class AutoReloadBlock3
{
	public static bool Prefix()
	{
		return false;
	}
}

[HarmonyPatch(typeof(PackageManager), "OnFileWatcherEventCreated", new[] { typeof(object), typeof(ReporterEventArgs) })]
public class AutoReloadBlock4
{
	public static bool Prefix()
	{
		return false;
	}
}

[HarmonyPatch(typeof(Workshop), "EventWorkshopItemInstalled", new[] {typeof(PublishedFileId) })]
public class AutoReloadBlock5
{
	public static bool Prefix()
	{
		return false;
	}
}

[HarmonyPatch(typeof(Workshop), "EventWorkshopSubscriptionChanged", new[] {typeof(PublishedFileId), typeof(bool) })]
public class AutoReloadBlock6
{
	public static bool Prefix()
	{
		return false;
	}
}

//[HarmonyPatch(typeof(PackageManager), "ForceAssetStateChanged", new Type[0])]
//public class AutoReloadBlock3
//{
//	public static bool Prefix()
//	{
//		return false;
//	}
//}

//[HarmonyPatch(typeof(PluginManager), "TriggerEvents", new Type[0])]
//public class AutoReloadBlock4
//{
//	public static bool Prefix()
//	{
//		return false;
//	}
//}
