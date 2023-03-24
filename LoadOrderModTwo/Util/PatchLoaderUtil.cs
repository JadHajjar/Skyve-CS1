namespace LoadOrderMod {
    using System;
    using System.Reflection;
    using ColossalFramework.PlatformServices;
    using ColossalFramework.Plugins;
    using ColossalFramework.UI;
    using UnityEngine;
    using KianCommons;

    public class PatchLoaderStatus {
        public const ulong PatchLoaderWorkshopId = 2041457644u;

        private bool _initialized;
        private bool _errorModalOpened;

        private static PatchLoaderStatus _instance;

        public static PatchLoaderStatus Instance => _instance ?? (_instance = new PatchLoaderStatus());

        public bool IsPatchLoaderAvailable { get; private set; }
        public int PatchLoaderStatusIndex { get; private set; } = -1;

        public bool IsSubscribed {
            get {
                PublishedFileId[] subscribedIds = PlatformService.workshop.GetSubscribedItems();
                if(subscribedIds == null) return false;

                foreach(PublishedFileId id in subscribedIds) {
                    if(id.AsUInt64 == PatchLoaderWorkshopId) return true;
                }

                return false;
            }
        }

        private PatchLoaderStatus() {
            Init();
        }

        private void Init() {
            if(_initialized) return;
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(Assembly assembly in assemblies) {
                if(assembly.GetName().Name.Equals("PatchLoaderMod")) {
                    IsPatchLoaderAvailable = true;
                }
            }
            _initialized = true;
        }

        public void Cleanup() {
            IsPatchLoaderAvailable = false;
            _initialized = false;
            _instance = null;
        }

        public void EnsureLoaderAvailable() {
            if(!IsPatchLoaderAvailable) {
                TrySubscribe();
            }
        }

        public bool IsAvailbleAndEnabled => IsPatchLoaderModEnabled() && IsPatchLoaderAvailable;

        public static readonly string WindowsCriticalErrorSolutions =
            "Possible solutions (follow order of operations):\n" +
            "---------------------------------------------------------------------\n" +
            WindowsCriticalErrorSolutions +
            "Solution 1:\n" +
            " 1.Close game completely\n" +
            " 2.Close Steam client (right-click try icon, 'Exit')\n" +
            " 3.Start Steam client normally\n" +
            " 4.Start game either directly from Steam\n" +
            "---------------------------------------------------------------------\n" +
            "Solution 2:\n" +
            " 1.Restart PC\n" +
            " 2.Start Steam client first\n" +
            " 3.Start the game after Steam launches\n" +
            "---------------------------------------------------------------------\n" +
            "======================================\n" +
            "If above solutions won't work, contact mod author for more info how to resolve the problem\n\n";

        private bool IsPatchLoaderModEnabled() {
            Log.Called();
            foreach(PluginManager.PluginInfo pluginInfo in PluginManager.instance.GetPluginsInfo()) {
                if(pluginInfo.name.Contains("PatchLoader") || pluginInfo.name.Contains("2041457644")) {
                    if(pluginInfo.isEnabled) {
                        FieldInfo fieldInfo = pluginInfo.userModInstance.GetType().GetField(
                            "_doorstopManager",
                            BindingFlags.Instance |
                            BindingFlags.NonPublic);
                        if(fieldInfo != null) {
                            object doorstopManager = fieldInfo.GetValue(pluginInfo.userModInstance);
                            if(doorstopManager != null) {
                                PropertyInfo upgradeManager = doorstopManager
                                                              .GetType().GetProperty(
                                                                  "UpgradeManager",
                                                                  BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                                if(upgradeManager != null) {
                                    object upgradeManagerValue = upgradeManager.GetValue(doorstopManager, null);
                                    if(upgradeManagerValue != null) {
                                        PropertyInfo stateProperty = upgradeManagerValue
                                                                     .GetType().GetProperty("State");
                                        if(stateProperty != null) {
                                            int stateValue = (int)stateProperty.GetValue(upgradeManagerValue, null);
                                            Log.Info("PatchLoader StateValue " + stateValue);
                                            PatchLoaderStatusIndex = stateValue;
                                            return stateValue < 2; //Latest or Outdated, other are Upgrade PhaseX or Error
                                        }
                                        Log.Error("State propertyInfo is null");
                                    } else {
                                        Log.Error("UpgradeManager property value is null");
                                    }
                                } else {
                                    Log.Error("UpgradeManager propertyInfo is null");
                                }
                            } else {
                                Log.Error("DoorstopManager field value is null");
                            }
                        } else {
                            Log.Error("DoorstopManager fieldInfo is null");
                        }
                    }
                    Log.Info("PatchLoader mod is disabled!");

                    PatchLoaderStatusIndex = -1;
                    return false;
                }
            }

            PatchLoaderStatusIndex = -1;
            Log.Info("PatchLoader mod not loaded!");
            return false;
        }

        private void TrySubscribe() {
            if(PlatformService.platformType != PlatformType.Steam) {
                ShowError();
                return;
            }

            if(PluginManager.noWorkshop) {
                ShowError();
                return;
            }
            if(!PlatformService.workshop.IsAvailable()) {
                ShowError();
                return;
            }

            if(!PlatformService.workshop.Subscribe(new PublishedFileId(PatchLoaderWorkshopId))) {
                ShowError();
            }
        }

        private void ShowError() {
            if(_errorModalOpened) return;

            string reason = "An error occurred while attempting to automatically subscribe to PatchLoader mod (no network connection?)";
            string solution = "You can manually download the PatchLoader mod from github.com/CitiesSkylinesMods/PatchLoader/releases";
            if(PlatformService.platformType != PlatformType.Steam) {
                reason = "Patch Loader mod could not be installed automatically because you are using a platform other than Steam.";
            } else if(PluginManager.noWorkshop) {
                reason = "Patch Loader mod could not be installed automatically because you are playing in --noWorkshop mode!";
                solution = "Restart without --noWorkshop or manually download the Harmony mod from github.com/CitiesSkylinesMods/PatchLoader/releases";
            } else if(!PlatformService.workshop.IsAvailable()) {
                reason = "Patch Loader mod could not be installed automatically because the Steam workshop is not available (no network connection?)";
            }

            string message = $"FPS Booster require the dependency 'Patch Loader mod' to work correctly!\n\n{reason}\n\n{solution}";
            ExceptionPanel exceptionPanel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
            exceptionPanel.SetMessage("Missing dependency: Patch Loader mod", message, false);
            exceptionPanel.component.Focus();
        }
    }
}