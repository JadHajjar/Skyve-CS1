using Extensions;

using SkyveApp.Domain.Compatibility;
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

internal class CompatibilityMessageControl : SlickControl
{
	private readonly List<ulong> _subscribingTo = new();
	private readonly Dictionary<PseudoPackage, Rectangle> _buttonRects = new();
	private readonly Dictionary<PseudoPackage, Rectangle> _modRects = new();
	private Rectangle allButtonRect;

	public CompatibilityMessageControl(PackageCompatibilityReportControl packageCompatibilityReportControl, ReportType type, ReportItem message)
	{
		Dock = DockStyle.Top;
		Type = type;
		Message = message;
		PackageCompatibilityReportControl = packageCompatibilityReportControl;

		if (message.Packages.Length != 0 && !message.Packages.All(x => x.Package is not null))
		{
			SteamUtil.WorkshopItemsLoaded += Invalidate;
		}
	}

	public ReportType Type { get; }
	public ReportItem Message { get; }
	public PackageCompatibilityReportControl PackageCompatibilityReportControl { get; }

	protected override void OnPaint(PaintEventArgs e)
	{
		try
		{
			e.Graphics.SetUp(BackColor);

			using var icon = Message.Status.Notification.GetIcon(false).Large;
			var actionHovered = false;
			var cursor = PointToClient(Cursor.Position);
			var pad = (int)(6 * UI.FontScale);
			var note = string.IsNullOrWhiteSpace(Message.Status.Note) ? null : LocaleCRNotes.Get(Message.Status.Note!).One;
			var color = Message.Status.Notification.GetColor().MergeColor(BackColor, 60);
			var iconRect = new Rectangle(Point.Empty, icon.Size).Pad(0, 0, -pad * 2, -pad * 2);
			var messageSize = e.Graphics.Measure(Message.Message, UI.Font(9F), Width - iconRect.Width - pad);
			var noteSize = e.Graphics.Measure(note, UI.Font(8.25F), Width - iconRect.Width - pad);
			var y = (int)(messageSize.Height + noteSize.Height + (noteSize.Height == 0 ? 0 : pad * 2));
			using var brush = new SolidBrush(color);

			GetAllButton(out var allText, out var allIcon, out var colorStyle);

			e.Graphics.FillRoundedRectangle(brush, iconRect, pad);
			e.Graphics.FillRoundedRectangle(brush, new Rectangle(iconRect.Width - (2 * pad), 0, 2 * pad, Height - pad), pad);

			e.Graphics.DrawImage(icon.Color(color.GetTextColor()), iconRect.CenterR(icon.Size));

			e.Graphics.DrawString(Message.Message, UI.Font(9F), new SolidBrush(ForeColor), ClientRectangle.Pad(iconRect.Width + pad, 0, 0, 0), new StringFormat { LineAlignment = y < Height && allText is null && !Message.Packages.Any() ? StringAlignment.Center : StringAlignment.Near });

			if (note is not null)
			{
				e.Graphics.DrawString(note, UI.Font(8.25F), new SolidBrush(Color.FromArgb(200, ForeColor)), ClientRectangle.Pad(iconRect.Width + pad, string.IsNullOrWhiteSpace(Message.Message) ? 0 : ((int)messageSize.Height + pad), 0, 0));
			}

			if (allText is not null)
			{
				var buttonIcon = IconManager.GetIcon(allIcon);
				var buttonSize = SlickButton.GetSize(e.Graphics, buttonIcon, allText, UI.Font(8.25F), UI.Scale(new Padding(4), UI.FontScale));

				allButtonRect = new Rectangle(0, y, Width, 0).Pad(iconRect.Width + pad, pad, 0, 0).Align(buttonSize, Message.Packages.Length > 0 ? ContentAlignment.TopCenter : ContentAlignment.TopLeft);

				SlickButton.DrawButton(e, allButtonRect, allText, UI.Font(8.25F), buttonIcon, UI.Scale(new Padding(4), UI.FontScale), allButtonRect.Contains(cursor) ? HoverState & ~HoverState.Focused : HoverState.Normal, colorStyle);

				actionHovered |= allButtonRect.Contains(cursor);

				y += allButtonRect.Height + (pad * 2);
			}

			if (Message.Packages.Length > 0)
			{
				var isDlc = Message.Type == ReportType.DlcMissing;
				var rect = ClientRectangle.Pad(iconRect.Width + pad, y + pad, 0, 0);

				rect.Height = (int)(40 * UI.FontScale);

				foreach (var packageID in Message.Packages)
				{
					var fore = ForeColor;

					actionHovered |= rect.Contains(cursor);

					_modRects[packageID] = rect;

					if (rect.Contains(cursor) && (!_buttonRects.ContainsKey(packageID) || !_buttonRects[packageID].Contains(cursor)))
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

					if (!(package?.Workshop ?? true) && package?.IconImage is not null)
					{
						using var unsatImg = new Bitmap(package.IconImage, UI.Scale(new Size(40, 40), UI.FontScale)).Tint(Sat: 0);
						e.Graphics.DrawRoundedImage(unsatImg, rect.Align(UI.Scale(new Size(40, 40), UI.FontScale), ContentAlignment.TopLeft), (int)(4 * UI.FontScale), FormDesign.Design.AccentBackColor);
					}
					else
					{
						e.Graphics.DrawRoundedImage(dlc?.Thumbnail ?? package?.IconImage ?? Properties.Resources.I_ModIcon.Color(fore), rect.Align(UI.Scale(new Size(isDlc ? (40 * 460 / 215) : 40, 40), UI.FontScale), ContentAlignment.TopLeft), pad, FormDesign.Design.AccentBackColor);
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

					if (buttonText is null || package?.IsCollection == true)
					{
						rect.Y += _modRects[packageID].Height + pad;
						continue;
					}

					var buttonIcon = IconManager.GetIcon(iconName);
					var buttonSize = SlickButton.GetSize(e.Graphics, buttonIcon, buttonText, UI.Font(7.5F), UI.Scale(new Padding(3), UI.FontScale));

					if (_subscribingTo.Contains(packageID))
					{
						_buttonRects[packageID] = Rectangle.Empty;
						DrawLoader(e.Graphics, rect.Align(new Size(24, 24), ContentAlignment.BottomRight));
					}
					else
					{
						_buttonRects[packageID] = _modRects[packageID].Align(buttonSize, ContentAlignment.BottomRight);

						SlickButton.DrawButton(e, _buttonRects[packageID], buttonText, UI.Font(7.5F), buttonIcon, UI.Scale(new Padding(3), UI.FontScale), _buttonRects[packageID].Contains(cursor) ? HoverState & ~HoverState.Focused : HoverState.Normal, Message.Status.Action is StatusAction.SelectOne ? ColorStyle.Active : ColorStyle.Green);
					}

					rect.Y += _modRects[packageID].Height + pad;
				}

				y = rect.Y;
			}

			Cursor = actionHovered ? Cursors.Hand : Cursors.Default;
			Height = Math.Max(iconRect.Height, y);
		}
		catch { }
	}

	private void GetAllButton(out string? allText, out string? allIcon, out ColorStyle colorStyle)
	{
		allText = null;
		allIcon = null;
		colorStyle = ColorStyle.Red;

		switch (Message.Status.Action)
		{
			case StatusAction.SubscribeToPackages:
				if (Message.Packages.Length > 1)
				{
					var max = Message.Packages.Max(x =>
					{
						var p = SteamUtil.GetItem(x)?.Package;

						if (p is null)
						{
							return 3;
						}
						else if (!p.IsIncluded)
						{
							return 2;
						}
						else if (!(p.Mod?.IsEnabled ?? true))
						{
							return 1;
						}

						return 0;
					});

					colorStyle = ColorStyle.Green;
					allText = max switch { 3 => Locale.SubscribeAll, 2 => Locale.IncludeAll, 1 => Locale.EnableAll, _ => null };
					allIcon = max switch { 3 => "I_Add", 2 => "I_Check", 1 => "I_Enabled", _ => null };
				}
				break;
			case StatusAction.RequiresConfiguration:
				allText = Locale.Snooze;
				allIcon = "I_Snooze";
				colorStyle = ColorStyle.Active;
				break;
			case StatusAction.UnsubscribeThis:
				allText = Locale.Unsubscribe;
				allIcon = "I_RemoveSteam";
				break;
			case StatusAction.UnsubscribeOther:
				allText = Message.Packages.Length switch { 0 => null, 1 => Locale.Unsubscribe, _ => Locale.UnsubscribeAll };
				allIcon = "I_RemoveSteam";
				break;
			case StatusAction.ExcludeThis:
				allText = Locale.Exclude;
				allIcon = "I_X";
				break;
			case StatusAction.ExcludeOther:
				allText = Message.Packages.Length switch { 0 => null, 1 => Locale.Exclude, _ => Locale.ExcludeAll };
				allIcon = "I_X";
				break;
			case StatusAction.RequestReview:
				allText = LocaleCR.RequestReview;
				allIcon = "I_RequestReview";
				colorStyle = ColorStyle.Active;
				break;
		}
	}

	protected override async void OnMouseClick(MouseEventArgs e)
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

		if (e.Button == MouseButtons.Left && allButtonRect.Contains(e.Location))
		{
			switch (Message.Status.Action)
			{
				case StatusAction.SubscribeToPackages:
					await CitiesManager.Subscribe(Message.Packages.Where(x => x.Package?.Package is null).Select(x => x.SteamId));
					ContentUtil.SetBulkIncluded(Message.Packages.SelectWhereNotNull(x => x.Package)!, true);
					ContentUtil.SetBulkEnabled(Message.Packages.SelectWhereNotNull(x => x.Package?.Package?.Mod)!, true);
					break;
				case StatusAction.RequiresConfiguration:
					// Snooze
					break;
				case StatusAction.UnsubscribeThis:
					await CitiesManager.UnSubscribe(new[] { PackageCompatibilityReportControl.Package.SteamId });
					break;
				case StatusAction.UnsubscribeOther:
					await CitiesManager.UnSubscribe(Message.Packages.Select(x => x.SteamId));
					break;
				case StatusAction.ExcludeThis:
					PackageCompatibilityReportControl.Package.IsIncluded = false;
					break;
				case StatusAction.ExcludeOther:
					foreach (var item in Message.Packages)
					{
						if (item.Package is not null)
						{
							item.Package.IsIncluded = false;
						}
					}
					break;
				case StatusAction.RequestReview:
					Program.MainForm.PushPanel(null, new PC_RequestReview(PackageCompatibilityReportControl.Package));
					break;
			}
		}
	}

	private async void Clicked(PseudoPackage item, bool button)
	{
		var package = item.Package;

		if (!button)
		{
			if (Message.Type is ReportType.DlcMissing)
			{
				PlatformUtil.OpenUrl($"https://store.steampowered.com/app/{item.SteamId}");
			}
			else if (package is not null)
			{
				Program.MainForm.PushPanel(null, package.IsCollection ? new PC_ViewCollection(package) : new PC_PackagePage(package));
			}
			else
			{
				PlatformUtil.OpenUrl($"https://steamcommunity.com/workshop/filedetails/?id={item.SteamId}");
			}

			return;
		}

		var p = package?.Package;

		if (p is null)
		{
			_subscribingTo.Add(item);

			Loading = true;

			await CitiesManager.Subscribe(new[] { item.SteamId });
		}
		else
		{
			p.IsIncluded = true;

			if (p.Mod is not null)
			{
				p.Mod.IsEnabled = true;
			}
		}

		switch (Message.Status.Action)
		{
			case StatusAction.SelectOne:
				foreach (var id in Message.Packages)
				{
					if (id != item)
					{
						var pp = id.Package;

						if (pp is not null)
						{
							pp.IsIncluded = false;
						}
					}
				}
				break;
			case StatusAction.Switch:
				PackageCompatibilityReportControl.Package.IsIncluded = false;
				PackageCompatibilityReportControl.Package.IsIncluded = false;
				break;
		}
	}
}