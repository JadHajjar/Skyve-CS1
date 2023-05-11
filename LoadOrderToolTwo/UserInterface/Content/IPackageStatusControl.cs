using Extensions;

using LoadOrderToolTwo.Domain.Compatibility;
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
public partial class IPackageStatusControl<T, TBase> : SlickControl where T : struct, Enum where TBase : IPackageStatus<T>, new()
{
	private PackageStatusTypeDropDown<T> typeDropDown;

	public IPackageStatusControl(TBase? item = default)
	{
		InitializeComponent();

		label1.Text = LocaleCR.LinkedPackages;

		typeDropDown = new()
		{
			Dock = DockStyle.Top,
			Text = typeof(T).Name,
		};

		P_Main.Controls.Add(typeDropDown, 0, 0);

		AutoInvalidate = false;
		AutoSize = true;

		if (item is not null)
		{
		if (!IsHandleCreated)
			CreateHandle();

			typeDropDown.SelectedItem = item.Type;
			DD_Action.SelectedItem = item.Action;
			TB_Note.Text = item.Note;
			TB_Note.Visible = I_Note.Selected = !string.IsNullOrWhiteSpace(item.Note);
			typeDropDown.SelectedItem = item.Type;

			foreach (var package in item.Packages ?? new ulong[0])
			{
				P_Packages.Controls.Add(new MiniPackageControl(package));
			}
		}
	}

	public TBase PackageStatus => new TBase()
	{
		Type = typeDropDown.SelectedItem,
		Action = DD_Action.SelectedItem,
		Note = TB_Note.Text,
		Packages = P_Packages.Controls.OfType<MiniPackageControl>().Select(x => x.Package.SteamId).ToArray(),
	};

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
