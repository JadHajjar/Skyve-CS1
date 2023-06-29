using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Compatibility.Enums;
using SkyveApp.Domain.Enums;
using SkyveApp.Domain.Systems;
using SkyveApp.Services;
using SkyveApp.Services.Interfaces;
using SkyveApp.Systems;
using SkyveApp.Systems.Compatibility;
using SkyveApp.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.StatusBubbles;

internal class AssetsBubble : StatusBubbleBase
{
	private readonly INotifier _notifier;
	private readonly IContentUtil _contentUtil;
	private readonly IContentManager _contentManager;
	private readonly IPlaysetManager _profileManager;

	public AssetsBubble()
	{
		_notifier = ServiceCenter.Get<INotifier>();
		_contentUtil = ServiceCenter.Get<IContentUtil>();
		_contentManager = ServiceCenter.Get<IContentManager>();
		_profileManager = ServiceCenter.Get<IPlaysetManager>();
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);

		if (!Live)
		{
			return;
		}

		ImageName = "I_Assets";
		Text = Locale.AssetsBubble;

		if (!_notifier.IsContentLoaded)
		{
			Loading = true;

			_notifier.ContentLoaded += Invalidate;
		}

		_notifier.WorkshopInfoUpdated += CentralManager_WorkshopInfoUpdated;
		_notifier.PackageInformationUpdated += Invalidate;
		_notifier.PlaysetChanged += ProfileManager_ProfileChanged;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);

		_notifier.ContentLoaded -= Invalidate;
		_notifier.WorkshopInfoUpdated -= CentralManager_WorkshopInfoUpdated;
		_notifier.PackageInformationUpdated -= Invalidate;
		_notifier.PlaysetChanged -= ProfileManager_ProfileChanged;
	}

	private void ProfileManager_ProfileChanged()
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
		if (!_notifier.IsContentLoaded)
		{
			DrawText(e, ref targetHeight, Locale.Loading, FormDesign.Design.InfoColor);
			return;
		}

		int assetsIncluded = 0, assetsOutOfDate = 0, assetsIncomplete = 0;
		var assetSize = 0L;

		var crDic = new Dictionary<NotificationType, int>();

		foreach (var asset in _contentManager.Assets)
		{
			if (!_contentUtil.IsIncluded(asset))
			{
				continue;
			}

			assetsIncluded++;
			assetSize += asset.LocalSize;

			if (asset.IsMod || Loading)
			{
				continue;
			}

			switch (_contentUtil.GetStatus(asset, out _))
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
			{
				crDic[notif]++;
			}
			else
			{
				crDic[notif] = 1;
			}
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
