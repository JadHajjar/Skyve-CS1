using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Compatibility.Enums;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Domain.Systems;
using SkyveApp.Services;
using SkyveApp.Services.Interfaces;
using SkyveApp.Systems;
using SkyveApp.Systems.Compatibility;
using SkyveApp.UserInterface.CompatibilityReport;
using SkyveApp.UserInterface.Content;
using SkyveApp.UserInterface.Forms;
using SkyveApp.UserInterface.Lists;
using SkyveApp.Utilities;

using SlickControls;

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Panels;
public partial class PC_PackagePage : PanelContent
{
	private readonly ItemListControl<IPackage>? LC_Items;
	private readonly ItemListControl<IPackage>? LC_References;
	private TagControl? addTagControl;

	private readonly INotifier _notifier = Program.Services.GetService<INotifier>();

	public IPackage Package { get; }

	public PC_PackagePage(IPackage package)
	{
		InitializeComponent();

		Package = package;

		PB_Icon.Package = package;
		PB_Icon.LoadImage(package.GetWorkshopInfo()?.IconUrl, Program.Services.GetService<IImageService>().GetImage);

		P_Info.SetPackage(package, this);

		T_CR.LinkedControl = new PackageCompatibilityReportControl(package);

		var tabs = slickTabControl1.Tabs.ToList();
		var crAvailable = Package.GetCompatibilityInfo().Data is not null;

		if (!crAvailable)
		{
			TLP_Info.ColumnStyles[1].Width = 0;
		}

		if (Package is Package p && p.Assets is not null && p.Assets.Length > 0)
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
			foreach (var item in Package.GetCompatibilityInfo().Data?.Package.Links ?? new())
			{
				FLP_Links.Controls.Add(new LinkControl { Link = item, Display = true });
			}

			label5.Visible = FLP_Links.Visible = FLP_Links.Controls.Count > 0;

			AddTags();
		}

		var references = package.GetPackagesThatReference().ToList();

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

		foreach (var item in Package.Tags)
		{
			var control = new TagControl { TagInfo = item, Display = true };
			control.MouseClick += TagControl_Click;
			FLP_Tags.Controls.Add(control);
		}

		if (Package.Package is not null)
		{
			addTagControl = new TagControl { ImageName = "I_Add" };
			addTagControl.MouseClick += AddTagControl_MouseClick;
			FLP_Tags.Controls.Add(addTagControl);
		}
	}

	private void TagControl_Click(object sender, EventArgs e)
	{
		if ((sender as TagControl)!.TagInfo.Source != Domain.Enums.TagSource.FindIt)
		{
			return;
		} (sender as TagControl)!.Dispose();

		Program.Services.GetService<IAssetUtil>().SetFindItTag(Package, FLP_Tags.Controls.OfType<TagControl>().Select(x => x.TagInfo.Source == Domain.Enums.TagSource.FindIt ? x.TagInfo.Value?.Replace(' ', '-') : null).WhereNotEmpty().ListStrings(" "));
		Program.MainForm?.TryInvoke(() => Program.MainForm.Invalidate(true));
	}

	private void CentralManager_PackageInformationUpdated()
	{
		P_Info.Invalidate();
		LC_Items?.Invalidate();
	}

	protected override void LocaleChanged()
	{
		var cr = Package.GetCompatibilityInfo().Data?.Package;

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
		var isPackageIncluded = item.IsIncluded;
		var isInstalled = item.Package is not null;

		var contentUtil = Program.Services.GetService<IContentUtil>();
		var subscriptionManager = Program.Services.GetService<ISubscriptionsManager>();
		var profileManager = Program.Services.GetService<IPlaysetManager>();
		var compatibilityManager = Program.Services.GetService<ICompatibilityManager>();

		return new SlickStripItem[]
		{
			  new (Locale.IncludeAllItemsInThisPackage, "I_Ok", !isPackageIncluded && isInstalled, action: () => { item.Package!.IsIncluded = true; })
			, new (Locale.ExcludeAllItemsInThisPackage, "I_Cancel", isPackageIncluded && isInstalled, action: () => { item.Package!.IsIncluded = false; })
			, new (isInstalled? Locale.ReDownloadPackage:Locale.DownloadPackage, "I_Install", SteamUtil.IsSteamAvailable(), action: () => Redownload(item))
			, new (Locale.MovePackageToLocalFolder, "I_PC", isInstalled && item.Workshop, action: () => contentUtil.MoveToLocalFolder(item))
			, new (string.Empty)
			, new (!item.Workshop && item is Asset ? Locale.DeleteAsset : Locale.DeletePackage, "I_Disposable", isInstalled && !(item.Package?.BuiltIn ?? false), action: () => AskThenDelete(item))
			, new (Locale.UnsubscribePackage, "I_Steam", isInstalled && item.Workshop && !(item.Package?.BuiltIn ?? false), action: () => subscriptionManager.UnSubscribe(new[] { item.SteamId }))
			, new (Locale.SubscribeToItem, "I_Steam", !isInstalled && item.Workshop, action: () => subscriptionManager.Subscribe(new[] { item.SteamId }))
			, new (string.Empty)
			, new (Locale.EditCompatibility, "I_CompatibilityReport", compatibilityManager.User.Manager || item.Author?.SteamId == compatibilityManager.User.SteamId , action: ()=>{ Program.MainForm.PushPanel(null, new PC_CompatibilityManagement(new[]{item.SteamId}));})
			, new (string.Empty)
			, new (Locale.EditTags, "I_Tag", isInstalled, action: () => EditTags(item))
			, new (Locale.OtherProfiles, "I_ProfileSettings", fade: true)
			, new (Locale.IncludeThisItemInAllProfiles, "I_Ok", tab: 1, action: () => { new BackgroundAction(() => profileManager.SetIncludedForAll(item, true)).Run(); item.IsIncluded = true; })
			, new (Locale.ExcludeThisItemInAllProfiles, "I_Cancel", tab: 1, action: () => { new BackgroundAction(() => profileManager.SetIncludedForAll(item, false)).Run(); item.IsIncluded = false; })
			, new (Locale.Copy, "I_Copy", item.Workshop, fade: true)
			, new (Locale.CopyPackageName, item.Workshop ? null : "I_Copy", tab: item.Workshop ? 1 : 0, action: () => Clipboard.SetText(item.ToString()))
			, new (Locale.CopyWorkshopLink, null, item.Workshop, tab: 1, action: () => Clipboard.SetText($"https://steamcommunity.com/workshop/filedetails?id={item.SteamId}"))
			, new (Locale.CopyWorkshopId, null, item.Workshop, tab: 1,  action: () => Clipboard.SetText(item.SteamId.ToString()))
			, new (string.Empty, show: item.Workshop, tab: 1)
			, new (Locale.CopyAuthorName, null, item.Workshop, tab: 1, action: () => Clipboard.SetText(item.Author?.Name))
			, new (Locale.CopyAuthorLink, null, item.Workshop, tab: 1, action: () => Clipboard.SetText($"{item.Author?.ProfileUrl}myworkshopfiles"))
			, new (Locale.CopyAuthorId, null, item.Workshop, tab: 1, action: () => Clipboard.SetText(item.Author?.ProfileUrl?.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Last()))
			, new (Locale.CopyAuthorSteamId, null, item.Workshop, tab: 1,  action: () => Clipboard.SetText(item.Author?.SteamId.ToString()))
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
				if (!item.Workshop && item is Asset asset)
				{
					CrossIO.DeleteFile(asset.FilePath);
				}
				else
				{
					Program.Services.GetService<IContentUtil>().DeleteAll(item.Folder);
				}
			}
			catch (Exception ex) { MessagePrompt.Show(ex, Locale.FailedToDeleteItem); }
		}
	}

	private static void Redownload<T>(T item) where T : IPackage
	{
		SteamUtil.Download(new IPackage[] { item });
	}
}
