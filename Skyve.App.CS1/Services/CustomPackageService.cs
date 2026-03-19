using Skyve.App.Interfaces;
using Skyve.App.UserInterface.Forms;
using Skyve.App.UserInterface.Panels;
using Skyve.Systems.CS1.Utilities;

using System.Windows.Forms;

namespace Skyve.App.CS1.Services;

internal class CustomPackageService : ICustomPackageService
{
	public SlickStripItem[] GetRightClickMenuItems(IPackage item)
	{
		return GetRightClickMenuItems(new[] { item });
	}

	public SlickStripItem[] GetRightClickMenuItems(IEnumerable<IPackage> items)
	{
		var list = items.ToList();
		var isInstalled = list.Any(item => item.LocalParentPackage is not null);
		var isLocal = isInstalled && list.Any(item => item.IsLocal);

		var bulkUtil = ServiceCenter.Get<IBulkUtil>();
		var packageUtil = ServiceCenter.Get<IPackageUtil>();
		var packageManager = ServiceCenter.Get<IPackageManager>();
		var subscriptionManager = ServiceCenter.Get<ISubscriptionsManager>();
		var profileManager = ServiceCenter.Get<IPlaysetManager>();
		var compatibilityManager = ServiceCenter.Get<ICompatibilityManager>();
		var userService = ServiceCenter.Get<IUserService>();

		return new SlickStripItem[]
		{
			  new (Locale.IncludeAllItemsInThisPackage.FormatPlural(list.Count), "Ok", () => { bulkUtil.SetBulkIncluded(list.SelectWhereNotNull(x => list.Count == 1 ? x.LocalParentPackage : x.LocalPackage)!, true); }, isInstalled && list.Any(item => !item.LocalPackage!.IsIncluded()))
			, new (Locale.ExcludeAllItemsInThisPackage.FormatPlural(list.Count), "Cancel", () => { bulkUtil.SetBulkIncluded(list.SelectWhereNotNull(x => list.Count == 1 ? x.LocalParentPackage : x.LocalPackage)!, false); }, isInstalled && list.Any(item => item.LocalPackage!.IsIncluded()))
			, new ((isInstalled ? Locale.ReDownloadPackage : Locale.DownloadPackage).FormatPlural(list.Count), "Install", () => Redownload(list), SteamUtil.IsSteamAvailable())
			, new (Locale.MovePackageToLocalFolder.FormatPlural(list.Count), "PC", () => list.SelectWhereNotNull(x => x.LocalParentPackage).Foreach(x => packageManager.MoveToLocalFolder(x !)), isInstalled && ! isLocal)
			, new ()
			, new ((isLocal && list[0] is IAsset ? Locale.DeleteAsset : Locale.DeletePackage).FormatPlural(list.Count), "Disposable", () => AskThenDelete(list), isInstalled)
			, new (Locale.UnsubscribePackage.FormatPlural(list.Count), "Steam", () => subscriptionManager.UnSubscribe(list.Cast < IPackageIdentity >()), isInstalled && ! isLocal)
			, new (Locale.SubscribeToItem.FormatPlural(list.Count), "Steam", () => subscriptionManager.Subscribe(list.Cast < IPackageIdentity >()), ! isInstalled && ! isLocal)
			, new ()
			, new (Locale.EditTagsOfPackage.FormatPlural(list.Count), "Tag", () => EditTags(list.SelectWhereNotNull(x => x.LocalParentPackage).SelectMany(x => x !.Assets)), isInstalled)
			, new (Locale.EditTags.FormatPlural(list.Count), "Tag", () => EditTags(list.SelectWhereNotNull(x => x.LocalPackage) !), isInstalled)
			, new (Locale.EditCompatibility.FormatPlural(list.Count), "CompatibilityReport", () => { App.Program.MainForm.PushPanel(null, new PC_CompatibilityManagement(items.Select(x => x.Id))); }, userService.User.Manager || list.Any(item => userService.User.Equals(item.GetWorkshopInfo() ?.Author)))
			, new ()
			, new (Locale.OtherPlaysets, "ProfileSettings")
			{
				SubItems = [
					new (Locale.IncludeThisItemInAllPlaysets.FormatPlural(list.Count), "Ok",  action: () => { new BackgroundAction(() => list.SelectWhereNotNull(x => x.LocalPackage).Foreach(x => profileManager.SetIncludedForAll(x!, true))).Run(); bulkUtil.SetBulkIncluded(list.SelectWhereNotNull(x => x.LocalPackage)!, true); }),
					new (Locale.ExcludeThisItemInAllPlaysets.FormatPlural(list.Count), "Cancel", action: () => { new BackgroundAction(() => list.SelectWhereNotNull(x => x.LocalPackage).Foreach(x => profileManager.SetIncludedForAll(x!, false))).Run(); bulkUtil.SetBulkIncluded(list.SelectWhereNotNull(x => x.LocalPackage)!, false);})]
			}
			, new (Locale.Copy, "Copy", !isLocal)
			{
				SubItems = [
					  new (Locale.CopyPackageName.FormatPlural(list.Count), "Copy",  action: () => Clipboard.SetText(list.ListStrings(CrossIO.NewLine)))
					, new (Locale.CopyWorkshopLink.FormatPlural(list.Count), null, () => Clipboard.SetText(list.ListStrings(x => x.Url, CrossIO.NewLine)))
					, new (Locale.CopyWorkshopId.FormatPlural(list.Count), null,  () => Clipboard.SetText(list.ListStrings(x => x.Id.ToString(), CrossIO.NewLine)))
					, new (  )
					, new (Locale.CopyAuthorName.FormatPlural(list.Count), null,  () => Clipboard.SetText(list.ListStrings(x => x.GetWorkshopInfo()?.Author?.Name , CrossIO.NewLine)))
					, new (Locale.CopyAuthorLink.FormatPlural(list.Count), null,  () => Clipboard.SetText(list.ListStrings(x => x.GetWorkshopInfo()?.Author?.ProfileUrl , CrossIO.NewLine)))
					, new (Locale.CopyAuthorSteamId.FormatPlural(list.Count), null,  () => Clipboard.SetText(list.ListStrings(x => x.GetWorkshopInfo()?.Author?.Id?.ToString() , CrossIO.NewLine)))]
			}
		};
	}

	private static EditTagsForm EditTags(IEnumerable<ILocalPackage> item)
	{
		var frm = new EditTagsForm(item);

		App.Program.MainForm.OnNextIdle(() =>
		{
			frm.Show(App.Program.MainForm);

			frm.ShowUp();
		});

		return frm;
	}

	private static void AskThenDelete<T>(IEnumerable<T> items) where T : IPackage
	{
		if (MessagePrompt.Show(Locale.AreYouSure + "\r\n\r\n" + Locale.ActionUnreversible.FormatPlural(items.Count()), PromptButtons.YesNo, form: App.Program.MainForm) == DialogResult.Yes)
		{
			foreach (var item in items.Where(x => !x.IsBuiltIn).SelectWhereNotNull(x => x.LocalPackage))
			{
				try
				{
					if (item!.IsLocal && item is IAsset asset)
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
	}

	private static void Redownload<T>(IEnumerable<T> item) where T : IPackageIdentity
	{
		ServiceCenter.Get<IDownloadService>().Download(item.Cast<IPackageIdentity>());
	}
}
