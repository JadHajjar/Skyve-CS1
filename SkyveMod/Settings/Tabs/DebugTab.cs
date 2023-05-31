using ColossalFramework;
using ColossalFramework.PlatformServices;
using ColossalFramework.UI;

using KianCommons;
using KianCommons.UI;

using System;

namespace SkyveMod.Settings.Tabs;
extern alias Injections;
static class DebugTab
{
	public static readonly SavedBool HotReload = new(nameof(HotReload), nameof(SkyveMod), false);

	public static void Make(ExtUITabstrip tabStrip)
	{
		var panelHelper = tabStrip.AddTabPage("Developer");
		//panelHelper.AddButton("Ensure All", CheckSubsUtil.EnsureAll);
		//g.AddButton("RequestItemDetails", OnRequestItemDetailsClicked);
		//g.AddButton("QueryItems", OnQueryItemsClicked);
		panelHelper.AddButton("RunCallbacks", OnRunCallbacksClicked);

		var bufferedToggle = panelHelper.AddCheckbox("Buffered Log", Log.Buffered, (val) => Log.Buffered = val) as UICheckBox;
		bufferedToggle.eventVisibilityChanged += new PropertyChangedEventHandler<bool>((_, ___) => bufferedToggle.isChecked = Log.Buffered);

		panelHelper.AddSavedToggle("Allow hot-reload and load of packages", HotReload, (val) => HotReload.value = val);
	}

	static void OnRunCallbacksClicked()
	{
		Log.Debug("RunCallbacks pressed");
		try
		{
			PlatformService.RunCallbacks();
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
		}
	}
	//static void OnQueryItemsClicked() {
	//    Log.Debug("QueryItems pressed");
	//    PlatformService.workshop.QueryItems().LogRet($"QueryItems()"); ;
	//}

	static void OnUGCQueryCompleted(UGCDetails result, bool ioError)
	{
		Log.Debug($"OnUGCQueryCompleted(result:{result.result} {result.publishedFileId}, ioError:{ioError})");
	}
}
