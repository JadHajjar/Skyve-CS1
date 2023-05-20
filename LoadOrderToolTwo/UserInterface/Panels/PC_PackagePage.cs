using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.UserInterface.CompatibilityReport;
using LoadOrderToolTwo.UserInterface.Content;
using LoadOrderToolTwo.UserInterface.Lists;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Panels;
public partial class PC_PackagePage : PanelContent
{
	private readonly ItemListControl<IPackage>? LC_Items;
	private TagControl? addTagControl;

	public IPackage Package { get; }

	public PC_PackagePage(IPackage package)
	{
		InitializeComponent();

		Package = package;

		PB_Icon.Package = package;
		PB_Icon.LoadImage(package.IconUrl, ImageManager.GetImage);

		P_Info.SetPackage(package, this);

		T_CR.LinkedControl = new PackageCompatibilityReportControl(package);

		var crAvailable = Package.GetCompatibilityInfo().Data is not null;

		if (!crAvailable)
			TLP_Info.ColumnStyles[1].Width = 0;

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
			slickTabControl1.Tabs = new[] { T_CR, T_Profiles };
			T_CR.Selected = true;
		}

		if (crAvailable)
		{
			foreach (var item in Package.GetCompatibilityInfo().Data?.Package.Links ?? new())
			{
				FLP_Links.Controls.Add(new LinkControl { Link = item, Display = true });
			}

			foreach (var item in Package.Tags)
			{
				var control = new TagControl { TagInfo = item, Display = true };
				control.MouseClick += TagControl_Click;
				FLP_Tags.Controls.Add(control);
			}

			addTagControl = new TagControl { ImageName = "I_Add" };
			addTagControl.MouseClick += AddTagControl_MouseClick;
			FLP_Tags.Controls.Add(addTagControl);

			label5.Visible = FLP_Links.Visible = FLP_Links.Controls.Count > 0;
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

		CentralManager.PackageInformationUpdated += CentralManager_PackageInformationUpdated;
	}

	private void AddTagControl_MouseClick(object sender, MouseEventArgs e)
	{
		var prompt = ShowInputPrompt(LocaleCR.AddGlobalTag);

		if (prompt.DialogResult != DialogResult.OK)
		{
			return;
		}

		if (string.IsNullOrWhiteSpace(prompt.Input) || FLP_Tags.Controls.Any(x => x.Text.Equals(prompt.Input, StringComparison.CurrentCultureIgnoreCase)))
		{
			return;
		}

		var control = new TagControl { TagInfo = new(Domain.Enums.TagSource.FindIt, prompt.Input) };
		control.Click += TagControl_Click;
		FLP_Tags.Controls.Add(control);
		addTagControl?.SendToBack();
	}

	private void TagControl_Click(object sender, EventArgs e)
	{
		var tag = (sender as TagControl)!.TagInfo;

		(sender as TagControl)!.Dispose();
	}

	private void CentralManager_PackageInformationUpdated()
	{
		P_Info.Invalidate();
		LC_Items?.Invalidate();
	}

	protected override void LocaleChanged()
	{
		var cr = Package.GetCompatibilityInfo().Data?.Package;

		if (cr is null) return;

		label1.Text = LocaleCR.Usage;
		label2.Text = (int)cr.Usage == -1 ? Locale.AnyUsage : cr.Usage.GetValues().ListStrings(x => LocaleCR.Get(x.ToString()), ", ");
		label3.Text = LocaleCR.PackageType;
		label4.Text = cr.Type == Domain.Compatibility.PackageType.GenericPackage ? (Package.IsMod ? Locale.Mod : Locale.Asset) : LocaleCR.Get(cr.Type.ToString());
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
		panel1.BackColor= LC_Items is null ? design.AccentBackColor: design.BackColor.Tint(Lum: design. Type.If(FormDesignType.Dark, 5, -5));
	}

	public override Color GetTopBarColor()
	{
		return FormDesign.Design.AccentBackColor;
	}

	internal static SlickStripItem[] GetRightClickMenuItems<T>(T item) where T : IPackage
	{
		var isPackageIncluded = item.IsIncluded;
		var isInstalled = item.Package is not null;

		return new SlickStripItem[]
		{
			  new (Locale.IncludeAllItemsInThisPackage, "I_Ok", !isPackageIncluded && isInstalled, action: () => { item.Package!.IsIncluded = true; })
			, new (Locale.ExcludeAllItemsInThisPackage, "I_Cancel", isPackageIncluded && isInstalled, action: () => { item.Package!.IsIncluded = false; })
			, new (Locale.ReDownloadPackage, "I_ReDownload", isInstalled && SteamUtil.IsSteamAvailable(), action: () => Redownload(item))
			, new (Locale.MovePackageToLocalFolder, "I_PC", isInstalled && item.Workshop, action: () => ContentUtil.MoveToLocalFolder(item))
			, new (string.Empty)
			, new (!item.Workshop && item is Asset ? Locale.DeleteAsset : Locale.DeletePackage, "I_Disposable", isInstalled && !(item.Package?.BuiltIn ?? false), action: () => AskThenDelete(item))
			, new (Locale.UnsubscribePackage, "I_Steam", isInstalled && item.Workshop && !(item.Package?.BuiltIn ?? false), action: async () => await CitiesManager.UnSubscribe(new[] { item.SteamId }))
			, new (Locale.SubscribeToItem, "I_Steam", !isInstalled && item.Workshop, action: async () => await CitiesManager.Subscribe(new[] { item.SteamId }))
			, new (string.Empty)
			, new (Locale.EditCompatibility, "I_CompatibilityReport", CompatibilityManager.User.Manager || item.Author?.SteamId == CompatibilityManager.User.SteamId , action: ()=>{ Program.MainForm.PushPanel(null, new PC_CompatibilityManagement(new[]{item.SteamId}));})
			, new (string.Empty)
			, new (Locale.EditTags, "I_Tag", isInstalled, action: () => { Program.MainForm.PushPanel(null, new PC_CompatibilityManagement(new[]{item.SteamId}));})
			, new (Locale.OtherProfiles, "I_ProfileSettings", fade: true)
			, new (Locale.IncludeThisItemInAllProfiles, "I_Ok", tab: 1, action: () => { new BackgroundAction(() => ProfileManager.SetIncludedForAll(item, true)).Run(); item.IsIncluded = true; })
			, new (Locale.ExcludeThisItemInAllProfiles, "I_Cancel", tab: 1, action: () => { new BackgroundAction(() => ProfileManager.SetIncludedForAll(item, false)).Run(); item.IsIncluded = false; })
			, new (Locale.Copy, "I_Copy", item.Workshop, fade: true)
			, new (Locale.CopyPackageName, item.Workshop ? null : "I_Copy", tab: item.Workshop ? 1 : 0, action: () => Clipboard.SetText(item.ToString()))
			, new (Locale.CopyWorkshopLink, null, item.Workshop, tab: 1, action: () => Clipboard.SetText($"https://steamcommunity.com/workshop/filedetails?id={item.SteamId}"))
			, new (Locale.CopyWorkshopId, null, item.Workshop, tab: 1,  action: () => Clipboard.SetText(item.SteamId.ToString()))
			, new (string.Empty, show: item.Workshop, tab: 1)
			, new (Locale.CopyAuthorName, null, item.Workshop, tab: 1, action: () => Clipboard.SetText(item.Author?.Name))
			, new (Locale.CopyAuthorLink, null, item.Workshop, tab: 1, action: () => Clipboard.SetText($"{item.Author?.ProfileUrl}myworkshopfiles"))
			, new (Locale.CopyAuthorId, null, item.Workshop, tab: 1, action: () => Clipboard.SetText(item.Author?.ProfileUrl.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Last()))
			, new (Locale.CopyAuthorSteamId, null, item.Workshop, tab: 1,  action: () => Clipboard.SetText(item.Author?.SteamId.ToString()))
		};
	}

	private static void AskThenDelete<T>(T item) where T : IPackage
	{
		if (MessagePrompt.Show(Locale.AreYouSure + "\r\n\r\n" + Locale.ActionUnreversible, PromptButtons.YesNo, form: Program.MainForm) == DialogResult.Yes)
		{
			try
			{
				if (!item.Workshop && item is Asset asset)
				{
					ExtensionClass.DeleteFile(asset.FileName);
				}
				else
				{
					ContentUtil.DeleteAll(item.Folder);
				}
			}
			catch (Exception ex) { MessagePrompt.Show(ex, Locale.FailedToDeleteItem); }
		}
	}

	private static void Redownload<T>(T item) where T : IPackage
	{
		SteamUtil.ReDownload(item);
	}
}
