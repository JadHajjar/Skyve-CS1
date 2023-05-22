using SkyveApp.Domain;
using SkyveApp.Domain.Interfaces;
using SkyveApp.UserInterface.Content;
using SkyveApp.Utilities;
using SkyveApp.Utilities.IO;
using SkyveApp.Utilities.Managers;

using SlickControls;

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Panels;
internal class PC_ViewCollection : PC_GenericPackageList
{
	private readonly ulong _id;

	internal PC_ViewCollection(IPackage collection) : base(collection.RequiredPackages.Select(x => new Profile.Asset { SteamId = x }))
	{
		_id = collection.SteamId;

		TLP_Main.SetColumn(P_FiltersContainer, 0);
		TLP_Main.SetColumnSpan(P_FiltersContainer, TLP_Main.ColumnCount);

		PB_Icon = new PackageIcon
		{
			Collection = true
		};
		PB_Icon.LoadImage(collection.IconUrl, ImageManager.GetImage);

		TLP_Main.Controls.Add(PB_Icon, 0, 0);
		TLP_Main.SetRowSpan(PB_Icon, 3);

		L_CollectionName = new Label
		{
			Text = collection.Name,
			AutoSize = true
		};
		TLP_Main.Controls.Add(L_CollectionName, 1, 0);
		TLP_Main.SetColumnSpan(L_CollectionName, TLP_Main.ColumnCount - 2);

		B_Steam = new SlickButton
		{
			ImageName = "I_Steam",
			AutoSize = true,
			Anchor = AnchorStyles.Right
		};
		B_Steam.Click += B_Steam_Click;
		TLP_Main.Controls.Add(B_Steam, TLP_Main.ColumnCount - 1, 0);
	}

	private void B_Steam_Click(object sender, System.EventArgs e)
	{
		PlatformUtil.OpenUrl($"https://steamcommunity.com/workshop/filedetails/?id={_id}");
	}

	public PackageIcon PB_Icon { get; }
	public Label L_CollectionName { get; }
	public SlickButton B_Steam { get; }

	protected override void UIChanged()
	{
		base.UIChanged();

		L_CollectionName.Font = UI.Font(12F, FontStyle.Bold);
		L_CollectionName.Margin = UI.Scale(new Padding(3, 3, 0, 5), UI.FontScale);
		PB_Icon.Size = UI.Scale(new Size(64, 64), UI.FontScale);
		PB_Icon.Margin = UI.Scale(new Padding(5, 0, 0, 5), UI.FontScale);
		B_Steam.Margin = UI.Scale(new Padding(0, 0, 5, 0), UI.FontScale);
	}

	protected override void LocaleChanged()
	{
		base.LocaleChanged();

		Text = Locale.CollectionTitle;
	}
}
