namespace LoadOrderToolTwo.UserInterface.Forms;

partial class AddPackageForm
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

	#region Windows Form Designer generated code

	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
			SlickControls.DynamicIcon dynamicIcon1 = new SlickControls.DynamicIcon();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.OT_ModAsset = new LoadOrderToolTwo.UserInterface.Generic.ThreeOptionToggle();
			this.TB_Search = new SlickControls.SlickTextBox();
			this.slickSpacer1 = new SlickControls.SlickSpacer();
			this.base_P_Content.SuspendLayout();
			this.base_P_Container.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.base_PB_Icon)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// base_P_Content
			// 
			this.base_P_Content.AutoScroll = true;
			this.base_P_Content.Controls.Add(this.tableLayoutPanel1);
			this.base_P_Content.Location = new System.Drawing.Point(1, 43);
			this.base_P_Content.Size = new System.Drawing.Size(757, 407);
			// 
			// base_P_Controls
			// 
			this.base_P_Controls.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(235)))), ((int)(((byte)(243)))));
			this.base_P_Controls.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(58)))), ((int)(((byte)(69)))));
			this.base_P_Controls.Location = new System.Drawing.Point(1, 43);
			// 
			// base_P_Top_Spacer
			// 
			this.base_P_Top_Spacer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(235)))), ((int)(((byte)(243)))));
			this.base_P_Top_Spacer.Size = new System.Drawing.Size(757, 6);
			// 
			// base_P_Container
			// 
			this.base_P_Container.Size = new System.Drawing.Size(759, 451);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Controls.Add(this.OT_ModAsset, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.TB_Search, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.slickSpacer1, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(757, 39);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// OT_ModAsset
			// 
			this.OT_ModAsset.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.OT_ModAsset.Cursor = System.Windows.Forms.Cursors.Hand;
			this.OT_ModAsset.Image1 = "I_Mods";
			this.OT_ModAsset.Image2 = "I_Assets";
			this.OT_ModAsset.Location = new System.Drawing.Point(381, 3);
			this.OT_ModAsset.Name = "OT_ModAsset";
			this.OT_ModAsset.Option1 = "Mods";
			this.OT_ModAsset.Option2 = "Assets";
			this.OT_ModAsset.OptionStyle1 = Extensions.ColorStyle.Active;
			this.OT_ModAsset.OptionStyle2 = Extensions.ColorStyle.Active;
			this.OT_ModAsset.Size = new System.Drawing.Size(373, 26);
			this.OT_ModAsset.TabIndex = 11;
			this.OT_ModAsset.SelectedValueChanged += new System.EventHandler(this.TB_Search_TextChanged);
			// 
			// TB_Search
			// 
			dynamicIcon1.Name = "I_Search";
			this.TB_Search.ImageName = dynamicIcon1;
			this.TB_Search.Location = new System.Drawing.Point(3, 3);
			this.TB_Search.Name = "TB_Search";
			this.TB_Search.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.TB_Search.SelectedText = "";
			this.TB_Search.SelectionLength = 0;
			this.TB_Search.SelectionStart = 0;
			this.TB_Search.ShowLabel = false;
			this.TB_Search.Size = new System.Drawing.Size(372, 26);
			this.TB_Search.TabIndex = 0;
			this.TB_Search.TextChanged += new System.EventHandler(this.TB_Search_TextChanged);
			this.TB_Search.IconClicked += new System.EventHandler(this.TB_Search_IconClicked);
			// 
			// slickSpacer1
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.slickSpacer1, 2);
			this.slickSpacer1.Dock = System.Windows.Forms.DockStyle.Top;
			this.slickSpacer1.Location = new System.Drawing.Point(3, 35);
			this.slickSpacer1.Name = "slickSpacer1";
			this.slickSpacer1.Size = new System.Drawing.Size(751, 1);
			this.slickSpacer1.TabIndex = 1;
			this.slickSpacer1.TabStop = false;
			this.slickSpacer1.Text = "slickSpacer1";
			// 
			// AddPackageForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(770, 462);
			this.MaximizedBounds = new System.Drawing.Rectangle(0, 0, 2560, 1380);
			this.Name = "AddPackageForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "";
			this.base_P_Content.ResumeLayout(false);
			this.base_P_Content.PerformLayout();
			this.base_P_Container.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.base_PB_Icon)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

	}

	#endregion

	private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	private SlickControls.SlickTextBox TB_Search;
	private SlickControls.SlickSpacer slickSpacer1;
	private Generic.ThreeOptionToggle OT_ModAsset;
}