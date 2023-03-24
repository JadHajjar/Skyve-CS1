using CompatibilityReport.CatalogData;

using Extensions;

using LoadOrderToolTwo.UserInterface.Panels;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface;

internal class CompatibilityMessageControl : SlickControl
{
	private readonly List<ulong> _subscribingTo = new();
	private readonly Dictionary<SmallMod, Rectangle> _buttonRects = new();
	private readonly Dictionary<SmallMod, Rectangle> _modRects = new();

	public CompatibilityMessageControl(PackageCompatibilityReportControl packageCompatibilityReportControl, Enums.ReportType type, CompatibilityManager.ReportMessage message)
	{
		Dock = DockStyle.Top;
		Type = type;
		Message = message;
		PackageCompatibilityReportControl = packageCompatibilityReportControl;

		if (message.LinkedPackages.Any())
		{
			Loading = true;
			new BackgroundAction("Loading package info", LoadPackages).Run();

			CentralManager.ContentLoaded += CentralManager_ContentLoaded;
		}
		else
		{
			AutoInvalidate = false;
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);

		CentralManager.ContentLoaded -= CentralManager_ContentLoaded;
	}

	private void CentralManager_ContentLoaded()
	{
		if (LinkedMods is null)
			return;

		foreach (var package in LinkedMods)
		{
			var localMod = ModsUtil.FindMod(package.SteamId);

			if (package.Package != localMod)
			{
				_subscribingTo.Remove(package.SteamId);
				package.Package = localMod;
			}
		}

		Loading = _subscribingTo.Any();
	}

	private async void LoadPackages()
	{
		var packages = new List<SmallMod>();
		var remainingPackages = new List<ulong>();

		foreach (var package in Message.LinkedPackages)
		{
			var localMod = ModsUtil.FindMod(package);

			if (localMod != null)
			{
				packages.Add(new SmallMod(package, localMod.Name, localMod.IconImage) { Package = localMod });
			}
			else
			{
				remainingPackages.Add(package);
			}
		}

		if (remainingPackages.Count > 0)
		{
			try
			{
				var steamData = await SteamUtil.GetWorkshopInfoAsync(remainingPackages.ToArray());

				foreach (var item in steamData)
				{
					try
					{
						var image = ImageManager.GetImage(item.Value.PreviewURL);

						packages.Add(new SmallMod(item.Key, item.Value.Title, image));
					}
					catch { packages.Add(new SmallMod(item.Key, item.Value.Title, null)); }
				}
			}
			catch
			{
				foreach (var item in remainingPackages)
				{
					var mod = CompatibilityManager.Catalog?.GetMod(item);

					if (mod != null)
					{
						packages.Add(new SmallMod(item, mod.Name, null));
					}
					else
					{
						packages.Add(new SmallMod(item, item.ToString(), null));
					}
				}
			}
		}

		LinkedMods = packages;
		Loading = false;
	}

	public Enums.ReportType Type { get; }
	public CompatibilityManager.ReportMessage Message { get; }
	private List<SmallMod>? LinkedMods { get; set; }
	public PackageCompatibilityReportControl PackageCompatibilityReportControl { get; }

	protected override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.Clear(BackColor);

		e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
		e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

		var actionHovered = false;
		var cursor = PointToClient(Cursor.Position);
		var pad = (int)(4 * UI.FontScale);
		var color = Message.Severity.GetSeverityColor().MergeColor(BackColor, 60);
		var iconRect = new Rectangle(Point.Empty, UI.Scale(new Size(24, 24), UI.FontScale));
		var messageSize = e.Graphics.Measure(Message.Message, UI.Font(9F), Width - iconRect.Width - pad);
		var noteSize = e.Graphics.Measure(Message.Note, UI.Font(8.25F), Width - iconRect.Width - pad);
		var y = (int)(messageSize.Height + noteSize.Height + pad);
		using var icon = Message.Severity.GetSeverityIcon(false);
		using var brush = new SolidBrush(color);

		e.Graphics.FillRoundedRectangle(brush, iconRect, pad);
		e.Graphics.FillRoundedRectangle(brush, new Rectangle(iconRect.Width - (2 * pad), 0, 2 * pad, Height - pad), pad);

		e.Graphics.DrawImage(icon.Color(color.GetTextColor()), iconRect.CenterR(icon.Size));

		e.Graphics.DrawString(Message.Message, UI.Font(9F), new SolidBrush(ForeColor), ClientRectangle.Pad(iconRect.Width + pad, 0, 0, 0), new StringFormat { LineAlignment = y < Height && Message.LinkedPackages.Length == 0 ? StringAlignment.Center : StringAlignment.Near });

		e.Graphics.DrawString(Message.Note, UI.Font(8.25F), new SolidBrush(Color.FromArgb(200, ForeColor)), ClientRectangle.Pad(iconRect.Width + pad, (int)messageSize.Height, 0, 0));

		if (Loading && LinkedMods is null)
		{
			DrawLoader(e.Graphics, ClientRectangle.Pad(iconRect.Width + pad, y, 0, 0).CenterR(24, 24));

			y += 32;
		}
		else if (LinkedMods is not null)
		{
			var rect = ClientRectangle.Pad(iconRect.Width + pad, y, 0, 0);

			rect.Height = (int)(50 * UI.FontScale);

			foreach (var item in LinkedMods)
			{
				var fore = ForeColor;

				actionHovered |= rect.Contains(cursor);

				_modRects[item] = rect;

				var buttonSize = Size.Empty;

				if (rect.Contains(cursor) && (!_buttonRects.ContainsKey(item) || !_buttonRects[item].Contains(cursor)))
				{
					if (HoverState.HasFlag(HoverState.Pressed))
					{
						fore = FormDesign.Design.ActiveForeColor;
					}

					e.Graphics.FillRoundedRectangle(new SolidBrush(Color.FromArgb(HoverState.HasFlag(HoverState.Pressed) ? 255 : 50, FormDesign.Design.ActiveColor)), rect.Pad(1), pad);
				}

				e.Graphics.DrawRoundedImage(item.Icon ?? Properties.Resources.I_ModIcon.Color(fore), rect.Align(UI.Scale(new Size(50, 50), UI.FontScale), ContentAlignment.TopLeft), pad, fore);

				e.Graphics.DrawString(item.Name, UI.Font(9F, FontStyle.Bold), new SolidBrush(fore), rect.Pad((int)(55 * UI.FontScale), 0, 0, 0));
				
				if (item.Package is not null)
				{
					e.Graphics.DrawString(Locale.ModOwned, UI.Font(7.5F, FontStyle.Italic), new SolidBrush(Color.FromArgb(150, fore)), rect.Pad((int)(55 * UI.FontScale), 0, 0, 0), new StringFormat { LineAlignment = StringAlignment.Far });
				}

				if (_subscribingTo.Contains(item.SteamId))
				{
					_buttonRects[item] = Rectangle.Empty;
					DrawLoader(e.Graphics, rect.Align(new Size(24, 24), ContentAlignment.BottomRight));
				}
				else if (item.Package is null || !item.Package.IsIncluded || !item.Package.IsEnabled)
				{
					var buttonText =
						item.Package is null ? Locale.Subscribe :
						Type is Enums.ReportType.Successors or Enums.ReportType.Alternatives ? Locale.Switch :
						Locale.Enable;

					var buttonIcon = ImageManager.GetIcon(
						item.Package is null ? "I_Add" :
						Type is Enums.ReportType.Successors or Enums.ReportType.Alternatives ? "I_Switch" :
						"I_Ok");

					buttonSize = SlickButton.GetSize(e.Graphics, buttonIcon, buttonText, UI.Font(8.25F));

					_buttonRects[item] = rect.Align(buttonSize, ContentAlignment.BottomRight);

					SlickButton.DrawButton(e, _buttonRects[item], buttonText, UI.Font(8.25F), buttonIcon, null, _buttonRects[item].Contains(cursor) ? (HoverState & ~HoverState.Focused) : HoverState.Normal, ColorStyle.Green);
				}

				rect.Y += rect.Height + pad;
			}

			y = rect.Y;
		}

		Cursor = actionHovered ? Cursors.Hand : Cursors.Default;
		Height = Math.Max(iconRect.Height, y);
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);

		if (e.Button != MouseButtons.Left)
		{
			return;
		}

		foreach (var item in _buttonRects)
		{
			if (item.Value.Contains(e.Location))
			{
				Clicked(item.Key, true);
				return;
			}
		}

		foreach (var item in _modRects)
		{
			if (item.Value.Contains(e.Location))
			{
				if (item.Key.Package is not null)
				{
					(FindForm() as BasePanelForm)?.PushPanel(null, new PC_PackagePage(item.Key.Package.Package));
				}
				else
				{
					Clicked(item.Key, false);
				}

				return;
			}
		}
	}

	private async void Clicked(SmallMod item, bool button)
	{
		if (item.Package is null)
		{
			try
			{
				if (button)
				{
					_subscribingTo.Add(item.SteamId);

					Loading = true;

					await CitiesManager.Subscribe(new[] { item.SteamId });
				}
				else
				{
					Process.Start($"https://steamcommunity.com/workshop/filedetails/?id={item.SteamId}");
				}
			}
			catch { } 

			return;
		}

		item.Package.IsIncluded = true;
		item.Package.IsEnabled = true;

		if (Type is Enums.ReportType.Successors or Enums.ReportType.Alternatives)
		{
			if (PackageCompatibilityReportControl.Package.Mod is not null)
			{
				PackageCompatibilityReportControl.Package.Mod.IsIncluded = false;
				PackageCompatibilityReportControl.Package.Mod.IsIncluded = false;
			}
		}

		PackageCompatibilityReportControl.Reset();
	}

	class SmallMod
	{
		public Bitmap? Icon { get; set; }
		public string Name { get; set; }
		public ulong SteamId { get; set; }
		public Domain.Mod? Package { get; set; }

		public SmallMod(ulong steamId, string name, Bitmap? icon)
		{
			Icon = icon;
			Name = name;
			SteamId = steamId;
		}
	}
}