using SkyveApp.Systems.CS1.Utilities;
using SkyveApp.UserInterface.CompatibilityReport;
using SkyveApp.UserInterface.Content;
using SkyveApp.UserInterface.Forms;
using SkyveApp.UserInterface.Lists;

using System.Drawing;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Panels;
public partial class PC_PackagePage : PanelContent
{
	private readonly ItemListControl<IPackage>? LC_Items;
	private readonly ItemListControl<IPackage>? LC_References;
	private TagControl? addTagControl;

	private readonly INotifier _notifier;
	private readonly ICompatibilityManager _compatibilityManager;
	private readonly IPackageUtil _packageUtil;
	private readonly ISettings _settings;

	public IPackage Package { get; }

	public PC_PackagePage(IPackage package)
	{
		ServiceCenter.Get(out _notifier, out _compatibilityManager, out _packageUtil, out _settings);

		InitializeComponent();

		Package = package;

		PB_Icon.Package = package;
		PB_Icon.LoadImage(package.GetWorkshopInfo()?.ThumbnailUrl, ServiceCenter.Get<IImageService>().GetImage);

		P_Info.SetPackage(package, this);

		T_CR.LinkedControl = new PackageCompatibilityReportControl(package);

		var tabs = slickTabControl1.Tabs.ToList();
		var crdata = _compatibilityManager.GetPackageInfo(Package);
		var crAvailable = crdata is not null;

		if (!crAvailable)
		{
			TLP_Info.ColumnStyles[1].Width = 0;
		}

		if (Package is ILocalPackageWithContents p && p.Assets is not null && p.Assets.Length > 0)
		{
			LC_Items = new ItemListControl<IPackage>
			{
				PackagePage = true,
				Dock = DockStyle.Fill
			};

			LC_Items.AddRange(p.Assets);

			P_List.Controls.Add(LC_Items);
		}
		else if (crAvailable)
		{
			TLP_Info.ColumnStyles[0].Width = 0;
		}
		else
		{
			tabs.Remove(T_Info);
			T_CR.Selected = true;
		}

		if (crAvailable)
		{
			foreach (var item in crdata?.Links ?? new())
			{
				FLP_Links.Controls.Add(new LinkControl { Link = item, Display = true });
			}

			label5.Visible = FLP_Links.Visible = FLP_Links.Controls.Count > 0;

			AddTags();
		}

		var references = _packageUtil.GetPackagesThatReference(package, _settings.UserSettings.ShowAllReferencedPackages).ToList();

		if (references.Count > 0)
		{
			LC_References = new ItemListControl<IPackage>
			{
				Dock = DockStyle.Fill
			};

			LC_References.AddRange(references);

			T_References.LinkedControl = LC_References;
		}
		else
		{
			tabs.Remove(T_References);
		}

		//if (!string.IsNullOrWhiteSpace(package.SteamDescription))
		//{
		//	var c = new SteamDescriptionViewer(package.SteamDescription!);

		//	T_Info.LinkedControl = c;
		//}

		var pc = new OtherProfilePackage(package)
		{
			Dock = DockStyle.Fill
		};

		T_Profiles.FillTab = true;
		T_Profiles.LinkedControl = pc;

		slickTabControl1.Tabs = tabs.ToArray();

		_notifier.PackageInformationUpdated += CentralManager_PackageInformationUpdated;
	}

	private void AddTagControl_MouseClick(object sender, MouseEventArgs e)
	{
		var frm = EditTags(Package);

		frm.FormClosed += (_, _) =>
		{
			if (frm.DialogResult == DialogResult.OK)
			{
				AddTags();
			}
		};
	}

	private void AddTags()
	{
		FLP_Tags.Controls.Clear(true);

		foreach (var item in Package.GetTags())
		{
			var control = new TagControl { TagInfo = item, Display = true };
			control.MouseClick += TagControl_Click;
			FLP_Tags.Controls.Add(control);
		}

		if (Package.LocalPackage is not null)
		{
			addTagControl = new TagControl { ImageName = "I_Add" };
			addTagControl.MouseClick += AddTagControl_MouseClick;
			FLP_Tags.Controls.Add(addTagControl);
		}
	}

	private void TagControl_Click(object sender, EventArgs e)
	{
		if (!(sender as TagControl)!.TagInfo!.IsCustom)
		{
			return;
		}

		(sender as TagControl)!.Dispose();

		ServiceCenter.Get<ITagsService>().SetTags(Package, FLP_Tags.Controls.OfType<TagControl>().Select(x => x.TagInfo!.IsCustom ? x.TagInfo.Value?.Replace(' ', '-') : null)!);
		Program.MainForm?.TryInvoke(() => Program.MainForm.Invalidate(true));
	}

	private void CentralManager_PackageInformationUpdated()
	{
		P_Info.Invalidate();
		LC_Items?.Invalidate();
	}

	protected override void LocaleChanged()
	{
		var cr = _compatibilityManager.GetPackageInfo(Package);

		if (cr is null)
		{
			return;
		}

		label1.Text = LocaleCR.Usage;
		label2.Text = cr.Usage.GetValues().If(x => x.Count() == Enum.GetValues(typeof(PackageUsage)).Length, x => Locale.AnyUsage.One, x => x.ListStrings(x => LocaleCR.Get(x.ToString()), ", "));
		label3.Text = LocaleCR.PackageType;
		label4.Text = cr.Type == PackageType.GenericPackage ? (Package.IsMod ? Locale.Mod : Locale.Asset) : LocaleCR.Get(cr.Type.ToString());
		label5.Text = LocaleCR.Links;
		label6.Text = LocaleSlickUI.Tags;
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		PB_Icon.Width = TLP_Top.Height = (int)(128 * UI.FontScale);
		TLP_About.Padding = UI.Scale(new Padding(5), UI.FontScale);
		label1.Margin = label3.Margin = label5.Margin = label6.Margin = UI.Scale(new Padding(3, 3, 0, 0), UI.FontScale);
		label2.Margin = label4.Margin = FLP_Links.Margin = FLP_Links.Margin = UI.Scale(new Padding(3, 3, 0, 7), UI.FontScale);
		label1.Font = label3.Font = label5.Font = label6.Font = UI.Font(7.5F, FontStyle.Bold);
	}

	protected override void DesignChanged(FormDesign design)
	{
		base.DesignChanged(design);

		BackColor = design.AccentBackColor;
		P_Content.BackColor = P_Back.BackColor = design.BackColor;
		label1.ForeColor = label3.ForeColor = label5.ForeColor = label6.ForeColor = design.InfoColor;
		panel1.BackColor = LC_Items is null ? design.AccentBackColor : design.BackColor.Tint(Lum: design.Type.If(FormDesignType.Dark, 5, -5));
	}

	public override Color GetTopBarColor()
	{
		return FormDesign.Design.AccentBackColor;
	}

	internal static SlickStripItem[] GetRightClickMenuItems<T>(T item) where T : IPackage
	{
		var isInstalled = item.LocalParentPackage is not null;
		var isPackageIncluded = isInstalled && item.LocalPackage!.IsIncluded();

		var packageUtil = ServiceCenter.Get<IPackageUtil>();
		var packageManager = ServiceCenter.Get<IPackageManager>();
		var subscriptionManager = ServiceCenter.Get<ISubscriptionsManager>();
		var profileManager = ServiceCenter.Get<IPlaysetManager>();
		var compatibilityManager = ServiceCenter.Get<ICompatibilityManager>();
		var userService = ServiceCenter.Get<IUserService>();

		return new SlickStripItem[]
		{
			  new (Locale.IncludeAllItemsInThisPackage, "I_Ok", !isPackageIncluded && isInstalled, action: () => { packageUtil.SetIncluded(item.LocalParentPackage!, true); })
			, new (Locale.ExcludeAllItemsInThisPackage, "I_Cancel", isPackageIncluded && isInstalled, action: () => { packageUtil.SetIncluded(item.LocalParentPackage!, false); })
			, new (isInstalled? Locale.ReDownloadPackage:Locale.DownloadPackage, "I_Install", SteamUtil.IsSteamAvailable(), action: () => Redownload(item))
			, new (Locale.MovePackageToLocalFolder, "I_PC", isInstalled && !item.IsLocal, action: () => packageManager.MoveToLocalFolder(item.LocalParentPackage!))
			, new (string.Empty)
			, new (item.IsLocal && item is IAsset ? Locale.DeleteAsset : Locale.DeletePackage, "I_Disposable", isInstalled && !item.IsBuiltIn , action: () => AskThenDelete(item))
			, new (Locale.UnsubscribePackage, "I_Steam", isInstalled && !item.IsLocal && !item.IsBuiltIn, action: () => subscriptionManager.UnSubscribe(new IPackage[] { item }))
			, new (Locale.SubscribeToItem, "I_Steam", !isInstalled && !item.IsLocal, action: () => subscriptionManager.Subscribe(new IPackage[] { item }))
			, new (string.Empty)
			, new (Locale.EditCompatibility, "I_CompatibilityReport", userService.User.Manager || item.GetWorkshopInfo()?.Author?.Id == userService.User.Id , action: ()=>{ Program.MainForm.PushPanel(null, new PC_CompatibilityManagement(new[]{item.Id}));})
			, new (string.Empty)
			, new (Locale.EditTags, "I_Tag", isInstalled, action: () => EditTags(item))
			, new (Locale.OtherPlaysets, "I_ProfileSettings", fade: true)
			, new (Locale.IncludeThisItemInAllPlaysets, "I_Ok", tab: 1, action: () => { new BackgroundAction(() => profileManager.SetIncludedForAll(item, true)).Run(); packageUtil.SetIncluded(item.LocalParentPackage!, true); })
			, new (Locale.ExcludeThisItemInAllPlaysets, "I_Cancel", tab: 1, action: () => { new BackgroundAction(() => profileManager.SetIncludedForAll(item, false)).Run(); packageUtil.SetIncluded(item.LocalParentPackage!, false);})
			, new (Locale.Copy, "I_Copy", !item.IsLocal, fade: true)
			, new (Locale.CopyPackageName, !item.IsLocal ? null : "I_Copy", tab: !item.IsLocal ? 1 : 0, action: () => Clipboard.SetText(item.ToString()))
			, new (Locale.CopyWorkshopLink, null, !item.IsLocal, tab: 1, action: () => Clipboard.SetText(item.Url ?? string.Empty))
			, new (Locale.CopyWorkshopId, null, !item.IsLocal, tab: 1,  action: () => Clipboard.SetText(item.Id.ToString()))
			, new (string.Empty, show: !item.IsLocal, tab: 1)
			, new (Locale.CopyAuthorName, null, !item.IsLocal, tab: 1, action: () => Clipboard.SetText(item.GetWorkshopInfo()?.Author?.Name ?? string.Empty))
			, new (Locale.CopyAuthorLink, null, !item.IsLocal, tab: 1, action: () => Clipboard.SetText(item.GetWorkshopInfo()?.Author?.ProfileUrl ?? string.Empty))
			, new (Locale.CopyAuthorSteamId, null, !item.IsLocal, tab: 1,  action: () => Clipboard.SetText(item.GetWorkshopInfo()?.Author?.Id?.ToString() ?? string.Empty))
		};
	}

	private static EditTagsForm EditTags<T>(T item) where T : IPackage
	{
		var frm = new EditTagsForm(item);

		Program.MainForm.OnNextIdle(() =>
		{
			frm.Show(Program.MainForm);

			frm.ShowUp();
		});

		return frm;
	}

	private static void AskThenDelete<T>(T item) where T : IPackage
	{
		if (MessagePrompt.Show(Locale.AreYouSure + "\r\n\r\n" + Locale.ActionUnreversible, PromptButtons.YesNo, form: Program.MainForm) == DialogResult.Yes)
		{
			try
			{
				if (item.IsLocal && item is IAsset asset)
				{
					CrossIO.DeleteFile(asset.FilePath);
				}
				else if (item.LocalParentPackage is not null)
				{
					ServiceCenter.Get<IPackageManager>().DeleteAll(item.LocalParentPackage.Folder);
				}
			}
			catch (Exception ex) { MessagePrompt.Show(ex, Locale.FailedToDeleteItem); }
		}
	}

	private static void Redownload<T>(T item) where T : IPackage
	{
		ServiceCenter.Get<IDownloadService>().Download(new IPackage[] { item });
	}
}
