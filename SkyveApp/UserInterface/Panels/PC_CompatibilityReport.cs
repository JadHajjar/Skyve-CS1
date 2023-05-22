using Extensions;

using SkyveApp.Domain.Compatibility;
using SkyveApp.Utilities;
using SkyveApp.Utilities.Managers;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Panels;
public partial class PC_CompatibilityReport : PanelContent
{
	private NotificationType CurrentKey;

	public PC_CompatibilityReport()
	{
		InitializeComponent();

		var hasPackages = CompatibilityManager.User.SteamId != 0 && CentralManager.Packages.Any(x => x.Author?.SteamId == CompatibilityManager.User.SteamId);
		B_ManageSingle.Visible = B_Manage.Visible = CompatibilityManager.User.Manager && !CompatibilityManager.User.Malicious;
		B_YourPackages.Visible = hasPackages && CompatibilityManager.User.Verified && !CompatibilityManager.User.Malicious;

		LC_Items.Visible = false;
		LC_Items.CanDrawItem += LC_Items_CanDrawItem;

		if (!CompatibilityManager.FirstLoadComplete)
		{
			PB_Loader.Visible = true;
			PB_Loader.Loading = true;
		}
		else
		{
			CompatibilityManager_ReportProcessed();
		}

		CompatibilityManager.ReportProcessed += CompatibilityManager_ReportProcessed;
	}

	protected override void LocaleChanged()
	{
		Text = Locale.CompatibilityReport;
	}


	protected override void UIChanged()
	{
		base.UIChanged();

		PB_Loader.Size = UI.Scale(new System.Drawing.Size(32, 32), UI.FontScale);
		PB_Loader.Location = ClientRectangle.Center(PB_Loader.Size);
	}

	private void CompatibilityManager_ReportProcessed()
	{
		if (CompatibilityManager.FirstLoadComplete)
		{
			var packages = CentralManager.Packages.ToList(x => x.GetCompatibilityInfo());
			this.TryInvoke(() => { LoadReport(packages); PB_Loader.Dispose(); });
		}
	}

	private void B_Manage_Click(object sender, EventArgs e)
	{
		Form.PushPanel(null, new PC_CompatibilityManagement());
	}

	private void B_YourPackages_Click(object sender, EventArgs e)
	{
		Form.PushPanel(null, new PC_CompatibilityManagement(CompatibilityManager.User.SteamId));
	}

	private void B_ManageSingle_Click(object sender, EventArgs e)
	{
		var form = new PC_SelectPackage() { Text = LocaleHelper.GetGlobalText("Select a package") };

		form.PackageSelected += Form_PackageSelected;

		Program.MainForm.PushPanel(null, form);

	}

	private void Form_PackageSelected(IEnumerable<ulong> packages)
	{
		Form.PushPanel(null, new PC_CompatibilityManagement(packages));
	}

	private void LoadReport(List<CompatibilityInfo> reports)
	{
		var notifs = reports.Select(x => x.Notification).Distinct().Where(x => x > NotificationType.Info).OrderByDescending(x => x).ToList();

		if (tabHeader.Tabs.Select(x => (NotificationType)x.Tag).SequenceEqual(notifs))
		{
			LC_Items.SetItems(reports);

			return;
		}

		var tabs = new List<SlickTab>();
		foreach (var report in notifs)
		{
			var tab = new SlickTab()
			{
				Tag = report,
				Text = LocaleCR.Get(report.ToString()),
				Tint = report.GetColor(),
				IconName = report.GetIcon(true)
			};

			tab.TabSelected += Tab_TabSelected;

			tabs.Add(tab);
		}

		tabHeader.Tabs = tabs.ToArray();
		LC_Items.SetItems(reports);
		LC_Items.Visible = true;
	}

	private void Tab_TabSelected(object sender, EventArgs e)
	{
		CurrentKey = (NotificationType)(sender as SlickTab)!.Tag;

		LC_Items.FilterChanged();
	}

	private void LC_Items_CanDrawItem(object sender, CanDrawItemEventArgs<CompatibilityInfo> e)
	{
		e.DoNotDraw = e.Item.Notification != CurrentKey;
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (keyData == (Keys.Control | Keys.Tab))
		{
			tabHeader.Next();
			return true;
		}

		if (keyData == (Keys.Control | Keys.Shift | Keys.Tab))
		{

			tabHeader.Previous();
			return true;
		}

		return base.ProcessCmdKey(ref msg, keyData);
	}
}
