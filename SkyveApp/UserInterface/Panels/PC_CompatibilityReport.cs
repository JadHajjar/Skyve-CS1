using Newtonsoft.Json;

using SkyveApp.Systems.Compatibility.Domain;
using SkyveApp.Systems.Compatibility.Domain.Api;
using SkyveApp.Systems.CS1.Utilities;

using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Panels;
public partial class PC_CompatibilityReport : PanelContent
{
	private ReviewRequest[]? reviewRequests;
	private NotificationType CurrentKey;
	private bool customReportLoaded;

	private readonly ICompatibilityManager _compatibilityManager = ServiceCenter.Get<ICompatibilityManager>();
	private readonly IPackageManager _contentManager = ServiceCenter.Get<IPackageManager>();
	private readonly INotifier _notifier = ServiceCenter.Get<INotifier>();
	private readonly IUserService _userService = ServiceCenter.Get<IUserService>();
	private readonly SkyveApiUtil _skyveApiUtil = ServiceCenter.Get<SkyveApiUtil>();

	public PC_CompatibilityReport() : base(ServiceCenter.Get<IUserService>().User.Manager && !ServiceCenter.Get<IUserService>().User.Malicious)
	{
		InitializeComponent();

		SetManagementButtons();

		LC_Items.Visible = false;
		LC_Items.CanDrawItem += LC_Items_CanDrawItem;

		if (!_compatibilityManager.FirstLoadComplete)
		{
			PB_Loader.Visible = true;
			PB_Loader.Loading = true;
		}
		else
		{
			CompatibilityManager_ReportProcessed();
		}

		_notifier.CompatibilityReportProcessed += CompatibilityManager_ReportProcessed;
	}

	protected override async Task<bool> LoadDataAsync()
	{
		reviewRequests = await _skyveApiUtil.GetReviewRequests();

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
		if (_compatibilityManager.FirstLoadComplete && !customReportLoaded)
		{
			var packages = _contentManager.Packages.ToList(x => x.GetCompatibilityInfo());

			packages.RemoveAll(x => x.GetNotification() < NotificationType.Unsubscribe && !(x.Package?.IsIncluded() == true));

			this.TryInvoke(() => { LoadReport(packages); PB_Loader.Dispose(); });
		}

		this.TryInvoke(SetManagementButtons);
	}

	private void SetManagementButtons()
	{
		var hasPackages = _userService.User.Id is not null && _contentManager.Packages.Any(x => x.GetWorkshopInfo()?.Author?.Id == _userService.User.Id);
		B_Requests.Visible = B_ManageSingle.Visible = B_Manage.Visible = _userService.User.Manager && !_userService.User.Malicious;
		B_YourPackages.Visible = hasPackages && _userService.User.Verified && !_userService.User.Malicious;
		B_Requests.Text = LocaleCR.ReviewRequests.Format(reviewRequests is null ? string.Empty : $"({reviewRequests.Length})");
	}

	private void B_Manage_Click(object sender, EventArgs e)
	{
		Form.PushPanel(null, new PC_CompatibilityManagement());
	}

	private void B_YourPackages_Click(object sender, EventArgs e)
	{
		Form.PushPanel(null, new PC_CompatibilityManagement(_userService.User));
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

	private void LoadReport(List<ICompatibilityInfo> reports)
	{
		var notifs = reports.Select(x => x.GetNotification()).Distinct().Where(x => x > NotificationType.Info).OrderByDescending(x => x).ToList();

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

	private void LC_Items_CanDrawItem(object sender, CanDrawItemEventArgs<ICompatibilityInfo> e)
	{
		e.DoNotDraw = e.Item.GetNotification() != CurrentKey;
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

			this.TryInvoke(() => LoadReport(items.ToList(x => (ICompatibilityInfo)x)));

			customReportLoaded = true;
		}
		catch { }
	}

	private async void B_Requests_Click(object sender, EventArgs e)
	{
		if (reviewRequests == null)
		{
			B_Requests.Loading = true;
			reviewRequests = await _skyveApiUtil.GetReviewRequests();
		}

		B_Requests.Loading = false;
		if (reviewRequests != null)
		{
			Form.Invoke(() => Form.PushPanel(null, new PC_ReviewRequests(reviewRequests)));
		}
	}
}
