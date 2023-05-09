using Extensions;

using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.UserInterface.Panels;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Content;
internal class MiniPackageControl : SlickControl
{
    public MiniPackageControl(IPackage package)
    {
		Package=package;
		Dock = DockStyle.Top;
	}

	protected override void UIChanged()
	{
		Height = (int)(32 * UI.FontScale);

		Padding = UI.Scale(new Padding(5), UI.FontScale);
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);

		switch (e.Button)
		{
			case MouseButtons.Left:
			case MouseButtons.None:
				(FindForm() as BasePanelForm)?.PushPanel(null, new PC_PackagePage(Package));
				break;
			case MouseButtons.Right:
				var items = PC_PackagePage.GetRightClickMenuItems(Package);

				this.TryBeginInvoke(() => SlickToolStrip.Show(Program.MainForm, items));
				break;
			case MouseButtons.Middle:
				Dispose();
				break;
		}
	}

	public IPackage Package { get; }

	protected override void OnPaint(PaintEventArgs e)
	{
        e.Graphics.SetUp(BackColor);

		var imageRect = ClientRectangle.Pad(Padding);
		imageRect.Width=imageRect.Height;
		var image = Package.IconImage;

		if (image is not null)
		{
			e.Graphics.DrawRoundedImage(image, imageRect, (int)(4 * UI.FontScale), FormDesign.Design.AccentBackColor);
		}
		else
		{
			using var generic = Properties.Resources.I_CollectionIcon.Color(FormDesign.Design.IconColor);

			e.Graphics.DrawRoundedImage(generic, imageRect, (int)(4 * UI.FontScale), FormDesign.Design.AccentBackColor);
		}

		e.Graphics.DrawString(Package.Name, Font, new SolidBrush(ForeColor), ClientRectangle.Pad(imageRect.Right + Padding.Left, Padding.Top, Padding.Right, Padding.Bottom).AlignToFontSize(Font, ContentAlignment.MiddleLeft));
	}
}
