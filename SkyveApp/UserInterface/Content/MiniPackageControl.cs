using Extensions;

using SkyveApp.Domain.Interfaces;
using SkyveApp.UserInterface.Panels;
using SkyveApp.Utilities;

using SlickControls;

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Content;
internal class MiniPackageControl : SlickControl
{
	private readonly IPackage? _package;
	public IPackage? Package => _package ?? SteamUtil.GetItem(SteamId);
	public ulong SteamId { get; }

	public bool ReadOnly { get; set; }

	public MiniPackageControl(ulong steamId)
	{
		Cursor = Cursors.Hand;
		SteamId = steamId;

		if (Package is null)
		{
			SteamUtil.WorkshopItemsLoaded += () => this.TryInvoke(() => Parent?.Parent?.Parent?.Invalidate(true));
		}
	}

	public MiniPackageControl(IPackage package)
	{
		Cursor = Cursors.Hand;
		_package = package;
		SteamId = package.SteamId;

		if (Package is null)
		{
			SteamUtil.WorkshopItemsLoaded += () => this.TryInvoke(() => Parent?.Parent?.Parent?.Invalidate(true));
		}
	}

	protected override void UIChanged()
	{
		Height = (int)(32 * UI.FontScale);

		Padding = UI.Scale(new Padding(5), UI.FontScale);
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);

		if (Package is null)
		{
			return;
		}

		switch (e.Button)
		{
			case MouseButtons.Left:
			case MouseButtons.None:
				var imageRect = ClientRectangle.Pad(Padding);
				imageRect = imageRect.Align(new Size(imageRect.Height, imageRect.Height), ContentAlignment.MiddleRight);

				if (!ReadOnly && imageRect.Contains(e.Location))
				{
					Dispose();
				}
				else
				{
					Program.MainForm.PushPanel(null, Package.IsCollection ? new PC_ViewCollection(Package) : new PC_PackagePage(Package));
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
		var image = Package?.IconImage;

		if (image is not null)
		{
			if (!Package!.Workshop)
			{
				using var unsatImg = new Bitmap(image, imageRect.Size).Tint(Sat: 0);
				e.Graphics.DrawRoundedImage(unsatImg, imageRect, (int)(4 * UI.FontScale), FormDesign.Design.AccentBackColor);
			}
			else
			{
				e.Graphics.DrawRoundedImage(image, imageRect, (int)(4 * UI.FontScale), FormDesign.Design.AccentBackColor);
			}
		}
		else
		{
			using var generic = Properties.Resources.I_CollectionIcon.Color(FormDesign.Design.IconColor);

			e.Graphics.DrawRoundedImage(generic, imageRect, (int)(4 * UI.FontScale), FormDesign.Design.AccentBackColor);
		}

		List<(Color Color, string Text)>? tags = null;

		var textRect = ClientRectangle.Pad(imageRect.Right + Padding.Left, Padding.Top, !ReadOnly && HoverState.HasFlag(HoverState.Hovered) ? (imageRect.Right + Padding.Left) : 0, Padding.Bottom).AlignToFontSize(Font, ContentAlignment.MiddleLeft);

		e.Graphics.DrawString(Package?.CleanName(out tags) ?? Locale.UnknownPackage, Font, new SolidBrush(ForeColor), textRect, new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

		var tagRect = new Rectangle(textRect.Right, textRect.Y, 0, textRect.Height);

		if (tags is not null)
		{
			foreach (var item in tags)
			{
				tagRect.X -= Padding.Left + e.DrawLabel(item.Text, null, item.Color, tagRect, ContentAlignment.MiddleRight, smaller: true).Width;
			}
		}

		if (!ReadOnly && HoverState.HasFlag(HoverState.Hovered))
		{
			imageRect = ClientRectangle.Pad(Padding).Align(imageRect.Size, ContentAlignment.MiddleRight);

			if (imageRect.Contains(PointToClient(Cursor.Position)))
			{
				e.Graphics.FillRoundedRectangle(new SolidBrush(Color.FromArgb(HoverState.HasFlag(HoverState.Pressed) ? 50 : 20, FormDesign.Design.RedColor.MergeColor(ForeColor, 65))), imageRect.Pad(1), (int)(4 * UI.FontScale));
			}

			using var img = IconManager.GetIcon("I_Disposable");

			e.Graphics.DrawImage(img.Color(FormDesign.Design.RedColor, (byte)(HoverState.HasFlag(HoverState.Pressed) ? 255 : 175)), imageRect.CenterR(img.Size));
		}

		if (Dock == DockStyle.None)
		{
			Width = (2 * (imageRect.Width + Padding.Horizontal)) + textRect.Right - tagRect.X + (int)e.Graphics.Measure(Package?.CleanName(out _) ?? Locale.UnknownPackage, Font).Width + 1;
		}
	}
}
