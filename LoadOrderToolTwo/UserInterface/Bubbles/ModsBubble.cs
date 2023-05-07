using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Enums;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.StatusBubbles;

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
		CentralManager.ModInformationUpdated += Invalidate;
		ProfileManager.ProfileChanged += ProfileManager_ProfileChanged;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);

		CentralManager.ContentLoaded -= Invalidate;
		CentralManager.WorkshopInfoUpdated -= CentralManager_WorkshopInfoUpdated;
		CentralManager.ModInformationUpdated -= Invalidate;
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
		var multipleModsIncluded = ModsUtil.GetDuplicateMods().Any();

		if (!CentralManager.SessionSettings.UserSettings.AdvancedIncludeEnable)
		{
			DrawValue(e, ref targetHeight, modsIncluded.ToString(), modsIncluded == 1 ? Locale.ModIncluded : Locale.ModIncludedPlural);
		}
		else if (modsIncluded == modsEnabled)
		{
			DrawValue(e, ref targetHeight, modsIncluded.ToString(), modsIncluded == 1 ? Locale.ModIncludedAndEnabled : Locale.ModIncludedAndEnabledPlural);
		}
		else
		{
			DrawValue(e, ref targetHeight, modsIncluded.ToString(), modsIncluded == 1 ? Locale.ModIncluded : Locale.ModIncludedPlural);
			DrawValue(e, ref targetHeight, modsEnabled.ToString(), modsEnabled == 1 ? Locale.ModEnabled : Locale.ModEnabledPlural);
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
			if ((int)group.Key % 0x10 == 0)
			{
				continue;
			}

			//DrawValue(e, ref targetHeight, group.Count().ToString(), group.Key switch
			//{
			//	ReportSeverity.MinorIssues => Locale.ModsWithMinorIssues,
			//	ReportSeverity.MajorIssues => Locale.ModsWithMajorIssues,
			//	ReportSeverity.Unsubscribe => Locale.ModsShouldUnsub,
			//	_ => ""
			//}, group.Key switch
			//{
			//	ReportSeverity.MinorIssues => FormDesign.Design.YellowColor,
			//	ReportSeverity.MajorIssues => FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.RedColor),
			//	ReportSeverity.Unsubscribe => FormDesign.Design.RedColor,
			//	_ => Color.Empty
			//});
		}

		if (multipleModsIncluded)
		{
			DrawText(e, ref targetHeight, Locale.MultipleModsIncluded, FormDesign.Design.RedColor);
		}
	}
}
