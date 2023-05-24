using Extensions;

using SkyveApp.Domain.Compatibility;
using SkyveApp.Domain.Interfaces;
using SkyveApp.UserInterface.CompatibilityReport;
using SkyveApp.Utilities;
using SkyveApp.Utilities.Managers;

using SlickControls;

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Panels;
public partial class PC_RequestReview : PanelContent
{
	private IPackageStatusControl<StatusType, PackageStatus>? statusControl;
	private IPackageStatusControl<InteractionType, PackageInteraction>? iteractionControl;

	public PC_RequestReview(IPackage package)
	{
		CurrentPackage = package;

		InitializeComponent();

		PB_Icon.Package = CurrentPackage;
		PB_Icon.Image = null;
		PB_Icon.LoadImage(CurrentPackage.IconUrl, ImageManager.GetImage);
		P_Info.SetPackage(CurrentPackage, null);
	}

	public IPackage CurrentPackage { get; }

	protected override void LocaleChanged()
	{
		Text = LocaleCR.RequestReview;
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		P_Main.Padding = UI.Scale(new Padding(7), UI.FontScale);
		PB_Icon.Width = TLP_Top.Height = (int)(128 * UI.FontScale);
		B_Apply.Margin = B_Apply.Padding = TB_Note.Margin = UI.Scale(new Padding(5), UI.FontScale);
		foreach (Control item in TLP_MainInfo.Controls)
		{
			item.Margin = UI.Scale(new Padding(5), UI.FontScale);
		}

		B_AddInteraction.Padding = B_AddStatus.Padding = UI.Scale(new Padding(15), UI.FontScale);
		B_AddInteraction.Font = B_AddStatus.Font = UI.Font(9.75F);
		B_AddInteraction.Margin = B_AddStatus.Margin = UI.Scale(new Padding(50, 40, 0, 0), UI.UIScale);

		TB_Note2.MinimumSize = TB_Note.MinimumSize = UI.Scale(new Size(0, 100), UI.UIScale);
	}

	protected override void DesignChanged(FormDesign design)
	{
		base.DesignChanged(design);

		P_Main.BackColor = design.AccentBackColor;
	}

	private void B_ReportIssue_Click(object sender, EventArgs e)
	{
		TLP_Actions.Hide();
		TLP_Button.Show();
		TLP_MainInfo.Show();
		P_Main.Show();
	}

	private void B_AddStatus_Click(object sender, EventArgs e)
	{
		tableLayoutPanel1.Controls.Add(statusControl = new IPackageStatusControl<StatusType, PackageStatus>(CurrentPackage) { Margin = TB_Note.Margin }, 0, 0);

		TLP_Button.Show();
		tableLayoutPanel1.Show();
		P_Main.Show();
		TLP_Actions.Hide();
	}

	private void B_AddInteraction_Click(object sender, EventArgs e)
	{
		tableLayoutPanel1.Controls.Add(iteractionControl = new IPackageStatusControl<InteractionType, PackageInteraction>(CurrentPackage) { Margin = TB_Note.Margin }, 0, 0);

		TLP_Button.Show();
		tableLayoutPanel1.Show();
		P_Main.Show();
		TLP_Actions.Hide();
	}

	private async void B_Apply_Click(object sender, EventArgs e)
	{
		if (B_Apply.Loading)
		{
			return;
		}

		B_Apply.Loading = true;

		var postPackage = new ReviewRequest
		{
			PackageId = CurrentPackage!.SteamId,
			PackageNote = TB_Note.Text
		};

		if (statusControl is not null)
		{
			postPackage.IsStatus = true;
			postPackage.StatusNote = statusControl.PackageStatus.Note;
			postPackage.StatusAction = (int)statusControl.PackageStatus.Action;
			postPackage.StatusPackages = statusControl.PackageStatus.Packages!.ListStrings(",");
			postPackage.StatusType = (int)statusControl.PackageStatus.Type;
		}
		else if (iteractionControl is not null)
		{
			postPackage.IsInteraction = true;
			postPackage.StatusNote = iteractionControl.PackageStatus.Note;
			postPackage.StatusAction = (int)iteractionControl.PackageStatus.Action;
			postPackage.StatusPackages = iteractionControl.PackageStatus.Packages!.ListStrings(",");
			postPackage.StatusType = (int)iteractionControl.PackageStatus.Type;
		}
		else
		{
			postPackage.PackageStability = (int)DD_Stability.SelectedItem;
			postPackage.PackageType = (int)DD_PackageType.SelectedItem;
			postPackage.PackageUsage = (int)DD_Usage.SelectedItems.Aggregate((prev, next) => prev | next);
			postPackage.RequiredDLCs = DD_DLCs.SelectedItems.Select(x => x.Id).ListStrings(",");
		}

		postPackage.LogFile = await Task.Run(() =>
		{
			using var stream = new MemoryStream();

			LogUtil.CreateZipToStream(stream);

			return stream.ToArray();
		});

		var response = await CompatibilityApiUtil.SendReviewRequest(postPackage);

		if (response.Success)
		{
			PushBack();
			ShowPrompt(Locale.ReviewRequestSent.Format(CurrentPackage.CleanName()), PromptButtons.OK, PromptIcons.Info);
		}
		else
		{
			ShowPrompt(Locale.ReviewRequestFailed.Format(CurrentPackage.CleanName(), response.Message), PromptButtons.OK, PromptIcons.Info);
		}

		B_Apply.Loading = false;
	}
}
