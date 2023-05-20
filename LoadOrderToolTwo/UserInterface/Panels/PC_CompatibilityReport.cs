using Extensions;

using LoadOrderToolTwo.Domain.Compatibility;
using LoadOrderToolTwo.UserInterface.CompatibilityReport;
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
		B_ManageSingle.Visible = B_Manage.Visible = CompatibilityManager.User.Manager && !CompatibilityManager.User.Malicious;
		B_YourPackages.Visible = hasPackages && CompatibilityManager.User.Verified && !CompatibilityManager.User.Malicious;

		if (CompatibilityManager.FirstLoadComplete)
		{
			slickPictureBox1.Visible = true;
			slickPictureBox1.Loading = true;
		}

		CompatibilityManager_ReportProcessed();

		CompatibilityManager.ReportProcessed += CompatibilityManager_ReportProcessed;
	}

	private void CompatibilityManager_ReportProcessed()
	{
		this.TryInvoke(() => { slickPictureBox1.Dispose(); LoadReport(CentralManager.Packages.Select(x => x.GetCompatibilityInfo())); });
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
		FLP_Reports.SuspendDrawing();
		FLP_Reports.Controls.Clear(true);

		reports = reports.ToList();

		foreach (var report in reports.GroupBy(x => x.Notification).OrderByDescending(x => x.Key))
		{
			if (report.Key <= NotificationType.Info)
				continue;

			var validReports = report.Key == NotificationType.Unsubscribe ? report
				: report.Where(x => x.Package.IsIncluded);

			FLP_Reports.Controls.Add(new CompatibilityGroupBubble(report.Key, report));
		}

		FLP_Reports.ResumeDrawing();
	}
}
