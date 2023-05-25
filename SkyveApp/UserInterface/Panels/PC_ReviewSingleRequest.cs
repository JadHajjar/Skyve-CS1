using Extensions;

using SkyveApp.Domain.Compatibility;
using SkyveApp.UserInterface.CompatibilityReport;
using SkyveApp.UserInterface.Content;
using SkyveApp.UserInterface.Generic;
using SkyveApp.Utilities;

using SlickControls;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Panels;
public partial class PC_ReviewSingleRequest : PanelContent
{
	private ReviewRequest _request;
	private SlickControl logControl;

	public PC_ReviewSingleRequest(ReviewRequest request) : base(true)
	{
		InitializeComponent();

		_request = request;

		TLP_Info.Controls.Add(new MiniPackageControl(request.PackageId) { Dock = DockStyle.Top }, 0, 0);
		TLP_Info.Controls.Add(new SteamUserControl(request.UserId) { InfoText = "Requested by", Dock = DockStyle.Top, Margin = UI.Scale(new Padding(5), UI.FontScale) }, 0, 1);

		logControl = new SlickControl
		{
			Text = "Log Files.zip",
			Dock = DockStyle.Top,
			Height = (int)(50 * UI.UIScale),
			Margin = UI.Scale(new Padding(5), UI.FontScale)
		};

		logControl.Paint += PC_ReviewSingleRequest_Paint;

		TLP_Info.Controls.Add(logControl, 0, 2);

		label2.Text = request.PackageNote.IfEmpty("No description given");

		if (request.IsInteraction)
		{
			tableLayoutPanel1.Controls.Add(new IPackageStatusControl<InteractionType, PackageInteraction>(SteamUtil.GetItem(request.PackageId), new PackageInteraction
			{
				Action = (StatusAction)request.StatusAction,
				IntType = request.StatusType,
				Note = request.StatusNote,
				Packages = request.StatusPackages?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(ulong.Parse).ToArray(),
			})
			{ BackColor = FormDesign.Design.AccentBackColor }, 0, 3);
		}
		else if (request.IsStatus)
		{
			tableLayoutPanel1.Controls.Add(new IPackageStatusControl<StatusType, PackageStatus>(SteamUtil.GetItem(request.PackageId), new PackageStatus
			{
				Action = (StatusAction)request.StatusAction,
				IntType = request.StatusType,
				Note = request.StatusNote,
				Packages = request.StatusPackages?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(ulong.Parse).ToArray(),
			})
			{ BackColor = FormDesign.Design.AccentBackColor }, 0, 3);
		}
	}

	protected override async Task<bool> LoadDataAsync()
	{
		logControl.Loading = true;

		var request = await CompatibilityApiUtil.GetReviewRequest(_request.UserId, _request.PackageId);

		if (request != null)
		{
			_request = request;

			return true;
		}

		return false;
	}

	protected override void OnDataLoad()
	{
		logControl.Loading = false;
	}

	protected override void OnLoadFail()
	{
		logControl.Dispose();
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		tableLayoutPanel1.ColumnStyles[0].Width = (int)(200 * UI.FontScale);
		label3.Font=label1.Font = UI.Font(7.5F, FontStyle.Bold);
	}

	private void PC_ReviewSingleRequest_Paint(object sender, PaintEventArgs e)
	{
		var ctrl = (sender as SlickControl)!;

		e.Graphics.SetUp(ctrl.BackColor);

		if (ctrl.Loading)
		{
			ctrl.DrawLoader(e.Graphics, ctrl.ClientRectangle.CenterR(UI.Scale(new Size(24, 24), UI.UIScale)));
			return;
		}

		using var fileIcon = IconManager.GetLargeIcon("I_File").Color(FormDesign.Design.MenuForeColor);

		var Padding = ctrl.Margin;
		var textSize = e.Graphics.Measure(ctrl.Text, new Font(Font, FontStyle.Bold), ctrl.Width - Padding.Left);
		var fileHeight = (int)textSize.Height + 3 + fileIcon.Height + Padding.Top;
		var fileRect = ctrl.ClientRectangle;
		var fileContentRect = fileRect.CenterR(Math.Max((int)textSize.Width + 3, fileIcon.Width), fileHeight);

		e.Graphics.FillRoundedRectangle(new SolidBrush(ctrl.HoverState.HasFlag(HoverState.Hovered) ? FormDesign.Design.MenuColor.MergeColor(FormDesign.Design.ActiveColor) : FormDesign.Design.MenuColor), fileRect, Padding.Left);
		e.Graphics.DrawImage(fileIcon, fileContentRect.Align(fileIcon.Size, ContentAlignment.TopCenter));
		e.Graphics.DrawString(ctrl.Text, new Font(Font, FontStyle.Bold), new SolidBrush(FormDesign.Design.MenuForeColor), fileContentRect, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far });
	}
}
