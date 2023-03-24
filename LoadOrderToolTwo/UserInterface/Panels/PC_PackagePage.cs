using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.Domain.Steam.Markdown;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Diagnostics;
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
		L_Title.Text = package.ToString().RemoveVersionText().Replace("&", "&&");
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

		T_Info.LinkedControl = c;

		T_CR.LinkedControl = new PackageCompatibilityReportControl(package);
	}

	public Package Package { get; }

	protected override void UIChanged()
	{
		base.UIChanged();

		PB_Icon.Width = TLP_Top.Height = (int)(128 * UI.FontScale);
		L_Title.Font = UI.Font(15F, FontStyle.Bold);
		L_Title.Margin = UI.Scale(new Padding(7), UI.FontScale);
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
		SteamUtil.ReDownload(Package.SteamId);
	}

	private void B_SteamPage_Click(object sender, EventArgs e)
	{
		try
		{ Process.Start(Package.SteamPage); }
		catch { }
	}

	private void B_Folder_Click(object sender, EventArgs e)
	{
		try
		{ Process.Start(Package.Folder); }
		catch { }
	}
}
