using ColossalFramework.UI;

using SkyveShared;

namespace SkyveMod.Settings.Tabs;
static class LoggingTab
{
	static SkyveConfig Config => ConfigUtil.Config;

	static UIComponent logAssetLoadingTimesToggle_;

	public static void Make(ExtUITabstrip tabStrip)
	{
		var panelHelper = tabStrip.AddTabPage("Logging");
		panelHelper.AddCheckbox(
			"Log asset loading times",
			ConfigUtil.Config.LogAssetLoadingTimes,
			val =>
			{
				ConfigUtil.Config.LogAssetLoadingTimes = val;
				ConfigUtil.SaveConfig();
				logAssetLoadingTimesToggle_.isEnabled = val;
			});

		logAssetLoadingTimesToggle_ = panelHelper.AddCheckbox(
			"Per Mod",
			ConfigUtil.Config.LogPerModAssetLoadingTimes,
			val =>
			{
				ConfigUtil.Config.LogPerModAssetLoadingTimes = val;
				ConfigUtil.SaveConfig();
			}) as UIComponent;
		Settings.Indent(logAssetLoadingTimesToggle_);

		logAssetLoadingTimesToggle_ = panelHelper.AddCheckbox(
			"Log per mod OnCreated() times",
			ConfigUtil.Config.LogPerModOnCreatedTimes,
			val =>
			{
				ConfigUtil.Config.LogPerModOnCreatedTimes = val;
				ConfigUtil.SaveConfig();
			}) as UIComponent;
	}
}
