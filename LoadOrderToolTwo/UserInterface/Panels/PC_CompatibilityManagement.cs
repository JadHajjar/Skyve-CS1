using Extensions;

using LoadOrderToolTwo.Domain.Compatibility;
using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.UserInterface.Content;
using LoadOrderToolTwo.UserInterface.Forms;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace LoadOrderToolTwo.UserInterface.Panels;
public partial class PC_CompatibilityManagement : PanelContent
{
	private readonly ulong _userId;
	private readonly Dictionary<ulong, IPackage> _packages = new();
	private int currentPage;
	private IPackage? currentPackage;

	private PC_CompatibilityManagement(bool load) : base(load)
	{
		InitializeComponent();
	}

	public PC_CompatibilityManagement() : this(true)
	{
		PB_Loading.BringToFront();
	}

	public PC_CompatibilityManagement(ulong userId) : this(false)
	{
		_userId = userId;

		foreach (var package in CentralManager.Packages)
		{
			if (package.Author?.SteamId == userId.ToString())
				_packages[package.SteamId] = package;
		}

		OnDataLoad();
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		TLP_Main.Padding = UI.Scale(new Padding(5, 0, 5, 5), UI.FontScale);
		B_Previous.Margin = B_Apply.Margin = UI.Scale(new Padding(0, 5, 0, 0), UI.FontScale);
		B_Skip.Margin = UI.Scale(new Padding(10, 5, 0, 0), UI.FontScale);
		P_Main.Padding = UI.Scale(new Padding(7), UI.FontScale);
		PB_Icon.Width = TLP_Top.Height = (int)(128 * UI.FontScale);
		I_Note.Size = UI.Scale(new Size(24, 24), UI.FontScale);
		I_Note.Padding = UI.Scale(new Padding(5), UI.FontScale);

		foreach (Control control in TLP_MainInfo.Controls)
		{
			control.Margin = UI.Scale(new Padding(5), UI.FontScale);
		}

		B_AddInteraction.Padding = B_AddStatus.Padding = UI.Scale(new Padding(15), UI.FontScale);
		B_AddInteraction.Font = B_AddStatus.Font = UI.Font(9.75F);
		B_AddInteraction.Margin = B_AddStatus.Margin = UI.Scale(new Padding(50,40,0,0), UI.UIScale);

		TB_Note.MinimumSize = UI.Scale(new Size(275, 100), UI.UIScale);
		P_Tags.MinimumSize = P_Links.MinimumSize = UI.Scale(new Size(275, 00), UI.UIScale);
	}

	protected override void DesignChanged(FormDesign design)
	{
		base.DesignChanged(design);

		P_Main.BackColor = design.AccentBackColor;
		P_Tags.BackColor = P_Links.BackColor = design.BackColor;
	}

	public override bool CanExit(bool toBeDisposed)
	{
		return !toBeDisposed
			|| currentPage < 0 
			|| currentPage >= _packages.Count
			|| ShowPrompt("Are you sure you want to conclude your session?", PromptButtons.YesNo, PromptIcons.Question) == DialogResult.Yes;
	}

	protected override async Task<bool> LoadDataAsync()
	{
		PB_Loading.Loading = true;

		var mods = await SteamUtil.QueryFilesAsync(Domain.Steam.SteamQueryOrder.RankedByLastUpdatedDate, requiredTags: new[] { "Mod" }, all: true);

		foreach (var mod in mods)
		{
			_packages.Add(mod.Key, mod.Value);
		}

		return true;
	}

	protected override void OnDataLoad()
	{
		SetPackage(_packages.Count - 1);
	}

	protected override void OnLoadFail()
	{
		ShowPrompt("Failed to load data, try again later", PromptButtons.OK, PromptIcons.Error);
		Form.PushBack();
	}

	private void SetPackage(int page)
	{
		if (page < 0 || page >= _packages.Count)
		{
			Form.PushBack();
			return;
		}

		currentPage = page;
		currentPackage = _packages.Values.OrderByDescending(x => x.ServerTime).ElementAt(page);

		PB_Icon.Package = currentPackage;
		PB_Icon.Image = null;
		PB_Icon.LoadImage(currentPackage.IconUrl, ImageManager.GetImage);
		P_Info.SetPackage(currentPackage, null);

		B_Skip.Enabled = currentPage > 0;
		B_Previous.Enabled = currentPage != _packages.Count - 1;
	}

	private void B_Skip_Click(object sender, EventArgs e)
	{
		SetPackage(currentPage-1);
	}

	private void B_Previous_Click(object sender, EventArgs e)
	{
		SetPackage(currentPage+1);
	}

	private void T_NewTag_Click(object sender, EventArgs e)
	{
		var prompt = ShowInputPrompt("Add a global tag");

		if (prompt.DialogResult == DialogResult.OK)
		{
			var control = new TagControl { Text = prompt.Input };
			control.Click += TagControl_Click;
			P_Tags.Controls.Add(control);
			T_NewTag.SendToBack();
		}
	}

	private void TagControl_Click(object sender, EventArgs e)
	{
		(sender as Control)?.Dispose();
	}

	private void T_NewLink_Click(object sender, EventArgs e)
	{
		var form = new AddLinkForm(currentPackage?.GetCompatibilityInfo().Links ?? new());
		
		form.Show(Form);

		form.LinksReturned += SetLinks;
	}

	private void SetLinks(IEnumerable<PackageLink> links)
	{

	}

	private void B_AddStatus_Click(object sender, EventArgs e)
	{
		var control = new IPackageStatusControl<StatusType>();

		FLP_Statuses.Controls.Add(control);
		B_AddStatus.SendToBack();
	}

	private void B_AddInteraction_Click(object sender, EventArgs e)
	{
		var control = new IPackageStatusControl<InteractionType>();

		FLP_Interactions.Controls.Add(control);
		B_AddInteraction.SendToBack();
	}

	private void B_Apply_Click(object sender, EventArgs e)
	{
		SetPackage(currentPage - 1);
	}

	private void I_Note_Click(object sender, EventArgs e)
	{
		TB_Note.Visible = I_Note.Selected = !I_Note.Selected;
	}
}
