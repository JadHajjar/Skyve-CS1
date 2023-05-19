using Extensions;

using LoadOrderToolTwo.Domain.Compatibility;
using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.Utilities;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Bubbles;
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

	protected override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.SetUp(BackColor);

		e.Graphics.FillRoundedRectangle(new SolidBrush(Type.GetColor()), new Rectangle(0, 1, Padding.Horizontal, Height - 2), Padding.Left);

		e.Graphics.FillRoundedRectangle(new SolidBrush(FormDesign.Design.AccentBackColor), ClientRectangle.Pad(Padding.Left / 2, 0, 0, 0), Padding.Left);

		using var icon = Type.GetIcon(true).Large;

		if (icon is null)
		{
			return;
		}

		var imgRect = ClientRectangle.Pad(Padding).Align(icon.Size, ContentAlignment.TopLeft);
		var textRect = new Rectangle(imgRect.Right + Padding.Left, imgRect.Y, 0, imgRect.Height).AlignToFontSize(UI.Font(9.75F, FontStyle.Bold), ContentAlignment.MiddleLeft);

		textRect.Size = Size.Ceiling(e.Graphics.Measure(LocaleCR.Get(Type.ToString()), UI.Font(9.75F, FontStyle.Bold), Width - textRect.Left - Padding.Right));

		e.Graphics.DrawImage(icon.Color(Type.GetColor()), imgRect);

		e.Graphics.DrawString(LocaleCR.Get(Type.ToString()), UI.Font(9.75F, FontStyle.Bold), new SolidBrush(Type.GetColor()), textRect);

		var y = Math.Max(textRect.Bottom, imgRect.Bottom) + Padding.Top;
		var actionHovered = false;

		if (true)
		{
			var buttonText = "Sub";
			var buttonImage = IconManager.GetIcon("I_Add");
			var buttonRect = ClientRectangle.Pad(Padding).Align(SlickButton.GetSize(e.Graphics, buttonImage, buttonText, Font), ContentAlignment.TopRight);

			SlickButton.DrawButton(e, buttonRect, buttonText, Font, buttonImage);
		}

		foreach (var kvp in reports)
		{
			var maxAction = kvp.Value.Max(y => y.Status.Action);

			foreach (var Message in kvp.Value)
			{
				if (maxAction != Message.Status.Action)
				{
					continue;
				}

				var isDlc = Message.Type == ReportType.DlcMissing;
				var rect = ClientRectangle.Pad(Padding.Horizontal, y, Padding.Right, Padding.Bottom);

				rect.Height = (int)(40 * UI.FontScale);

				var packages = maxAction is StatusAction.SelectOne ? new[] { new PseudoPackage(kvp.Key) } : Message.Packages;

				foreach (var packageID in packages)
				{
					var fore = ForeColor;

					actionHovered |= rect.Contains(CursorLocation);

					_modRects[packageID] = rect;

					if (rect.Contains(CursorLocation) && (!_buttonRects.ContainsKey(packageID) || !_buttonRects[packageID].Contains(CursorLocation)))
					{
						if (HoverState.HasFlag(HoverState.Pressed))
						{
							fore = FormDesign.Design.ActiveColor;
						}

						using var gradientbrush = new LinearGradientBrush(ClientRectangle.Pad(rect.Height / 2, 0, 0, 0), Color.FromArgb(50, fore), Color.Empty, LinearGradientMode.Horizontal);

						e.Graphics.FillRectangle(gradientbrush, rect.Pad(rect.Height / 2, 0, 0, 0));
					}

					var dlc = isDlc ? SteamUtil.Dlcs.FirstOrDefault(x => x.Id == packageID) : null;
					var package = packageID.Package;

					e.Graphics.DrawRoundedImage(dlc?.Thumbnail ?? package?.IconImage ?? Properties.Resources.I_ModIcon.Color(fore), rect.Align(UI.Scale(new Size(isDlc ? (40 * 460 / 215) : 40, 40), UI.FontScale), ContentAlignment.TopLeft), Padding.Left, FormDesign.Design.AccentBackColor);

					List<string>? tags = null;

					textRect = rect.Pad((int)(((isDlc ? (40 * 460 / 215) : 40) + 3) * UI.FontScale), 0, 0, 0).AlignToFontSize(Font, ContentAlignment.MiddleLeft);

					e.Graphics.DrawString(dlc?.Name.Remove("Cities: Skylines - ").Replace("Content Creator Pack", "CCP") ?? package?.Name?.RemoveVersionText(out tags) ?? Locale.UnknownPackage, UI.Font(7.5F, FontStyle.Bold), new SolidBrush(fore), textRect, new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

					var tagRect = new Rectangle(textRect.Left, textRect.Y, 0, textRect.Height);

					if (tags is not null)
					{
						foreach (var item in tags)
						{
							if (item.ToLower() == "stable")
							{ continue; }

							var tcolor = item.ToLower() switch
							{
								"alpha" or "experimental" => Color.FromArgb(200, FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.RedColor)),
								"beta" or "test" or "testing" => Color.FromArgb(180, FormDesign.Design.YellowColor),
								"deprecated" or "obsolete" or "abandoned" or "broken" => Color.FromArgb(225, FormDesign.Design.RedColor),
								_ => (Color?)null
							};

							tagRect.X += Padding.Left + e.DrawLabel(tcolor is null ? item : LocaleHelper.GetGlobalText(item.ToUpper()), null, tcolor ?? FormDesign.Design.ButtonColor, tagRect, ContentAlignment.BottomLeft, smaller: true).Width;
						}
					}

					string? buttonText = null;
					string? iconName = null;

					switch (Message.Status.Action)
					{
						case StatusAction.SubscribeToPackages:
							var p = package?.Package;

							if (p is null)
							{
								buttonText = Locale.Subscribe;
								iconName = "I_Add";
							}
							else if (!p.IsIncluded)
							{
								buttonText = Locale.Include;
								iconName = "I_Check";
							}
							else if (!(p.Mod?.IsEnabled ?? true))
							{
								buttonText = Locale.Enable;
								iconName = "I_Enabled";
							}
							break;
						case StatusAction.SelectOne:
							buttonText = Locale.SelectThisPackage;
							iconName = "I_Ok";
							break;
						case StatusAction.Switch:
							buttonText = Locale.Switch;
							iconName = "I_Switch";
							break;
					}

					if (buttonText is null)
					{
						rect.Y += _modRects[packageID].Height + Padding.Left;
						continue;
					}

					var buttonIcon = IconManager.GetIcon(iconName);
					var buttonSize = SlickButton.GetSize(e.Graphics, buttonIcon, buttonText, UI.Font(7.5F), UI.Scale(new Padding(3), UI.FontScale));

					//if (_subscribingTo.Contains(packageID))
					//{
					//	_buttonRects[packageID] = Rectangle.Empty;
					//	DrawLoader(e.Graphics, rect.Align(new Size(24, 24), ContentAlignment.BottomRight));
					//}
					//else
					{
						_buttonRects[packageID] = _modRects[packageID].Align(buttonSize, ContentAlignment.BottomRight);

						SlickButton.DrawButton(e, _buttonRects[packageID], buttonText, UI.Font(7.5F), buttonIcon, UI.Scale(new Padding(3), UI.FontScale), _buttonRects[packageID].Contains(CursorLocation) ? HoverState & ~HoverState.Focused : HoverState.Normal, Message.Status.Action is StatusAction.SelectOne ? ColorStyle.Active : ColorStyle.Green);
					}

					rect.Y += _modRects[packageID].Height + Padding.Left;
				}

				y = rect.Y;
			}
		}

		Height = y + Padding.Bottom;
	}
}
