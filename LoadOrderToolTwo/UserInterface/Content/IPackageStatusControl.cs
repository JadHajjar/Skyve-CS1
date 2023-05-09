using Extensions;

using LoadOrderToolTwo.UserInterface.Dropdowns;
using LoadOrderToolTwo.UserInterface.Forms;
using LoadOrderToolTwo.Utilities;

using SlickControls;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Content;
public partial class IPackageStatusControl<T> : SlickControl where T : struct, Enum
{
private PackageStatusTypeDropDown<T> typeDropDown;

	public IPackageStatusControl()
	{
		InitializeComponent();

		label1.Text = LocaleCR.LinkedPackages;

		typeDropDown = new()
		{
			Text = typeof(T).Name,
		};

		P_Main.Controls.Add(typeDropDown, 0, 0);

		AutoInvalidate = false;
		AutoSize = true;
	}

	protected override void DesignChanged(FormDesign design)
	{
		P_Main.BackColor = design.BackColor;
	}

	protected override void UIChanged()
	{
		MinimumSize = UI.Scale(new Size(250, 0), UI.UIScale);
		P_Main.Padding =slickSpacer1.Margin= UI.Scale(new Padding(5), UI.FontScale);
		CloseIcon.Size = UI.Scale(new Size(16, 16), UI.FontScale);
		TB_Note.MinimumSize = new Size(0, (int)(64 * UI.FontScale));
		I_AddPackage.Size = I_Note.Size = UI.Scale(new Size(24, 24), UI.FontScale);
		I_AddPackage.Padding=I_Note.Padding = UI.Scale(new Padding(5), UI.FontScale);
	}

	private void I_Note_Click(object sender, EventArgs e)
	{
		TB_Note.Visible = I_Note.Selected = !I_Note.Selected;
	}

	private void I_AddPackage_Click(object sender, EventArgs e)
	{
		var form = new AddPackageForm();

		form.PackageSelected += Form_PackageSelected;

		form.Show(FindForm());
	}

	private void Form_PackageSelected(Domain.Steam.SteamWorkshopItem obj)
	{
		P_Packages.Controls.Add(new MiniPackageControl(obj));
	}

	private void CloseIcon_Click(object sender, EventArgs e)
	{
		Dispose();
	}
}
