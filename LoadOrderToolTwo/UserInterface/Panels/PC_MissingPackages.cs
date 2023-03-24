using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Utilities;

using SlickControls;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Panels;
public partial class PC_MissingPackages : PanelContent
{
	private Dictionary<ulong, PackageViewControl> _workshopPackages = new();

	public PC_MissingPackages(List<Profile.Mod> missingMods, List<Profile.Asset> missingAssets)
	{
		InitializeComponent();

		MissingMods = missingMods;
		MissingAssets = missingAssets;

		foreach (var package in MissingAssets.Concat(MissingMods).GroupBy(x => x.SteamId))
		{
			if (package.Key != 0)
			{
				AddItem(_workshopPackages[package.Key] = new(package.Last()));
			}
		}

		foreach (var mod in MissingMods.Where(x => x.SteamId == 0))
		{
			AddItem(new(mod));
		}

		foreach (var asset in MissingAssets.Where(x => x.SteamId == 0))
		{
			AddItem(new(asset));
		}

		StartDataLoad();
	}

	private void AddItem(PackageViewControl control)
	{
		if (TLP_Contents.Controls.Count % 2 == 0)
		{
			TLP_Contents.RowCount++;
			TLP_Contents.RowStyles.Add(new());
		}

		TLP_Contents.Controls.Add(control, TLP_Contents.Controls.Count % 2, TLP_Contents.RowCount - 1);
	}

	protected override bool LoadData()
	{
		var steamIds = MissingAssets.Select(x => x.SteamId).Concat(MissingMods.Select(x => x.SteamId)).Distinct().ToArray();

		var info = SteamUtil.GetWorkshopInfoAsync(steamIds).Result;

		foreach (var item in info)
		{
			if (_workshopPackages.ContainsKey(item.Key))
				_workshopPackages[item.Key].SetWorkshopItem(item.Value);
		}

		return base.LoadData();
	}

	public List<Profile.Mod> MissingMods { get; }
	public List<Profile.Asset> MissingAssets { get; }

	internal static void PromptMissingPackages(BasePanelForm form, List<Profile.Mod> missingMods, List<Profile.Asset> missingAssets)
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
