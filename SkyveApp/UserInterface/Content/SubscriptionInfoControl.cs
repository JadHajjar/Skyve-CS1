﻿using Extensions;

using SkyveApp.Utilities;
using SkyveApp.Utilities.Managers;

using SlickControls;

using System;
using System.Drawing;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Content;
internal class SubscriptionInfoControl : SlickControl
{
	private readonly Timer refreshTimer;
	private Rectangle buttonRect;
	private Rectangle cancelRect;

	public SubscriptionInfoControl()
	{
		Visible = false;
		refreshTimer = new Timer() { Interval = 1000 };
		refreshTimer.Tick += RefreshTimer_Tick;
		refreshTimer.Enabled = true;
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);

		if (cancelRect.Contains(e.Location))
		{
			Cursor = Cursors.Hand;
			SlickTip.SetTo(this, "CancelSubs");
		}
		else if (buttonRect.Contains(e.Location))
		{
			Cursor = Cursors.Hand;
			SlickTip.SetTo(this, "ApplySubs");
		}
		else
		{
			Cursor = Cursors.Default;
			SlickTip.SetTo(this, "SubsPendingInfo");
		}
	}

	private void RefreshTimer_Tick(object sender, EventArgs e)
	{
		if (Visible != SubscriptionsManager.SubscriptionsPending)
		{
			Visible = !Visible;

			Program.MainForm.TryInvoke(() => Program.MainForm.Invalidate(true));
		}

		if (Visible)
		{
			Invalidate();
		}
	}

	protected override void UIChanged()
	{
		Padding = UI.Scale(new Padding(5), UI.FontScale);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.SetUp(BackColor);

		e.Graphics.FillRoundedRectangle(new SolidBrush(Color.FromArgb(100, FormDesign.Design.GreenColor)), ClientRectangle, Padding.Left);

		var y = Padding.Top;

		if (SubscriptionsManager.SubscribingTo.Count > 0)
		{
			e.Graphics.DrawStringItem(Locale.PendingSubscribeTo.FormatPlural(SubscriptionsManager.SubscribingTo.Count), Font, FormDesign.Design.MenuForeColor, Width - Padding.Horizontal, 0, ref y);
		}

		if (SubscriptionsManager.UnsubscribingFrom.Count > 0)
		{
			e.Graphics.DrawStringItem(Locale.PendingUnsubscribeFrom.FormatPlural(SubscriptionsManager.UnsubscribingFrom.Count), Font, FormDesign.Design.MenuForeColor, Width - Padding.Horizontal, 0, ref y);
		}

		using var buttonIcon = IconManager.GetSmallIcon("I_AppIcon");
		var buttonSize = SlickButton.GetSize(e.Graphics, buttonIcon, LocaleSlickUI.Apply, UI.Font(6.75F), new(4, 2, 2, 2));
		buttonRect = ClientRectangle.Pad(Padding).Align(buttonSize, ContentAlignment.BottomRight);

		SlickButton.DrawButton(e, buttonRect, LocaleSlickUI.Apply, UI.Font(6.75F), buttonIcon, new Padding(4, 2, 2, 2), buttonRect.Contains(PointToClient(Cursor.Position)) ? (HoverState & ~HoverState.Focused) : (HoverState & HoverState.Focused), ColorStyle.Green);

		using var cancelButtonIcon = IconManager.GetSmallIcon("I_Cancel");
		buttonSize = SlickButton.GetSize(e.Graphics, cancelButtonIcon, LocaleSlickUI.Cancel, UI.Font(6.75F), new(4, 2, 2, 2));
		cancelRect = ClientRectangle.Pad(Padding).Align(buttonSize, ContentAlignment.BottomLeft);

		SlickLabel.DrawLabel(e, cancelRect, LocaleSlickUI.Cancel, UI.Font(6.75F), cancelButtonIcon, new Padding(4, 2, 2, 2), FormDesign.Design.MenuForeColor, cancelRect.Contains(PointToClient(Cursor.Position)) ? (HoverState & ~HoverState.Focused) : HoverState.Normal, ColorStyle.Red);

		Height = y + buttonSize.Height + Padding.Bottom;
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);

		if (e.Button == MouseButtons.None || (e.Button == MouseButtons.Left && buttonRect.Contains(e.Location)))
		{
			CitiesManager.RunStub();
		}
		else if (e.Button == MouseButtons.Left && cancelRect.Contains(e.Location))
		{
			SubscriptionsManager.CancelPendingItems();
		}
	}
}
