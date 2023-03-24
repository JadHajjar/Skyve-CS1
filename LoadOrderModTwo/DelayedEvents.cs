using System;
using ColossalFramework.Packaging;
using ColossalFramework.Plugins;
using KianCommons;
using static KianCommons.ReflectionHelpers;
using ColossalFramework.Threading;

namespace LoadOrderMod {
    //public interface IEventInvoker {
    //    public bool IsEnabled { get; }
    //    public bool IsMarked { get; set; }

    //    public void Disable();
    //    public void Enable();

    //    public void Trigger();
    //    public void Force();
    //    public void Flush();
    //}

    public static class AssetStateChangedInvoker {
        public static void Disable() => PackageManager.DisableEvents();
        public static void Enable() => PackageManager.EnabledEvents();
        public static bool IsEnabled =>
            (int)GetFieldValue<PackageManager>("m_EventsEnabled") == 0;
        
        public static bool IsDirty { get; set; }

        public static void Trigger() {
            if (IsEnabled) {
                IsDirty = false;
                if (Dispatcher.currentSafe == ThreadHelper.dispatcher) {
                    PackageManager.ForceAssetStateChanged();
                } else {
                    ThreadHelper.dispatcher.Dispatch(delegate () {
                        PackageManager.ForceAssetStateChanged();
                    });
                }
            } else {
                IsDirty = true;
            }
        }

        public static void Force() {
            IsDirty = false;
            PackageManager.ForceAssetStateChanged();
        }

        public static void Flush() {
            if (IsDirty)
                Trigger();
        }
    }

    public static class PackagesChangedInvoker {
        public static void Disable() => PackageManager.DisableEvents();
        public static void Enable() => PackageManager.EnabledEvents();
        public static bool IsEnabled =>
            (int)GetFieldValue<PackageManager>("m_EventsEnabled") == 0;
        public static bool IsDirty { get; set; }

        public static void Trigger() {
            if (IsEnabled) {
                // includes AssetStateChanged
                IsDirty = AssetStateChangedInvoker.IsDirty = false;
                InvokeMethod(typeof(PackageManager), "TriggerEvents");
            } else IsDirty = true;
        }

        public static void Force() {
            IsDirty = AssetStateChangedInvoker.IsDirty = false;
            PackageManager.ForcePackagesChanged();
        }

        public static void Flush() {
            if (IsDirty)
                Trigger();
        }
    }

    public static class PluginsChangedInvoker {
        public static void Disable() => PluginManager.DisableEvents();
        public static void Enable() => PluginManager.EnabledEvents();
        public static bool IsEnabled =>
            (int)GetFieldValue<PluginManager>("m_EventsEnabled") == 0;
        public static bool IsDirty { get; set; }

        public static void Trigger() {
            if (IsEnabled) {
                // includes PluginsStateChanged
                IsDirty = PluginsStateChangedInvoker.IsDirty = false;
                //TODO use Dispatcher
                PluginManager.instance.ForcePluginsChanged();
                // ForcePluginsChanged() calls TriggerEventPluginsChanged()
                // InvokeMethod(PluginManager.instance, "TriggerEventPluginsChanged");
            } else IsDirty = true;
        }

        public static void Force() {
            IsDirty = PluginsStateChangedInvoker.IsDirty = false;
            //todo: force (this does not actually force).
            PluginManager.instance.ForcePluginsChanged();
        }

        public static void Flush() {
            if (IsDirty)
                Trigger();
        }
    }

    public static class PluginsStateChangedInvoker {
        public static void Disable() => PluginManager.DisableEvents();
        public static void Enable() => PluginManager.EnabledEvents();
        public static bool IsEnabled =>
            (int)GetFieldValue<PluginManager>("m_EventsEnabled") == 0;
        public static bool IsDirty { get; set; }

        public static void Trigger() {
            if (IsEnabled) {
                IsDirty = false;
                // TODO use Dispatcher
                InvokeMethod(PluginManager.instance, "TriggerEventPluginsStateChanged");
            } else IsDirty = true;
        }

        public static void Force() {
            IsDirty = false;
            // todo: force 
            InvokeMethod(PluginManager.instance, "TriggerEventPluginsStateChanged");
        }

        public static void Flush() {
            if (IsDirty)
                Trigger();
        }
    }

    public static class DelayedEventInvoker {
        public static void Enable() {
            PackageManager.EnabledEvents();
            PluginManager.EnabledEvents();
        }
        public static void Disable() {
            PackageManager.DisableEvents();
            PluginManager.DisableEvents();
        }
        public static void Flush() {
            PackagesChangedInvoker.Flush();
            AssetStateChangedInvoker.Flush();
            PluginsChangedInvoker.Flush();
            PluginsStateChangedInvoker.Flush();
        }
    }
}
