using ColossalFramework.UI;

using KianCommons;
using KianCommons.UI;

using SkyveMod.Settings.Tabs;

using SkyveShared;

using System;

using UnityEngine;

namespace SkyveMod.Settings;
extern alias Injections;
public static class Settings
{
	static SkyveConfig Config => ConfigUtil.Config;

	public static void OnSettingsUI(UIHelper helper)
	{
		try
		{
			if (!Helpers.InStartupMenu)
			{
				helper.AddLabel("Only available in startup menu");
			}

			var tabStrip = ExtUITabstrip.Create(helper);
			//SubscriptionsTab.Make(tabStrip);
			StartupTab.Make(tabStrip);
			LoggingTab.Make(tabStrip);
			DebugTab.Make(tabStrip);
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
		}
	}

	public static void Indent(UIComponent c, int n = 1)
	{

		if (c.Find<UILabel>("Label") is UILabel label)
		{
			label.padding = new RectOffset(22 * n, 0, 0, 0);
		}

		if (c.Find<UISprite>("Unchecked") is UISprite check)
		{
			check.relativePosition += new Vector3(22 * n, 0);
		}
	}

	public static void Pairup(UICheckBox checkbox, UIButton button)
	{
		Assertion.NotNull(checkbox, "checkbox");
		Assertion.NotNull(button, "button");
		var container = checkbox.parent;
		Assertion.NotNull(container, "container");
		var panel = container.AddUIComponent<UIPanel>();
		panel.width = container.width;
		panel.height = button.height;

		checkbox.AlignTo(panel, UIAlignAnchor.TopLeft);
		checkbox.relativePosition += new Vector3(0, 10);
		button.AlignTo(panel, UIAlignAnchor.TopRight);
		button.relativePosition -= new Vector3(button.width, 0);
	}
}
