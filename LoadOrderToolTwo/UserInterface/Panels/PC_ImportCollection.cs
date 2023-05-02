using LoadOrderToolTwo.UserInterface.Content;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.IO;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Panels;
internal class PC_ImportCollection : PC_GenericPackageList
{
	private readonly string? _id;

	internal PC_ImportCollection(Domain.Steam.SteamWorkshopItem collection, Dictionary<ulong, Domain.Steam.SteamWorkshopItem> contents) : base(contents.Values)
	{
		_id = collection.PublishedFileID;

		PB_Icon = new PackageIcon
		{
			Collection = true
		};
		PB_Icon.LoadImage(collection.PreviewURL, ImageManager.GetImage);

		TLP_Main.Controls.Add(PB_Icon, 0, 0);
		TLP_Main.SetRowSpan(PB_Icon, 4);

		L_CollectionName = new Label
		{
			Text = collection.Title,
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
