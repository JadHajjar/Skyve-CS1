using Extensions;

using Newtonsoft.Json;

using SkyveApp.Domain.Compatibility;
using SkyveApp.Domain.Compatibility.Api;
using SkyveApp.Domain.Compatibility.Enums;
using SkyveApp.Services;
using SkyveApp.Utilities;
using SkyveApp.Utilities.Managers;

using SlickControls;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Panels;
public partial class PC_CompatibilityReport : PanelContent
{
	private ReviewRequest[]? reviewRequests;
	private NotificationType CurrentKey;
	private bool customReportLoaded;

	public PC_CompatibilityReport() : base(CompatibilityManager.User.Manager && !CompatibilityManager.User.Malicious)
	{
		InitializeComponent();

		SetManagementButtons();

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

	protected override async Task<bool> LoadDataAsync()
	{
		reviewRequests = await SkyveApiUtil.GetReviewRequests();

		return true;
	}

	protected override void OnDataLoad()
	{
		B_Requests.Text = LocaleCR.ReviewRequests.Format(reviewRequests is null ? string.Empty : $"({reviewRequests.Length})");
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
		if (CompatibilityManager.FirstLoadComplete && !customReportLoaded)
		{
			var packages = CentralManager.Packages.ToList(x => x.GetCompatibilityInfo());

			packages.RemoveAll(x => x.Notification < NotificationType.Unsubscribe && !x.Package.IsIncluded);

			this.TryInvoke(() => { LoadReport(packages); PB_Loader.Dispose(); });
		}

		this.TryInvoke(SetManagementButtons);
	}

	private void SetManagementButtons()
	{
		var hasPackages = CompatibilityManager.User.SteamId != 0 && CentralManager.Packages.Any(x => x.Author?.SteamId == CompatibilityManager.User.SteamId);
		B_Requests.Visible = B_ManageSingle.Visible = B_Manage.Visible = CompatibilityManager.User.Manager && !CompatibilityManager.User.Malicious;
		B_YourPackages.Visible = hasPackages && CompatibilityManager.User.Verified && !CompatibilityManager.User.Malicious;
		B_Requests.Text = LocaleCR.ReviewRequests.Format(reviewRequests is null ? string.Empty : $"({reviewRequests.Length})");
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

	internal void Import(string file)
	{
		if (Path.GetExtension(file).ToLower() is ".zip")
		{
			using var stream = File.OpenRead(file);
			using var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read, false);

			var entry = zipArchive.GetEntry("Skyve\\CompatibilityReport.json") ?? zipArchive.GetEntry("Skyve/CompatibilityReport.json");

			if (entry is null)
			{
				return;
			}

			file = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");

			entry.ExtractToFile(file);
		}

		try
		{
			var items = JsonConvert.DeserializeObject<List<CompatibilityInfo>>(File.ReadAllText(file));

			customReportLoaded = false;

			this.TryInvoke(() => LoadReport(items));

			customReportLoaded = true;
		}
		catch { }
	}

	private async void B_Requests_Click(object sender, EventArgs e)
	{
		if (reviewRequests == null)
		{
			B_Requests.Loading = true;
			reviewRequests = await SkyveApiUtil.GetReviewRequests();
		}

		B_Requests.Loading = false;
		if (reviewRequests != null)
		{
			Form.Invoke(() => Form.PushPanel(null, new PC_ReviewRequests(reviewRequests)));
		}
	}
}
