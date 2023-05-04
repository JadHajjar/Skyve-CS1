using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using static CompatibilityReport.CatalogData.Enums;

namespace LoadOrderToolTwo.UserInterface.StatusBubbles;

internal class AssetsBubble : StatusBubbleBase
{
	public AssetsBubble()
	{ }

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);

		if (!Live)
		{
			return;
		}

		ImageName = "I_Assets";
		Text = Locale.AssetsBubble;

		if (!CentralManager.IsContentLoaded)
		{
			Loading = true;

			CentralManager.ContentLoaded += Invalidate;
		}

		CentralManager.WorkshopInfoUpdated += CentralManager_WorkshopInfoUpdated;

		ProfileManager.ProfileChanged += ProfileManager_ProfileChanged;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);

		CentralManager.ContentLoaded -= Invalidate;
		CentralManager.WorkshopInfoUpdated -= CentralManager_WorkshopInfoUpdated;
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

		var assets = 0;
		var outOfDate = 0;
		var assetsSize = 0L;
		var groups = CentralManager.Packages.Where(x => !x.IsMod && x.Assets.Any(a => a.IsIncluded)).GroupBy(x => x.CompatibilityReport?.Severity);

		foreach (var item in CentralManager.Assets)
		{
			if (item.IsIncluded)
			{
				assets++;
				assetsSize += item.FileSize;

				if (item.Package.Status == Domain.Enums.DownloadStatus.OutOfDate)
				{
					outOfDate++;
				}
			}
		}

		DrawValue(e, ref targetHeight, assets.ToString(), assets == 1 ? Locale.AssetIncluded : Locale.AssetIncludedPlural);
		DrawValue(e, ref targetHeight, assetsSize.SizeString(), Locale.TotalSize);

		if (outOfDate > 0)
		{
			DrawValue(e, ref targetHeight, outOfDate.ToString(), outOfDate == 1 ? Locale.AssetOutOfDate : Locale.AssetOutOfDatePlural, FormDesign.Design.YellowColor);
		}

		foreach (var group in groups.OrderBy(x => x.Key))
		{
			if (!(group.Key > ReportSeverity.Remarks))
			{
				continue;
			}

			DrawValue(e, ref targetHeight, group.Count().ToString(), group.Key switch
			{
				ReportSeverity.MinorIssues => Locale.AssetsWithMinorIssues,
				ReportSeverity.MajorIssues => Locale.AssetsWithMajorIssues,
				ReportSeverity.Unsubscribe => Locale.AssetsShouldUnsub,
				_ => ""
			}, group.Key switch
			{
				ReportSeverity.MinorIssues => FormDesign.Design.YellowColor,
				ReportSeverity.MajorIssues => FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.RedColor),
				ReportSeverity.Unsubscribe => FormDesign.Design.RedColor,
				_ => Color.Empty
			});
		}
	}
}
