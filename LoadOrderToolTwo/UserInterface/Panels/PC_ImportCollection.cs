using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Panels;
public partial class PC_ImportCollection : PanelContent
{
	private readonly string _id;

	public PC_ImportCollection(Domain.Steam.SteamWorkshopItem collection, Dictionary<ulong, Domain.Steam.SteamWorkshopItem> contents)
	{
		InitializeComponent();

		Text = Locale.CollectionTitle;
		_id = collection.PublishedFileID;
		L_Title.Text = collection.Title.Replace("&", "&&");

		PB_Icon.Collection = true;
		PB_Icon.LoadImage(collection.PreviewURL, ImageManager.GetImage);

		TLP_Contents.RowCount = 0;
		TLP_Contents.RowStyles.Clear();

		foreach (var item in contents.Values)
		{
			if (TLP_Contents.Controls.Count % 2 == 0)
			{
				TLP_Contents.RowCount++;
				TLP_Contents.RowStyles.Add(new());
			}

			TLP_Contents.Controls.Add(new PackageViewControl(item), TLP_Contents.Controls.Count % 2, TLP_Contents.RowCount - 1);
		}
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		B_ExInclude.Width = (int)(250 * UI.FontScale);
		PB_Icon.Width = TLP_Top.Height = (int)(128 * UI.FontScale);
		TLP_Top.Height += 20;
		L_Title.Font = UI.Font(14F, FontStyle.Bold);
		L_Title.Margin = UI.Scale(new Padding(7), UI.FontScale);
	}

	protected override void DesignChanged(FormDesign design)
	{
		base.DesignChanged(design);

		BackColor = design.AccentBackColor;
		panel1.BackColor = panel2.BackColor = P_Back.BackColor = design.BackColor;
	}

	public override Color GetTopBarColor()
	{
		return FormDesign.Design.AccentBackColor;
	}

	private void B_SteamPage_Click(object sender, System.EventArgs e)
	{
		try
		{ Process.Start($"https://steamcommunity.com/workshop/filedetails/?id={_id}"); }
		catch { }
	}
}
