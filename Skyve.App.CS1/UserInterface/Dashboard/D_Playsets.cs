using Skyve.App.Interfaces;
using Skyve.App.UserInterface.Dashboard;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Skyve.App.CS2.UserInterface.Dashboard;

internal class D_Playsets : IDashboardItem
{
	private readonly IPlaysetManager _playsetManager;
	private readonly INotifier _notifier;
	private readonly ICitiesManager _citiesManager;
	private bool isCitiesRunning;
	private bool loadingFromGameLaunch;
	private bool isCitiesAvailable;

	public D_Playsets()
	{
		ServiceCenter.Get(out _playsetManager, out _notifier, out _citiesManager);

		_notifier.PlaysetChanged += _notifier_PlaysetChanged;
		_notifier.PlaysetUpdated += _notifier_PlaysetUpdated;

		_citiesManager.MonitorTick += CitiesManager_MonitorTick;
		_citiesManager.LaunchingStatusChanged += CitiesManager_LaunchingStatusChanged;

		Loading = !_notifier.PlaysetsLoaded;
	}

	protected override void Dispose(bool disposing)
	{
		_notifier.PlaysetChanged -= _notifier_PlaysetChanged;
		_notifier.PlaysetUpdated -= _notifier_PlaysetUpdated;
		_citiesManager.MonitorTick -= CitiesManager_MonitorTick;
		_citiesManager.LaunchingStatusChanged -= CitiesManager_LaunchingStatusChanged;

		base.Dispose(disposing);
	}

	private void CitiesManager_LaunchingStatusChanged(bool obj)
	{
		this.TryInvoke(() =>
		{
			Loading = obj || !_notifier.PlaysetsLoaded;
			loadingFromGameLaunch = obj;
			Enabled = !obj;
		});
	}

	private void CitiesManager_MonitorTick(bool isAvailable, bool isRunning)
	{
		isCitiesAvailable = isAvailable;

		if (isCitiesRunning != isRunning)
		{
			isCitiesRunning = isRunning;

			OnResizeRequested();
		}
	}

	private void _notifier_PlaysetUpdated()
	{
		Loading = !_notifier.PlaysetsLoaded;

		this.TryInvoke(OnResizeRequested);
	}

	private void _notifier_PlaysetChanged()
	{
		Loading = !_notifier.PlaysetsLoaded;

		this.TryInvoke(() =>
		{
			Enabled = true;
			OnResizeRequested();
		});
	}

	private void SwitchTo(ICustomPlayset item)
	{
		Enabled = false;
		Loading = true;
		_playsetManager.SetCurrentPlayset(item);
		OnResizeRequested();
	}

	private void RightClick(IPlayset? playset)
	{
		if (playset is not null)
		{
			//SlickToolStrip.Show(App.Program.MainForm, ServiceCenter.Get<ICustomPackageService>().GetRightClickMenuItems(playset, true));
		}
	}

	protected override DrawingDelegate GetDrawingMethod(int width)
	{
		if (width / UI.FontScale < 350)
		{
			return DrawHorizontal;
		}

		return DrawVertical;
	}

	private void DrawHorizontal(PaintEventArgs e, bool applyDrawing, ref int preferredHeight)
	{
		Draw(e, applyDrawing, ref preferredHeight, true);
	}

	private void DrawVertical(PaintEventArgs e, bool applyDrawing, ref int preferredHeight)
	{
		Draw(e, applyDrawing, ref preferredHeight, false);
	}

	protected override void DrawHeader(PaintEventArgs e, bool applyDrawing, ref int preferredHeight)
	{
		DrawSection(e, applyDrawing, ref preferredHeight, "Playsets", Locale.Playset.Plural);
	}

	protected override void DrawBackground(PaintEventArgs e, Rectangle clipRectangle)
	{
		var hasColor = _playsetManager.CurrentPlayset?.Color is not null;

		e.Graphics.FillRoundedRectangleWithShadow(clipRectangle, BorderRadius, Padding.Right, shadow: hasColor ? Color.FromArgb(10, _playsetManager.CurrentPlayset!.Color!.Value) : null, addOutline: hasColor || !MovementBlocked);
	}

	private void Draw(PaintEventArgs e, bool applyDrawing, ref int preferredHeight, bool horizontal)
	{
		if (Loading && !loadingFromGameLaunch)
		{
			DrawLoadingSection(e, applyDrawing, ref preferredHeight, Locale.Playset.Plural);
		}
		else
		{
			DrawSection(e, applyDrawing, ref preferredHeight, _playsetManager.CurrentPlayset.Name ?? Locale.TemporaryPlayset, _playsetManager.CurrentPlayset.GetIcon() ?? "Playsets", _playsetManager.CurrentPlayset is null ? null : Locale.CurrentPlayset);
		}

		_buttonRightClickActions[headerRectangle] = () => RightClick(_playsetManager.CurrentPlayset);

		using var fontSmall = UI.Font(6.75F);

		DrawButton(e, applyDrawing, ref preferredHeight, !isCitiesAvailable ? null : (App.Program.MainForm as MainForm)!.LaunchStopCities, new ButtonDrawArgs
		{
			Text = LocaleHelper.GetGlobalText(isCitiesRunning ? "StopCities" : "StartCities"),
			Rectangle = e.ClipRectangle.Pad(Margin),
			Icon = isCitiesRunning ? "Stop" : "CS",
			Padding = UI.Scale(new Padding(2)),
			Enabled = Enabled && isCitiesAvailable,
			Control = loadingFromGameLaunch ? this : null
		});

		if (_playsetManager.CurrentPlayset != null)
		{
			if (applyDrawing)
			{
				var backColor = _playsetManager.CurrentPlayset.Color ?? FormDesign.Design.BackColor.Tint(Lum: FormDesign.Design.IsDarkTheme ? 5 : -4, Sat: 5);
				using var colorBrush = Gradient(backColor, 2.5f);
				e.Graphics.FillRoundedRectangle(colorBrush, new Rectangle(e.ClipRectangle.X + Margin.Left, preferredHeight, e.ClipRectangle.Width - Margin.Horizontal, Margin.Top), Margin.Top / 2);
			}

			preferredHeight += Margin.Vertical;
		}

		var favs = _playsetManager.Playsets.AllWhere(x => x.IsFavorite);

		if (favs.Count == 0)
		{
			preferredHeight += Margin.Top / 2;
			return;
		}

		preferredHeight += Margin.Top;

		e.Graphics.DrawStringItem(Locale.FavoritePlaysets, fontSmall, Color.FromArgb(150, FormDesign.Design.ForeColor), e.ClipRectangle.Pad(Margin).Pad(UI.Scale(2), 0, 0, 0), ref preferredHeight, applyDrawing);

		preferredHeight -= Margin.Top;

		var preferredSize = horizontal ? 125 : 100;
		var columns = (int)Math.Max(1, Math.Floor((e.ClipRectangle.Width - Margin.Left) / (preferredSize * UI.FontScale)));
		var columnWidth = (e.ClipRectangle.Width - Margin.Left) / columns;
		var height = (horizontal ? 0 : columnWidth) + UI.Scale(35);

		for (var i = 0; i < favs.Count; i++)
		{
			if (i > 0 && (i % columns) == 0)
			{
				preferredHeight += height;
			}

			var rect = new Rectangle(e.ClipRectangle.X + (Margin.Left / 2) + (i % columns * columnWidth), preferredHeight, columnWidth, height);

			if (applyDrawing)
			{
				DrawPlayset(e, favs[i], rect.Pad(Margin.Left / 2), horizontal);
			}
		}

		preferredHeight += Margin.Top + height;
	}

	private void DrawPlayset(PaintEventArgs e, ICustomPlayset playset, Rectangle rect, bool horizontal)
	{
		_buttonActions[rect] = () => SwitchTo(playset);
		_buttonRightClickActions[rect] = () => RightClick(playset);

		var customPlayset = playset;
		Bitmap? banner = null;

		var backColor = customPlayset.Color ?? banner?.GetThemedAverageColor() ?? FormDesign.Design.BackColor.Tint(Lum: FormDesign.Design.IsDarkTheme ? 5 : -4, Sat: 5);
		using var backBrush = Gradient(backColor.MergeColor(BackColor, 80), rect, customPlayset.Color.HasValue ? 3F : 1F);

		e.Graphics.FillRoundedRectangleWithShadow(rect, Margin.Left / 2, UI.Scale(6), Color.Empty, playset.Equals(_playsetManager.CurrentPlayset) ? Color.FromArgb(10, FormDesign.Design.GreenColor) : Color.FromArgb(5, backColor.MergeColor(FormDesign.Design.ForeColor, 75)), playset.Equals(_playsetManager.CurrentPlayset));

		e.Graphics.FillRoundedRectangle(backBrush, rect, Margin.Left / 2);

		var bannerRect = horizontal ? rect.Pad(Margin.Left / 4).Align(new Size(rect.Height - (Margin.Top / 2), rect.Height - (Margin.Top / 2)), ContentAlignment.MiddleLeft) : rect.Pad(Margin.Left / 2).ClipTo(rect.Width - Margin.Left);
		var onBannerColor = backColor.GetTextColor();

		if (banner is null)
		{
			using var brush = new SolidBrush(Color.FromArgb(40, onBannerColor));

			e.Graphics.FillRoundedRectangle(brush, bannerRect, Margin.Left / 2);

			using var icon = customPlayset.Usage.GetIcon().Get(bannerRect.Width * 3 / 4).Color(onBannerColor);

			e.Graphics.DrawImage(icon, bannerRect.CenterR(icon.Size));
		}
		else
		{
			e.Graphics.DrawRoundedImage(banner, bannerRect, UI.Scale(5), backColor.MergeColor(BackColor, 80));
		}

		if (HoverState.HasFlag(HoverState.Hovered) && rect.Contains(CursorLocation))
		{
			using var brush = new SolidBrush(Color.FromArgb(30, onBannerColor));

			e.Graphics.FillRoundedRectangle(brush, rect, Margin.Left / 2);
		}

		var textRect = horizontal ? rect.Pad(Margin.Left / 2 + bannerRect.Width, Margin.Left / 2, Margin.Left / 2, Margin.Left / 2) : rect.Pad(Margin.Left / 2, bannerRect.Height + Margin.Top, Margin.Left / 2, Margin.Left / 2);
		using var textBrush = new SolidBrush(onBannerColor);
		using var textFont = UI.Font(horizontal ? 8.5F : 8.75F, FontStyle.Bold).FitTo(playset.Name, textRect, e.Graphics);
		using var centerFormat = new StringFormat { LineAlignment = StringAlignment.Center };

		if (horizontal)
			e.Graphics.DrawHighResText(playset.Name, textFont, textBrush, textRect, 2, horizontal ? centerFormat : null);
		else
		{
			e.Graphics.DrawString(playset.Name, textFont, textBrush, textRect, horizontal ? centerFormat : null);
		}
	}
}
