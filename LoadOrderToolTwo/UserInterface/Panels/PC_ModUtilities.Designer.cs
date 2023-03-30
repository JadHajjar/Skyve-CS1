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
			this.P_Filters = new SlickControls.RoundedGroupPanel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.B_LoadCollection = new SlickControls.SlickButton();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.roundedGroupPanel1 = new SlickControls.RoundedGroupPanel();
			this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
			this.dragAndDropControl1 = new LoadOrderToolTwo.UserInterface.Generic.DragAndDropControl();
			this.P_DuplicateMods = new SlickControls.RoundedGroupPanel();
			this.P_ModIssues = new SlickControls.RoundedGroupPanel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.B_ReDownload = new SlickControls.SlickButton();
			this.L_OutOfDate = new System.Windows.Forms.Label();
			this.L_Incomplete = new System.Windows.Forms.Label();
			this.slickScroll1 = new SlickControls.SlickScroll();
			this.P_Container = new System.Windows.Forms.Panel();
			this.P_Filters.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			this.roundedGroupPanel1.SuspendLayout();
			this.tableLayoutPanel4.SuspendLayout();
			this.P_ModIssues.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.P_Container.SuspendLayout();
			this.SuspendLayout();
			// 
			// base_Text
			// 
			this.base_Text.Location = new System.Drawing.Point(-2, 3);
			// 
			// TB_CollectionLink
			// 
			this.TB_CollectionLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
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
    "/\\?id=)?(\\d{9,20})$";
			this.TB_CollectionLink.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.TB_CollectionLink_PreviewKeyDown);
			// 
			// P_Filters
			// 
			this.P_Filters.AddOutline = true;
			this.P_Filters.AutoSize = true;
			this.P_Filters.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.P_Filters.Controls.Add(this.tableLayoutPanel1);
			this.P_Filters.Dock = System.Windows.Forms.DockStyle.Top;
			this.P_Filters.Location = new System.Drawing.Point(3, 3);
			this.P_Filters.Name = "P_Filters";
			this.P_Filters.Padding = new System.Windows.Forms.Padding(7, 38, 7, 7);
			this.P_Filters.Size = new System.Drawing.Size(694, 100);
			this.P_Filters.TabIndex = 15;
			this.P_Filters.Text = "CollectionTitle";
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
			this.B_LoadCollection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.B_LoadCollection.ColorShade = null;
			this.B_LoadCollection.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_LoadCollection.Image = global::LoadOrderToolTwo.Properties.Resources.I_Import;
			this.B_LoadCollection.Location = new System.Drawing.Point(577, 3);
			this.B_LoadCollection.Name = "B_LoadCollection";
			this.B_LoadCollection.Padding = new System.Windows.Forms.Padding(10, 15, 10, 15);
			this.B_LoadCollection.Size = new System.Drawing.Size(100, 49);
			this.B_LoadCollection.SpaceTriggersClick = true;
			this.B_LoadCollection.TabIndex = 15;
			this.B_LoadCollection.Text = "LoadCollection";
			this.B_LoadCollection.Click += new System.EventHandler(this.B_LoadCollection_Click);
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.AutoSize = true;
			this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel3.ColumnCount = 1;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.Controls.Add(this.roundedGroupPanel1, 0, 1);
			this.tableLayoutPanel3.Controls.Add(this.P_DuplicateMods, 0, 2);
			this.tableLayoutPanel3.Controls.Add(this.P_ModIssues, 0, 3);
			this.tableLayoutPanel3.Controls.Add(this.P_Filters, 0, 0);
			this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel3.MinimumSize = new System.Drawing.Size(700, 0);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 4;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(700, 471);
			this.tableLayoutPanel3.TabIndex = 17;
			// 
			// roundedGroupPanel1
			// 
			this.roundedGroupPanel1.AddOutline = true;
			this.roundedGroupPanel1.AutoSize = true;
			this.roundedGroupPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.roundedGroupPanel1.Controls.Add(this.tableLayoutPanel4);
			this.roundedGroupPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.roundedGroupPanel1.Location = new System.Drawing.Point(3, 109);
			this.roundedGroupPanel1.Name = "roundedGroupPanel1";
			this.roundedGroupPanel1.Padding = new System.Windows.Forms.Padding(7, 38, 7, 7);
			this.roundedGroupPanel1.Size = new System.Drawing.Size(694, 201);
			this.roundedGroupPanel1.TabIndex = 18;
			this.roundedGroupPanel1.Text = "LsmImport";
			this.roundedGroupPanel1.Visible = false;
			// 
			// tableLayoutPanel4
			// 
			this.tableLayoutPanel4.AutoSize = true;
			this.tableLayoutPanel4.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel4.ColumnCount = 2;
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel4.Controls.Add(this.dragAndDropControl1, 0, 0);
			this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Top;
			this.tableLayoutPanel4.Location = new System.Drawing.Point(7, 38);
			this.tableLayoutPanel4.Name = "tableLayoutPanel4";
			this.tableLayoutPanel4.RowCount = 1;
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel4.Size = new System.Drawing.Size(680, 156);
			this.tableLayoutPanel4.TabIndex = 0;
			// 
			// dragAndDropControl1
			// 
			this.dragAndDropControl1.AllowDrop = true;
			this.dragAndDropControl1.Cursor = System.Windows.Forms.Cursors.Hand;
			this.dragAndDropControl1.Dock = System.Windows.Forms.DockStyle.Top;
			this.dragAndDropControl1.Location = new System.Drawing.Point(3, 3);
			this.dragAndDropControl1.Name = "dragAndDropControl1";
			this.dragAndDropControl1.RegexTest = "\\- Assets Report([_\\-]\\d+)+\\.htm$";
			this.dragAndDropControl1.Size = new System.Drawing.Size(674, 150);
			this.dragAndDropControl1.StartingFolder = null;
			this.dragAndDropControl1.TabIndex = 16;
			this.dragAndDropControl1.Text = "LsmImportInfo";
			this.dragAndDropControl1.ValidExtensions = null;
			this.dragAndDropControl1.FileSelected += new System.Action<string>(this.LSMDragDrop_FileSelected);
			// 
			// P_DuplicateMods
			// 
			this.P_DuplicateMods.AddOutline = true;
			this.P_DuplicateMods.AutoSize = true;
			this.P_DuplicateMods.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.P_DuplicateMods.ColorStyle = Extensions.ColorStyle.Red;
			this.P_DuplicateMods.Dock = System.Windows.Forms.DockStyle.Top;
			this.P_DuplicateMods.Location = new System.Drawing.Point(3, 316);
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
			this.P_ModIssues.Location = new System.Drawing.Point(3, 367);
			this.P_ModIssues.Name = "P_ModIssues";
			this.P_ModIssues.Padding = new System.Windows.Forms.Padding(7, 38, 7, 7);
			this.P_ModIssues.Size = new System.Drawing.Size(694, 101);
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
			this.tableLayoutPanel2.Controls.Add(this.B_ReDownload, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.L_OutOfDate, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.L_Incomplete, 0, 1);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(7, 38);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
			this.tableLayoutPanel2.RowCount = 2;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(680, 56);
			this.tableLayoutPanel2.TabIndex = 0;
			// 
			// B_ReDownload
			// 
			this.B_ReDownload.ColorShade = null;
			this.B_ReDownload.ColorStyle = Extensions.ColorStyle.Green;
			this.B_ReDownload.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_ReDownload.Location = new System.Drawing.Point(566, 13);
			this.B_ReDownload.Name = "B_ReDownload";
			this.tableLayoutPanel2.SetRowSpan(this.B_ReDownload, 2);
			this.B_ReDownload.Size = new System.Drawing.Size(111, 40);
			this.B_ReDownload.SpaceTriggersClick = true;
			this.B_ReDownload.TabIndex = 14;
			this.B_ReDownload.Text = "FixAllIssues";
			this.B_ReDownload.Click += new System.EventHandler(this.B_ReDownload_Click);
			// 
			// L_OutOfDate
			// 
			this.L_OutOfDate.AutoSize = true;
			this.L_OutOfDate.Location = new System.Drawing.Point(3, 10);
			this.L_OutOfDate.Name = "L_OutOfDate";
			this.L_OutOfDate.Size = new System.Drawing.Size(55, 23);
			this.L_OutOfDate.TabIndex = 15;
			this.L_OutOfDate.Text = "label1";
			// 
			// L_Incomplete
			// 
			this.L_Incomplete.AutoSize = true;
			this.L_Incomplete.Location = new System.Drawing.Point(3, 36);
			this.L_Incomplete.Name = "L_Incomplete";
			this.L_Incomplete.Size = new System.Drawing.Size(55, 20);
			this.L_Incomplete.TabIndex = 15;
			this.L_Incomplete.Text = "label1";
			// 
			// slickScroll1
			// 
			this.slickScroll1.Dock = System.Windows.Forms.DockStyle.Right;
			this.slickScroll1.LinkedControl = this.tableLayoutPanel3;
			this.slickScroll1.Location = new System.Drawing.Point(775, 30);
			this.slickScroll1.Name = "slickScroll1";
			this.slickScroll1.Size = new System.Drawing.Size(8, 408);
			this.slickScroll1.SmallHandle = true;
			this.slickScroll1.Style = SlickControls.StyleType.Vertical;
			this.slickScroll1.TabIndex = 18;
			this.slickScroll1.TabStop = false;
			this.slickScroll1.Text = "slickScroll1";
			// 
			// P_Container
			// 
			this.P_Container.Controls.Add(this.tableLayoutPanel3);
			this.P_Container.Dock = System.Windows.Forms.DockStyle.Fill;
			this.P_Container.Location = new System.Drawing.Point(0, 30);
			this.P_Container.Name = "P_Container";
			this.P_Container.Size = new System.Drawing.Size(775, 408);
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
			this.Controls.SetChildIndex(this.base_Text, 0);
			this.Controls.SetChildIndex(this.slickScroll1, 0);
			this.Controls.SetChildIndex(this.P_Container, 0);
			this.P_Filters.ResumeLayout(false);
			this.P_Filters.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel3.ResumeLayout(false);
			this.tableLayoutPanel3.PerformLayout();
			this.roundedGroupPanel1.ResumeLayout(false);
			this.roundedGroupPanel1.PerformLayout();
			this.tableLayoutPanel4.ResumeLayout(false);
			this.P_ModIssues.ResumeLayout(false);
			this.P_ModIssues.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.P_Container.ResumeLayout(false);
			this.P_Container.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	private SlickControls.SlickTextBox TB_CollectionLink;
	private SlickControls.RoundedGroupPanel P_Filters;
	private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	private SlickControls.SlickButton B_LoadCollection;
	private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
	private SlickControls.RoundedGroupPanel P_ModIssues;
	private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
	private SlickControls.SlickScroll slickScroll1;
	private System.Windows.Forms.Panel P_Container;
	private SlickControls.RoundedGroupPanel P_DuplicateMods;
	private SlickControls.SlickButton B_ReDownload;
	private System.Windows.Forms.Label L_OutOfDate;
	private System.Windows.Forms.Label L_Incomplete;
	private SlickControls.RoundedGroupPanel roundedGroupPanel1;
	private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
	private Generic.DragAndDropControl dragAndDropControl1;
}
