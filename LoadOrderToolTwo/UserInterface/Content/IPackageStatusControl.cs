using Extensions;

using LoadOrderToolTwo.Domain.Compatibility;
using LoadOrderToolTwo.UserInterface.Dropdowns;
using LoadOrderToolTwo.UserInterface.Panels;
using LoadOrderToolTwo.Utilities;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Content;
public partial class IPackageStatusControl<T, TBase> : SlickControl where T : struct, Enum where TBase : IPackageStatus<T>, new()
{
	private readonly PackageStatusTypeDropDown<T> typeDropDown;

	public IPackageStatusControl(TBase? item = default)
	{
		InitializeComponent();

		L_LinkedPackages.Text = LocaleCR.LinkedPackages;
		L_OutputTitle.Text = LocaleCR.OutputText;

		typeDropDown = new()
		{
			Dock = DockStyle.Top,
			Text = typeof(T).Name,
		};

		P_Main.Controls.Add(typeDropDown, 0, 0);
		P_Main.SetColumnSpan(typeDropDown, 3);

		AutoInvalidate = false;
		AutoSize = true;

		typeDropDown.SelectedItemChanged += TypeDropDown_SelectedItemChanged;

		if (item is not null)
		{
			if (!IsHandleCreated)
			{
				CreateHandle();
			}

			typeDropDown.SelectedItem = item.Type;
			DD_Action.SelectedItem = item.Action;
			TB_Note.Text = item.Note;
			TB_Note.Visible = I_Note.Selected = !string.IsNullOrWhiteSpace(item.Note);

			foreach (var package in item.Packages ?? new ulong[0])
			{
				P_Packages.Controls.Add(new MiniPackageControl(package) { Dock = DockStyle.Top });
			}
		}
		else
		{
			TypeDropDown_SelectedItemChanged(this, EventArgs.Empty);
		}

		SlickTip.SetTo(I_Copy, "Copy");
		SlickTip.SetTo(I_Paste, "Paste");
		SlickTip.SetTo(I_AddPackage, "Add Package");
		SlickTip.SetTo(I_Note, "Show/Hide Note");
	}

	private void TypeDropDown_SelectedItemChanged(object sender, EventArgs e)
	{
		var actions = CRNAttribute.GetAttribute(typeDropDown.SelectedItem).ValidActions;

		if (actions.Length == 0)
		{
			DD_Action.Items = Enum.GetValues(typeof(StatusAction)).Cast<StatusAction>().Where(x => CRNAttribute.GetAttribute(x).Browsable).ToArray();
			DD_Action.SelectedItem = DD_Action.Items[0];
		}
		else
		{
			DD_Action.Items = actions;
			DD_Action.SelectedItem = DD_Action.Items[0];
		}

		L_Output.Invalidate();
	}

	public TBase PackageStatus => new TBase()
	{
		Type = typeDropDown.SelectedItem,
		Action = DD_Action.SelectedItem,
		Note = TB_Note.Text,
		Packages = P_Packages.Controls.OfType<MiniPackageControl>().Select(x => x.SteamId).ToArray(),
	};

	protected override void DesignChanged(FormDesign design)
	{
		P_Main.BackColor = design.BackColor;
		L_OutputTitle.ForeColor = design.LabelColor;
		L_Output.ForeColor = design.InfoColor;
	}

	protected override void UIChanged()
	{
		MinimumSize = UI.Scale(new Size(250, 0), UI.UIScale);
		P_Main.Padding = slickSpacer1.Margin = L_OutputTitle.Margin = UI.Scale(new Padding(5), UI.FontScale);
		CloseIcon.Size = UI.Scale(new Size(16, 16), UI.FontScale);
		TB_Note.MinimumSize = new Size(0, (int)(64 * UI.FontScale));
		I_Paste.Size = I_Copy.Size = I_AddPackage.Size = I_Note.Size = UI.Scale(new Size(24, 24), UI.FontScale);
		I_Paste.Padding = I_Copy.Padding = I_AddPackage.Padding = I_Note.Padding = UI.Scale(new Padding(5), UI.FontScale);
		L_OutputTitle.Font = UI.Font(7.5F, FontStyle.Bold);
	}

	private void I_Note_Click(object sender, EventArgs e)
	{
		TB_Note.Visible = I_Note.Selected = !I_Note.Selected;
	}

	private void I_AddPackage_Click(object sender, EventArgs e)
	{
		var form = new PC_SelectPackage();

		form.PackageSelected += Form_PackageSelected;

		Program.MainForm.PushPanel(null, form);
	}

	private void Form_PackageSelected(IEnumerable<ulong> packages)
	{
		foreach (var item in packages)
		{
			if (!P_Packages.Controls.OfType<MiniPackageControl>().Any(x => x.SteamId == item))
			{
				P_Packages.Controls.Add(new MiniPackageControl(item) { Dock = DockStyle.Top });
			}
		}
	}

	private void CloseIcon_Click(object sender, EventArgs e)
	{
		Dispose();
	}

	private void I_Copy_Click(object sender, EventArgs e)
	{
		if (P_Packages.Controls.Count > 0)
		{
			Clipboard.SetText(P_Packages.Controls.OfType<MiniPackageControl>().Select(x => x.SteamId).ListStrings(","));
		}
	}

	private void I_Paste_Click(object sender, EventArgs e)
	{
		if (!Clipboard.ContainsText())
		{
			return;
		}

		var text = Clipboard.GetText().Split(',');

		for (var i = 0; i < text.Length; i++)
		{
			if (ulong.TryParse(text[i], out var id))
			{
				if (!P_Packages.Controls.OfType<MiniPackageControl>().Any(x => x.SteamId == id))
				{
					P_Packages.Controls.Add(new MiniPackageControl(id) { Dock = DockStyle.Top });
				}
			}
		}
	}

	private void RefreshOutput(object sender, ControlEventArgs e)
	{
		L_Output.Invalidate();
	}

	private void RefreshOutput(object sender, EventArgs e)
	{
		L_Output.Invalidate();
	}

	private void L_Output_Paint(object sender, PaintEventArgs e)
	{
		e.Graphics.SetUp(L_Output.BackColor);

		string message;

		var action = LocaleCR.Get($"Action_{DD_Action.SelectedItem}");
		var actionText = P_Packages.Controls.Count switch { 0 => action.Zero, 1 => action.One, _ => action.Plural } ?? action.One;
		if (typeDropDown.SelectedItem is InteractionType.Successor)
		{
			var translation = LocaleCR.Get($"{typeof(T).Name.Remove("Type")}_{InteractionType.SucceededBy}");
			message = string.Format($"{translation.One}\r\n\r\n{actionText}", (P_Packages.Controls.FirstOrDefault(x => true) as MiniPackageControl)?.Package?.CleanName(), (PanelContent.GetParentPanel(this) as PC_CompatibilityManagement)?.CurrentPackage?.CleanName()).Trim();
		}
		else
		{
			var translation = LocaleCR.Get($"{typeof(T).Name.Remove("Type")}_{typeDropDown.SelectedItem}");
			var text = P_Packages.Controls.Count switch { 0 => translation.Zero, 1 => translation.One, _ => translation.Plural } ?? translation.One;
			message = string.Format($"{text}\r\n\r\n{actionText}", (PanelContent.GetParentPanel(this) as PC_CompatibilityManagement)?.CurrentPackage?.CleanName(), (P_Packages.Controls.FirstOrDefault(x => true) as MiniPackageControl)?.Package?.CleanName()).Trim();
		}

		e.Graphics.DrawString(message, L_Output.Font, new SolidBrush(L_Output.ForeColor), L_Output.ClientRectangle);

		L_Output.Height = (int)e.Graphics.Measure(message, L_Output.Font, L_Output.Width).Height + 3;
	}
}
