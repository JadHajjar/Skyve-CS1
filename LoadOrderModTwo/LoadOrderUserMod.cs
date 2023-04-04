extern alias Injections;

using CitiesHarmony.API;

using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;
using ColossalFramework.UI;

using ICities;

using KianCommons;

using LoadOrderMod.Data;
using LoadOrderMod.UI;
using LoadOrderMod.UI.EntryAction;
using LoadOrderMod.UI.EntryStatus;
using LoadOrderMod.Util;

using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

using UnityEngine;
using UnityEngine.SceneManagement;

using SteamUtilities = Injections.LoadOrderInjections.SteamUtilities;

namespace LoadOrderMod;
public class LoadOrderUserMod : IUserMod
{
	public static Version ModVersion => typeof(LoadOrderUserMod).Assembly.GetName().Version;
	public static string VersionString => ModVersion.ToString(2);
	public string Name => "Load Order Mod " + VersionString;
	public string Description => "Manage your custom content easily, from one place.";
	public static string HARMONY_ID = "CS.TDW.LoadOrder";
	private UIButton lotButton;

	//static LoadOrderMod() => Log.Debug("Static Ctor "   + Environment.StackTrace);
	//public LoadOrderMod() => Log.Debug("Instance Ctor " + Environment.StackTrace);

	static bool HasDuplicate()
	{
		var currentASM = typeof(LoadOrderUserMod).Assembly;
		foreach (var plugin in PluginManager.instance.GetPluginsInfo())
		{
			foreach (var a in plugin.GetAssemblies())
			{
				if (a != currentASM && a.Name() == currentASM.Name())
				{
					return true;
				}
			}
		}
		return false;
	}

	void CheckDuplicate()
	{
		if (HasDuplicate())
		{
			var m = "There are multiple versions of Load Order Mod. Please exluclude all but one.";
			Log.DisplayError(m);
			throw new Exception(m);
		}
	}

	public void OnEnabled()
	{
		CheckDuplicate();
		try
		{
			Log.Called();


			Util.LoadOrderUtil.ApplyGameLoggingImprovements();
			Log.Info("Cloud.enabled=" + (PlatformService.cloud?.enabled).ToSTR(), true);

			var args = Environment.GetCommandLineArgs();
			Log.Info("command line args are: " + string.Join(" ", args));

			Log.ShowGap = true;
#if DEBUG
			Log.Buffered = true;
#else
			Log.Buffered = false;
#endif
			var items = PlatformService.workshop.GetSubscribedItems();
			Log.Info("Subscribed Items are: " + items.ToSTR());

			//Log.Debug("Testing StackTrace:\n" + new StackTrace(true).ToString(), copyToGameLog: true);
			//KianCommons.UI.TextureUtil.EmbededResources = false;
			//HelpersExtensions.VERBOSE = false;
			//foreach(var p in ColossalFramework.Plugins.PluginManager.instance.GetPluginsInfo()) {
			//    string savedKey = p.name + p.modPath.GetHashCode().ToString() + ".enabled";
			//    Log.Debug($"plugin info: savedKey={savedKey} cachedName={p.name} modPath={p.modPath}");
			//}
			CheckPatchLoader();

			HarmonyHelper.DoOnHarmonyReady(() =>
			{
				//HarmonyLib.Harmony.DEBUG = true;
				HarmonyUtil.InstallHarmony(HARMONY_ID, null, null); // continue on error.
			});
			SceneManager.sceneLoaded += OnSceneLoaded;
			SceneManager.activeSceneChanged += OnActiveSceneChanged;

			LoadingManager.instance.m_introLoaded += LoadOrderUtil.TurnOffSteamPanels;
			LoadingManager.instance.m_introLoaded += CheckPatchLoader;

			LoadOrderUtil.TurnOffSteamPanels();

			try
			{
				if (!File.Exists(Path.Combine(DataLocation.localApplicationData, Path.Combine("LoadOrderTwo", "SetupComplete.txt"))))
				{
					Debug.Log("Filling tool configuration");
					PrepareTool();
				}
				else
					Debug.Log("Skipping tool configuration, SetupComplete.txt was detected");
			}
			catch (Exception ex) { Debug.LogException(ex); }

			var introLoaded = ContentManagerUtil.IsIntroLoaded;
			if (introLoaded)
			{
				CacheUtil.CacheData();
			}
			else
			{
				var resetIsEnabledForAssets = Environment.GetCommandLineArgs().Any(_arg => _arg == "-reset-assets");
				if (resetIsEnabledForAssets)
				{
					LoadOrderUtil.ResetIsEnabledForAssets();
				}
				LoadingManager.instance.m_introLoaded += CacheUtil.CacheData;
			}

			if (!Settings.ConfigUtil.Config.IgnoranceIsBliss)
			{
				CheckSubsUtil.RegisterEvents();
			}

			SceneManager.sceneLoaded += MainMenuLoaded;

			MainMenuLoaded(default, default);

			Log.Flush();
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
		}
	}

	private void PrepareTool()
	{
		var currentToolFolder = Path.Combine(PluginManager.instance.FindPluginInfo(Assembly.GetExecutingAssembly())?.modPath, "Tool");
		var config = Path.Combine(currentToolFolder, "LoadOrderToolTwo.exe.config");

		if (File.Exists(config))
		{
			PrepareFirstTimeConfig(config);
		}
		else
			Debug.Warning("Tool configuration was not found in: " + config);
	}

	private void PrepareFirstTimeConfig(string configFilePath)
	{
		// Load the external application's .config file
		var configFileMap = new ExeConfigurationFileMap
		{
			ExeConfigFilename = configFilePath
		};
		var externalConfig = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

		// Get the appsettings section from the external config
		var appSettings = externalConfig.AppSettings;

		// Iterate through the appsettings keys and values
		foreach (var key in appSettings.Settings.AllKeys)
		{
			switch (key)
			{
				case "GamePath":
					appSettings.Settings[key].Value = DataLocation.applicationBase;
					break;
				case "AppDataPath":
					appSettings.Settings[key].Value = DataLocation.localApplicationData;
					break;
				case "Platform":
					if (Application.platform is RuntimePlatform.OSXEditor or RuntimePlatform.OSXPlayer)
					{
						appSettings.Settings[key].Value = "MacOSX";
					}
					else if (Application.platform == RuntimePlatform.LinuxPlayer)
					{
						appSettings.Settings[key].Value = "Linux";
					}
					break;
			}
		}

		externalConfig.Save();
	}

	public void OnDisabled()
	{
		try
		{
			SceneManager.sceneLoaded -= MainMenuLoaded;

			foreach (var item in GameObject.FindObjectsOfType<EntryStatusPanel>())
			{
				if (item)
				{
					GameObject.Destroy(item.gameObject);
				}
			}
			foreach (var item in GameObject.FindObjectsOfType<EntryActionPanel>())
			{
				if (item)
				{
					GameObject.Destroy(item?.gameObject);
				}
			}

			LoadingManager.instance.m_introLoaded -= CacheUtil.CacheData;
			LoadingManager.instance.m_introLoaded -= LoadOrderUtil.TurnOffSteamPanels;
			LoadingManager.instance.m_introLoaded -= CheckPatchLoader;
			HarmonyUtil.UninstallHarmony(HARMONY_ID);
			MonoStatus.Release();
			LOMAssetDataExtension.Release();

			Settings.ConfigUtil.Terminate();
			CheckSubsUtil.RemoveEvents();
			Log.Buffered = false;

			try
			{
				if (lotButton != null && lotButton)
				{
					GameObject.Destroy(lotButton?.gameObject);
				}
			}
			catch { }

			Debug.Log("Load Order Mod Disabled");
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
		}
	}

	private void MainMenuLoaded(Scene arg0, LoadSceneMode arg1)
	{
		var centerPanel = GameObject.Find("MenuContainer")?.GetComponent<UIPanel>().Find<UISlicedSprite>("CenterPart")?.Find<UIPanel>("MenuArea")?.Find<UIPanel>("Menu");

		var continueButton = centerPanel?.Find<UIButton>("Exit");

		if (continueButton != null)
		{
			lotButton = centerPanel.AddUIComponent<UIButton>();

			continueButton.ShalowClone(lotButton, true);
			lotButton.text = "LOAD ORDER TOOL";
			lotButton.name = lotButton.cachedName = "LOTBUTTON";
			lotButton.stringUserData = "";
			lotButton.atlas = continueButton.atlas;
			lotButton.font = continueButton.font;
			lotButton.disabledBgSprite = continueButton.disabledBgSprite;
			lotButton.disabledFgSprite = continueButton.disabledFgSprite;
			lotButton.focusedBgSprite = continueButton.focusedBgSprite;
			lotButton.focusedFgSprite = continueButton.focusedFgSprite;
			lotButton.hoveredBgSprite = continueButton.hoveredBgSprite;
			lotButton.hoveredFgSprite = continueButton.hoveredFgSprite;
			lotButton.normalBgSprite = continueButton.normalBgSprite;
			lotButton.normalFgSprite = continueButton.normalFgSprite;
			lotButton.pressedBgSprite = continueButton.pressedBgSprite;
			lotButton.pressedFgSprite = continueButton.pressedFgSprite;
			lotButton.zOrder -= 3;

			lotButton.eventClick += LOT_eventClick;

			(GameObject.Find("MenuContainer")?.GetComponent<UIPanel>().Find<UISlicedSprite>("CenterPart")).height += lotButton.height;
		}
	}

	private void LOT_eventClick(UIComponent component, UIMouseEventParameter eventParam)
	{
		var currentToolFolder = Path.Combine(PluginManager.instance.FindPluginInfo(Assembly.GetExecutingAssembly())?.modPath, "Tool");
		var toolPath = Path.Combine(currentToolFolder, "LoadOrderToolTwo.exe");
		var openTools = false;

		if (Application.platform is RuntimePlatform.OSXEditor or RuntimePlatform.OSXPlayer or RuntimePlatform.LinuxPlayer)
		{
			System.Diagnostics.Process.Start(Directory.GetParent(toolPath).FullName);
			return;
		}

		try
		{
			openTools = System.Diagnostics.Process.GetProcessesByName(Path.GetFileNameWithoutExtension(toolPath)).Length > 0;
		}
		catch { }

		try
		{
			if (openTools)
			{
				File.WriteAllText(Path.Combine(Directory.GetParent(toolPath).FullName, "Wake"), "It's time to wake up");
			}
			else if (File.Exists(toolPath))
			{
				System.Diagnostics.Process.Start(toolPath, "-stub");
			}
			else
			{
				var panel = UIView.library.ShowModal<ExceptionPanel>("ToolMissing");
				panel.SetMessage("Load Order Tool Missing", "The Load Order Tool application is missing from your computer.\r\n\r\nThis may be caused by missing files in the mod folder, or that your antivirus removed it.", true);
			}
		}
		catch (Exception ex) { UnityEngine.Debug.LogException(ex); }
	}

	public void CheckPatchLoader()
	{
		Log.Info("SteamUtilities.Initialized=" + SteamUtilities.Initialized);
		if (!SteamUtilities.Initialized && PatchLoaderStatus.Instance.IsAvailbleAndEnabled)
		{
			Log.DisplayWarning($"Patch Loader Ineffective. Some LOM features might not work!\n\n" + PatchLoaderStatus.WindowsCriticalErrorSolutions);
		}
	}

	public static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		Log.Info($"OnSceneLoaded({scene.name}, {mode})", true);
		if (scene.name == "MainMenu")
		{
			MonoStatus.Ensure();
		}
		Log.Flush();
	}

	public static void OnActiveSceneChanged(Scene from, Scene to)
	{
		Log.Info($"OnActiveSceneChanged({from.name}, {to.name})", true);
		Log.Flush();
		if (Helpers.InStartupMenu)
		{
			LoadOrderUtil.TurnOffSteamPanels();
		}
	}

	public void OnSettingsUI(UIHelperBase helper)
	{
		Settings.Settings.OnSettingsUI(helper as UIHelper);
	}
}
