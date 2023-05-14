using Extensions;
using System.Linq;
using System;

using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;
using LoadOrderToolTwo.Utilities;
using System.Threading.Tasks;
using LoadOrderToolTwo.UserInterface.Forms;
using LoadOrderToolTwo.Domain.Interfaces;
using System.Collections.Generic;

namespace LoadOrderToolTwo.UserInterface.Panels;
public partial class PC_CompatibilityReport : PanelContent
{
	private ulong userId;
	private bool hasPackages;
	private bool isManager;

	public PC_CompatibilityReport() : base(true)
	{
		InitializeComponent();

		Text = string.Empty;

		PB_Loading.BringToFront();
	}

	protected override async Task<bool> LoadDataAsync()
	{
		userId = SteamUtil.GetLoggedInSteamId();
		hasPackages = userId != 0 && CentralManager.Packages.Any(x => x.Author?.SteamId == userId.ToString());
		isManager = await CompatibilityApiUtil.IsCommunityManager(userId);

		return true;
	}

	protected override void OnLoadFail() => OnDataLoad();

	protected override void OnDataLoad()
	{
		B_ManageSingle.Visible = B_Manage.Visible = isManager;
		B_YourPackages.Visible = hasPackages;
		TLP_Buttons.Visible = isManager || hasPackages;

		PB_Loading.Dispose();
	}

	protected override void OnCreateControl()
	{
		base.OnCreateControl();

		PB_Loading.Loading = true;
	}

	private void B_Manage_Click(object sender, EventArgs e)
	{
		if (isManager)
		{
			Form.PushPanel(null, new PC_CompatibilityManagement());
		}
	}

	private void B_YourPackages_Click(object sender, EventArgs e)
	{
		if (hasPackages)
		{
			Form.PushPanel(null, new PC_CompatibilityManagement(userId));
		}
	}

	private void B_ManageSingle_Click(object sender, EventArgs e)
	{
		if (isManager)
		{
			var form = new PC_SelectPackage() { Text = LocaleHelper.GetGlobalText("Select a package") };

			form.PackageSelected += Form_PackageSelected;

			Program.MainForm.PushPanel(null, form);
		}

	}

	private void Form_PackageSelected(IEnumerable<IPackage> packages)
	{
		Form.PushPanel(null, new PC_CompatibilityManagement(packages));
	}
}
