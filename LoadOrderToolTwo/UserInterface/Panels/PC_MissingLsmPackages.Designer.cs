using LoadOrderToolTwo.Utilities.Managers;

namespace LoadOrderToolTwo.UserInterface.Panels;

partial class PC_MissingLsmPackages
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
			CentralManager.AssetInformationUpdated -= CentralManager_ContentLoaded;
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
			this.TB_Search = new SlickControls.SlickTextBox();
			this.B_SubscribeAll = new SlickControls.SlickButton();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.B_IncludeAll = new SlickControls.SlickButton();
			this.LC_Items = new LoadOrderToolTwo.UserInterface.Lists.GenericPackageListControl();
			this.DD_Tags = new LoadOrderToolTwo.UserInterface.Dropdowns.TagsDropDown();
			this.L_Counts = new System.Windows.Forms.Label();
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
			this.slickSpacer1.Location = new System.Drawing.Point(0, 185);
			this.slickSpacer1.Margin = new System.Windows.Forms.Padding(0);
			this.slickSpacer1.Name = "slickSpacer1";
			this.slickSpacer1.Size = new System.Drawing.Size(783, 2);
			this.slickSpacer1.TabIndex = 19;
			this.slickSpacer1.TabStop = false;
			this.slickSpacer1.Text = "slickSpacer1";
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
			// B_SubscribeAll
			// 
			this.B_SubscribeAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.B_SubscribeAll.ColorShade = null;
			this.B_SubscribeAll.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_SubscribeAll.Location = new System.Drawing.Point(425, 3);
			this.B_SubscribeAll.Name = "B_SubscribeAll";
			this.B_SubscribeAll.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);
			this.B_SubscribeAll.Size = new System.Drawing.Size(355, 30);
			this.B_SubscribeAll.SpaceTriggersClick = true;
			this.B_SubscribeAll.TabIndex = 2;
			this.B_SubscribeAll.Text = "SubscribeAllButton";
			this.B_SubscribeAll.Click += new System.EventHandler(this.B_SteamPage_Click);
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.AutoSize = true;
			this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.Controls.Add(this.L_Counts, 1, 2);
			this.tableLayoutPanel2.Controls.Add(this.DD_Tags, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.TB_Search, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.B_SubscribeAll, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.B_IncludeAll, 1, 1);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 30);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 3;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.Size = new System.Drawing.Size(783, 157);
			this.tableLayoutPanel2.TabIndex = 0;
			// 
			// B_IncludeAll
			// 
			this.B_IncludeAll.ColorShade = null;
			this.B_IncludeAll.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_IncludeAll.Location = new System.Drawing.Point(425, 53);
			this.B_IncludeAll.Name = "B_IncludeAll";
			this.B_IncludeAll.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);
			this.B_IncludeAll.Size = new System.Drawing.Size(343, 30);
			this.B_IncludeAll.SpaceTriggersClick = true;
			this.B_IncludeAll.TabIndex = 2;
			this.B_IncludeAll.Text = "IncludeAllButton";
			this.B_IncludeAll.Click += new System.EventHandler(this.B_IncludeAll_Click);
			// 
			// LC_Items
			// 
			this.LC_Items.AutoInvalidate = false;
			this.LC_Items.AutoScroll = true;
			this.LC_Items.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LC_Items.HighlightOnHover = true;
			this.LC_Items.Location = new System.Drawing.Point(0, 187);
			this.LC_Items.Name = "LC_Items";
			this.LC_Items.SeparateWithLines = true;
			this.LC_Items.Size = new System.Drawing.Size(783, 251);
			this.LC_Items.TabIndex = 18;
			// 
			// DD_Tags
			// 
			this.DD_Tags.Cursor = System.Windows.Forms.Cursors.Hand;
			this.DD_Tags.Font = new System.Drawing.Font("Nirmala UI", 15F);
			this.DD_Tags.Location = new System.Drawing.Point(7, 57);
			this.DD_Tags.Margin = new System.Windows.Forms.Padding(7);
			this.DD_Tags.Name = "DD_Tags";
			this.DD_Tags.Padding = new System.Windows.Forms.Padding(7);
			this.tableLayoutPanel2.SetRowSpan(this.DD_Tags, 2);
			this.DD_Tags.Size = new System.Drawing.Size(212, 66);
			this.DD_Tags.TabIndex = 3;
			this.DD_Tags.SelectedItemChanged += new System.EventHandler(this.OT_Workshop_SelectedValueChanged);
			// 
			// L_Counts
			// 
			this.L_Counts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.L_Counts.AutoSize = true;
			this.L_Counts.Location = new System.Drawing.Point(362, 53);
			this.L_Counts.Name = "L_Counts";
			this.L_Counts.Size = new System.Drawing.Size(59, 25);
			this.L_Counts.TabIndex = 4;
			this.L_Counts.Text = "label1";
			this.L_Counts.UseMnemonic = false;
			// 
			// PC_MissingLsmPackages
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.LC_Items);
			this.Controls.Add(this.slickSpacer1);
			this.Controls.Add(this.tableLayoutPanel2);
			this.LabelBounds = new System.Drawing.Point(-2, 3);
			this.Name = "PC_MissingLsmPackages";
			this.Padding = new System.Windows.Forms.Padding(0, 30, 0, 0);
			this.Controls.SetChildIndex(this.base_Text, 0);
			this.Controls.SetChildIndex(this.tableLayoutPanel2, 0);
			this.Controls.SetChildIndex(this.slickSpacer1, 0);
			this.Controls.SetChildIndex(this.LC_Items, 0);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	private Lists.GenericPackageListControl LC_Items;
	private SlickControls.SlickSpacer slickSpacer1;
	private SlickControls.SlickTextBox TB_Search;
	private SlickControls.SlickButton B_SubscribeAll;
	private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
	private SlickControls.SlickButton B_IncludeAll;
	private Dropdowns.TagsDropDown DD_Tags;
	private System.Windows.Forms.Label L_Counts;
}
