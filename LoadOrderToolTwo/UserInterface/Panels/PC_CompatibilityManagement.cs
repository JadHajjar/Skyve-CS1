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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace LoadOrderToolTwo.UserInterface.Panels;
public partial class PC_CompatibilityManagement : PanelContent
{
	private readonly List<ulong> _pages = new List<ulong>();
	private readonly Dictionary<ulong, IPackage?> _packages = new();
	private int currentPage;
	private IPackage? currentPackage;
	private PostPackage? postPackage;
	private PostPackage? lastPackageData;

	internal IPackage? CurrentPackage => currentPackage;

	private PC_CompatibilityManagement(bool load) : base(load)
	{
		InitializeComponent();

		SlickTip.SetTo(P_Tags, "GlobalTagsInfo");
		SlickTip.SetTo(B_ReuseData, "ReuseData_Tip");
		T_Statuses.Text = LocaleCR.StatusesCount.Format(0);
		T_Interactions.Text = LocaleCR.InteractionCount.Format(0);
	}

	public PC_CompatibilityManagement() : this(true)
	{
		PB_Loading.BringToFront();
	}

	public PC_CompatibilityManagement(ulong userId) : this(false)
	{
		foreach (var package in CentralManager.Packages)
		{
			if (package.Author?.SteamId == userId)
			{
				_packages[package.SteamId] = package;
			}
		}

		_pages.AddRange(_packages.OrderByDescending(x => x.Value?.ServerTime).Select(x => x.Key));
		CB_BlackListId.Visible = CB_BlackListName.Visible = false;

		SetPackage(0);
	}

	public PC_CompatibilityManagement(IEnumerable<ulong> packages) : this(false)
	{
		foreach (var package in packages)
		{
			_packages[package] = SteamUtil.GetItem(package);
		}

		_pages.AddRange(_packages.OrderByDescending(x => x.Value?.ServerTime).Select(x => x.Key));

		SetPackage(0);
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		TLP_Main.Padding = UI.Scale(new Padding(5, 0, 5, 5), UI.FontScale);
		B_Skip.Margin = B_Previous.Margin = B_Apply.Margin = L_Page.Margin = B_ReuseData.Margin = UI.Scale(new Padding(1, 5, 1, 0), UI.FontScale);
		P_Main.Padding = UI.Scale(new Padding(7), UI.FontScale);
		PB_Icon.Width = TLP_Top.Height = (int)(128 * UI.FontScale);

		foreach (Control control in TLP_MainInfo.Controls)
		{
			control.Margin = UI.Scale(new Padding(5), UI.FontScale);
		}

		B_AddInteraction.Padding = B_AddStatus.Padding = UI.Scale(new Padding(15), UI.FontScale);
		B_AddInteraction.Font = B_AddStatus.Font = UI.Font(9.75F);
		B_AddInteraction.Margin = B_AddStatus.Margin = UI.Scale(new Padding(50, 40, 0, 0), UI.UIScale);

		TB_Note.MinimumSize = UI.Scale(new Size(0, 100), UI.UIScale);
		P_Tags.MinimumSize = P_Links.MinimumSize = UI.Scale(new Size(300, 00), UI.UIScale);
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
			|| currentPage <= 0
			|| currentPage >= _packages.Count - 1
			|| ShowPrompt(LocaleCR.ConfirmEndSession, PromptButtons.YesNo, PromptIcons.Question) == DialogResult.Yes;
	}

	protected override async Task<bool> LoadDataAsync()
	{
		PB_Loading.Loading = true;

		var compData = await CompatibilityApiUtil.Catalogue();
		var mods = await SteamUtil.QueryFilesAsync(Domain.Steam.SteamQueryOrder.RankedByLastUpdatedDate, requiredTags: new[] { "Mod" }, all: true);

		foreach (var mod in mods)
		{
			if (mod.Value.ServerTime > (compData?.Packages.FirstOrDefault(x => x.SteamId == mod.Key)?.ReviewDate ?? DateTime.MinValue))
			{
				_packages.Add(mod.Key, mod.Value);
			}
		}

		_pages.AddRange(_packages.OrderBy(x => x.Value?.ServerTime).Select(x => x.Key));

		return true;
	}

	protected override void OnDataLoad()
	{
		SetPackage(0);
		PB_Loading.Dispose();
	}

	protected override void OnLoadFail()
	{
		ShowPrompt(LocaleCR.CrDataLoadFailed, PromptButtons.OK, PromptIcons.Error);
		Form.PushBack();
	}

	private async void SetPackage(int page)
	{
		if (page < 0 || page >= _packages.Count)
		{
			Form?.PushBack();
			return;
		}

		L_Page.Text = $"{_packages.Count - page} / {_packages.Count}";

		PB_LoadingPackage.BringToFront();
		PB_LoadingPackage.Loading = true;

		currentPage = page;
		currentPackage = _packages[_pages[page]];

		try
		{
			currentPackage ??= await SteamUtil.GetItemAsync(_pages[page]);

			var catalogue = await CompatibilityApiUtil.Catalogue(currentPackage!.SteamId);

			postPackage = catalogue?.Packages.FirstOrDefault()?.CloneTo<Package, PostPackage>();

			postPackage ??= CompatibilityManager.GetAutomatedReport(currentPackage).CloneTo<Package, PostPackage>();

			postPackage.BlackListId = catalogue?.BlackListedIds?.Contains(postPackage.SteamId) ?? false;
			postPackage.BlackListName = catalogue?.BlackListedNames?.Contains(postPackage.Name ?? string.Empty) ?? false;

			SetData(postPackage);

			PB_Icon.Package = currentPackage;
			PB_Icon.Image = null;
			PB_Icon.LoadImage(currentPackage.IconUrl, ImageManager.GetImage);
			P_Info.SetPackage(currentPackage, null);

			B_Previous.Enabled = currentPage > 0;
			B_Skip.Enabled = currentPage != _packages.Count - 1;

			PB_LoadingPackage.SendToBack();
			PB_LoadingPackage.Loading = false;
		}
		catch { OnLoadFail(); }
	}

	private void SetData(PostPackage postPackage)
	{
		if (!IsHandleCreated)
		{
			CreateHandle();
		}

		CB_BlackListName.Checked = postPackage.BlackListName;
		CB_BlackListId.Checked = postPackage.BlackListId;
		DD_Stability.SelectedItem = postPackage.Stability;
		DD_DLCs.SelectedItems = SteamUtil.Dlcs.Where(x => postPackage.RequiredDLCs?.Contains(x.Id) ?? false);
		DD_Usage.SelectedItems = Enum.GetValues(typeof(PackageUsage)).Cast<PackageUsage>().Where(x => postPackage.Usage.HasFlag(x));
		TB_Note.Text = postPackage.Note;

		P_Tags.Controls.Clear(true, x => !string.IsNullOrEmpty(x.Text));
		FLP_Statuses.Controls.Clear(true, x => x is IPackageStatusControl<StatusType, PackageStatus>);
		FLP_Interactions.Controls.Clear(true, x => x is IPackageStatusControl<InteractionType, PackageInteraction>);

		foreach (var item in postPackage.Tags ?? new())
		{
			var control = new TagControl { Text = item };
			control.Click += TagControl_Click;
			P_Tags.Controls.Add(control);
			T_NewTag.SendToBack();
		}

		SetLinks(postPackage.Links ?? new());

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
		SetPackage(ModifierKeys.HasFlag(Keys.Shift) ? (_packages.Count - 1) : (currentPage + 1));
	}

	private void B_Previous_Click(object sender, EventArgs e)
	{
		SetPackage(ModifierKeys.HasFlag(Keys.Shift) ? 0 : (currentPage - 1));
	}

	private void T_NewTag_Click(object sender, EventArgs e)
	{
		var prompt = ShowInputPrompt(LocaleCR.AddGlobalTag);

		if (prompt.DialogResult != DialogResult.OK)
		{
			return;
		}

		if (string.IsNullOrWhiteSpace(prompt.Input) || P_Tags.Controls.Any(x => x.Text.Equals(prompt.Input, StringComparison.CurrentCultureIgnoreCase)))
		{
			return;
		}

		var control = new TagControl { Text = prompt.Input };
		control.Click += TagControl_Click;
		P_Tags.Controls.Add(control);
		T_NewTag.SendToBack();
	}

	private void TagControl_Click(object sender, EventArgs e)
	{
		(sender as Control)?.Dispose();
	}

	private void T_NewLink_Click(object sender, EventArgs e)
	{
		var form = new AddLinkForm(P_Links.Controls.OfType<LinkControl>().ToList(x => x.Link));

		form.Show(Form);

		form.LinksReturned += SetLinks;
	}

	private void SetLinks(IEnumerable<PackageLink> links)
	{
		P_Links.Controls.Clear(true, x => x is LinkControl);

		foreach (var item in links.OrderBy(x => x.Type))
		{
			var control = new LinkControl { Link = item };
			control.Click += T_NewLink_Click;
			P_Links.Controls.Add(control);
		}

		T_NewLink.SendToBack();
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
		{
			return;
		}

		postPackage!.SteamId = currentPackage!.SteamId;
		postPackage.FileName = Path.GetFileName(currentPackage.Package?.Mod?.FileName ?? string.Empty);
		postPackage.Name = currentPackage.Name;
		postPackage.ReviewDate = DateTime.UtcNow;
		postPackage.AuthorId = currentPackage.Author?.SteamId ?? 0;
		postPackage.Author = new Author
		{
			SteamId = postPackage.AuthorId,
			Name = currentPackage.Author?.Name,
		};

		postPackage.Stability = DD_Stability.SelectedItem;
		postPackage.Usage = DD_Usage.SelectedItems.Aggregate((prev, next) => prev | next);
		postPackage.RequiredDLCs = DD_DLCs.SelectedItems.Select(x => x.Id).ToArray();
		postPackage.Note = TB_Note.Text;
		postPackage.Tags = P_Tags.Controls.OfType<TagControl>().Where(x => !string.IsNullOrEmpty(x.Text)).ToList(x => x.Text);
		postPackage.Links = P_Links.Controls.OfType<LinkControl>().ToList(x => x.Link);
		postPackage.Statuses = FLP_Statuses.Controls.OfType<IPackageStatusControl<StatusType, PackageStatus>>().ToList(x => x.PackageStatus);
		postPackage.Interactions = FLP_Interactions.Controls.OfType<IPackageStatusControl<InteractionType, PackageInteraction>>().ToList(x => x.PackageStatus);

		if (postPackage.Stability == PackageStability.NotReviewed)
		{
			ShowPrompt(LocaleCR.PleaseReviewTheStability, PromptButtons.OK, PromptIcons.Hand);
			return;
		}

		if (postPackage.Usage == 0)
		{
			ShowPrompt(LocaleCR.PleaseReviewPackageUsage, PromptButtons.OK, PromptIcons.Hand);
			return;
		}

		if (postPackage.Statuses.Any(x => !CRNAttribute.GetAttribute(x.Type).Browsable))
		{
			ShowPrompt(LocaleCR.PleaseReviewPackageStatuses, PromptButtons.OK, PromptIcons.Hand);
			return;
		}

		if (postPackage.Interactions.Any(x => !CRNAttribute.GetAttribute(x.Type).Browsable || !(x.Packages?.Any() ?? false)))
		{
			ShowPrompt(LocaleCR.PleaseReviewPackageInteractions, PromptButtons.OK, PromptIcons.Hand);
			return;
		}

		B_Apply.Loading = true;

		var response = await CompatibilityApiUtil.SaveEntry(postPackage);

		B_Apply.Loading = false;

		if (!response.Success)
		{
			ShowPrompt(response.Message, PromptButtons.OK, PromptIcons.Error);
			return;
		}

		lastPackageData = postPackage;
		B_ReuseData.Visible = true;

		SetPackage(currentPage + 1);
	}

	private void FLP_Statuses_ControlAdded(object sender, ControlEventArgs e)
	{
		T_Statuses.Text = LocaleCR.StatusesCount.Format(FLP_Statuses.Controls.Count - 1);
		T_Interactions.Text = LocaleCR.InteractionCount.Format(FLP_Interactions.Controls.Count - 1);
	}

	private void B_ReuseData_Click(object sender, EventArgs e)
	{
		if (lastPackageData is not null)
		{
			SetData(lastPackageData);
		}

		if (ModifierKeys.HasFlag(Keys.Shift))
		{
			B_Apply_Click(sender, e);
		}
	}
}
