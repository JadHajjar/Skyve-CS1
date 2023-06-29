using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Compatibility.Enums;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Domain.Steam;
using SkyveApp.Services;
using SkyveApp.Services.Interfaces;
using SkyveApp.Systems;
using SkyveApp.UserInterface.CompatibilityReport;
using SkyveApp.UserInterface.Content;
using SkyveApp.UserInterface.Forms;
using SkyveApp.UserInterface.Lists;
using SkyveApp.Utilities;

using SlickControls;

using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Panels;
public partial class PC_UserPage : PanelContent
{
	private readonly ItemListControl<IPackage> LC_Items;
	private readonly ProfileListControl L_Profiles;
	private TagControl? addTagControl;

	public ulong UserId { get; }
	public SteamUser? User { get; private set; }

	public PC_UserPage(ulong user) : base (true)
	{
		InitializeComponent();

		UserId = user;
		User = SteamUtil.GetUser(user);

		PB_Icon.UserId = UserId;

		if (User is not null)
		{
			PB_Icon.LoadImage(User.AvatarUrl, ServiceCenter.Get<IImageService>().GetImage);
			P_Info.SetUser(User, this);
		}

		L_Profiles = new (true)
		{
			GridView = true,
		};

		LC_Items = new ()
		{
			IsGenericPage = true,
		};

		LC_Items.SetSorting(Domain.Enums.PackageSorting.UpdateTime, true);
	}

	protected override async Task<bool> LoadDataAsync()
	{
		if  (User is null)
		{
			User = await SteamUtil.GetUserAsync(UserId);

			if (User is not null)
			{
				PB_Icon.LoadImage(User.AvatarUrl, ServiceCenter.Get<IImageService>().GetImage);
				P_Info.SetUser(User, this);
			}
		}

		var profiles = await SkyveApiUtil.GetUserProfiles(UserId);

		if (profiles?.Any() ?? false)
		{
			L_Profiles.SetItems(profiles);

			this.TryInvoke(() =>
			{
				T_Profiles.LinkedControl = L_Profiles;

				if (T_Profiles.Selected)
					T_Profiles.Selected = true;
			});
		}
		else
		{
			this.TryInvoke(() => tabControl.Tabs = tabControl.Tabs.Where(x => x != T_Profiles).ToArray());
		}

		var results = await SteamUtil.GetWorkshopItemsByUserAsync(UserId, true);

		LC_Items.SetItems(results.Values);

		return true;
	}

	protected override void OnDataLoad()
	{
		if (LC_Items.ItemCount == 0)
		{
			OnLoadFail();
			return;
		}

		T_Packages.LinkedControl = LC_Items;

		if (T_Packages.Selected)
			T_Packages.Selected = true;
	}

	protected override void OnLoadFail()
	{
		tabControl.Tabs = tabControl.Tabs.Where(x => x != T_Packages).ToArray();
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		PB_Icon.Width = TLP_Top.Height = (int)(128 / 2 * UI.FontScale);
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
		var isInstalled = item.Package is not null;
		var contentUtil = ServiceCenter.Get<IContentUtil>();
		var subscriptionsManager = ServiceCenter.Get<ISubscriptionsManager>();
		var compatibilityManager = ServiceCenter.Get<ICompatibilityManager>();
		var profileManager = ServiceCenter.Get<IPlaysetManager>();

		return new SlickStripItem[]
		{
			  new (Locale.IncludeAllItemsInThisPackage, "I_Ok", !isPackageIncluded && isInstalled, action: () => { item.Package!.IsIncluded = true; })
			, new (Locale.ExcludeAllItemsInThisPackage, "I_Cancel", isPackageIncluded && isInstalled, action: () => { item.Package!.IsIncluded = false; })
			, new (isInstalled? Locale.ReDownloadPackage:Locale.DownloadPackage, "I_Install", SteamUtil.IsSteamAvailable(), action: () => Redownload(item))
			, new (Locale.MovePackageToLocalFolder, "I_PC", isInstalled && item.Workshop, action: () => contentUtil.MoveToLocalFolder(item))
			, new (string.Empty)
			, new (!item.Workshop && item is Asset ? Locale.DeleteAsset : Locale.DeletePackage, "I_Disposable", isInstalled && !(item.Package?.BuiltIn ?? false), action: () => AskThenDelete(item))
			, new (Locale.UnsubscribePackage, "I_Steam", isInstalled && item.Workshop && !(item.Package?.BuiltIn ?? false), action: () => subscriptionsManager.UnSubscribe(new[] { item.SteamId }))
			, new (Locale.SubscribeToItem, "I_Steam", !isInstalled && item.Workshop, action: () => subscriptionsManager.Subscribe(new[] { item.SteamId }))
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
					ServiceCenter.Get<IContentUtil>().DeleteAll(item.Folder);
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
