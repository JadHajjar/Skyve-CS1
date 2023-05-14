using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.UserInterface.Panels;
using LoadOrderToolTwo.Utilities;

using SlickControls;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Content;
internal class MiniPackageControl : SlickControl
{
	public IPackage Package { get; private set; }

	public MiniPackageControl(IPackage package)
	{
		Package = package;
		Cursor = Cursors.Hand;
	}

	public MiniPackageControl(ulong steamId)
	{
		Package = new Profile.Asset { SteamId = steamId };
		Cursor = Cursors.Hand;

		new BackgroundAction(async () =>
		{
			var data = await SteamUtil.GetWorkshopInfoAsync(new[] { steamId });

			if (data.ContainsKey(steamId))
			{
				Package = data[steamId];
				Invalidate();
			}
		}).Run();
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
				var imageRect = ClientRectangle.Pad(Padding);
				imageRect = imageRect.Align(new Size(imageRect.Height, imageRect.Height), ContentAlignment.MiddleRight);
				
				if (imageRect.Contains(e.Location))
				{
					Dispose();
				}
				else
				{
					Program.MainForm.PushPanel(null, new PC_PackagePage(Package));
				}

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

	protected override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.SetUp(BackColor);

		if (HoverState.HasFlag(HoverState.Hovered))
		{
			using var brush = new LinearGradientBrush(ClientRectangle.Pad(Height / 2, 0, 0, 0), FormDesign.Design.AccentBackColor, Color.Empty, LinearGradientMode.Horizontal);
			e.Graphics.FillRectangle(brush, ClientRectangle.Pad(Height / 2, 0, 0, 0).Pad(Padding));
		}

		var imageRect = ClientRectangle.Pad(Padding);
		imageRect.Width = imageRect.Height;
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

		e.Graphics.DrawString(Package.Name?.RemoveVersionText(out _) ?? Locale.UnknownPackage, Font, new SolidBrush(ForeColor), ClientRectangle.Pad(imageRect.Right + Padding.Left, Padding.Top, imageRect.Right + Padding.Left, Padding.Bottom).AlignToFontSize(Font, ContentAlignment.MiddleLeft), new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

		if (HoverState.HasFlag(HoverState.Hovered))
		{
			imageRect = ClientRectangle.Pad(Padding).Align(imageRect.Size, ContentAlignment.MiddleRight);

			if(imageRect.Contains(PointToClient(Cursor.Position)))
			e.Graphics.FillRoundedRectangle(new SolidBrush(Color.FromArgb(HoverState.HasFlag(HoverState.Pressed) ? 50 : 20, FormDesign.Design.RedColor.MergeColor(ForeColor, 65))), imageRect.Pad(1), (int)(4 * UI.FontScale));

			using var img = IconManager.GetIcon("I_Disposable");

			e.Graphics.DrawImage(img.Color(FormDesign.Design.RedColor, (byte)(HoverState.HasFlag(HoverState.Pressed) ? 255 : 175)), imageRect.CenterR(img.Size));
		}

		if (Dock == DockStyle.None)
		{
			Width = 2 * (imageRect.Width + Padding.Horizontal) + (int)e.Graphics.Measure(Package.Name?.RemoveVersionText(out _) ?? Locale.UnknownPackage, Font).Width + 1;
		}
	}
}
