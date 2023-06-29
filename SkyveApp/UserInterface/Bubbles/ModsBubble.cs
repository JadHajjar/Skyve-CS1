using Extensions;

using SkyveApp.Domain.Compatibility.Enums;
using SkyveApp.Domain.Enums;
using SkyveApp.Services;
using SkyveApp.Services.Interfaces;
using SkyveApp.Systems.Compatibility;
using SkyveApp.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.StatusBubbles;

internal class ModsBubble : StatusBubbleBase
{
	private readonly ISettings _settings;
	private readonly INotifier _notifier;
	private readonly IContentManager _contentManager;
	private readonly IPlaysetManager _profileManager;

	public ModsBubble()
	{
		_settings = ServiceCenter.Get<ISettings>();
		_notifier = ServiceCenter.Get<INotifier>();
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

		ImageName = "I_Mods";
		Text = Locale.ModsBubble;

		if (!_notifier.IsContentLoaded)
		{
			Loading = true;

			_notifier.ContentLoaded += Invalidate;
		}

		_notifier.WorkshopInfoUpdated += CentralManager_WorkshopInfoUpdated;
		_notifier.PackageInformationUpdated += Invalidate;
		_profileManager.ProfileChanged += ProfileManager_ProfileChanged;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);

		_notifier.ContentLoaded -= Invalidate;
		_notifier.WorkshopInfoUpdated -= CentralManager_WorkshopInfoUpdated;
		_notifier.PackageInformationUpdated -= Invalidate;
		_profileManager.ProfileChanged -= ProfileManager_ProfileChanged;
	}

	private void ProfileManager_ProfileChanged(Playset obj)
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

		int modsIncluded = 0, modsEnabled = 0, modsOutOfDate = 0, modsIncomplete = 0;

		var crDic = new Dictionary<NotificationType, int>();

		foreach (var mod in _contentManager.Mods)
		{
			if (!mod.IsIncluded)
			{
				continue;
			}

			modsIncluded++;

			if (mod.IsEnabled)
			{
				modsEnabled++;
			}

			if (Loading)
			{
				continue;
			}

			switch (mod.Package.Status)
			{
				case DownloadStatus.OutOfDate:
					modsOutOfDate++;
					break;
				case DownloadStatus.PartiallyDownloaded:
					modsIncomplete++;
					break;
			}

			var notif = mod.GetCompatibilityInfo().Notification;

			if (crDic.ContainsKey(notif))
			{
				crDic[notif]++;
			}
			else
			{
				crDic[notif] = 1;
			}
		}

		if (!_settings.SessionSettings.UserSettings.AdvancedIncludeEnable)
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
			DrawText(e, ref targetHeight, Locale.OutOfDateCount.FormatPlural(modsOutOfDate, Locale.Mod.FormatPlural(modsOutOfDate).ToLower()), FormDesign.Design.YellowColor);
		}

		if (modsIncomplete > 0)
		{
			DrawText(e, ref targetHeight, Locale.IncompleteCount.FormatPlural(modsIncomplete, Locale.Mod.FormatPlural(modsIncomplete).ToLower()), FormDesign.Design.RedColor);
		}

		foreach (var group in crDic.OrderBy(x => x.Key))
		{
			if (group.Key <= NotificationType.Info)
			{
				continue;
			}

			DrawText(e, ref targetHeight, LocaleCR.Get($"{group.Key}Count").FormatPlural(group.Value, Locale.Mod.FormatPlural(group.Value).ToLower()), group.Key.GetColor());
		}
	}
}
