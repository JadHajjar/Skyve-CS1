using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.UserInterface.CompatibilityReport;
using LoadOrderToolTwo.UserInterface.Lists;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.IO;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Panels;
public partial class PC_PackagePage : PanelContent
{
	public PC_PackagePage(IPackage package)
	{
		InitializeComponent();

		Package = package;

		Text = Locale.Back;
		T_Info.Text = Locale.ContentAndInfo;
		T_CR.Text = Locale.CompatibilityReport;
		T_Profiles.Text = Locale.OtherProfiles;
		PB_Icon.Package = package;
		PB_Icon.LoadImage(package.IconUrl, ImageManager.GetImage);

		P_Info.SetPackage(package, this);

		T_CR.LinkedControl = new PackageCompatibilityReportControl(package);

		if (Package is Package p && p.Assets is not null && p.Assets.Length > 0)
		{
			var c = new ItemListControl<IPackage>
			{
				PackagePage = true,
				Dock = DockStyle.Fill
			};

			c.AddRange(p.Assets);

			T_Info.FillTab = true;
			T_Info.LinkedControl = c;
		}
		else
		{
			slickTabControl1.Tabs = new[] { T_CR, T_Profiles };
			T_CR.Selected = true;
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

	private void CentralManager_PackageInformationUpdated()
	{
		P_Info.Invalidate();
		T_Info.LinkedControl?.Invalidate();
	}

	public IPackage Package { get; }

	protected override void UIChanged()
	{
		base.UIChanged();

		PB_Icon.Width = TLP_Top.Height = (int)(128 * UI.FontScale);
	}

	protected override void DesignChanged(FormDesign design)
	{
		base.DesignChanged(design);

		BackColor = design.AccentBackColor;
		P_Content.BackColor = P_Back.BackColor = design.BackColor;
	}

	public override Color GetTopBarColor()
	{
		return FormDesign.Design.AccentBackColor;
	}

	internal static SlickStripItem[] GetRightClickMenuItems<T>(T item) where T : IPackage
	{
		var isPackageIncluded = item.IsIncluded;

		return new SlickStripItem[]
		{
			  new (Locale.IncludeAllItemsInThisPackage, "I_Ok", !isPackageIncluded, action: () => { item.IsIncluded = true; })
			, new (Locale.ExcludeAllItemsInThisPackage, "I_Cancel", isPackageIncluded, action: () => { item.IsIncluded = false; })
			, new (Locale.ReDownloadPackage, "I_ReDownload", SteamUtil.IsSteamAvailable(), action: () => Redownload(item))
			, new (Locale.MovePackageToLocalFolder, "I_PC",item.Workshop, action: () => ContentUtil.MoveToLocalFolder(item))
			, new (string.Empty)
			, new (!item.Workshop && item is Asset ? Locale.DeleteAsset : Locale.DeletePackage, "I_Disposable", !(item.Package?.BuiltIn ?? false), action: () => AskThenDelete(item))
			, new (Locale.UnsubscribePackage, "I_Steam", item.Workshop && !(item.Package?.BuiltIn ?? false), action: async () => await CitiesManager.Subscribe(new[] { item.SteamId }, true))
			, new (string.Empty)
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
			, new (Locale.CopyAuthorSteamId, null, item.Workshop, tab: 1,  action: () => Clipboard.SetText(item.Author?.SteamId))
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
