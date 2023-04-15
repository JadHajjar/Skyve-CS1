using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.UserInterface.CompatibilityReport;
using LoadOrderToolTwo.UserInterface.Lists;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.IO;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Drawing;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Panels;
public partial class PC_PackagePage : PanelContent
{
	public PC_PackagePage(Package package)
	{
		InitializeComponent();

		Package = package;

		Text = Locale.Back;
		T_Info.Text = Locale.ContentAndInfo;
		T_CR.Text = Locale.CompatibilityReport;
		T_Profiles.Text = Locale.OtherProfiles;
		L_Title.Text = package.ToString().RemoveVersionText(out _);
		PB_Icon.Package = package;
		PB_Icon.LoadImage(package.IconUrl, ImageManager.GetImage);

		P_Info.SetPackage(package);

		var c = new ItemListControl<IPackage>
		{
			Dock = DockStyle.Fill
		};

		c.AddRange(Package.Assets!);

		if (Package.Mod != null)
		{
			c.Add(Package.Mod);
		}

		T_Info.FillTab = true;
		T_Info.LinkedControl = c;

		//if (!string.IsNullOrWhiteSpace(package.SteamDescription))
		//{
		//	var c = new SteamDescriptionViewer(package.SteamDescription!);

		//	T_Info.LinkedControl = c;
		//}

		T_CR.LinkedControl = new PackageCompatibilityReportControl(package);

		var pc = new OtherProfilePackage(package)
		{
			Dock = DockStyle.Fill
		};

		T_Profiles.FillTab = true;
		T_Profiles.LinkedControl = pc;
	}

	public Package Package { get; }

	protected override void UIChanged()
	{
		base.UIChanged();

		PB_Icon.Width = TLP_Top.Height = (int)(128 * UI.FontScale);
		L_Title.Font = UI.Font(15F, FontStyle.Bold);
		L_Title.Margin = UI.Scale(new Padding(7), UI.FontScale);
		T_CR.Icon = ImageManager.GetIcon(nameof(Properties.Resources.I_CompatibilityReport));
		T_Info.Icon = ImageManager.GetIcon(nameof(Properties.Resources.I_Content));
		T_Profiles.Icon = ImageManager.GetIcon(nameof(Properties.Resources.I_ProfileSettings));
	}

	protected override void DesignChanged(FormDesign design)
	{
		base.DesignChanged(design);

		BackColor = design.AccentBackColor;
		P_Content.BackColor = P_Back.BackColor = design.BackColor;
	}

	public override Color GetTopBarColor()
	{
		return FormDesign.Design.AccentBackColor;
	}

	private void B_Redownload_Click(object sender, EventArgs e)
	{
		SteamUtil.ReDownload(Package);
	}

	private void B_SteamPage_Click(object sender, EventArgs e)
	{
		PlatformUtil.OpenUrl(Package.SteamPage);
	}

	private void B_Folder_Click(object sender, EventArgs e)
	{
		PlatformUtil.OpenFolder(Package.Folder);
	}
}
