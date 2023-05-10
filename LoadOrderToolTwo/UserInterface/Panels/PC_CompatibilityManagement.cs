using Extensions;

using LoadOrderToolTwo.Domain.Compatibility;
using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.Domain.Steam;
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

namespace LoadOrderToolTwo.UserInterface.Panels;
public partial class PC_CompatibilityManagement : PanelContent
{
	private readonly Dictionary<ulong, IPackage> _packages = new();
	private int currentPage;
	private IPackage? currentPackage;
	private PostPackage? postPackage;

	private PC_CompatibilityManagement(bool load) : base(load)
	{
		InitializeComponent();

		if (!load)
		{
			CB_BlackListId.Visible = CB_BlackListName.Visible = false;
		}
	}

	public PC_CompatibilityManagement() : this(true)
	{
		PB_Loading.BringToFront();
	}

	public PC_CompatibilityManagement(ulong userId) : this(false)
	{
		foreach (var package in CentralManager.Packages)
		{
			if (package.Author?.SteamId == userId.ToString())
			{
				_packages[package.SteamId] = package;
			}
		}

		OnDataLoad();
	}

	public PC_CompatibilityManagement(IPackage package) : this(false)
	{
		_packages[package.SteamId] = package;

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
		B_AddInteraction.Margin = B_AddStatus.Margin = UI.Scale(new Padding(50, 40, 0, 0), UI.UIScale);

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

	private async void SetPackage(int page)
	{
		if (page < 0 || page >= _packages.Count)
		{
			Form.PushBack();
			return;
		}

		currentPage = page;
		currentPackage = _packages.Values.OrderByDescending(x => x.ServerTime).ElementAt(page);

		var catalogue = await CompatibilityApiUtil.Catalogue(currentPackage.SteamId);

		postPackage = catalogue?.Packages.FirstOrDefault()?.CloneTo<Package, PostPackage>() ?? new();

		SetData(postPackage);

		PB_Icon.Package = currentPackage;
		PB_Icon.Image = null;
		PB_Icon.LoadImage(currentPackage.IconUrl, ImageManager.GetImage);
		P_Info.SetPackage(currentPackage, null);

		B_Skip.Enabled = currentPage > 0;
		B_Previous.Enabled = currentPage != _packages.Count - 1;
	}

	private void SetData(PostPackage postPackage)
	{
		CB_BlackListName.Checked = postPackage.BlackListName;
		CB_BlackListId.Checked = postPackage.BlackListId;
		DD_Stability.SelectedItem = postPackage.Stability;
		DD_Usage.SelectedItem = postPackage.Usage;
		TB_Note.Text = postPackage.Note;
		TB_Note.Visible = I_Note.Selected = !string.IsNullOrWhiteSpace(postPackage.Note);

		P_Tags.Controls.Clear(true, x => !string.IsNullOrEmpty(x.Text));
		P_Links.Controls.Clear(true, x => x is LinkControl);
		FLP_Statuses.Controls.Clear(true, x => x is IPackageStatusControl<StatusType, PackageStatus>);
		FLP_Interactions.Controls.Clear(true, x => x is IPackageStatusControl<InteractionType, PackageInteraction>);

		foreach (var item in postPackage.Tags ?? new())
		{
			var control = new TagControl { Text = item };
			control.Click += TagControl_Click;
			P_Tags.Controls.Add(control);
			T_NewTag.SendToBack();
		}

		foreach (var item in postPackage.Links ?? new())
		{
			var control = new LinkControl { Link = item };
			control.Click += TagControl_Click;
			P_Links.Controls.Add(control);
			T_NewLink.SendToBack();
		}

		foreach (var item in postPackage.Statuses ?? new())
		{
			var control = new IPackageStatusControl<StatusType, PackageStatus>(item);

			FLP_Statuses.Controls.Add(control);
			B_AddStatus.SendToBack();
		}

		foreach (var item in postPackage.Interactions ?? new())
		{
			var control = new IPackageStatusControl<InteractionType, PackageInteraction>(item);

			FLP_Interactions.Controls.Add(control);
			B_AddInteraction.SendToBack();
		}
	}

	private void B_Skip_Click(object sender, EventArgs e)
	{
		SetPackage(currentPage - 1);
	}

	private void B_Previous_Click(object sender, EventArgs e)
	{
		SetPackage(currentPage + 1);
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
		var control = new IPackageStatusControl<StatusType, PackageStatus>();

		FLP_Statuses.Controls.Add(control);
		B_AddStatus.SendToBack();
	}

	private void B_AddInteraction_Click(object sender, EventArgs e)
	{
		var control = new IPackageStatusControl<InteractionType, PackageInteraction>();

		FLP_Interactions.Controls.Add(control);
		B_AddInteraction.SendToBack();
	}

	private async void B_Apply_Click(object sender, EventArgs e)
	{
		if (B_Apply.Loading)
			return;

		B_Apply.Loading = true;

		postPackage!.SteamId = currentPackage!.SteamId;
		postPackage.FileName = currentPackage.Package?.Mod?.FileName;
		postPackage.Name = currentPackage.Name;
		postPackage.ReviewDate = DateTime.UtcNow;
		postPackage.AuthorId = ulong.TryParse(currentPackage.Author?.SteamId, out var id) ? id : 0;
		postPackage.Author = new Author
		{
			SteamId = postPackage.AuthorId,
			Name = currentPackage.Author?.Name,
		};

		postPackage.Stability = DD_Stability.SelectedItem;
		postPackage.Usage = DD_Usage.SelectedItem;
		postPackage.Note = I_Note.Selected ? string.Empty : TB_Note.Text;
		postPackage.Tags = P_Tags.Controls.OfType<TagControl>().Where(x => !string.IsNullOrEmpty(x.Text)).ToList(x => x.Text);
		postPackage.Links = P_Tags.Controls.OfType<LinkControl>().ToList(x => x.Link);
		postPackage.Statuses = FLP_Statuses.Controls.OfType<IPackageStatusControl<StatusType, PackageStatus>>().ToList(x => x.PackageStatus);
		postPackage.Interactions = FLP_Statuses.Controls.OfType<IPackageStatusControl<InteractionType, PackageInteraction>>().ToList(x => x.PackageStatus);

		await CompatibilityApiUtil.SaveEntry(postPackage);

		SetPackage(currentPage - 1);
	}

	private void I_Note_Click(object sender, EventArgs e)
	{
		TB_Note.Visible = I_Note.Selected = !I_Note.Selected;
	}
}
