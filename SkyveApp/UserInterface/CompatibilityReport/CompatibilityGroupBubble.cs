using Extensions;

using SkyveApp.Domain.Compatibility;
using SkyveApp.Domain.Interfaces;
using SkyveApp.UserInterface.Panels;
using SkyveApp.Utilities;
using SkyveApp.Utilities.IO;
using SkyveApp.Utilities.Managers;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.CompatibilityReport;
internal class CompatibilityGroupBubble : SlickImageControl
{
	private readonly Dictionary<PseudoPackage, Rectangle> _buttonRects = new();
	private readonly Dictionary<PseudoPackage, Rectangle> _modRects = new();
	private readonly Dictionary<IPackage, ReportItem[]> reports = new();
	public int Count { get; set; }
	public LocaleHelper.Translation InfoText { get; set; }
	public NotificationType Type { get; }

	public CompatibilityGroupBubble(NotificationType type, IEnumerable<CompatibilityInfo> infos)
	{
		Type = type;

		foreach (var info in infos)
		{
			reports[info.Package] = info.ReportItems.Where(x => x.Status.Notification == type).ToArray();
		}
	}

	protected override void UIChanged()
	{
		Margin = UI.Scale(new Padding(10, 10, 0, 0), UI.FontScale);
		Padding = UI.Scale(new Padding(5), UI.FontScale);
		Width = (int)(300 * UI.FontScale);
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);

		foreach (var item in _buttonRects)
		{
			if (item.Value.Contains(e.Location))
			{
				if (e.Button == MouseButtons.Left)
				{
					Clicked(item.Key, true);
				}

				return;
			}
		}

		foreach (var item in _modRects)
		{
			if (item.Value.Contains(e.Location))
			{
				if (e.Button == MouseButtons.Left)
				{
					Clicked(item.Key, false);
				}
				else if (e.Button == MouseButtons.Right && item.Key.Package is not null)
				{
					var items = PC_PackagePage.GetRightClickMenuItems(item.Key.Package);

					this.TryBeginInvoke(() => SlickToolStrip.Show(Program.MainForm, items));
				}

				return;
			}
		}

		//if (e.Button == MouseButtons.Left && allButtonRect.Contains(e.Location))
		//{
		//	switch (Message.Status.Action)
		//	{
		//		case StatusAction.SubscribeToPackages:
		//			await CitiesManager.Subscribe(Message.Packages.Where(x => x.Package?.Package is null).Select(x => x.SteamId));
		//			ContentUtil.SetBulkIncluded(Message.Packages.SelectWhereNotNull(x => x.Package)!, true);
		//			ContentUtil.SetBulkEnabled(Message.Packages.SelectWhereNotNull(x => x.Package?.Package?.Mod)!, true);
		//			break;
		//		case StatusAction.RequiresConfiguration:
		//			// Snooze
		//			break;
		//		case StatusAction.UnsubscribeThis:
		//			await CitiesManager.UnSubscribe(new[] { PackageCompatibilityReportControl.Package.SteamId });
		//			break;
		//		case StatusAction.UnsubscribeOther:
		//			await CitiesManager.UnSubscribe(Message.Packages.Select(x => x.SteamId));
		//			break;
		//		case StatusAction.ExcludeThis:
		//			PackageCompatibilityReportControl.Package.IsIncluded = false;
		//			break;
		//		case StatusAction.ExcludeOther:
		//			foreach (var item in Message.Packages)
		//			{
		//				if (item.Package is not null)
		//					item.Package.IsIncluded = false;
		//			}
		//			break;
		//		case StatusAction.RequestReview:
		//			Program.MainForm.PushPanel(null, new PC_RequestReview(PackageCompatibilityReportControl.Package));
		//			break;
		//	}
		//}
	}

	private async void Clicked(PseudoPackage item, bool button)
	{
		var package = item.Package;

		//if (!button)
		{
			if (package is not null)
			{
				Program.MainForm.PushPanel(null, package.IsCollection ? new PC_ViewCollection(package) : new PC_PackagePage(package));
			}
			else
			{
				PlatformUtil.OpenUrl($"https://steamcommunity.com/workshop/filedetails/?id={item.SteamId}");
			}

			return;
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		var clientRectangle = ClientRectangle.Pad(Padding.Left / 2, 0, 0, 0);

		e.Graphics.SetUp(BackColor);

		e.Graphics.FillRoundedRectangle(new SolidBrush(Type.GetColor()), new Rectangle(0, 1, Padding.Horizontal, Height - 2), Padding.Left);

		e.Graphics.FillRoundedRectangle(new SolidBrush(FormDesign.Design.AccentBackColor.MergeColor(Type.GetColor(), 90)), clientRectangle, Padding.Left);

		using var icon = Type.GetIcon(true).Large;

		if (icon is null)
		{
			return;
		}

		var imgRect = clientRectangle.Pad(Padding).Align(icon.Size, ContentAlignment.TopLeft);
		var textRect = new Rectangle(imgRect.Right + Padding.Left, imgRect.Y, 0, imgRect.Height).AlignToFontSize(UI.Font(9.75F, FontStyle.Bold), ContentAlignment.MiddleLeft);

		textRect.Size = Size.Ceiling(e.Graphics.Measure(LocaleCR.Get(Type.ToString()), UI.Font(9.75F, FontStyle.Bold), Width - textRect.Left - Padding.Right));

		e.Graphics.DrawImage(icon.Color(Type.GetColor()), imgRect);

		e.Graphics.DrawString(LocaleCR.Get(Type.ToString()), UI.Font(9.75F, FontStyle.Bold), new SolidBrush(Type.GetColor()), textRect);

		var y = Math.Max(textRect.Bottom, imgRect.Bottom) + Padding.Top;
		var actionHovered = false;

		if (false)
		{
			var buttonText = "Sub";
			var buttonImage = IconManager.GetIcon("I_Add");
			var buttonRect = clientRectangle.Pad(Padding).Align(SlickButton.GetSize(e.Graphics, buttonImage, buttonText, Font), ContentAlignment.TopRight);

			SlickButton.DrawButton(e, buttonRect, buttonText, Font, buttonImage);
		}

		foreach (var kvp in reports.Take(10))
		{
			var maxAction = kvp.Value.Max(y => y.Status.Action);

			var rect = clientRectangle.Pad(Padding.Horizontal, y, Padding.Right, Padding.Bottom);

			rect.Height = (int)(40 * UI.FontScale);

			var packageID = new PseudoPackage(kvp.Key);

			if (e.ClipRectangle.IntersectsWith(rect))
			{
				PaintPackage(e, rect, new PseudoPackage(kvp.Key));
			}

			actionHovered |= rect.Contains(CursorLocation);

			rect.Y += rect.Height + Padding.Bottom;

			y = rect.Y;

			foreach (var Message in kvp.Value)
			{
				if (maxAction != Message.Status.Action)
				{
					continue;
				}
			}
		}

		Height = y + Padding.Bottom;

		Cursor = actionHovered ? Cursors.Hand : Cursors.Default;
	}

	private void PaintPackage(PaintEventArgs e, Rectangle rect, PseudoPackage packageID)
	{
		var fore = ForeColor;

		_modRects[packageID] = rect;

		if (rect.Contains(CursorLocation) && (!_buttonRects.ContainsKey(packageID) || !_buttonRects[packageID].Contains(CursorLocation)))
		{
			if (HoverState.HasFlag(HoverState.Pressed))
			{
				fore = FormDesign.Design.ActiveColor;
			}

			using var gradientbrush = new LinearGradientBrush(rect, Color.FromArgb(50, fore), Color.Empty, LinearGradientMode.Horizontal);

			e.Graphics.FillRectangle(gradientbrush, rect.Pad(rect.Height / 2, 0, 0, 0));
		}

		var isDlc = false;
		var dlc = isDlc ? SteamUtil.Dlcs.FirstOrDefault(x => x.Id == packageID) : null;
		var package = packageID.Package;

		if (!(package?.Workshop ?? true) && package?.IconImage is not null)
		{
			using var unsatImg = new Bitmap(package.IconImage, UI.Scale(new Size(40, 40), UI.FontScale)).Tint(Sat: 0);
			e.Graphics.DrawRoundedImage(unsatImg, rect.Align(UI.Scale(new Size(40, 40), UI.FontScale), ContentAlignment.TopLeft), (int)(4 * UI.FontScale), FormDesign.Design.AccentBackColor);
		}
		else
		{
			e.Graphics.DrawRoundedImage(dlc?.Thumbnail ?? package?.IconImage ?? Properties.Resources.I_ModIcon.Color(fore), rect.Align(UI.Scale(new Size(isDlc ? 40 * 460 / 215 : 40, 40), UI.FontScale), ContentAlignment.TopLeft), Padding.Left, FormDesign.Design.AccentBackColor);
		}

		List<(Color Color, string Text)>? tags = null;

		var textRect = rect.Pad((int)(((isDlc ? 40 * 460 / 215 : 40) + 3) * UI.FontScale), 0, 0, 0).AlignToFontSize(Font, ContentAlignment.MiddleLeft);

		e.Graphics.DrawString(dlc?.Name.Remove("Cities: Skylines - ").Replace("Content Creator Pack", "CCP") ?? package?.Name?.RemoveVersionText(out tags) ?? Locale.UnknownPackage, UI.Font(7.5F, FontStyle.Bold), new SolidBrush(fore), textRect, new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

		var tagRect = new Rectangle(textRect.Left, textRect.Y, 0, textRect.Height);

		if (tags is not null)
		{
			foreach (var item in tags)
			{
				tagRect.X += Padding.Left + e.DrawLabel(item.Text, null, item.Color, tagRect, ContentAlignment.BottomLeft, smaller: true).Width;
			}
		}

		//switch (Message.Status.Action)
		//{
		//	case StatusAction.SubscribeToPackages:
		//		var p = package?.Package;

		//		if (p is null)
		//		{
		//			buttonText = Locale.Subscribe;
		//			iconName = "I_Add";
		//		}
		//		else if (!p.IsIncluded)
		//		{
		//			buttonText = Locale.Include;
		//			iconName = "I_Check";
		//		}
		//		else if (!(p.Mod?.IsEnabled ?? true))
		//		{
		//			buttonText = Locale.Enable;
		//			iconName = "I_Enabled";
		//		}
		//		break;
		//	case StatusAction.SelectOne:
		//		buttonText = Locale.SelectThisPackage;
		//		iconName = "I_Ok";
		//		break;
		//	case StatusAction.Switch:
		//		buttonText = Locale.Switch;
		//		iconName = "I_Switch";
		//		break;
		//}

		//if (buttonText is null)
		//{
		//	rect.Y += _modRects[packageID].Height + Padding.Left;
		//	continue;
		//}

		//var buttonIcon = IconManager.GetIcon(iconName);
		//var buttonSize = SlickButton.GetSize(e.Graphics, buttonIcon, buttonText, UI.Font(7.5F), UI.Scale(new Padding(3), UI.FontScale));

		////if (_subscribingTo.Contains(packageID))
		////{
		////	_buttonRects[packageID] = Rectangle.Empty;
		////	DrawLoader(e.Graphics, rect.Align(new Size(24, 24), ContentAlignment.BottomRight));
		////}
		////else
		//{
		//	_buttonRects[packageID] = _modRects[packageID].Align(buttonSize, ContentAlignment.BottomRight);

		//	SlickButton.DrawButton(e, _buttonRects[packageID], buttonText, UI.Font(7.5F), buttonIcon, UI.Scale(new Padding(3), UI.FontScale), _buttonRects[packageID].Contains(CursorLocation) ? HoverState & ~HoverState.Focused : HoverState.Normal, Message.Status.Action is StatusAction.SelectOne ? ColorStyle.Active : ColorStyle.Green);
		//}
	}
}
