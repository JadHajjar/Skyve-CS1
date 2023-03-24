using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Steam;
using LoadOrderToolTwo.UserInterface.Panels;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

using static CompatibilityReport.CatalogData.Enums;

namespace LoadOrderToolTwo.UserInterface;
internal class PackageViewControl : SlickImageControl
{
	private readonly bool _isMod;
	private readonly bool _isLocal;

	private SteamWorkshopItem? Item;
	private CompatibilityManager.ReportInfo? CompatibilityReport;
	private Package? LocalPackage;
	private Rectangle buttonRect;

	private PackageViewControl()
	{
		Anchor = AnchorStyles.Left | AnchorStyles.Right;
	}

	public PackageViewControl(SteamWorkshopItem item) : this()
	{
		SetWorkshopItem(item);

		_isMod = item.Tags.Any("Mod");
	}

	public PackageViewControl(Profile.Asset package) : this()
	{
		_isLocal = package.SteamId == 0;
		_isMod = package is Profile.Mod;
		Text = package.Name;
	}

	public void SetWorkshopItem(SteamWorkshopItem item)
	{
		Item = item;

		var steamId = ulong.Parse(item.PublishedFileID);

		LocalPackage = CentralManager.Packages.FirstOrDefault(x => x.SteamId == steamId);
		CompatibilityReport = CompatibilityManager.GetCompatibilityReport(steamId);

		LoadImage(item.PreviewURL, ImageManager.GetImage);
	}

	protected override void UIChanged()
	{
		Padding = UI.Scale(new Padding(5), UI.FontScale);
		Margin = UI.Scale(new Padding(3), UI.FontScale);
		Height = (int)(85 * UI.FontScale);
		Font = UI.Font(9F, FontStyle.Bold);
	}

	private State GetState()
	{
		if (LocalPackage == null)
		{
			return State.Unsubscribed;
		}

		if (LocalPackage.IsIncluded)
		{
			return (LocalPackage.Mod?.IsEnabled ?? true) ? State.Enabled : State.Disabled;
		}

		return State.Excluded;
	}

	private enum State
	{
		Unsubscribed,
		Disabled,
		Enabled,
		Excluded
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		var iconRect = new Rectangle(Padding.Left, Padding.Top, Height - Padding.Vertical, Height - Padding.Vertical);

		if (iconRect.Contains(e.Location))
		{
			if (LocalPackage != null)
			{
				(FindForm() as BasePanelForm)?.PushPanel(null, new PC_PackagePage(LocalPackage));
			}
			else if (Item != null)
			{
				try
				{ Process.Start($"https://steamcommunity.com/workshop/filedetails/?id={Item.PublishedFileID}"); }
				catch { }
			}
		}

		if (buttonRect.Contains(e.Location))
		{

		}

		base.OnMouseClick(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.SetUp(BackColor);

		var iconRect = new Rectangle(Padding.Left, Padding.Top, Height - Padding.Vertical, Height - Padding.Vertical);

		e.Graphics.FillRoundedRectangle(new SolidBrush(FormDesign.Design.BackColor), ClientRectangle.Pad(Padding), Padding.Left);
		e.Graphics.DrawRoundedRectangle(new Pen(iconRect.Contains(PointToClient(CursorLocation))? FormDesign.Design.ActiveColor: FormDesign.Design.AccentColor, (float)(1.5 * UI.FontScale)), ClientRectangle.Pad(Padding), Padding.Left);

		if (HoverState.HasFlag(HoverState.Hovered))
		{
			using var brush = new LinearGradientBrush(ClientRectangle, Color.FromArgb(FormDesign.Design.Type == FormDesignType.Light ? 150 : 220, FormDesign.Design.AccentColor), Color.Empty, LinearGradientMode.Horizontal);
			e.Graphics.FillRoundedRectangle(brush, ClientRectangle.Pad(Padding), Padding.Left);
		}

		if (Loading)
		{
			DrawLoader(e.Graphics, iconRect.CenterR(UI.Scale(new Size(32, 32), UI.FontScale)));
		}
		else
		{
			e.Graphics.DrawRoundedImage(Image ?? (_isMod ? Properties.Resources.I_ModIcon : Properties.Resources.I_AssetIcon).Color(FormDesign.Design.IconColor), iconRect, Padding.Left, Image is null ? null : FormDesign.Design.IconColor, topRight: false, botRight: false);
		}

		var text = (Item?.Title ?? Text).RemoveVersionText().IfEmpty(Text);
		var textRect = ClientRectangle.Pad(Padding.Horizontal + iconRect.Width, Padding.Vertical, 0, 0);
		e.Graphics.DrawString(text, Font, new SolidBrush(ForeColor), textRect);

		var x = Padding.Left + iconRect.Width;
		var y = (int)e.Graphics.Measure(text, Font, textRect.Width).Height + Padding.Vertical;
		var secondY = y;

		if (_isLocal)
		{
			var localRect = DrawLabel(e, Locale.Local, Properties.Resources.I_Local_16, FormDesign.Design.ButtonColor.MergeColor(FormDesign.Design.ActiveColor, 50), ClientRectangle.Pad(x, y, 0, 0), ContentAlignment.TopLeft);

			secondY = Math.Max(secondY, localRect.Bottom);
			x = localRect.Right;
		}

		if (Item?.Author is not null)
		{
			var authorRect = DrawLabel(e, Item.Author.Name, Properties.Resources.I_Developer_16, FormDesign.Design.ButtonColor.MergeColor(FormDesign.Design.ActiveColor, 75), ClientRectangle.Pad(x, y, 0, 0), ContentAlignment.TopLeft);

			secondY = Math.Max(secondY, authorRect.Bottom);
			x = authorRect.Right;
		}

		if (CompatibilityReport is not null)
		{
			secondY = Math.Max(secondY, DrawLabel(e, LocaleHelper.GetGlobalText(CompatibilityReport.Severity == ReportSeverity.Unsubscribe ? Locale.ShouldNotBeSubscribed : $"CR_{CompatibilityReport.Severity}"), Properties.Resources.I_CompatibilityReport_16, (CompatibilityReport.Severity switch
			{
				ReportSeverity.MinorIssues => FormDesign.Design.YellowColor,
				ReportSeverity.MajorIssues => FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.RedColor),
				ReportSeverity.Unsubscribe => FormDesign.Design.RedColor,
				ReportSeverity.Remarks => FormDesign.Design.ButtonColor,
				_ => FormDesign.Design.GreenColor
			}).MergeColor(FormDesign.Design.BackColor, 60), ClientRectangle.Pad(x, y, 0, 0), ContentAlignment.TopLeft).Bottom);
		}

		x = Padding.Left + iconRect.Width;

		if (Item?.Tags is not null)
		{
			foreach (var tag in Item.Tags)
			{
				var tagRect = DrawLabel(e, tag, null, FormDesign.Design.ButtonColor, ClientRectangle.Pad(x, secondY, 0, 0), ContentAlignment.TopLeft);
				x = tagRect.Right;
			}
		}

		if (_isLocal)
		{
			DrawLabel(e, _isMod ? "Mod" : "Asset", null, FormDesign.Design.ButtonColor, ClientRectangle.Pad(x, secondY, 0, 0), ContentAlignment.TopLeft);
		}
		//var buttonIcon = GetState() switch { State.Excluded => Locale.Include, State.Enabled  State.Unsubscribed => Locale.Subscribe}
		var buttonSize = SlickButton.GetSize(e.Graphics, Properties.Resources.I_Add, "Subscribe", new Font(Font, FontStyle.Regular));
		buttonRect = ClientRectangle.Pad(Padding).Pad(Padding).Align(buttonSize, ContentAlignment.BottomRight);
		var hovered = buttonRect.Contains(PointToClient(Cursor.Position));

		SlickButton.DrawButton(e, buttonRect, "Subscribe", new Font(Font, FontStyle.Regular), Properties.Resources.I_Add, null, hovered ? HoverState & ~HoverState.Focused : HoverState.Normal);

		Cursor = hovered || iconRect.Contains(PointToClient(Cursor.Position)) ? Cursors.Hand : Cursors.Default;
	}

	private Rectangle DrawLabel(PaintEventArgs e, string? text, Bitmap? icon, Color color, Rectangle rectangle, ContentAlignment alignment)
	{
		if (text == null)
		{
			return Rectangle.Empty;
		}

		var size = e.Graphics.Measure(text, UI.Font(7.5F)).ToSize();

		if (icon is not null)
		{
			size.Width += icon.Width + Padding.Left;
		}

		size.Width += Padding.Left;

		rectangle = rectangle.Pad(Padding).Align(size, alignment);

		using var backBrush = rectangle.Gradient(color);
		using var foreBrush = new SolidBrush(color.GetTextColor());

		e.Graphics.FillRoundedRectangle(backBrush, rectangle, (int)(3 * UI.FontScale));
		e.Graphics.DrawString(text, UI.Font(7.5F), foreBrush, icon is null ? rectangle : rectangle.Pad(icon.Width + (Padding.Left * 2) - 2, 0, 0, 0), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

		if (icon is not null)
		{
			e.Graphics.DrawImage(icon.Color(color.GetTextColor()), rectangle.Pad(Padding.Left, 0, 0, 0).Align(icon.Size, ContentAlignment.MiddleLeft));
		}

		return rectangle;
	}
}
