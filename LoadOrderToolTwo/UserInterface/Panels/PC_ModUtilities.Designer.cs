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
			this.B_ReDownload = new SlickControls.SlickButton();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.B_LoadCollection = new SlickControls.SlickButton();
			this.P_Filters.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
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
			this.TB_CollectionLink.Size = new System.Drawing.Size(651, 49);
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
			this.P_Filters.Location = new System.Drawing.Point(3, 49);
			this.P_Filters.Name = "P_Filters";
			this.P_Filters.Padding = new System.Windows.Forms.Padding(7, 38, 7, 7);
			this.P_Filters.Size = new System.Drawing.Size(777, 100);
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
			this.tableLayoutPanel1.Size = new System.Drawing.Size(763, 55);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// B_ReDownload
			// 
			this.B_ReDownload.ColorShade = null;
			this.B_ReDownload.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_ReDownload.Location = new System.Drawing.Point(3, 3);
			this.B_ReDownload.Name = "B_ReDownload";
			this.B_ReDownload.Size = new System.Drawing.Size(306, 40);
			this.B_ReDownload.SpaceTriggersClick = true;
			this.B_ReDownload.TabIndex = 2;
			this.B_ReDownload.Text = "RedownloadMods";
			this.B_ReDownload.Click += new System.EventHandler(this.B_ReDownload_Click);
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.ColumnCount = 1;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.Controls.Add(this.B_ReDownload, 0, 0);
			this.tableLayoutPanel3.Controls.Add(this.P_Filters, 0, 1);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 30);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 2;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel3.Size = new System.Drawing.Size(783, 408);
			this.tableLayoutPanel3.TabIndex = 17;
			// 
			// B_LoadCollection
			// 
			this.B_LoadCollection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.B_LoadCollection.ColorShade = null;
			this.B_LoadCollection.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_LoadCollection.Image = global::LoadOrderToolTwo.Properties.Resources.I_Import;
			this.B_LoadCollection.Location = new System.Drawing.Point(660, 3);
			this.B_LoadCollection.Name = "B_LoadCollection";
			this.B_LoadCollection.Padding = new System.Windows.Forms.Padding(10, 15, 10, 15);
			this.B_LoadCollection.Size = new System.Drawing.Size(100, 49);
			this.B_LoadCollection.SpaceTriggersClick = true;
			this.B_LoadCollection.TabIndex = 15;
			this.B_LoadCollection.Text = "LoadCollection";
			this.B_LoadCollection.Click += new System.EventHandler(this.B_LoadCollection_Click);
			// 
			// PC_ModUtilities
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.tableLayoutPanel3);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(58)))), ((int)(((byte)(69)))));
			this.LabelBounds = new System.Drawing.Point(-2, 3);
			this.Name = "PC_ModUtilities";
			this.Padding = new System.Windows.Forms.Padding(0, 30, 0, 0);
			this.Controls.SetChildIndex(this.base_Text, 0);
			this.Controls.SetChildIndex(this.tableLayoutPanel3, 0);
			this.P_Filters.ResumeLayout(false);
			this.P_Filters.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel3.ResumeLayout(false);
			this.tableLayoutPanel3.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	private SlickControls.SlickTextBox TB_CollectionLink;
	private SlickControls.RoundedGroupPanel P_Filters;
	private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	private SlickControls.SlickButton B_LoadCollection;
	private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
	private SlickControls.SlickButton B_ReDownload;
}
