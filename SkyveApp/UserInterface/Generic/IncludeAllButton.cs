using Extensions;

using SkyveApp.Domain.Interfaces;
using SkyveApp.UserInterface.Lists;
using SkyveApp.Utilities.Managers;

using SlickControls;

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Generic;
internal class IncludeAllButton<T> : SlickControl where T : IPackage
{
	private readonly bool _doubleButtons;
	private readonly ItemListControl<T>? LC_Items;
	private Rectangle IncludedRect;
	private Rectangle EnabledRect;
	private Rectangle ActionRect;

	public event EventHandler? ActionClicked;
	public event EventHandler? IncludeAllClicked;
	public event EventHandler? ExcludeAllClicked;
	public event EventHandler? EnableAllClicked;
	public event EventHandler? DisableAllClicked;
	public event EventHandler? SubscribeAllClicked;

	public IncludeAllButton(ItemListControl<T>? lC_Items)
	{
		Margin = default;
		Cursor = Cursors.Hand;
		LC_Items = lC_Items;
		_doubleButtons = CentralManager.SessionSettings.UserSettings.AdvancedIncludeEnable;
	}

	protected override void UIChanged()
	{
		Margin = Padding = UI.Scale(new Padding(3, 2, 3, 2), UI.FontScale);

		var ItemHeight = (int)((CentralManager.SessionSettings.UserSettings.LargeItemOnHover ? 64 : 36) * UI.FontScale);
		var includeItemHeight = CentralManager.SessionSettings.UserSettings.LargeItemOnHover ? (ItemHeight / 2) : ItemHeight;

		Size = new Size((_doubleButtons ? (includeItemHeight * 2 * 9 / 10) : (includeItemHeight + 1)) + includeItemHeight, includeItemHeight * 2 / 3);
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);

		if (e.Button == MouseButtons.Left)
		{
			var packages = LC_Items?.SafeGetItems() ?? new();
			var subscribe = packages.Any(x => x.Item.Package is null);

			if (IncludedRect.Contains(e.Location))
			{
				if (subscribe)
				{
					SubscribeAllClicked?.Invoke(this, e);
				}
				else if (packages.All(x => x.Item.IsIncluded))
				{
					ExcludeAllClicked?.Invoke(this, e);
				}
				else
				{
					IncludeAllClicked?.Invoke(this, e);
				}
			}
			else if (EnabledRect.Contains(e.Location))
			{
				if (subscribe)
				{
					SubscribeAllClicked?.Invoke(this, e);
				}
				else if (packages.All(x => x.Item.Package?.Mod?.IsEnabled ?? true))
				{
					DisableAllClicked?.Invoke(this, e);
				}
				else
				{
					EnableAllClicked?.Invoke(this, e);
				}
			}
			else if (ActionRect.Contains(e.Location))
			{
				ActionClicked?.Invoke(this, e);
			}
		}
		else if (e.Button == MouseButtons.Right)
		{
			ActionClicked?.Invoke(this, e);
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.SetUp(BackColor);

		var ItemHeight = (int)((CentralManager.SessionSettings.UserSettings.LargeItemOnHover ? 64 : 36) * UI.FontScale);
		var includeItemHeight = CentralManager.SessionSettings.UserSettings.LargeItemOnHover ? (ItemHeight / 2) : ItemHeight;
		var rectangle = ClientRectangle;
		var CursorLocation = PointToClient(Cursor.Position);
		var color = FormDesign.Design.ActiveColor;
		var packages = LC_Items?.SafeGetItems() ?? new();
		var subscribe = packages.Any(x => x.Item.Package is null);

		if (_doubleButtons && !subscribe)
		{
			IncludedRect = rectangle.Pad(0, 0, 0, 0).Align(new Size(includeItemHeight * 9 / 10, includeItemHeight * 2 / 3), ContentAlignment.MiddleLeft);
			EnabledRect = IncludedRect.Pad(IncludedRect.Width, 0, -IncludedRect.Width, 0);
			ActionRect = EnabledRect.Pad(EnabledRect.Width, 0, -EnabledRect.Width, 0);
		}
		else
		{
			EnabledRect = default;
			IncludedRect = rectangle.Pad(0, 0, 0, 0).Align(new Size(includeItemHeight + 1, includeItemHeight * 2 / 3), ContentAlignment.MiddleLeft);
			ActionRect = IncludedRect.Pad(IncludedRect.Width, 0, -includeItemHeight, 0);
		}

		if (HoverState.HasFlag(HoverState.Hovered))
		{
			if (IncludedRect.Contains(CursorLocation))
			{
				color = packages.All(x => x.Item.IsIncluded) ? FormDesign.Design.RedColor : FormDesign.Design.GreenColor;
			}
			else if (EnabledRect.Contains(CursorLocation))
			{
				color = packages.All(x => x.Item.Package?.Mod?.IsEnabled ?? true) ? FormDesign.Design.RedColor : FormDesign.Design.GreenColor;
			}
		}

		var incl = new DynamicIcon(subscribe ? "I_Add"
		: packages.All(x => x.Item.IsIncluded) ? "I_Ok" : "I_Enabled");
		var inclIcon = CentralManager.SessionSettings.UserSettings.LargeItemOnHover ? incl.Large : incl.Get(includeItemHeight / 2);

		if (HoverState.HasFlag(HoverState.Hovered) && IncludedRect.Contains(CursorLocation))
		{
			using var brush1 = IncludedRect.Gradient(HoverState.HasFlag(HoverState.Pressed) ? color : Color.FromArgb(30, ForeColor), 1.5F);
			e.Graphics.FillRoundedRectangle(brush1, IncludedRect, 4);
		}
		e.Graphics.DrawImage(inclIcon.Color(!IncludedRect.Contains(CursorLocation) ? ForeColor : HoverState.HasFlag(HoverState.Pressed) ? FormDesign.Design.ActiveForeColor : color), IncludedRect.CenterR(inclIcon.Size));

		if (_doubleButtons && EnabledRect != default)
		{
			var enl = new DynamicIcon(packages.All(x => x.Item.Package?.Mod?.IsEnabled ?? true) ? "I_Checked" : "I_Checked_OFF");
			var enlIcon = CentralManager.SessionSettings.UserSettings.LargeItemOnHover ? enl.Large : enl.Get(includeItemHeight / 2);

			if (HoverState.HasFlag(HoverState.Hovered) && EnabledRect.Contains(CursorLocation))
			{
				using var brush2 = EnabledRect.Gradient(HoverState.HasFlag(HoverState.Pressed) ? color : Color.FromArgb(30, ForeColor), 1.5F);
				e.Graphics.FillRoundedRectangle(brush2, EnabledRect, 4);
			}
			e.Graphics.DrawImage(enlIcon.Color(!EnabledRect.Contains(CursorLocation) ? ForeColor : HoverState.HasFlag(HoverState.Pressed) ? FormDesign.Design.ActiveForeColor : color), EnabledRect.CenterR(enlIcon.Size));
		}

		var action = new DynamicIcon("I_Actions");
		var actionIcon = CentralManager.SessionSettings.UserSettings.LargeItemOnHover ? action.Large : action.Get(includeItemHeight / 2);

		if (HoverState.HasFlag(HoverState.Hovered) && ActionRect.Contains(CursorLocation))
		{
			using var brush3 = ActionRect.Gradient(HoverState.HasFlag(HoverState.Pressed) ? color : Color.FromArgb(30, ForeColor), 1.5F);
			e.Graphics.FillRoundedRectangle(brush3, ActionRect, 4);
		}
		e.Graphics.DrawImage(actionIcon.Color(!ActionRect.Contains(CursorLocation) ? ForeColor : HoverState.HasFlag(HoverState.Pressed) ? FormDesign.Design.ActiveForeColor : color), ActionRect.CenterR(actionIcon.Size));
	}
}
