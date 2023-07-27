using System.Drawing;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Content;
internal class TroubleshootInfoControl : SlickControl
{
	private Rectangle buttonRect;
	private Rectangle cancelRect;

	private readonly ITroubleshootSystem _troubleshootSystem;

	public TroubleshootInfoControl()
	{
		var system = ServiceCenter.Get<ITroubleshootSystem>();

		_troubleshootSystem = system;

		if (system != null)
		{
			system.AskForConfirmation += AskForConfirmation;
			system.StageChanged += CheckVisibility;
			system.PromptResult += PromptResult;
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);

		if (cancelRect.Contains(e.Location))
		{
			Cursor = Cursors.Hand;
			SlickTip.SetTo(this, "CancelDownloads");
		}
		else if (buttonRect.Contains(e.Location))
		{
			Cursor = Cursors.Hand;
			SlickTip.SetTo(this, "ApplyDownloads");
		}
		else
		{
			Cursor = Cursors.Default;
			SlickTip.SetTo(this, "DownloadsPendingInfo");
		}
	}

	private async void AskForConfirmation()
	{
		await _troubleshootSystem.ApplyConfirmation(		MessagePrompt.Show("Was the issue fixed?", PromptButtons.YesNo, PromptIcons.Question, form: Program.MainForm) == DialogResult.No );
	}

	private void PromptResult(List<ILocalPackage> list)
	{
		MessagePrompt.Show("The following packages might be the cause of your issues:\r\n\r\n" + list.ListStrings("\r\n"), icon: PromptIcons.Ok, form: Program.MainForm);
	}

	protected override void OnCreateControl()
	{
		base.OnCreateControl();

		if (Live)
			CheckVisibility();
	}

	private void CheckVisibility()
	{
		if (Visible != _troubleshootSystem.IsInProgress)
		{
			this.TryInvoke(() => Visible = _troubleshootSystem.IsInProgress);
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
		if (!Live)
			return;

		e.Graphics.SetUp(BackColor);

		e.Graphics.FillRoundedRectangle(new SolidBrush(Color.FromArgb(75, FormDesign.Design.RedColor)), ClientRectangle, Padding.Left);

		var y = Padding.Top;

		e.Graphics.DrawStringItem(_troubleshootSystem.CurrentAction, Font, FormDesign.Design.MenuForeColor, Width - Padding.Horizontal, 0, ref y);

		e.Graphics.DrawStringItem($"Stage {_troubleshootSystem.CurrentStage} / {_troubleshootSystem.TotalStages}", Font, FormDesign.Design.MenuForeColor, Width - Padding.Horizontal, 0, ref y);

		using var buttonIcon = IconManager.GetSmallIcon("I_ArrowRight");
		var buttonSize = SlickButton.GetSize(e.Graphics, buttonIcon, "Next Stage", UI.Font(6.75F), new(4, 2, 2, 2));
		buttonRect = ClientRectangle.Pad(Padding).Align(buttonSize, ContentAlignment.BottomRight);

		SlickButton.DrawButton(e, buttonRect, "Next Stage", UI.Font(6.75F), buttonIcon, new Padding(4, 2, 2, 2), buttonRect.Contains(PointToClient(Cursor.Position)) ? (HoverState & ~HoverState.Focused) : (HoverState & HoverState.Focused), ColorStyle.Green);

		using var cancelButtonIcon = IconManager.GetSmallIcon("I_Cancel");
		buttonSize = SlickButton.GetSize(e.Graphics, cancelButtonIcon, LocaleSlickUI.Cancel, UI.Font(6.75F), new(4, 2, 2, 2));
		cancelRect = ClientRectangle.Pad(Padding).Align(buttonSize, ContentAlignment.BottomLeft);

		SlickLabel.DrawLabel(e, cancelRect, LocaleSlickUI.Cancel, UI.Font(6.75F), cancelButtonIcon, new Padding(4, 2, 2, 2), FormDesign.Design.MenuForeColor, cancelRect.Contains(PointToClient(Cursor.Position)) ? (HoverState & ~HoverState.Focused) : HoverState.Normal, ColorStyle.Red);

		Height = y + buttonSize.Height + Padding.Bottom;
	}

	protected override async void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);

		if (e.Button == MouseButtons.None || (e.Button == MouseButtons.Left && buttonRect.Contains(e.Location)))
		{
			await _troubleshootSystem.NextStage();
		}
		else if (e.Button == MouseButtons.Left && cancelRect.Contains(e.Location))
		{
			await _troubleshootSystem.Stop();
		}
	}
}
