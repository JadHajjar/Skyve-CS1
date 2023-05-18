using Extensions;

using LoadOrderToolTwo.Domain.Compatibility;
using LoadOrderToolTwo.UserInterface.Lists;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Panels;
public partial class PC_CompatibilityReport : PanelContent
{
	public PC_CompatibilityReport()
	{
		InitializeComponent();

		Text = string.Empty;
		var hasPackages = CompatibilityManager.User.SteamId != 0 && CentralManager.Packages.Any(x => x.Author?.SteamId == CompatibilityManager.User.SteamId);
		B_ManageSingle.Visible = B_Manage.Visible = CompatibilityManager.User.Manager;
		B_YourPackages.Visible = hasPackages;
		TLP_Buttons.Visible = CompatibilityManager.User.Manager || hasPackages;

		LoadReport(CentralManager.Packages.Select(x => x.GetCompatibilityInfo()));
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

	private void LoadReport(IEnumerable<CompatibilityInfo> reports)
	{
		TLP_Reports.SuspendDrawing();
		TLP_Reports.Controls.Clear(true);
		TLP_Reports.RowStyles.Clear();

		foreach (var report in reports.GroupBy(x => x.Notification).OrderByDescending(x => x.Key))
		{
			if (report.Key <= NotificationType.Info)
				continue;

			TLP_Reports.RowStyles.Add(new());

			var tlp = new RoundedGroupTableLayoutPanel
			{
				Text = LocaleCR.Get(report.Key.ToString()),
				Dock = DockStyle.Top,
				AutoSize = true,
				UseFirstRowForPadding = true,
				AutoSizeMode = AutoSizeMode.GrowAndShrink,
				AddOutline = true,
				Margin = UI.Scale(new Padding(3, 10, 15, 0), UI.FontScale)
			};

			tlp.RowStyles.Add(new RowStyle(SizeType.Absolute));
			tlp.RowStyles.Add(new RowStyle());
			tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent));

			var button = new SlickButton
			{
				Text = "Do_All",
				Anchor = AnchorStyles.Top | AnchorStyles.Right
			};

			var list = new CompatibilityReportList();

			list.SetItems(report);

			tlp.Controls.Add(button, 0, 0);
			tlp.Controls.Add(list, 0, 1);

			TLP_Reports.Controls.Add(tlp, 0, TLP_Reports.RowStyles.Count - 1);

			button.AutoSize=true;
		}

		TLP_Reports.ResumeDrawing();
	}
}
