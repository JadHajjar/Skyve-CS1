using Extensions;

using LoadOrderToolTwo.UserInterface.Dropdowns;

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

		typeDropDown = new()
		{
			Text = typeof(T).Name,
		};

		roundedTableLayoutPanel1.Controls.Add(typeDropDown, 0, 0);

		AutoInvalidate = false;
		AutoSize = true;
	}

	protected override void DesignChanged(FormDesign design)
	{
		roundedTableLayoutPanel1.BackColor = design.BackColor;
	}

	protected override void UIChanged()
	{
		MinimumSize = UI.Scale(new Size(250, 0), UI.UIScale);
		roundedTableLayoutPanel1.Padding = UI.Scale(new Padding(5), UI.FontScale);
		topIcon1.Size = UI.Scale(new Size(20, 20), UI.FontScale);
		slickTextBox1.MinimumSize = new Size(0, (int)(64 * UI.FontScale));
	}
}
