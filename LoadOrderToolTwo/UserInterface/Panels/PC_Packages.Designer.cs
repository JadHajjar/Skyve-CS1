using LoadOrderToolTwo.Utilities.Managers;

namespace LoadOrderToolTwo.UserInterface.Panels;

partial class PC_Packages
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PC_Packages));
			this.TLP_Main = new System.Windows.Forms.TableLayoutPanel();
			this.P_ActionsContainer = new System.Windows.Forms.Panel();
			this.P_Actions = new SlickControls.RoundedGroupPanel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.B_DisEnable = new LoadOrderToolTwo.UserInterface.DoubleButton();
			this.B_ExInclude = new LoadOrderToolTwo.UserInterface.DoubleButton();
			this.slickSpacer2 = new SlickControls.SlickSpacer();
			this.slickSpacer1 = new SlickControls.SlickSpacer();
			this.TB_Search = new SlickControls.SlickTextBox();
			this.DD_Sorting = new LoadOrderToolTwo.UserInterface.SortingDropDown();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.L_Duplicates = new System.Windows.Forms.Label();
			this.L_Counts = new System.Windows.Forms.Label();
			this.P_FiltersContainer = new System.Windows.Forms.Panel();
			this.P_Filters = new SlickControls.RoundedGroupPanel();
			this.I_ClearFilters = new SlickControls.SlickIcon();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.OT_Included = new LoadOrderToolTwo.UserInterface.ThreeOptionToggle();
			this.OT_Workshop = new LoadOrderToolTwo.UserInterface.ThreeOptionToggle();
			this.OT_Enabled = new LoadOrderToolTwo.UserInterface.ThreeOptionToggle();
			this.TLP_Dates = new System.Windows.Forms.TableLayoutPanel();
			this.L_From1 = new System.Windows.Forms.Label();
			this.L_To1 = new System.Windows.Forms.Label();
			this.L_From2 = new System.Windows.Forms.Label();
			this.L_To2 = new System.Windows.Forms.Label();
			this.L_DateUpdated = new System.Windows.Forms.Label();
			this.L_DateSubscribed = new System.Windows.Forms.Label();
			this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
			this.DT_SubFrom = new SlickControls.SlickDateTime();
			this.DT_SubTo = new SlickControls.SlickDateTime();
			this.DT_UpdateFrom = new SlickControls.SlickDateTime();
			this.DT_UpdateTo = new SlickControls.SlickDateTime();
			this.DD_PackageStatus = new LoadOrderToolTwo.UserInterface.PackageStatusDropDown();
			this.DD_ReportSeverity = new LoadOrderToolTwo.UserInterface.ReportSeverityDropDown();
			this.B_Filters = new SlickControls.SlickButton();
			this.B_Actions = new SlickControls.SlickButton();
			this.L_FilterCount = new System.Windows.Forms.Label();
			this.TLP_Main.SuspendLayout();
			this.P_ActionsContainer.SuspendLayout();
			this.P_Actions.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			this.P_FiltersContainer.SuspendLayout();
			this.P_Filters.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.TLP_Dates.SuspendLayout();
			this.tableLayoutPanel5.SuspendLayout();
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
			this.TLP_Main.ColumnCount = 4;
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.TLP_Main.Controls.Add(this.P_ActionsContainer, 0, 2);
			this.TLP_Main.Controls.Add(this.slickSpacer2, 0, 3);
			this.TLP_Main.Controls.Add(this.slickSpacer1, 0, 5);
			this.TLP_Main.Controls.Add(this.TB_Search, 0, 0);
			this.TLP_Main.Controls.Add(this.DD_Sorting, 3, 0);
			this.TLP_Main.Controls.Add(this.tableLayoutPanel3, 0, 4);
			this.TLP_Main.Controls.Add(this.P_FiltersContainer, 0, 1);
			this.TLP_Main.Controls.Add(this.B_Filters, 1, 0);
			this.TLP_Main.Controls.Add(this.B_Actions, 2, 0);
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
			this.TLP_Main.SetColumnSpan(this.P_ActionsContainer, 4);
			this.P_ActionsContainer.Controls.Add(this.P_Actions);
			this.P_ActionsContainer.Dock = System.Windows.Forms.DockStyle.Top;
			this.P_ActionsContainer.Location = new System.Drawing.Point(0, 280);
			this.P_ActionsContainer.Margin = new System.Windows.Forms.Padding(0);
			this.P_ActionsContainer.Name = "P_ActionsContainer";
			this.P_ActionsContainer.Size = new System.Drawing.Size(895, 127);
			this.P_ActionsContainer.TabIndex = 14;
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
			this.B_DisEnable.TabIndex = 0;
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
			this.B_ExInclude.TabIndex = 0;
			this.B_ExInclude.LeftClicked += new System.EventHandler(this.B_ExInclude_LeftClicked);
			this.B_ExInclude.RightClicked += new System.EventHandler(this.B_ExInclude_RightClicked);
			// 
			// slickSpacer2
			// 
			this.TLP_Main.SetColumnSpan(this.slickSpacer2, 4);
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
			this.TLP_Main.SetColumnSpan(this.slickSpacer1, 4);
			this.slickSpacer1.Dock = System.Windows.Forms.DockStyle.Top;
			this.slickSpacer1.Location = new System.Drawing.Point(0, 455);
			this.slickSpacer1.Margin = new System.Windows.Forms.Padding(0);
			this.slickSpacer1.Name = "slickSpacer1";
			this.slickSpacer1.Size = new System.Drawing.Size(895, 2);
			this.slickSpacer1.TabIndex = 7;
			this.slickSpacer1.TabStop = false;
			this.slickSpacer1.Text = "slickSpacer1";
			// 
			// TB_Search
			// 
			this.TB_Search.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.TB_Search.Image = ((System.Drawing.Image)(resources.GetObject("TB_Search.Image")));
			this.TB_Search.LabelText = "Search";
			this.TB_Search.Location = new System.Drawing.Point(3, 3);
			this.TB_Search.Name = "TB_Search";
			this.TB_Search.Placeholder = "SearchMods";
			this.TB_Search.SelectedText = "";
			this.TB_Search.SelectionLength = 0;
			this.TB_Search.SelectionStart = 0;
			this.TB_Search.Size = new System.Drawing.Size(140, 47);
			this.TB_Search.TabIndex = 0;
			this.TB_Search.TextChanged += new System.EventHandler(this.FilterChanged);
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
			this.DD_Sorting.TabIndex = 4;
			this.DD_Sorting.SelectedItemChanged += new System.EventHandler(this.DD_Sorting_SelectedItemChanged);
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.AutoSize = true;
			this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel3.ColumnCount = 2;
			this.TLP_Main.SetColumnSpan(this.tableLayoutPanel3, 4);
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
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
			this.L_Duplicates.Location = new System.Drawing.Point(3, 0);
			this.L_Duplicates.Name = "L_Duplicates";
			this.L_Duplicates.Size = new System.Drawing.Size(55, 23);
			this.L_Duplicates.TabIndex = 2;
			this.L_Duplicates.Text = "label1";
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
			// P_FiltersContainer
			// 
			this.TLP_Main.SetColumnSpan(this.P_FiltersContainer, 4);
			this.P_FiltersContainer.Controls.Add(this.P_Filters);
			this.P_FiltersContainer.Dock = System.Windows.Forms.DockStyle.Top;
			this.P_FiltersContainer.Location = new System.Drawing.Point(0, 53);
			this.P_FiltersContainer.Margin = new System.Windows.Forms.Padding(0);
			this.P_FiltersContainer.Name = "P_FiltersContainer";
			this.P_FiltersContainer.Size = new System.Drawing.Size(895, 227);
			this.P_FiltersContainer.TabIndex = 9;
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
			this.P_Filters.Size = new System.Drawing.Size(895, 183);
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
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Controls.Add(this.OT_Included, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.OT_Workshop, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.OT_Enabled, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.TLP_Dates, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.DD_PackageStatus, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.DD_ReportSeverity, 2, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(7, 38);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 4;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(881, 138);
			this.tableLayoutPanel1.TabIndex = 0;
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
			this.OT_Included.Size = new System.Drawing.Size(287, 40);
			this.OT_Included.TabIndex = 1;
			this.OT_Included.SelectedValueChanged += new System.EventHandler(this.FilterChanged);
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
			this.OT_Workshop.Size = new System.Drawing.Size(287, 40);
			this.OT_Workshop.TabIndex = 1;
			this.OT_Workshop.SelectedValueChanged += new System.EventHandler(this.FilterChanged);
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
			this.tableLayoutPanel1.SetRowSpan(this.OT_Enabled, 2);
			this.OT_Enabled.Size = new System.Drawing.Size(287, 40);
			this.OT_Enabled.TabIndex = 1;
			this.OT_Enabled.SelectedValueChanged += new System.EventHandler(this.FilterChanged);
			// 
			// TLP_Dates
			// 
			this.TLP_Dates.ColumnCount = 6;
			this.tableLayoutPanel1.SetColumnSpan(this.TLP_Dates, 2);
			this.TLP_Dates.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Dates.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TLP_Dates.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Dates.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Dates.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TLP_Dates.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Dates.Controls.Add(this.L_From1, 0, 0);
			this.TLP_Dates.Controls.Add(this.L_To1, 2, 0);
			this.TLP_Dates.Controls.Add(this.L_From2, 3, 0);
			this.TLP_Dates.Controls.Add(this.L_To2, 5, 0);
			this.TLP_Dates.Controls.Add(this.L_DateUpdated, 4, 0);
			this.TLP_Dates.Controls.Add(this.L_DateSubscribed, 1, 0);
			this.TLP_Dates.Controls.Add(this.tableLayoutPanel5, 0, 1);
			this.TLP_Dates.Dock = System.Windows.Forms.DockStyle.Top;
			this.TLP_Dates.Location = new System.Drawing.Point(293, 0);
			this.TLP_Dates.Margin = new System.Windows.Forms.Padding(0);
			this.TLP_Dates.Name = "TLP_Dates";
			this.TLP_Dates.RowCount = 2;
			this.tableLayoutPanel1.SetRowSpan(this.TLP_Dates, 2);
			this.TLP_Dates.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Dates.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Dates.Size = new System.Drawing.Size(588, 61);
			this.TLP_Dates.TabIndex = 7;
			// 
			// L_From1
			// 
			this.L_From1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.L_From1.AutoSize = true;
			this.L_From1.Location = new System.Drawing.Point(3, 0);
			this.L_From1.Name = "L_From1";
			this.L_From1.Size = new System.Drawing.Size(55, 23);
			this.L_From1.TabIndex = 0;
			this.L_From1.Text = "label1";
			// 
			// L_To1
			// 
			this.L_To1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.L_To1.AutoSize = true;
			this.L_To1.Location = new System.Drawing.Point(236, 0);
			this.L_To1.Name = "L_To1";
			this.L_To1.Size = new System.Drawing.Size(55, 23);
			this.L_To1.TabIndex = 0;
			this.L_To1.Text = "label1";
			// 
			// L_From2
			// 
			this.L_From2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.L_From2.AutoSize = true;
			this.L_From2.Location = new System.Drawing.Point(297, 0);
			this.L_From2.Name = "L_From2";
			this.L_From2.Size = new System.Drawing.Size(55, 23);
			this.L_From2.TabIndex = 0;
			this.L_From2.Text = "label1";
			// 
			// L_To2
			// 
			this.L_To2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.L_To2.AutoSize = true;
			this.L_To2.Location = new System.Drawing.Point(530, 0);
			this.L_To2.Name = "L_To2";
			this.L_To2.Size = new System.Drawing.Size(55, 23);
			this.L_To2.TabIndex = 0;
			this.L_To2.Text = "label1";
			// 
			// L_DateUpdated
			// 
			this.L_DateUpdated.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.L_DateUpdated.AutoSize = true;
			this.L_DateUpdated.Location = new System.Drawing.Point(413, 0);
			this.L_DateUpdated.Name = "L_DateUpdated";
			this.L_DateUpdated.Size = new System.Drawing.Size(55, 23);
			this.L_DateUpdated.TabIndex = 0;
			this.L_DateUpdated.Text = "label1";
			// 
			// L_DateSubscribed
			// 
			this.L_DateSubscribed.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.L_DateSubscribed.AutoSize = true;
			this.L_DateSubscribed.Location = new System.Drawing.Point(119, 0);
			this.L_DateSubscribed.Name = "L_DateSubscribed";
			this.L_DateSubscribed.Size = new System.Drawing.Size(55, 23);
			this.L_DateSubscribed.TabIndex = 0;
			this.L_DateSubscribed.Text = "label1";
			// 
			// tableLayoutPanel5
			// 
			this.tableLayoutPanel5.ColumnCount = 4;
			this.TLP_Dates.SetColumnSpan(this.tableLayoutPanel5, 6);
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tableLayoutPanel5.Controls.Add(this.DT_SubFrom, 0, 0);
			this.tableLayoutPanel5.Controls.Add(this.DT_SubTo, 1, 0);
			this.tableLayoutPanel5.Controls.Add(this.DT_UpdateFrom, 2, 0);
			this.tableLayoutPanel5.Controls.Add(this.DT_UpdateTo, 3, 0);
			this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel5.Location = new System.Drawing.Point(0, 23);
			this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel5.Name = "tableLayoutPanel5";
			this.tableLayoutPanel5.RowCount = 1;
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel5.Size = new System.Drawing.Size(588, 100);
			this.tableLayoutPanel5.TabIndex = 1;
			// 
			// DT_SubFrom
			// 
			this.DT_SubFrom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DT_SubFrom.LabelText = "";
			this.DT_SubFrom.Location = new System.Drawing.Point(3, 3);
			this.DT_SubFrom.Name = "DT_SubFrom";
			this.DT_SubFrom.Required = false;
			this.DT_SubFrom.SelectedPart = SlickControls.SlickDateTime.DatePart.Day;
			this.DT_SubFrom.Size = new System.Drawing.Size(141, 25);
			this.DT_SubFrom.TabIndex = 0;
			this.DT_SubFrom.Value = new System.DateTime(2023, 3, 22, 0, 0, 0, 0);
			this.DT_SubFrom.ValueChanged += new System.EventHandler(this.FilterChanged);
			// 
			// DT_SubTo
			// 
			this.DT_SubTo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DT_SubTo.LabelText = "";
			this.DT_SubTo.Location = new System.Drawing.Point(150, 3);
			this.DT_SubTo.Name = "DT_SubTo";
			this.DT_SubTo.Required = false;
			this.DT_SubTo.SelectedPart = SlickControls.SlickDateTime.DatePart.Day;
			this.DT_SubTo.Size = new System.Drawing.Size(141, 25);
			this.DT_SubTo.TabIndex = 1;
			this.DT_SubTo.Value = new System.DateTime(2023, 3, 22, 0, 0, 0, 0);
			this.DT_SubTo.ValueChanged += new System.EventHandler(this.FilterChanged);
			// 
			// DT_UpdateFrom
			// 
			this.DT_UpdateFrom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DT_UpdateFrom.LabelText = "";
			this.DT_UpdateFrom.Location = new System.Drawing.Point(297, 3);
			this.DT_UpdateFrom.Name = "DT_UpdateFrom";
			this.DT_UpdateFrom.Required = false;
			this.DT_UpdateFrom.SelectedPart = SlickControls.SlickDateTime.DatePart.Day;
			this.DT_UpdateFrom.Size = new System.Drawing.Size(141, 25);
			this.DT_UpdateFrom.TabIndex = 1;
			this.DT_UpdateFrom.Value = new System.DateTime(2023, 3, 22, 0, 0, 0, 0);
			this.DT_UpdateFrom.ValueChanged += new System.EventHandler(this.FilterChanged);
			// 
			// DT_UpdateTo
			// 
			this.DT_UpdateTo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DT_UpdateTo.LabelText = "";
			this.DT_UpdateTo.Location = new System.Drawing.Point(444, 3);
			this.DT_UpdateTo.Name = "DT_UpdateTo";
			this.DT_UpdateTo.Required = false;
			this.DT_UpdateTo.SelectedPart = SlickControls.SlickDateTime.DatePart.Day;
			this.DT_UpdateTo.Size = new System.Drawing.Size(141, 25);
			this.DT_UpdateTo.TabIndex = 1;
			this.DT_UpdateTo.Value = new System.DateTime(2023, 3, 22, 0, 0, 0, 0);
			this.DT_UpdateTo.ValueChanged += new System.EventHandler(this.FilterChanged);
			// 
			// DD_PackageStatus
			// 
			this.DD_PackageStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DD_PackageStatus.Cursor = System.Windows.Forms.Cursors.Hand;
			this.DD_PackageStatus.Font = new System.Drawing.Font("Nirmala UI", 15F);
			this.DD_PackageStatus.Location = new System.Drawing.Point(300, 71);
			this.DD_PackageStatus.Margin = new System.Windows.Forms.Padding(7);
			this.DD_PackageStatus.Name = "DD_PackageStatus";
			this.DD_PackageStatus.Padding = new System.Windows.Forms.Padding(7);
			this.tableLayoutPanel1.SetRowSpan(this.DD_PackageStatus, 2);
			this.DD_PackageStatus.Size = new System.Drawing.Size(279, 60);
			this.DD_PackageStatus.TabIndex = 6;
			this.DD_PackageStatus.SelectedItemChanged += new System.EventHandler(this.FilterChanged);
			// 
			// DD_ReportSeverity
			// 
			this.DD_ReportSeverity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DD_ReportSeverity.Cursor = System.Windows.Forms.Cursors.Hand;
			this.DD_ReportSeverity.Font = new System.Drawing.Font("Nirmala UI", 15F);
			this.DD_ReportSeverity.Location = new System.Drawing.Point(593, 71);
			this.DD_ReportSeverity.Margin = new System.Windows.Forms.Padding(7);
			this.DD_ReportSeverity.Name = "DD_ReportSeverity";
			this.DD_ReportSeverity.Padding = new System.Windows.Forms.Padding(7);
			this.tableLayoutPanel1.SetRowSpan(this.DD_ReportSeverity, 2);
			this.DD_ReportSeverity.Size = new System.Drawing.Size(281, 60);
			this.DD_ReportSeverity.TabIndex = 4;
			this.DD_ReportSeverity.SelectedItemChanged += new System.EventHandler(this.FilterChanged);
			// 
			// B_Filters
			// 
			this.B_Filters.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.B_Filters.ColorShade = null;
			this.B_Filters.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_Filters.Image = global::LoadOrderToolTwo.Properties.Resources.I_Filter;
			this.B_Filters.Location = new System.Drawing.Point(388, 3);
			this.B_Filters.Name = "B_Filters";
			this.B_Filters.Size = new System.Drawing.Size(100, 47);
			this.B_Filters.SpaceTriggersClick = true;
			this.B_Filters.TabIndex = 13;
			this.B_Filters.Text = "ShowFilters";
			this.B_Filters.Click += new System.EventHandler(this.B_Filters_Click);
			// 
			// B_Actions
			// 
			this.B_Actions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.B_Actions.ColorShade = null;
			this.B_Actions.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_Actions.Image = global::LoadOrderToolTwo.Properties.Resources.I_Filter;
			this.B_Actions.Location = new System.Drawing.Point(494, 3);
			this.B_Actions.Name = "B_Actions";
			this.B_Actions.Size = new System.Drawing.Size(100, 47);
			this.B_Actions.SpaceTriggersClick = true;
			this.B_Actions.TabIndex = 13;
			this.B_Actions.Text = "ShowActions";
			this.B_Actions.Click += new System.EventHandler(this.B_Actions_Click);
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
			// PC_Packages
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.TLP_Main);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(58)))), ((int)(((byte)(69)))));
			this.LabelBounds = new System.Drawing.Point(-2, 3);
			this.Name = "PC_Packages";
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
			this.TLP_Dates.ResumeLayout(false);
			this.TLP_Dates.PerformLayout();
			this.tableLayoutPanel5.ResumeLayout(false);
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
	private System.Windows.Forms.TableLayoutPanel TLP_Dates;
	private System.Windows.Forms.Label L_From1;
	private System.Windows.Forms.Label L_To1;
	private System.Windows.Forms.Label L_From2;
	private System.Windows.Forms.Label L_To2;
	private System.Windows.Forms.Label L_DateUpdated;
	private System.Windows.Forms.Label L_DateSubscribed;
	private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
	private SlickControls.SlickDateTime DT_SubFrom;
	private SlickControls.SlickDateTime DT_SubTo;
	private SlickControls.SlickDateTime DT_UpdateFrom;
	private SlickControls.SlickDateTime DT_UpdateTo;
	private System.Windows.Forms.Label L_FilterCount;
}
