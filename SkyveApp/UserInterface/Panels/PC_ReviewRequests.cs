using Extensions;

using SkyveApp.Domain.Compatibility;
using SkyveApp.Utilities;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Panels;
public partial class PC_ReviewRequests : PanelContent
{
	private readonly List<ReviewRequest> _reviewRequests;

	internal ulong CurrentPackage;

	public PC_ReviewRequests(ReviewRequest[] reviewRequests)
	{
		InitializeComponent();

		_reviewRequests = reviewRequests.ToList();

		packageCrList.SetItems(reviewRequests.Select(x => x.PackageId).Distinct());
		packageCrList.CanDrawItem += PackageCrList_CanDrawItem;

		TB_Search.Placeholder = $"{LocaleSlickUI.Search}..";
		Text = LocaleCR.ReviewRequests.Format($"({reviewRequests?.Length})");
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		TLP_List.Padding = UI.Scale(new Padding(5), UI.FontScale);
		TLP_List.Width = (int)(210 * UI.FontScale);
	}

	protected override void DesignChanged(FormDesign design)
	{
		base.DesignChanged(design);

	}

	private void packageCrList_ItemMouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			SetPackage((ulong)sender);
		}

		if (e.Button == MouseButtons.Right)
		{
			SlickToolStrip.Show(Form, packageCrList.PointToClient(e.Location), PC_PackagePage.GetRightClickMenuItems(SteamUtil.GetItem((ulong)sender)!));
		}
	}

	private void SetPackage(ulong sender)
	{
		CurrentPackage = sender;

		packageCrList.Invalidate();
		reviewRequestList1.SetItems(_reviewRequests.Where(x => x.PackageId == sender));
	}

	private void PackageCrList_CanDrawItem(object sender, CanDrawItemEventArgs<ulong> e)
	{
		var package = SteamUtil.GetItem(e.Item);

		if (package is null)
		{
			return;
		}

		e.DoNotDraw = !(TB_Search.Text.SearchCheck(package.ToString())
			|| TB_Search.Text.SearchCheck(package.Author?.Name)
			|| package.SteamId.ToString().IndexOf(TB_Search.Text, StringComparison.OrdinalIgnoreCase) != -1);
	}

	private void reviewRequestList1_ItemMouseClick(object sender, MouseEventArgs e)
	{
		Form.Invoke(() => Form.PushPanel(new PC_ReviewSingleRequest((ReviewRequest)sender)));
	}

	private void TB_Search_TextChanged(object sender, EventArgs e)
	{
		TB_Search.ImageName = string.IsNullOrWhiteSpace(TB_Search.Text) ? "I_Search" : "I_ClearSearch";

		packageCrList.FilterChanged();
	}

	private void TB_Search_IconClicked(object sender, EventArgs e)
	{
		TB_Search.Text = string.Empty;
	}
}
