using Extensions;

using LoadOrderToolTwo.Domain;

using SlickControls;

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface;
internal class PackageIcon : SlickImageControl
{
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public Package? Package { get; set; }
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool Collection { get; set; }

	protected override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.Clear(FormDesign.Design.BackColor);

		e.Graphics.FillRectangle(new SolidBrush(FormDesign.Design.AccentBackColor), new Rectangle(0, 0, Width, Height / 2));

		if (Loading)
		{
			DrawLoader(e.Graphics, ClientRectangle.CenterR(UI.Scale(new Size(32, 32), UI.UIScale)));
			return;
		}

		e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

		if (Image == null)
		{
			using var image = (Collection ? Properties.Resources.I_CollectionIcon : Package?.Mod is not null ? Properties.Resources.I_ModIcon : Properties.Resources.I_AssetIcon).Color(FormDesign.Design.IconColor);
			var iconRect = ClientRectangle.CenterR(image.Size);

			e.Graphics.FillRoundedRectangle(new SolidBrush(FormDesign.Design.IconColor), ClientRectangle, (int)(10 * UI.FontScale));
			e.Graphics.FillRoundedRectangle(new SolidBrush(FormDesign.Design.BackColor), iconRect.Pad(1), (int)(10 * UI.FontScale));

			e.Graphics.DrawRoundedImage(image, iconRect, (int)(10 * UI.FontScale), FormDesign.Design.AccentBackColor);
		}
		else
		{
			e.Graphics.DrawRoundedImage(Image, ClientRectangle, (int)(10 * UI.FontScale), FormDesign.Design.AccentBackColor);
		}
	}
}
