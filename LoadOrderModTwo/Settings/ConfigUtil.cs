namespace LoadOrderMod.Settings {
    extern alias Injections;
    using Injections.LoadOrderInjections;
    using ColossalFramework.IO;
    using ColossalFramework.Packaging;
    using ColossalFramework.PlatformServices;
    using ColossalFramework.Plugins;
    using HarmonyLib;
    using KianCommons;
    using KianCommons.Plugins;
    using LoadOrderMod.Util;
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using static ColossalFramework.Plugins.PluginManager;
    using static KianCommons.ReflectionHelpers;
    using System.Globalization;
    using System.Collections;
    using LoadOrderShared;

    public static class ConfigUtil {
        public static string LocalLoadOrderPath => Path.Combine(DataLocation.localApplicationData, "LoadOrderTwo");

        private static Hashtable assetsTable_;

        private static LoadOrderConfig config_;
        public static LoadOrderConfig Config {
            get {
                try {
                    Init();
                    return config_;
                } catch (Exception ex) {
                    Log.Exception(ex);
                    return null;
                }
            }
        }

        private static void Init() {
            if (config_ != null) return; // already initialized.
            LogCalled();
            config_ =
                LoadOrderConfig.Deserialize()
                ?? new LoadOrderConfig();

            int n = Math.Max(PlatformService.workshop.GetSubscribedItemCount(),  config_.Assets.Length);
            assetsTable_ = new Hashtable(n * 10);
            foreach(var assetInfo in config_.Assets)
                assetsTable_[assetInfo.Path] = assetInfo;
                
            SaveThread.Init();
        }

        public static void Terminate() {
            LogCalled();
            SaveThread.Terminate();
            config_ = null;
        }

        public static void SaveConfig() {
            try {
                //LogCalled();
                SaveThread.Dirty = false;
                if (config_ == null) return;
                lock (SaveThread.LockObject)
                    config_.Serialize();
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }


        // this is useful to store author details after call back.
        internal static class SaveThread {
            const int INTERVAL_MS = 1000;
            static bool isRunning_;
            private static Thread thread_;

            public static bool Dirty = false;
            public readonly static object LockObject = new object();

            static SaveThread() => Init();

            internal static void Init() {
                if (isRunning_ != null) return; // already running.
                thread_ = new Thread(RunThread);
                thread_.Name = "SaveThread";
                thread_.IsBackground = true;
                isRunning_ = true;
                thread_.Start();
            }

            internal static void Terminate() {
                Flush();
                isRunning_ = false;
                thread_ = null;
            }

            private static void RunThread() {
                try {
                    while (isRunning_) {
                        Thread.Sleep(INTERVAL_MS);
                        Flush();
                    }
                    Log.Info("Save Thread Exiting...");
                } catch (Exception ex) {
                    Log.Exception(ex);
                }
            }
            public static void Flush() {
                if (Dirty)
                    SaveConfig();
            }
        }

        internal static bool HasLoadOrder(this PluginInfo p) {
            var mod = p.GetModConfig();
            if (mod == null)
                return false;
            return mod.LoadOrder != LoadOrderConfig.DefaultLoadOrder;
        }

        internal static int GetLoadOrder(this PluginInfo p) {
            var mod = p.GetModConfig();
            if (mod == null)
                return LoadOrderConfig.DefaultLoadOrder;
            return mod.LoadOrder;
        }

        internal static ModInfo GetModConfig(this PluginInfo p) =>
            Config?.Mods?.FirstOrDefault(item => item.Path == p.modPath);

        internal static AssetInfo GetAssetConfig(this Package.Asset a) =>
            assetsTable_[a.GetPath()] as AssetInfo;
        
        internal static string GetPath(this Package.Asset a) => a.package.packagePath;
    }
}
