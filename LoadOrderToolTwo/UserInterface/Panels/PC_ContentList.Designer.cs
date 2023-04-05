using LoadOrderToolTwo.UserInterface.Dropdowns;
using LoadOrderToolTwo.UserInterface.Generic;
using LoadOrderToolTwo.Utilities.Managers;

namespace LoadOrderToolTwo.UserInterface.Panels;

partial class PC_ContentList<T>
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
			CentralManager.WorkshopInfoUpdated -= CentralManager_WorkshopInfoUpdated;
			CentralManager.PackageInformationUpdated -= CentralManager_WorkshopInfoUpdated;
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
			this.TLP_Main = new System.Windows.Forms.TableLayoutPanel();
			this.P_ActionsContainer = new System.Windows.Forms.Panel();
			this.P_Actions = new SlickControls.RoundedGroupPanel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.B_DisEnable = new LoadOrderToolTwo.UserInterface.Generic.DoubleButton();
			this.B_ExInclude = new LoadOrderToolTwo.UserInterface.Generic.DoubleButton();
			this.slickSpacer2 = new SlickControls.SlickSpacer();
			this.slickSpacer1 = new SlickControls.SlickSpacer();
			this.B_Refresh = new SlickControls.SlickButton();
			this.TB_Search = new SlickControls.SlickTextBox();
			this.DD_Sorting = new LoadOrderToolTwo.UserInterface.Dropdowns.SortingDropDown();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.L_Duplicates = new System.Windows.Forms.Label();
			this.L_Counts = new System.Windows.Forms.Label();
			this.L_FilterCount = new System.Windows.Forms.Label();
			this.P_FiltersContainer = new System.Windows.Forms.Panel();
			this.P_Filters = new SlickControls.RoundedGroupPanel();
			this.I_ClearFilters = new SlickControls.SlickIcon();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
			this.DR_SubscribeTime = new SlickControls.SlickDateRange();
			this.OT_Workshop = new LoadOrderToolTwo.UserInterface.Generic.ThreeOptionToggle();
			this.OT_Included = new LoadOrderToolTwo.UserInterface.Generic.ThreeOptionToggle();
			this.OT_Enabled = new LoadOrderToolTwo.UserInterface.Generic.ThreeOptionToggle();
			this.DR_ServerTime = new SlickControls.SlickDateRange();
			this.DD_PackageStatus = new LoadOrderToolTwo.UserInterface.Dropdowns.PackageStatusDropDown();
			this.DD_ReportSeverity = new LoadOrderToolTwo.UserInterface.Dropdowns.ReportSeverityDropDown();
			this.DD_Tags = new LoadOrderToolTwo.UserInterface.Dropdowns.TagsDropDown();
			this.DD_Profile = new LoadOrderToolTwo.UserInterface.Dropdowns.ProfilesDropDown();
			this.B_Filters = new SlickControls.SlickButton();
			this.B_Actions = new SlickControls.SlickButton();
			this.TLP_Main.SuspendLayout();
			this.P_ActionsContainer.SuspendLayout();
			this.P_Actions.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			this.P_FiltersContainer.SuspendLayout();
			this.P_Filters.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel4.SuspendLayout();
			this.SuspendLayout();
			// 
			// base_Text
			// 
			this.base_Text.Location = new System.Drawing.Point(-2, 3);
			this.base_Text.Size = new System.Drawing.Size(49, 26);
			this.base_Text.Text = "Mods";
			// 
			// TLP_Main
			// 
			this.TLP_Main.ColumnCount = 5;
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Main.Controls.Add(this.P_ActionsContainer, 0, 2);
			this.TLP_Main.Controls.Add(this.slickSpacer2, 0, 3);
			this.TLP_Main.Controls.Add(this.slickSpacer1, 0, 5);
			this.TLP_Main.Controls.Add(this.B_Refresh, 1, 0);
			this.TLP_Main.Controls.Add(this.TB_Search, 0, 0);
			this.TLP_Main.Controls.Add(this.DD_Sorting, 4, 0);
			this.TLP_Main.Controls.Add(this.tableLayoutPanel3, 0, 4);
			this.TLP_Main.Controls.Add(this.P_FiltersContainer, 0, 1);
			this.TLP_Main.Controls.Add(this.B_Filters, 2, 0);
			this.TLP_Main.Controls.Add(this.B_Actions, 3, 0);
			this.TLP_Main.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TLP_Main.Location = new System.Drawing.Point(0, 30);
			this.TLP_Main.Name = "TLP_Main";
			this.TLP_Main.RowCount = 7;
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TLP_Main.Size = new System.Drawing.Size(895, 486);
			this.TLP_Main.TabIndex = 0;
			// 
			// P_ActionsContainer
			// 
			this.TLP_Main.SetColumnSpan(this.P_ActionsContainer, 5);
			this.P_ActionsContainer.Controls.Add(this.P_Actions);
			this.P_ActionsContainer.Dock = System.Windows.Forms.DockStyle.Top;
			this.P_ActionsContainer.Location = new System.Drawing.Point(0, 280);
			this.P_ActionsContainer.Margin = new System.Windows.Forms.Padding(0);
			this.P_ActionsContainer.Name = "P_ActionsContainer";
			this.P_ActionsContainer.Size = new System.Drawing.Size(895, 127);
			this.P_ActionsContainer.TabIndex = 2;
			this.P_ActionsContainer.Visible = false;
			// 
			// P_Actions
			// 
			this.P_Actions.AddOutline = true;
			this.P_Actions.AutoSize = true;
			this.P_Actions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.P_Actions.Controls.Add(this.tableLayoutPanel2);
			this.P_Actions.Dock = System.Windows.Forms.DockStyle.Top;
			this.P_Actions.Location = new System.Drawing.Point(0, 0);
			this.P_Actions.Name = "P_Actions";
			this.P_Actions.Padding = new System.Windows.Forms.Padding(7, 38, 7, 7);
			this.P_Actions.Size = new System.Drawing.Size(895, 91);
			this.P_Actions.TabIndex = 3;
			this.P_Actions.Text = "Actions";
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.AutoSize = true;
			this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel2.Controls.Add(this.B_DisEnable, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.B_ExInclude, 0, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(7, 38);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(881, 46);
			this.tableLayoutPanel2.TabIndex = 0;
			// 
			// B_DisEnable
			// 
			this.B_DisEnable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.B_DisEnable.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_DisEnable.Image1 = "I_Disabled";
			this.B_DisEnable.Image2 = "I_Enabled";
			this.B_DisEnable.Location = new System.Drawing.Point(443, 3);
			this.B_DisEnable.Name = "B_DisEnable";
			this.B_DisEnable.Option1 = "DisableAll";
			this.B_DisEnable.Option2 = "EnableAll";
			this.B_DisEnable.Size = new System.Drawing.Size(435, 40);
			this.B_DisEnable.TabIndex = 1;
			this.B_DisEnable.LeftClicked += new System.EventHandler(this.B_DisEnable_LeftClicked);
			this.B_DisEnable.RightClicked += new System.EventHandler(this.B_DisEnable_RightClicked);
			// 
			// B_ExInclude
			// 
			this.B_ExInclude.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.B_ExInclude.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_ExInclude.Image1 = "I_X";
			this.B_ExInclude.Image2 = "I_Check";
			this.B_ExInclude.Location = new System.Drawing.Point(3, 3);
			this.B_ExInclude.Name = "B_ExInclude";
			this.B_ExInclude.Option1 = "ExcludeAll";
			this.B_ExInclude.Option2 = "IncludeAll";
			this.B_ExInclude.Size = new System.Drawing.Size(434, 40);
			this.B_ExInclude.TabIndex = 1;
			this.B_ExInclude.LeftClicked += new System.EventHandler(this.B_ExInclude_LeftClicked);
			this.B_ExInclude.RightClicked += new System.EventHandler(this.B_ExInclude_RightClicked);
			// 
			// slickSpacer2
			// 
			this.TLP_Main.SetColumnSpan(this.slickSpacer2, 5);
			this.slickSpacer2.Dock = System.Windows.Forms.DockStyle.Top;
			this.slickSpacer2.Location = new System.Drawing.Point(0, 407);
			this.slickSpacer2.Margin = new System.Windows.Forms.Padding(0);
			this.slickSpacer2.Name = "slickSpacer2";
			this.slickSpacer2.Size = new System.Drawing.Size(895, 2);
			this.slickSpacer2.TabIndex = 8;
			this.slickSpacer2.TabStop = false;
			this.slickSpacer2.Text = "slickSpacer2";
			// 
			// slickSpacer1
			// 
			this.TLP_Main.SetColumnSpan(this.slickSpacer1, 5);
			this.slickSpacer1.Dock = System.Windows.Forms.DockStyle.Top;
			this.slickSpacer1.Location = new System.Drawing.Point(0, 455);
			this.slickSpacer1.Margin = new System.Windows.Forms.Padding(0);
			this.slickSpacer1.Name = "slickSpacer1";
			this.slickSpacer1.Size = new System.Drawing.Size(895, 2);
			this.slickSpacer1.TabIndex = 7;
			this.slickSpacer1.TabStop = false;
			this.slickSpacer1.Text = "slickSpacer1";
			// 
			// B_Refresh
			// 
			this.B_Refresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.B_Refresh.ColorShade = null;
			this.B_Refresh.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_Refresh.Location = new System.Drawing.Point(282, 3);
			this.B_Refresh.Name = "B_Refresh";
			this.B_Refresh.Size = new System.Drawing.Size(100, 47);
			this.B_Refresh.SpaceTriggersClick = true;
			this.B_Refresh.TabIndex = 1;
			this.B_Refresh.Click += new System.EventHandler(this.FilterChanged);
			// 
			// TB_Search
			// 
			this.TB_Search.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.TB_Search.Image = global::LoadOrderToolTwo.Properties.Resources.I_Search;
			this.TB_Search.LabelText = "Search";
			this.TB_Search.Location = new System.Drawing.Point(3, 3);
			this.TB_Search.Name = "TB_Search";
			this.TB_Search.Placeholder = "SearchPackages";
			this.TB_Search.SelectedText = "";
			this.TB_Search.SelectionLength = 0;
			this.TB_Search.SelectionStart = 0;
			this.TB_Search.Size = new System.Drawing.Size(140, 47);
			this.TB_Search.TabIndex = 0;
			this.TB_Search.TextChanged += new System.EventHandler(this.TB_Search_TextChanged);
			this.TB_Search.IconClicked += new System.EventHandler(this.TB_Search_IconClicked);
			// 
			// DD_Sorting
			// 
			this.DD_Sorting.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DD_Sorting.Cursor = System.Windows.Forms.Cursors.Hand;
			this.DD_Sorting.Font = new System.Drawing.Font("Nirmala UI", 15F);
			this.DD_Sorting.Location = new System.Drawing.Point(604, 7);
			this.DD_Sorting.Margin = new System.Windows.Forms.Padding(7);
			this.DD_Sorting.Name = "DD_Sorting";
			this.DD_Sorting.Padding = new System.Windows.Forms.Padding(7);
			this.DD_Sorting.Size = new System.Drawing.Size(284, 39);
			this.DD_Sorting.TabIndex = 3;
			this.DD_Sorting.SelectedItemChanged += new System.EventHandler(this.DD_Sorting_SelectedItemChanged);
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.AutoSize = true;
			this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel3.ColumnCount = 2;
			this.TLP_Main.SetColumnSpan(this.tableLayoutPanel3, 5);
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.Controls.Add(this.L_Duplicates, 0, 0);
			this.tableLayoutPanel3.Controls.Add(this.L_Counts, 1, 0);
			this.tableLayoutPanel3.Controls.Add(this.L_FilterCount, 0, 1);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Top;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 409);
			this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 2;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel3.Size = new System.Drawing.Size(895, 46);
			this.tableLayoutPanel3.TabIndex = 6;
			// 
			// L_Duplicates
			// 
			this.L_Duplicates.AutoSize = true;
			this.L_Duplicates.Cursor = System.Windows.Forms.Cursors.Hand;
			this.L_Duplicates.Location = new System.Drawing.Point(3, 0);
			this.L_Duplicates.Name = "L_Duplicates";
			this.L_Duplicates.Size = new System.Drawing.Size(55, 23);
			this.L_Duplicates.TabIndex = 2;
			this.L_Duplicates.Tag = "NoMouseDown";
			this.L_Duplicates.Text = "label1";
			this.L_Duplicates.Click += new System.EventHandler(this.L_Duplicates_Click);
			// 
			// L_Counts
			// 
			this.L_Counts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.L_Counts.AutoSize = true;
			this.L_Counts.Location = new System.Drawing.Point(837, 0);
			this.L_Counts.Name = "L_Counts";
			this.tableLayoutPanel3.SetRowSpan(this.L_Counts, 2);
			this.L_Counts.Size = new System.Drawing.Size(55, 23);
			this.L_Counts.TabIndex = 1;
			this.L_Counts.Text = "label1";
			// 
			// L_FilterCount
			// 
			this.L_FilterCount.AutoSize = true;
			this.L_FilterCount.Location = new System.Drawing.Point(3, 23);
			this.L_FilterCount.Name = "L_FilterCount";
			this.L_FilterCount.Size = new System.Drawing.Size(55, 23);
			this.L_FilterCount.TabIndex = 2;
			this.L_FilterCount.Text = "label1";
			// 
			// P_FiltersContainer
			// 
			this.TLP_Main.SetColumnSpan(this.P_FiltersContainer, 5);
			this.P_FiltersContainer.Controls.Add(this.P_Filters);
			this.P_FiltersContainer.Dock = System.Windows.Forms.DockStyle.Top;
			this.P_FiltersContainer.Location = new System.Drawing.Point(0, 53);
			this.P_FiltersContainer.Margin = new System.Windows.Forms.Padding(0);
			this.P_FiltersContainer.Name = "P_FiltersContainer";
			this.P_FiltersContainer.Size = new System.Drawing.Size(895, 227);
			this.P_FiltersContainer.TabIndex = 1;
			this.P_FiltersContainer.Visible = false;
			// 
			// P_Filters
			// 
			this.P_Filters.AddOutline = true;
			this.P_Filters.AutoSize = true;
			this.P_Filters.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.P_Filters.Controls.Add(this.I_ClearFilters);
			this.P_Filters.Controls.Add(this.tableLayoutPanel1);
			this.P_Filters.Dock = System.Windows.Forms.DockStyle.Top;
			this.P_Filters.Location = new System.Drawing.Point(0, 0);
			this.P_Filters.Name = "P_Filters";
			this.P_Filters.Padding = new System.Windows.Forms.Padding(7, 38, 7, 7);
			this.P_Filters.Size = new System.Drawing.Size(895, 257);
			this.P_Filters.TabIndex = 3;
			this.P_Filters.Text = "Filters";
			// 
			// I_ClearFilters
			// 
			this.I_ClearFilters.ActiveColor = null;
			this.I_ClearFilters.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.I_ClearFilters.ColorStyle = Extensions.ColorStyle.Red;
			this.I_ClearFilters.Cursor = System.Windows.Forms.Cursors.Hand;
			this.I_ClearFilters.Location = new System.Drawing.Point(855, 1);
			this.I_ClearFilters.Name = "I_ClearFilters";
			this.I_ClearFilters.Size = new System.Drawing.Size(32, 32);
			this.I_ClearFilters.TabIndex = 1;
			this.I_ClearFilters.Click += new System.EventHandler(this.I_ClearFilters_Click);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel4, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.DD_PackageStatus, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.DD_ReportSeverity, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.DD_Tags, 2, 1);
			this.tableLayoutPanel1.Controls.Add(this.DD_Profile, 3, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(7, 38);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(881, 212);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// tableLayoutPanel4
			// 
			this.tableLayoutPanel4.AutoSize = true;
			this.tableLayoutPanel4.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel4.ColumnCount = 2;
			this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel4, 4);
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel4.Controls.Add(this.DR_SubscribeTime, 1, 0);
			this.tableLayoutPanel4.Controls.Add(this.OT_Workshop, 0, 2);
			this.tableLayoutPanel4.Controls.Add(this.OT_Included, 0, 0);
			this.tableLayoutPanel4.Controls.Add(this.OT_Enabled, 0, 1);
			this.tableLayoutPanel4.Controls.Add(this.DR_ServerTime, 1, 2);
			this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Top;
			this.tableLayoutPanel4.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel4.Name = "tableLayoutPanel4";
			this.tableLayoutPanel4.RowCount = 4;
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel4.Size = new System.Drawing.Size(881, 138);
			this.tableLayoutPanel4.TabIndex = 4;
			// 
			// slickDateRange1
			// 
			this.DR_SubscribeTime.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DR_SubscribeTime.Cursor = System.Windows.Forms.Cursors.Hand;
			this.DR_SubscribeTime.Location = new System.Drawing.Point(443, 3);
			this.DR_SubscribeTime.Name = "slickDateRange1";
			this.DR_SubscribeTime.Size = new System.Drawing.Size(435, 40);
			this.DR_SubscribeTime.TabIndex = 14;
			this.DR_SubscribeTime.RangeChanged += new System.EventHandler(this.FilterChanged);
			// 
			// OT_Workshop
			// 
			this.OT_Workshop.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.OT_Workshop.Cursor = System.Windows.Forms.Cursors.Hand;
			this.OT_Workshop.Image1 = "I_Local";
			this.OT_Workshop.Image2 = "I_Steam";
			this.OT_Workshop.Location = new System.Drawing.Point(3, 95);
			this.OT_Workshop.Name = "OT_Workshop";
			this.OT_Workshop.Option1 = "Local";
			this.OT_Workshop.Option2 = "Workshop";
			this.OT_Workshop.OptionStyle1 = Extensions.ColorStyle.Active;
			this.OT_Workshop.OptionStyle2 = Extensions.ColorStyle.Active;
			this.OT_Workshop.Size = new System.Drawing.Size(434, 40);
			this.OT_Workshop.TabIndex = 2;
			this.OT_Workshop.SelectedValueChanged += new System.EventHandler(this.FilterChanged);
			// 
			// OT_Included
			// 
			this.OT_Included.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.OT_Included.Cursor = System.Windows.Forms.Cursors.Hand;
			this.OT_Included.Image1 = "I_X";
			this.OT_Included.Image2 = "I_Check";
			this.OT_Included.Location = new System.Drawing.Point(3, 3);
			this.OT_Included.Name = "OT_Included";
			this.OT_Included.Option1 = "Excluded";
			this.OT_Included.Option2 = "Included";
			this.OT_Included.Size = new System.Drawing.Size(434, 40);
			this.OT_Included.TabIndex = 0;
			this.OT_Included.SelectedValueChanged += new System.EventHandler(this.FilterChanged);
			// 
			// OT_Enabled
			// 
			this.OT_Enabled.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.OT_Enabled.Cursor = System.Windows.Forms.Cursors.Hand;
			this.OT_Enabled.Image1 = "I_Disabled";
			this.OT_Enabled.Image2 = "I_Enabled";
			this.OT_Enabled.Location = new System.Drawing.Point(3, 49);
			this.OT_Enabled.Name = "OT_Enabled";
			this.OT_Enabled.Option1 = "Disabled";
			this.OT_Enabled.Option2 = "Enabled";
			this.OT_Enabled.Size = new System.Drawing.Size(434, 40);
			this.OT_Enabled.TabIndex = 1;
			this.OT_Enabled.SelectedValueChanged += new System.EventHandler(this.FilterChanged);
			// 
			// slickDateRange2
			// 
			this.DR_ServerTime.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DR_ServerTime.Cursor = System.Windows.Forms.Cursors.Hand;
			this.DR_ServerTime.Location = new System.Drawing.Point(443, 95);
			this.DR_ServerTime.Name = "slickDateRange2";
			this.tableLayoutPanel4.SetRowSpan(this.DR_ServerTime, 2);
			this.DR_ServerTime.Size = new System.Drawing.Size(435, 40);
			this.DR_ServerTime.TabIndex = 14;
			this.DR_ServerTime.RangeChanged += new System.EventHandler(this.FilterChanged);
			// 
			// DD_PackageStatus
			// 
			this.DD_PackageStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DD_PackageStatus.Cursor = System.Windows.Forms.Cursors.Hand;
			this.DD_PackageStatus.Font = new System.Drawing.Font("Nirmala UI", 15F);
			this.DD_PackageStatus.Location = new System.Drawing.Point(7, 145);
			this.DD_PackageStatus.Margin = new System.Windows.Forms.Padding(7);
			this.DD_PackageStatus.Name = "DD_PackageStatus";
			this.DD_PackageStatus.Padding = new System.Windows.Forms.Padding(7);
			this.DD_PackageStatus.Size = new System.Drawing.Size(206, 60);
			this.DD_PackageStatus.TabIndex = 0;
			this.DD_PackageStatus.SelectedItemChanged += new System.EventHandler(this.FilterChanged);
			// 
			// DD_ReportSeverity
			// 
			this.DD_ReportSeverity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DD_ReportSeverity.Cursor = System.Windows.Forms.Cursors.Hand;
			this.DD_ReportSeverity.Font = new System.Drawing.Font("Nirmala UI", 15F);
			this.DD_ReportSeverity.Location = new System.Drawing.Point(227, 145);
			this.DD_ReportSeverity.Margin = new System.Windows.Forms.Padding(7);
			this.DD_ReportSeverity.Name = "DD_ReportSeverity";
			this.DD_ReportSeverity.Padding = new System.Windows.Forms.Padding(7);
			this.DD_ReportSeverity.Size = new System.Drawing.Size(206, 60);
			this.DD_ReportSeverity.TabIndex = 1;
			this.DD_ReportSeverity.SelectedItemChanged += new System.EventHandler(this.FilterChanged);
			// 
			// DD_Tags
			// 
			this.DD_Tags.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DD_Tags.Cursor = System.Windows.Forms.Cursors.Hand;
			this.DD_Tags.Font = new System.Drawing.Font("Nirmala UI", 15F);
			this.DD_Tags.Location = new System.Drawing.Point(447, 145);
			this.DD_Tags.Margin = new System.Windows.Forms.Padding(7);
			this.DD_Tags.Name = "DD_Tags";
			this.DD_Tags.Padding = new System.Windows.Forms.Padding(7);
			this.DD_Tags.Size = new System.Drawing.Size(206, 60);
			this.DD_Tags.TabIndex = 2;
			this.DD_Tags.SelectedItemChanged += new System.EventHandler(this.FilterChanged);
			// 
			// DD_Profile
			// 
			this.DD_Profile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DD_Profile.Cursor = System.Windows.Forms.Cursors.Hand;
			this.DD_Profile.Font = new System.Drawing.Font("Nirmala UI", 15F);
			this.DD_Profile.Location = new System.Drawing.Point(667, 145);
			this.DD_Profile.Margin = new System.Windows.Forms.Padding(7);
			this.DD_Profile.Name = "DD_Profile";
			this.DD_Profile.Padding = new System.Windows.Forms.Padding(7);
			this.DD_Profile.Size = new System.Drawing.Size(207, 60);
			this.DD_Profile.TabIndex = 2;
			this.DD_Profile.SelectedItemChanged += new System.EventHandler(this.FilterChanged);
			// 
			// B_Filters
			// 
			this.B_Filters.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.B_Filters.ColorShade = null;
			this.B_Filters.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_Filters.Location = new System.Drawing.Point(388, 3);
			this.B_Filters.Name = "B_Filters";
			this.B_Filters.Size = new System.Drawing.Size(100, 47);
			this.B_Filters.SpaceTriggersClick = true;
			this.B_Filters.TabIndex = 1;
			this.B_Filters.Text = "ShowFilters";
			this.B_Filters.Click += new System.EventHandler(this.B_Filters_Click);
			// 
			// B_Actions
			// 
			this.B_Actions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.B_Actions.ColorShade = null;
			this.B_Actions.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_Actions.Location = new System.Drawing.Point(494, 3);
			this.B_Actions.Name = "B_Actions";
			this.B_Actions.Size = new System.Drawing.Size(100, 47);
			this.B_Actions.SpaceTriggersClick = true;
			this.B_Actions.TabIndex = 2;
			this.B_Actions.Text = "ShowActions";
			this.B_Actions.Click += new System.EventHandler(this.B_Actions_Click);
			// 
			// PC_ContentList
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.TLP_Main);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(58)))), ((int)(((byte)(69)))));
			this.LabelBounds = new System.Drawing.Point(-2, 3);
			this.Name = "PC_ContentList";
			this.Padding = new System.Windows.Forms.Padding(0, 30, 0, 0);
			this.Size = new System.Drawing.Size(895, 516);
			this.Text = "Mods";
			this.Controls.SetChildIndex(this.base_Text, 0);
			this.Controls.SetChildIndex(this.TLP_Main, 0);
			this.TLP_Main.ResumeLayout(false);
			this.TLP_Main.PerformLayout();
			this.P_ActionsContainer.ResumeLayout(false);
			this.P_ActionsContainer.PerformLayout();
			this.P_Actions.ResumeLayout(false);
			this.P_Actions.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel3.ResumeLayout(false);
			this.tableLayoutPanel3.PerformLayout();
			this.P_FiltersContainer.ResumeLayout(false);
			this.P_FiltersContainer.PerformLayout();
			this.P_Filters.ResumeLayout(false);
			this.P_Filters.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.tableLayoutPanel4.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	private System.Windows.Forms.TableLayoutPanel TLP_Main;
	private SlickControls.SlickTextBox TB_Search;
	private ThreeOptionToggle OT_Workshop;
	private ThreeOptionToggle OT_Enabled;
	private ThreeOptionToggle OT_Included;
	private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	private SlickControls.RoundedGroupPanel P_Filters;
	private ReportSeverityDropDown DD_ReportSeverity;
	private PackageStatusDropDown DD_PackageStatus;
	private SlickControls.SlickIcon I_ClearFilters;
	private DoubleButton B_ExInclude;
	private DoubleButton B_DisEnable;
	private System.Windows.Forms.Label L_Counts;
	private SortingDropDown DD_Sorting;
	private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
	private SlickControls.SlickSpacer slickSpacer1;
	private System.Windows.Forms.Label L_Duplicates;
	private SlickControls.SlickSpacer slickSpacer2;
	private System.Windows.Forms.Panel P_FiltersContainer;
	private SlickControls.SlickButton B_Filters;
	private System.Windows.Forms.Panel P_ActionsContainer;
	private SlickControls.RoundedGroupPanel P_Actions;
	private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
	private SlickControls.SlickButton B_Actions;
	private System.Windows.Forms.Label L_FilterCount;
	private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
	private TagsDropDown DD_Tags;
	private ProfilesDropDown DD_Profile;
	private SlickControls.SlickButton B_Refresh;
	private SlickControls.SlickDateRange DR_SubscribeTime;
	private SlickControls.SlickDateRange DR_ServerTime;
}
