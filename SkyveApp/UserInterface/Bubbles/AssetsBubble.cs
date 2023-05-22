using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Compatibility;
using SkyveApp.Domain.Enums;
using SkyveApp.Utilities;
using SkyveApp.Utilities.Managers;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.StatusBubbles;

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

		int assetsIncluded = 0, assetsOutOfDate = 0, assetsIncomplete = 0;
		var assetSize = 0L;

		var crDic = new Dictionary<NotificationType, int>();

		foreach (var asset in CentralManager.Assets)
		{
			if (!asset.IsIncluded)
				continue;

			assetsIncluded++;
			assetSize += asset.FileSize;

			if (asset.IsMod)
				continue;

			switch (asset.Package.Status)
			{
				case DownloadStatus.OutOfDate:
					assetsOutOfDate++;
					break;
				case DownloadStatus.PartiallyDownloaded:
					assetsIncomplete++;
					break;
			}

			var notif = asset.GetCompatibilityInfo().Notification;

			if (crDic.ContainsKey(notif))
				crDic[notif]++;
			else
				crDic[notif] = 1;
		}

		DrawText(e, ref targetHeight, Locale.IncludedCount.FormatPlural(assetsIncluded, Locale.Asset.FormatPlural(assetsIncluded).ToLower()));
		DrawValue(e, ref targetHeight, assetSize.SizeString(), Locale.TotalSize);

		if (assetsOutOfDate > 0)
		{
			DrawText(e, ref targetHeight, Locale.OutOfDateCount.FormatPlural(assetsOutOfDate, Locale.Mod.FormatPlural(assetsOutOfDate).ToLower()), FormDesign.Design.YellowColor);
		}

		if (assetsIncomplete > 0)
		{
			DrawText(e, ref targetHeight, Locale.IncompleteCount.FormatPlural(assetsIncomplete, Locale.Mod.FormatPlural(assetsIncomplete).ToLower()), FormDesign.Design.RedColor);
		}

		foreach (var group in crDic.OrderBy(x => x.Key))
		{
			if (group.Key <= NotificationType.Info)
			{
				continue;
			}

			DrawText(e, ref targetHeight, LocaleCR.Get($"{group.Key}Count").FormatPlural(group.Value, Locale.Asset.FormatPlural(group.Value).ToLower()), group.Key.GetColor());
		}
	}
}
