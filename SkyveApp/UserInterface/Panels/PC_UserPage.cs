using SkyveApp.Domain.CS1.Steam;
using SkyveApp.UserInterface.Lists;

using System.Drawing;
using System.Threading.Tasks;

namespace SkyveApp.UserInterface.Panels;
public partial class PC_UserPage : PanelContent
{
	private readonly ItemListControl<IPackage> LC_Items;
	private readonly ProfileListControl L_Profiles;

	private readonly IWorkshopService _workshopService = ServiceCenter.Get<IWorkshopService>();

	public IUser User { get; }

	public PC_UserPage(IUser user) : base(true)
	{
		InitializeComponent();

		User = user;

		PB_Icon.LoadImage(User.AvatarUrl, ServiceCenter.Get<IImageService>().GetImage);
		P_Info.SetUser(User, this);

		L_Profiles = new(true)
		{
			GridView = true,
		};

		LC_Items = new()
		{
			IsGenericPage = true,
		};

		LC_Items.SetSorting(PackageSorting.UpdateTime, true);
	}

	protected override async Task<bool> LoadDataAsync()
	{
		var profiles = await ServiceCenter.Get<SkyveApiUtil>().GetUserProfiles(User.Id!);

		if (profiles?.Any() ?? false)
		{
			L_Profiles.SetItems(profiles);

			this.TryInvoke(() =>
			{
				T_Profiles.LinkedControl = L_Profiles;

				if (T_Profiles.Selected)
				{
					T_Profiles.Selected = true;
				}
			});
		}
		else
		{
			this.TryInvoke(() => tabControl.Tabs = tabControl.Tabs.Where(x => x != T_Profiles).ToArray());
		}

		var results = await _workshopService.GetWorkshopItemsByUserAsync(User.Id!);

		LC_Items.SetItems(results.Select(x => new WorkshopPackage(x)));

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
		{
			T_Packages.Selected = true;
		}
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
}
