using LoadOrderToolTwo.Utilities.Managers;

namespace LoadOrderToolTwo.UserInterface.Panels;

partial class PC_MissingPackages
{
	/// <summary> 
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.IContainer components = null;

	/// <summary> 
	/// Clean up any resources being used.
	/// </summary>
	/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
	protected override void Dispose(bool disposing)
	{
		if (disposing && (components != null))
		{
			CentralManager.ContentLoaded -= CentralManager_ContentLoaded;
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	#region Component Designer generated code

	/// <summary> 
	/// Required method for Designer support - do not modify 
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
			this.slickSpacer1 = new SlickControls.SlickSpacer();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.L_Counts = new System.Windows.Forms.Label();
			this.T_All = new SlickControls.SlickTab();
			this.T_Mods = new SlickControls.SlickTab();
			this.T_Assets = new SlickControls.SlickTab();
			this.TB_Search = new SlickControls.SlickTextBox();
			this.B_SteamPage = new SlickControls.SlickButton();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.OT_Workshop = new LoadOrderToolTwo.UserInterface.Generic.ThreeOptionToggle();
			this.LC_Items = new LoadOrderToolTwo.UserInterface.Lists.GenericPackageListControl();
			this.DD_Tags = new LoadOrderToolTwo.UserInterface.Dropdowns.TagsDropDown();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// base_Text
			// 
			this.base_Text.Location = new System.Drawing.Point(-2, 3);
			// 
			// slickSpacer1
			// 
			this.slickSpacer1.Dock = System.Windows.Forms.DockStyle.Top;
			this.slickSpacer1.Location = new System.Drawing.Point(0, 225);
			this.slickSpacer1.Margin = new System.Windows.Forms.Padding(0);
			this.slickSpacer1.Name = "slickSpacer1";
			this.slickSpacer1.Size = new System.Drawing.Size(783, 2);
			this.slickSpacer1.TabIndex = 19;
			this.slickSpacer1.TabStop = false;
			this.slickSpacer1.Text = "slickSpacer1";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.L_Counts, 3, 0);
			this.tableLayoutPanel1.Controls.Add(this.T_All, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.T_Mods, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.T_Assets, 2, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 189);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(783, 36);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// L_Counts
			// 
			this.L_Counts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.L_Counts.AutoSize = true;
			this.L_Counts.Location = new System.Drawing.Point(728, 15);
			this.L_Counts.Name = "L_Counts";
			this.L_Counts.Size = new System.Drawing.Size(52, 21);
			this.L_Counts.TabIndex = 1;
			this.L_Counts.Text = "label1";
			this.L_Counts.UseMnemonic = false;
			// 
			// T_All
			// 
			this.T_All.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.T_All.Cursor = System.Windows.Forms.Cursors.Hand;
			this.T_All.Icon = null;
			this.T_All.LinkedControl = null;
			this.T_All.Location = new System.Drawing.Point(3, 3);
			this.T_All.Name = "T_All";
			this.T_All.Selected = true;
			this.T_All.Size = new System.Drawing.Size(206, 30);
			this.T_All.TabIndex = 0;
			this.T_All.TabStop = false;
			this.T_All.Text = "AllItems";
			this.T_All.TabSelected += new System.EventHandler(this.T_Assets_TabSelected);
			// 
			// T_Mods
			// 
			this.T_Mods.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.T_Mods.Cursor = System.Windows.Forms.Cursors.Hand;
			this.T_Mods.Icon = null;
			this.T_Mods.LinkedControl = null;
			this.T_Mods.Location = new System.Drawing.Point(215, 3);
			this.T_Mods.Name = "T_Mods";
			this.T_Mods.Selected = false;
			this.T_Mods.Size = new System.Drawing.Size(206, 30);
			this.T_Mods.TabIndex = 1;
			this.T_Mods.TabStop = false;
			this.T_Mods.Text = "Mods";
			this.T_Mods.TabSelected += new System.EventHandler(this.T_Assets_TabSelected);
			// 
			// T_Assets
			// 
			this.T_Assets.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.T_Assets.Cursor = System.Windows.Forms.Cursors.Hand;
			this.T_Assets.Icon = null;
			this.T_Assets.LinkedControl = null;
			this.T_Assets.Location = new System.Drawing.Point(427, 3);
			this.T_Assets.Name = "T_Assets";
			this.T_Assets.Selected = false;
			this.T_Assets.Size = new System.Drawing.Size(206, 30);
			this.T_Assets.TabIndex = 2;
			this.T_Assets.TabStop = false;
			this.T_Assets.Text = "Assets";
			this.T_Assets.TabSelected += new System.EventHandler(this.T_Assets_TabSelected);
			// 
			// TB_Search
			// 
			this.TB_Search.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.TB_Search.Image = global::LoadOrderToolTwo.Properties.Resources.I_Search;
			this.TB_Search.LabelText = "Search";
			this.TB_Search.Location = new System.Drawing.Point(3, 3);
			this.TB_Search.Name = "TB_Search";
			this.TB_Search.Placeholder = "SearchCollection";
			this.TB_Search.SelectedText = "";
			this.TB_Search.SelectionLength = 0;
			this.TB_Search.SelectionStart = 0;
			this.TB_Search.Size = new System.Drawing.Size(140, 44);
			this.TB_Search.TabIndex = 0;
			this.TB_Search.TextChanged += new System.EventHandler(this.TB_Search_TextChanged);
			this.TB_Search.IconClicked += new System.EventHandler(this.TB_Search_IconClicked);
			// 
			// B_SteamPage
			// 
			this.B_SteamPage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.B_SteamPage.ColorShade = null;
			this.B_SteamPage.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_SteamPage.Location = new System.Drawing.Point(437, 3);
			this.B_SteamPage.Name = "B_SteamPage";
			this.B_SteamPage.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);
			this.B_SteamPage.Size = new System.Drawing.Size(343, 30);
			this.B_SteamPage.SpaceTriggersClick = true;
			this.B_SteamPage.TabIndex = 2;
			this.B_SteamPage.Text = "SubscribeAllButton";
			this.B_SteamPage.Click += new System.EventHandler(this.B_SteamPage_Click);
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.AutoSize = true;
			this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.Controls.Add(this.DD_Tags, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.TB_Search, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.OT_Workshop, 0, 2);
			this.tableLayoutPanel2.Controls.Add(this.B_SteamPage, 1, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 30);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 3;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.Size = new System.Drawing.Size(783, 159);
			this.tableLayoutPanel2.TabIndex = 0;
			// 
			// OT_Workshop
			// 
			this.OT_Workshop.Cursor = System.Windows.Forms.Cursors.Hand;
			this.OT_Workshop.Image1 = "I_Local";
			this.OT_Workshop.Image2 = "I_Steam";
			this.OT_Workshop.Location = new System.Drawing.Point(3, 117);
			this.OT_Workshop.Name = "OT_Workshop";
			this.OT_Workshop.Option1 = "Local";
			this.OT_Workshop.Option2 = "Workshop";
			this.OT_Workshop.OptionStyle1 = Extensions.ColorStyle.Active;
			this.OT_Workshop.OptionStyle2 = Extensions.ColorStyle.Active;
			this.OT_Workshop.Size = new System.Drawing.Size(374, 39);
			this.OT_Workshop.TabIndex = 1;
			this.OT_Workshop.SelectedValueChanged += new System.EventHandler(this.OT_Workshop_SelectedValueChanged);
			// 
			// LC_Items
			// 
			this.LC_Items.AutoInvalidate = false;
			this.LC_Items.AutoScroll = true;
			this.LC_Items.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LC_Items.HighlightOnHover = true;
			this.LC_Items.Location = new System.Drawing.Point(0, 227);
			this.LC_Items.Name = "LC_Items";
			this.LC_Items.SeparateWithLines = true;
			this.LC_Items.Size = new System.Drawing.Size(783, 211);
			this.LC_Items.TabIndex = 18;
			// 
			// DD_Tags
			// 
			this.DD_Tags.Cursor = System.Windows.Forms.Cursors.Hand;
			this.DD_Tags.Font = new System.Drawing.Font("Nirmala UI", 15F);
			this.DD_Tags.Location = new System.Drawing.Point(3, 53);
			this.DD_Tags.Name = "DD_Tags";
			this.DD_Tags.Padding = new System.Windows.Forms.Padding(7);
			this.DD_Tags.Size = new System.Drawing.Size(212, 58);
			this.DD_Tags.TabIndex = 4;
			// 
			// PC_MissingPackages
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.LC_Items);
			this.Controls.Add(this.slickSpacer1);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this.tableLayoutPanel2);
			this.LabelBounds = new System.Drawing.Point(-2, 3);
			this.Name = "PC_MissingPackages";
			this.Padding = new System.Windows.Forms.Padding(0, 30, 0, 0);
			this.Controls.SetChildIndex(this.tableLayoutPanel2, 0);
			this.Controls.SetChildIndex(this.base_Text, 0);
			this.Controls.SetChildIndex(this.tableLayoutPanel1, 0);
			this.Controls.SetChildIndex(this.slickSpacer1, 0);
			this.Controls.SetChildIndex(this.LC_Items, 0);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	private Lists.GenericPackageListControl LC_Items;
	private SlickControls.SlickSpacer slickSpacer1;
	private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	private System.Windows.Forms.Label L_Counts;
	private SlickControls.SlickTab T_All;
	private SlickControls.SlickTab T_Mods;
	private SlickControls.SlickTab T_Assets;
	private SlickControls.SlickTextBox TB_Search;
	private SlickControls.SlickButton B_SteamPage;
	private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
	private Generic.ThreeOptionToggle OT_Workshop;
	private Dropdowns.TagsDropDown DD_Tags;
}
