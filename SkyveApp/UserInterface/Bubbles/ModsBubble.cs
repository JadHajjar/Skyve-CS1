using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Compatibility;
using SkyveApp.Domain.Enums;
using SkyveApp.Utilities;
using SkyveApp.Utilities.Managers;

using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.StatusBubbles;

internal class ModsBubble : StatusBubbleBase
{
	public ModsBubble()
	{ }

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);

		if (!Live)
		{
			return;
		}

		ImageName = "I_Mods";
		Text = Locale.ModsBubble;

		if (!CentralManager.IsContentLoaded)
		{
			Loading = true;

			CentralManager.ContentLoaded += Invalidate;
		}

		CentralManager.WorkshopInfoUpdated += CentralManager_WorkshopInfoUpdated;
		CentralManager.PackageInformationUpdated += Invalidate;
		ProfileManager.ProfileChanged += ProfileManager_ProfileChanged;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);

		CentralManager.ContentLoaded -= Invalidate;
		CentralManager.WorkshopInfoUpdated -= CentralManager_WorkshopInfoUpdated;
		CentralManager.PackageInformationUpdated -= Invalidate;
		ProfileManager.ProfileChanged -= ProfileManager_ProfileChanged;
	}

	private void ProfileManager_ProfileChanged(Profile obj)
	{
		Invalidate();
	}

	private void CentralManager_WorkshopInfoUpdated()
	{
		if (Loading)
		{
			Loading = false;
		}
		else
		{
			Invalidate();
		}
	}

	protected override void CustomDraw(PaintEventArgs e, ref int targetHeight)
	{
		if (!CentralManager.IsContentLoaded)
		{
			DrawText(e, ref targetHeight, Locale.Loading, FormDesign.Design.InfoColor);
			return;
		}

		var modsIncluded = CentralManager.Mods.Count(x => x.IsIncluded);
		var modsEnabled = CentralManager.Mods.Count(x => x.IsEnabled && x.IsIncluded);
		var modsOutOfDate = CentralManager.Mods.Count(x => x.IsIncluded && x.Package.Status == DownloadStatus.OutOfDate);
		var modsIncomplete = CentralManager.Mods.Count(x => x.IsIncluded && x.Package.Status == DownloadStatus.PartiallyDownloaded);

		if (!CentralManager.SessionSettings.UserSettings.AdvancedIncludeEnable)
		{
			DrawText(e, ref targetHeight, Locale.IncludedCount.FormatPlural(modsIncluded, Locale.Mod.FormatPlural(modsIncluded).ToLower()));
		}
		else if (modsIncluded == modsEnabled)
		{
			DrawText(e, ref targetHeight, Locale.IncludedEnabledCount.FormatPlural(modsIncluded, Locale.Mod.FormatPlural(modsIncluded).ToLower()));
		}
		else
		{
			DrawText(e, ref targetHeight, Locale.IncludedCount.FormatPlural(modsIncluded, Locale.Mod.FormatPlural(modsIncluded).ToLower()));
			DrawText(e, ref targetHeight, Locale.EnabledCount.FormatPlural(modsEnabled, Locale.Mod.FormatPlural(modsEnabled).ToLower()));
		}

		if (modsOutOfDate > 0)
		{
			DrawValue(e, ref targetHeight, modsOutOfDate.ToString(), modsOutOfDate == 1 ? Locale.ModOutOfDate : Locale.ModOutOfDatePlural, FormDesign.Design.YellowColor);
		}

		if (modsIncomplete > 0)
		{
			DrawValue(e, ref targetHeight, modsIncomplete.ToString(), modsIncomplete == 1 ? Locale.ModIncomplete : Locale.ModIncompletePlural, FormDesign.Design.YellowColor);
		}

		var groups = CentralManager.Mods.Where(x => x.IsIncluded).GroupBy(x => x.Package.GetCompatibilityInfo().Notification);

		foreach (var group in groups.OrderBy(x => x.Key))
		{
			if (group.Key <= NotificationType.Info)
			{
				continue;
			}

			DrawText(e, ref targetHeight, LocaleCR.Get($"{group.Key}Count").Format(group.Count(), Locale.Mod.FormatPlural(group.Count()).ToLower()), group.Key.GetColor());
		}
	}
}
