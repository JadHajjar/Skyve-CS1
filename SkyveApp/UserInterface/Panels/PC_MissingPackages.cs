using Extensions;

using SkyveApp.Domain.Interfaces;
using SkyveApp.Services;
using SkyveApp.Services.Interfaces;
using SkyveApp.Utilities;

using SlickControls;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Panels;
internal partial class PC_MissingPackages : PC_GenericPackageList
{
	private bool allowExit;

	public PC_MissingPackages(List<Playset.Mod> missingMods, List<Playset.Asset> missingAssets) : base(missingAssets.Concat(missingMods))
	{
		ServiceCenter.Get<INotifier>().ContentLoaded += CentralManager_ContentLoaded;
	}

	protected override void Dispose(bool disposing)
	{
		ServiceCenter.Get<INotifier>().ContentLoaded -= CentralManager_ContentLoaded;

		base.Dispose(disposing);
	}

	protected override void LocaleChanged()
	{
		Text = string.Format(Locale.MissingPackagesProfile, ServiceCenter.Get<IPlaysetManager>().CurrentPlayset.Name);
	}

	private void CentralManager_ContentLoaded()
	{
		var items = new List<IPackage>(LC_Items.Items);

		foreach (var item in items)
		{
			if (item is Playset.Mod mod)
			{
				var localMod = ServiceCenter.Get<IPlaysetManager>().GetMod(mod);

				if (localMod is null)
				{
					continue;
				}

				localMod.IsIncluded = true;
				localMod.IsEnabled = mod.Enabled;

				LC_Items.Remove(item);
			}
			else if (item is Playset.Asset asset)
			{
				var localAsset = ServiceCenter.Get<IPlaysetManager>().GetAsset(asset);

				if (localAsset is null)
				{
					continue;
				}

				localAsset.IsIncluded = true;

				LC_Items.Remove(item);
			}
		}

		if (LC_Items.ItemCount == 0)
		{
			this.TryInvoke(PushBack);
		}
	}

	public override bool CanExit(bool toBeDisposed)
	{
		if (toBeDisposed && !allowExit && LC_Items.ItemCount > 0)
		{
			if (ShowPrompt(Locale.MissingItemsRemain, PromptButtons.OKCancel, PromptIcons.Hand) == DialogResult.OK)
			{
				allowExit = true;
				Form.PushBack();
			}

			return false;
		}

		return true;
	}

	internal static void PromptMissingPackages(BasePanelForm form, List<Playset.Mod> missingMods, List<Playset.Asset> missingAssets)
	{
		var pauseEvent = new AutoResetEvent(false);

		form.TryInvoke(() =>
		{
			var panel = new PC_MissingPackages(missingMods, missingAssets);

			form.PushPanel(null, panel);

			panel.Disposed += (s, e) =>
			{
				pauseEvent.Set();
			};
		});

		pauseEvent.WaitOne();
	}
}
