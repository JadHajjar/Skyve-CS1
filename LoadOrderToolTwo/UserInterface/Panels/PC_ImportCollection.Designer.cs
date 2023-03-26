using LoadOrderToolTwo.UserInterface.Content;
using LoadOrderToolTwo.UserInterface.Generic;

namespace LoadOrderToolTwo.UserInterface.Panels;

partial class PC_ImportCollection
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
			this.TLP_Top = new System.Windows.Forms.TableLayoutPanel();
			this.panel2 = new System.Windows.Forms.TableLayoutPanel();
			this.panel1 = new System.Windows.Forms.Panel();
			this.L_Title = new System.Windows.Forms.Label();
			this.P_Back = new System.Windows.Forms.Panel();
			this.slickSpacer1 = new SlickControls.SlickSpacer();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.T_All = new SlickControls.SlickTab();
			this.T_Mods = new SlickControls.SlickTab();
			this.T_Assets = new SlickControls.SlickTab();
			this.L_Counts = new System.Windows.Forms.Label();
			this.TB_Search = new SlickControls.SlickTextBox();
			this.B_SteamPage = new SlickControls.SlickButton();
			this.LC_Items = new LoadOrderToolTwo.UserInterface.Lists.GenericPackageListControl();
			this.B_ExInclude = new LoadOrderToolTwo.UserInterface.Generic.DoubleButton();
			this.B_UnsubSub = new LoadOrderToolTwo.UserInterface.Generic.DoubleButton();
			this.PB_Icon = new LoadOrderToolTwo.UserInterface.Content.PackageIcon();
			this.TLP_Top.SuspendLayout();
			this.panel2.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// base_Text
			// 
			this.base_Text.Location = new System.Drawing.Point(-2, 3);
			// 
			// TLP_Top
			// 
			this.TLP_Top.ColumnCount = 4;
			this.TLP_Top.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.TLP_Top.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Top.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TLP_Top.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Top.Controls.Add(this.L_Title, 2, 0);
			this.TLP_Top.Controls.Add(this.panel2, 2, 1);
			this.TLP_Top.Controls.Add(this.panel1, 0, 2);
			this.TLP_Top.Controls.Add(this.PB_Icon, 1, 0);
			this.TLP_Top.Controls.Add(this.P_Back, 0, 1);
			this.TLP_Top.Controls.Add(this.B_SteamPage, 3, 0);
			this.TLP_Top.Dock = System.Windows.Forms.DockStyle.Top;
			this.TLP_Top.Location = new System.Drawing.Point(0, 30);
			this.TLP_Top.Margin = new System.Windows.Forms.Padding(0);
			this.TLP_Top.Name = "TLP_Top";
			this.TLP_Top.RowCount = 3;
			this.TLP_Top.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TLP_Top.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TLP_Top.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
			this.TLP_Top.Size = new System.Drawing.Size(783, 100);
			this.TLP_Top.TabIndex = 13;
			// 
			// panel2
			// 
			this.panel2.ColumnCount = 2;
			this.TLP_Top.SetColumnSpan(this.panel2, 2);
			this.panel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.panel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.panel2.Controls.Add(this.TB_Search, 0, 0);
			this.panel2.Controls.Add(this.B_ExInclude, 1, 1);
			this.panel2.Controls.Add(this.B_UnsubSub, 1, 0);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(132, 45);
			this.panel2.Margin = new System.Windows.Forms.Padding(0);
			this.panel2.Name = "panel2";
			this.panel2.RowCount = 2;
			this.TLP_Top.SetRowSpan(this.panel2, 2);
			this.panel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.panel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.panel2.Size = new System.Drawing.Size(651, 55);
			this.panel2.TabIndex = 6;
			// 
			// panel1
			// 
			this.TLP_Top.SetColumnSpan(this.panel1, 2);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 90);
			this.panel1.Margin = new System.Windows.Forms.Padding(0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(132, 10);
			this.panel1.TabIndex = 5;
			// 
			// L_Title
			// 
			this.L_Title.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.L_Title.AutoSize = true;
			this.L_Title.Location = new System.Drawing.Point(135, 22);
			this.L_Title.Name = "L_Title";
			this.L_Title.Size = new System.Drawing.Size(55, 23);
			this.L_Title.TabIndex = 1;
			this.L_Title.Text = "label1";
			this.L_Title.UseMnemonic = false;
			// 
			// P_Back
			// 
			this.P_Back.Dock = System.Windows.Forms.DockStyle.Fill;
			this.P_Back.Location = new System.Drawing.Point(0, 45);
			this.P_Back.Margin = new System.Windows.Forms.Padding(0);
			this.P_Back.Name = "P_Back";
			this.P_Back.Size = new System.Drawing.Size(32, 45);
			this.P_Back.TabIndex = 2;
			// 
			// slickSpacer1
			// 
			this.slickSpacer1.Dock = System.Windows.Forms.DockStyle.Top;
			this.slickSpacer1.Location = new System.Drawing.Point(0, 166);
			this.slickSpacer1.Margin = new System.Windows.Forms.Padding(0);
			this.slickSpacer1.Name = "slickSpacer1";
			this.slickSpacer1.Size = new System.Drawing.Size(783, 2);
			this.slickSpacer1.TabIndex = 15;
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
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 130);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(783, 36);
			this.tableLayoutPanel1.TabIndex = 16;
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
			this.T_All.TabIndex = 17;
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
			this.T_Mods.TabIndex = 17;
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
			this.T_Assets.TabIndex = 17;
			this.T_Assets.TabStop = false;
			this.T_Assets.Text = "Assets";
			this.T_Assets.TabSelected += new System.EventHandler(this.T_Assets_TabSelected);
			// 
			// L_Counts
			// 
			this.L_Counts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.L_Counts.AutoSize = true;
			this.L_Counts.Location = new System.Drawing.Point(725, 13);
			this.L_Counts.Name = "L_Counts";
			this.L_Counts.Size = new System.Drawing.Size(55, 23);
			this.L_Counts.TabIndex = 1;
			this.L_Counts.Text = "label1";
			this.L_Counts.UseMnemonic = false;
			// 
			// TB_Search
			// 
			this.TB_Search.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.TB_Search.Image = global::LoadOrderToolTwo.Properties.Resources.I_Search;
			this.TB_Search.LabelText = "Search";
			this.TB_Search.Location = new System.Drawing.Point(3, 5);
			this.TB_Search.Name = "TB_Search";
			this.TB_Search.Placeholder = "SearchCollection";
			this.panel2.SetRowSpan(this.TB_Search, 2);
			this.TB_Search.SelectedText = "";
			this.TB_Search.SelectionLength = 0;
			this.TB_Search.SelectionStart = 0;
			this.TB_Search.Size = new System.Drawing.Size(140, 47);
			this.TB_Search.TabIndex = 18;
			this.TB_Search.TextChanged += new System.EventHandler(this.TB_Search_TextChanged);
			this.TB_Search.IconClicked += new System.EventHandler(this.TB_Search_IconClicked);
			// 
			// B_SteamPage
			// 
			this.B_SteamPage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.B_SteamPage.ColorShade = null;
			this.B_SteamPage.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_SteamPage.Image = global::LoadOrderToolTwo.Properties.Resources.I_Steam;
			this.B_SteamPage.Location = new System.Drawing.Point(749, 15);
			this.B_SteamPage.Name = "B_SteamPage";
			this.B_SteamPage.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);
			this.B_SteamPage.Size = new System.Drawing.Size(31, 27);
			this.B_SteamPage.SpaceTriggersClick = true;
			this.B_SteamPage.TabIndex = 4;
			this.B_SteamPage.Click += new System.EventHandler(this.B_SteamPage_Click);
			// 
			// LC_Items
			// 
			this.LC_Items.AutoInvalidate = false;
			this.LC_Items.AutoScroll = true;
			this.LC_Items.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LC_Items.HighlightOnHover = true;
			this.LC_Items.Location = new System.Drawing.Point(0, 168);
			this.LC_Items.Name = "LC_Items";
			this.LC_Items.SeparateWithLines = true;
			this.LC_Items.Size = new System.Drawing.Size(783, 270);
			this.LC_Items.TabIndex = 14;
			// 
			// B_ExInclude
			// 
			this.B_ExInclude.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.B_ExInclude.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_ExInclude.Image1 = "I_X";
			this.B_ExInclude.Image2 = "I_Check";
			this.B_ExInclude.Location = new System.Drawing.Point(328, 30);
			this.B_ExInclude.Name = "B_ExInclude";
			this.B_ExInclude.Option1 = "ExcludeAll";
			this.B_ExInclude.Option2 = "IncludeAll";
			this.B_ExInclude.Size = new System.Drawing.Size(320, 22);
			this.B_ExInclude.TabIndex = 1;
			this.B_ExInclude.LeftClicked += new System.EventHandler(this.B_ExInclude_LeftClicked);
			this.B_ExInclude.RightClicked += new System.EventHandler(this.B_ExInclude_RightClicked);
			// 
			// B_UnsubSub
			// 
			this.B_UnsubSub.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.B_UnsubSub.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_UnsubSub.Image1 = "I_RemoveSteam";
			this.B_UnsubSub.Image2 = "I_Add";
			this.B_UnsubSub.Location = new System.Drawing.Point(328, 3);
			this.B_UnsubSub.Name = "B_UnsubSub";
			this.B_UnsubSub.Option1 = "SubscribeAll";
			this.B_UnsubSub.Option2 = "UnsubscribeAll";
			this.B_UnsubSub.Size = new System.Drawing.Size(320, 21);
			this.B_UnsubSub.TabIndex = 19;
			this.B_UnsubSub.LeftClicked += new System.EventHandler(this.B_UnsubSub_LeftClicked);
			this.B_UnsubSub.RightClicked += new System.EventHandler(this.B_UnsubSub_RightClicked);
			// 
			// PB_Icon
			// 
			this.PB_Icon.Dock = System.Windows.Forms.DockStyle.Left;
			this.PB_Icon.Location = new System.Drawing.Point(32, 0);
			this.PB_Icon.Margin = new System.Windows.Forms.Padding(0);
			this.PB_Icon.Name = "PB_Icon";
			this.TLP_Top.SetRowSpan(this.PB_Icon, 2);
			this.PB_Icon.Size = new System.Drawing.Size(100, 90);
			this.PB_Icon.TabIndex = 0;
			this.PB_Icon.TabStop = false;
			// 
			// PC_ImportCollection
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.LC_Items);
			this.Controls.Add(this.slickSpacer1);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this.TLP_Top);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(58)))), ((int)(((byte)(69)))));
			this.LabelBounds = new System.Drawing.Point(-2, 3);
			this.Name = "PC_ImportCollection";
			this.Padding = new System.Windows.Forms.Padding(0, 30, 0, 0);
			this.Controls.SetChildIndex(this.base_Text, 0);
			this.Controls.SetChildIndex(this.TLP_Top, 0);
			this.Controls.SetChildIndex(this.tableLayoutPanel1, 0);
			this.Controls.SetChildIndex(this.slickSpacer1, 0);
			this.Controls.SetChildIndex(this.LC_Items, 0);
			this.TLP_Top.ResumeLayout(false);
			this.TLP_Top.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	private System.Windows.Forms.TableLayoutPanel TLP_Top;
	private PackageIcon PB_Icon;
	private System.Windows.Forms.Label L_Title;
	private System.Windows.Forms.Panel P_Back;
	private SlickControls.SlickButton B_SteamPage;
	private System.Windows.Forms.TableLayoutPanel panel2;
	private System.Windows.Forms.Panel panel1;
	private DoubleButton B_ExInclude;
	private Lists.GenericPackageListControl LC_Items;
	private SlickControls.SlickSpacer slickSpacer1;
	private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	private SlickControls.SlickTab T_All;
	private SlickControls.SlickTab T_Mods;
	private SlickControls.SlickTab T_Assets;
	private SlickControls.SlickTextBox TB_Search;
	private System.Windows.Forms.Label L_Counts;
	private DoubleButton B_UnsubSub;
}
