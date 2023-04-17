namespace LoadOrderToolTwo.UserInterface.Panels;

partial class PC_ModUtilities
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
			LC_Duplicates?.Dispose();
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
			this.TB_CollectionLink = new SlickControls.SlickTextBox();
			this.P_Collecttions = new SlickControls.RoundedGroupPanel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.B_LoadCollection = new SlickControls.SlickButton();
			this.TLP_Main = new System.Windows.Forms.TableLayoutPanel();
			this.P_BOB = new SlickControls.RoundedGroupPanel();
			this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
			this.DD_BOB = new LoadOrderToolTwo.UserInterface.Generic.DragAndDropControl();
			this.P_LsmReport = new SlickControls.RoundedGroupPanel();
			this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
			this.DD_Unused = new LoadOrderToolTwo.UserInterface.Generic.DragAndDropControl();
			this.DD_Missing = new LoadOrderToolTwo.UserInterface.Generic.DragAndDropControl();
			this.P_DuplicateMods = new SlickControls.RoundedGroupPanel();
			this.P_ModIssues = new SlickControls.RoundedGroupPanel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
			this.L_OutOfDate = new System.Windows.Forms.Label();
			this.L_Incomplete = new System.Windows.Forms.Label();
			this.B_ReDownload = new SlickControls.SlickButton();
			this.slickScroll1 = new SlickControls.SlickScroll();
			this.P_Container = new System.Windows.Forms.Panel();
			this.P_Collecttions.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.TLP_Main.SuspendLayout();
			this.P_BOB.SuspendLayout();
			this.tableLayoutPanel6.SuspendLayout();
			this.P_LsmReport.SuspendLayout();
			this.tableLayoutPanel4.SuspendLayout();
			this.P_ModIssues.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.tableLayoutPanel5.SuspendLayout();
			this.P_Container.SuspendLayout();
			this.SuspendLayout();
			// 
			// base_Text
			// 
			this.base_Text.Location = new System.Drawing.Point(-2, 3);
			// 
			// TB_CollectionLink
			// 
			this.TB_CollectionLink.Dock = System.Windows.Forms.DockStyle.Top;
			this.TB_CollectionLink.LabelText = "CollectionLink";
			this.TB_CollectionLink.Location = new System.Drawing.Point(3, 3);
			this.TB_CollectionLink.Name = "TB_CollectionLink";
			this.TB_CollectionLink.Placeholder = "PasteCollection";
			this.TB_CollectionLink.Required = true;
			this.TB_CollectionLink.SelectedText = "";
			this.TB_CollectionLink.SelectionLength = 0;
			this.TB_CollectionLink.SelectionStart = 0;
			this.TB_CollectionLink.Size = new System.Drawing.Size(568, 49);
			this.TB_CollectionLink.TabIndex = 13;
			this.TB_CollectionLink.Validation = SlickControls.ValidationType.Regex;
			this.TB_CollectionLink.ValidationRegex = "^(?:https:\\/\\/steamcommunity\\.com\\/(?:(?:sharedfiles)|(?:workshop))\\/filedetails\\" +
    "/\\?id=)?(\\d{8,20})";
			this.TB_CollectionLink.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.TB_CollectionLink_PreviewKeyDown);
			// 
			// P_Collecttions
			// 
			this.P_Collecttions.AddOutline = true;
			this.P_Collecttions.AutoSize = true;
			this.P_Collecttions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.P_Collecttions.Controls.Add(this.tableLayoutPanel1);
			this.P_Collecttions.Dock = System.Windows.Forms.DockStyle.Top;
			this.P_Collecttions.Location = new System.Drawing.Point(3, 177);
			this.P_Collecttions.Name = "P_Collecttions";
			this.P_Collecttions.Padding = new System.Windows.Forms.Padding(7, 38, 7, 7);
			this.P_Collecttions.Size = new System.Drawing.Size(694, 100);
			this.P_Collecttions.TabIndex = 15;
			this.P_Collecttions.Text = "CollectionTitle";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Controls.Add(this.B_LoadCollection, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.TB_CollectionLink, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(7, 38);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(680, 55);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// B_LoadCollection
			// 
			this.B_LoadCollection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.B_LoadCollection.ColorShade = null;
			this.B_LoadCollection.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_LoadCollection.Location = new System.Drawing.Point(577, 3);
			this.B_LoadCollection.Name = "B_LoadCollection";
			this.B_LoadCollection.Padding = new System.Windows.Forms.Padding(10, 15, 10, 15);
			this.B_LoadCollection.Size = new System.Drawing.Size(100, 49);
			this.B_LoadCollection.SpaceTriggersClick = true;
			this.B_LoadCollection.TabIndex = 15;
			this.B_LoadCollection.Text = "LoadCollection";
			this.B_LoadCollection.Click += new System.EventHandler(this.B_LoadCollection_Click);
			// 
			// TLP_Main
			// 
			this.TLP_Main.AutoSize = true;
			this.TLP_Main.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.TLP_Main.ColumnCount = 1;
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TLP_Main.Controls.Add(this.P_BOB, 0, 4);
			this.TLP_Main.Controls.Add(this.P_LsmReport, 0, 3);
			this.TLP_Main.Controls.Add(this.P_DuplicateMods, 0, 0);
			this.TLP_Main.Controls.Add(this.P_ModIssues, 0, 1);
			this.TLP_Main.Controls.Add(this.P_Collecttions, 0, 2);
			this.TLP_Main.Location = new System.Drawing.Point(0, 0);
			this.TLP_Main.MinimumSize = new System.Drawing.Size(700, 0);
			this.TLP_Main.Name = "TLP_Main";
			this.TLP_Main.RowCount = 5;
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Main.Size = new System.Drawing.Size(700, 694);
			this.TLP_Main.TabIndex = 17;
			// 
			// P_BOB
			// 
			this.P_BOB.AddOutline = true;
			this.P_BOB.AutoSize = true;
			this.P_BOB.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.P_BOB.Controls.Add(this.tableLayoutPanel6);
			this.P_BOB.Dock = System.Windows.Forms.DockStyle.Top;
			this.P_BOB.Location = new System.Drawing.Point(3, 490);
			this.P_BOB.Name = "P_BOB";
			this.P_BOB.Padding = new System.Windows.Forms.Padding(7, 38, 7, 7);
			this.P_BOB.Size = new System.Drawing.Size(694, 201);
			this.P_BOB.TabIndex = 19;
			this.P_BOB.Text = "BOBImport";
			// 
			// tableLayoutPanel6
			// 
			this.tableLayoutPanel6.AutoSize = true;
			this.tableLayoutPanel6.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel6.ColumnCount = 1;
			this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel6.Controls.Add(this.DD_BOB, 0, 0);
			this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Top;
			this.tableLayoutPanel6.Location = new System.Drawing.Point(7, 38);
			this.tableLayoutPanel6.Name = "tableLayoutPanel6";
			this.tableLayoutPanel6.RowCount = 1;
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel6.Size = new System.Drawing.Size(680, 156);
			this.tableLayoutPanel6.TabIndex = 0;
			// 
			// DD_BOB
			// 
			this.DD_BOB.AllowDrop = true;
			this.DD_BOB.Cursor = System.Windows.Forms.Cursors.Hand;
			this.DD_BOB.Dock = System.Windows.Forms.DockStyle.Top;
			this.DD_BOB.Location = new System.Drawing.Point(3, 3);
			this.DD_BOB.Name = "DD_BOB";
			this.DD_BOB.Size = new System.Drawing.Size(674, 150);
			this.DD_BOB.TabIndex = 16;
			this.DD_BOB.Text = "BOBImportMissingInfo";
			this.DD_BOB.ValidExtensions = new string[] {
        ".xml"};
			this.DD_BOB.FileSelected += new System.Action<string>(this.DD_BOB_FileSelected);
			this.DD_BOB.ValidFile += new System.Func<object, string, bool>(this.DD_BOB_ValidFile);
			// 
			// P_LsmReport
			// 
			this.P_LsmReport.AddOutline = true;
			this.P_LsmReport.AutoSize = true;
			this.P_LsmReport.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.P_LsmReport.Controls.Add(this.tableLayoutPanel4);
			this.P_LsmReport.Dock = System.Windows.Forms.DockStyle.Top;
			this.P_LsmReport.Location = new System.Drawing.Point(3, 283);
			this.P_LsmReport.Name = "P_LsmReport";
			this.P_LsmReport.Padding = new System.Windows.Forms.Padding(7, 38, 7, 7);
			this.P_LsmReport.Size = new System.Drawing.Size(694, 201);
			this.P_LsmReport.TabIndex = 18;
			this.P_LsmReport.Text = "LsmImport";
			// 
			// tableLayoutPanel4
			// 
			this.tableLayoutPanel4.AutoSize = true;
			this.tableLayoutPanel4.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel4.ColumnCount = 2;
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel4.Controls.Add(this.DD_Unused, 0, 0);
			this.tableLayoutPanel4.Controls.Add(this.DD_Missing, 0, 0);
			this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Top;
			this.tableLayoutPanel4.Location = new System.Drawing.Point(7, 38);
			this.tableLayoutPanel4.Name = "tableLayoutPanel4";
			this.tableLayoutPanel4.RowCount = 1;
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel4.Size = new System.Drawing.Size(680, 156);
			this.tableLayoutPanel4.TabIndex = 0;
			// 
			// DD_Unused
			// 
			this.DD_Unused.AllowDrop = true;
			this.DD_Unused.Cursor = System.Windows.Forms.Cursors.Hand;
			this.DD_Unused.Dock = System.Windows.Forms.DockStyle.Top;
			this.DD_Unused.Location = new System.Drawing.Point(343, 3);
			this.DD_Unused.Name = "DD_Unused";
			this.DD_Unused.Size = new System.Drawing.Size(334, 150);
			this.DD_Unused.TabIndex = 17;
			this.DD_Unused.Text = "LsmImportUnusedInfo";
			this.DD_Unused.FileSelected += new System.Action<string>(this.LSM_UnusedDrop_FileSelected);
			this.DD_Unused.ValidFile += new System.Func<object, string, bool>(this.LSMDragDrop_ValidFile);
			// 
			// DD_Missing
			// 
			this.DD_Missing.AllowDrop = true;
			this.DD_Missing.Cursor = System.Windows.Forms.Cursors.Hand;
			this.DD_Missing.Dock = System.Windows.Forms.DockStyle.Top;
			this.DD_Missing.Location = new System.Drawing.Point(3, 3);
			this.DD_Missing.Name = "DD_Missing";
			this.DD_Missing.Size = new System.Drawing.Size(334, 150);
			this.DD_Missing.TabIndex = 16;
			this.DD_Missing.Text = "LsmImportMissingInfo";
			this.DD_Missing.FileSelected += new System.Action<string>(this.LSMDragDrop_FileSelected);
			this.DD_Missing.ValidFile += new System.Func<object, string, bool>(this.LSMDragDrop_ValidFile);
			// 
			// P_DuplicateMods
			// 
			this.P_DuplicateMods.AddOutline = true;
			this.P_DuplicateMods.AutoSize = true;
			this.P_DuplicateMods.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.P_DuplicateMods.ColorStyle = Extensions.ColorStyle.Red;
			this.P_DuplicateMods.Dock = System.Windows.Forms.DockStyle.Top;
			this.P_DuplicateMods.Location = new System.Drawing.Point(3, 3);
			this.P_DuplicateMods.Name = "P_DuplicateMods";
			this.P_DuplicateMods.Padding = new System.Windows.Forms.Padding(7, 38, 7, 7);
			this.P_DuplicateMods.Size = new System.Drawing.Size(694, 45);
			this.P_DuplicateMods.TabIndex = 17;
			this.P_DuplicateMods.Text = "DuplicateMods";
			// 
			// P_ModIssues
			// 
			this.P_ModIssues.AddOutline = true;
			this.P_ModIssues.AutoSize = true;
			this.P_ModIssues.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.P_ModIssues.ColorStyle = Extensions.ColorStyle.Yellow;
			this.P_ModIssues.Controls.Add(this.tableLayoutPanel2);
			this.P_ModIssues.Dock = System.Windows.Forms.DockStyle.Top;
			this.P_ModIssues.Location = new System.Drawing.Point(3, 54);
			this.P_ModIssues.Name = "P_ModIssues";
			this.P_ModIssues.Padding = new System.Windows.Forms.Padding(7, 38, 7, 7);
			this.P_ModIssues.Size = new System.Drawing.Size(694, 117);
			this.P_ModIssues.TabIndex = 16;
			this.P_ModIssues.Text = "DetectedIssues";
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.AutoSize = true;
			this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel5, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.B_ReDownload, 1, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(7, 38);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.Size = new System.Drawing.Size(680, 72);
			this.tableLayoutPanel2.TabIndex = 0;
			// 
			// tableLayoutPanel5
			// 
			this.tableLayoutPanel5.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.tableLayoutPanel5.AutoSize = true;
			this.tableLayoutPanel5.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel5.ColumnCount = 1;
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel5.Controls.Add(this.L_OutOfDate, 0, 0);
			this.tableLayoutPanel5.Controls.Add(this.L_Incomplete, 0, 1);
			this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel5.Name = "tableLayoutPanel5";
			this.tableLayoutPanel5.RowCount = 2;
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel5.Size = new System.Drawing.Size(61, 66);
			this.tableLayoutPanel5.TabIndex = 18;
			// 
			// L_OutOfDate
			// 
			this.L_OutOfDate.AutoSize = true;
			this.L_OutOfDate.Location = new System.Drawing.Point(3, 10);
			this.L_OutOfDate.Margin = new System.Windows.Forms.Padding(3, 10, 3, 0);
			this.L_OutOfDate.Name = "L_OutOfDate";
			this.L_OutOfDate.Size = new System.Drawing.Size(55, 23);
			this.L_OutOfDate.TabIndex = 15;
			this.L_OutOfDate.Text = "label1";
			// 
			// L_Incomplete
			// 
			this.L_Incomplete.AutoSize = true;
			this.L_Incomplete.Location = new System.Drawing.Point(3, 43);
			this.L_Incomplete.Margin = new System.Windows.Forms.Padding(3, 10, 3, 0);
			this.L_Incomplete.Name = "L_Incomplete";
			this.L_Incomplete.Size = new System.Drawing.Size(55, 23);
			this.L_Incomplete.TabIndex = 15;
			this.L_Incomplete.Text = "label1";
			// 
			// B_ReDownload
			// 
			this.B_ReDownload.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.B_ReDownload.ColorShade = null;
			this.B_ReDownload.ColorStyle = Extensions.ColorStyle.Green;
			this.B_ReDownload.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_ReDownload.Location = new System.Drawing.Point(566, 16);
			this.B_ReDownload.Name = "B_ReDownload";
			this.B_ReDownload.Size = new System.Drawing.Size(111, 40);
			this.B_ReDownload.SpaceTriggersClick = true;
			this.B_ReDownload.TabIndex = 14;
			this.B_ReDownload.Text = "FixAllIssues";
			this.B_ReDownload.Click += new System.EventHandler(this.B_ReDownload_Click);
			// 
			// slickScroll1
			// 
			this.slickScroll1.Dock = System.Windows.Forms.DockStyle.Right;
			this.slickScroll1.LinkedControl = this.TLP_Main;
			this.slickScroll1.Location = new System.Drawing.Point(775, 30);
			this.slickScroll1.Name = "slickScroll1";
			this.slickScroll1.Size = new System.Drawing.Size(8, 695);
			this.slickScroll1.SmallHandle = true;
			this.slickScroll1.Style = SlickControls.StyleType.Vertical;
			this.slickScroll1.TabIndex = 18;
			this.slickScroll1.TabStop = false;
			this.slickScroll1.Text = "slickScroll1";
			// 
			// P_Container
			// 
			this.P_Container.Controls.Add(this.TLP_Main);
			this.P_Container.Dock = System.Windows.Forms.DockStyle.Fill;
			this.P_Container.Location = new System.Drawing.Point(0, 30);
			this.P_Container.Name = "P_Container";
			this.P_Container.Size = new System.Drawing.Size(775, 695);
			this.P_Container.TabIndex = 19;
			// 
			// PC_ModUtilities
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.P_Container);
			this.Controls.Add(this.slickScroll1);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(58)))), ((int)(((byte)(69)))));
			this.LabelBounds = new System.Drawing.Point(-2, 3);
			this.Name = "PC_ModUtilities";
			this.Padding = new System.Windows.Forms.Padding(0, 30, 0, 0);
			this.Size = new System.Drawing.Size(783, 725);
			this.Controls.SetChildIndex(this.base_Text, 0);
			this.Controls.SetChildIndex(this.slickScroll1, 0);
			this.Controls.SetChildIndex(this.P_Container, 0);
			this.P_Collecttions.ResumeLayout(false);
			this.P_Collecttions.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.TLP_Main.ResumeLayout(false);
			this.TLP_Main.PerformLayout();
			this.P_BOB.ResumeLayout(false);
			this.P_BOB.PerformLayout();
			this.tableLayoutPanel6.ResumeLayout(false);
			this.P_LsmReport.ResumeLayout(false);
			this.P_LsmReport.PerformLayout();
			this.tableLayoutPanel4.ResumeLayout(false);
			this.P_ModIssues.ResumeLayout(false);
			this.P_ModIssues.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.tableLayoutPanel5.ResumeLayout(false);
			this.tableLayoutPanel5.PerformLayout();
			this.P_Container.ResumeLayout(false);
			this.P_Container.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	private SlickControls.SlickTextBox TB_CollectionLink;
	private SlickControls.RoundedGroupPanel P_Collecttions;
	private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	private SlickControls.SlickButton B_LoadCollection;
	private System.Windows.Forms.TableLayoutPanel TLP_Main;
	private SlickControls.RoundedGroupPanel P_ModIssues;
	private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
	private SlickControls.SlickScroll slickScroll1;
	private System.Windows.Forms.Panel P_Container;
	private SlickControls.RoundedGroupPanel P_DuplicateMods;
	private SlickControls.SlickButton B_ReDownload;
	private System.Windows.Forms.Label L_OutOfDate;
	private System.Windows.Forms.Label L_Incomplete;
	private SlickControls.RoundedGroupPanel P_LsmReport;
	private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
	private Generic.DragAndDropControl DD_Missing;
	private Generic.DragAndDropControl DD_Unused;
	private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
	private SlickControls.RoundedGroupPanel P_BOB;
	private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
	private Generic.DragAndDropControl DD_BOB;
}
