namespace SkyveApp.UserInterface.Panels;

partial class PC_CompatibilityReport
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
			SkyveApp.Utilities.Managers.CompatibilityManager.ReportProcessed -= CompatibilityManager_ReportProcessed;
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
			SlickControls.DynamicIcon dynamicIcon1 = new SlickControls.DynamicIcon();
			SlickControls.DynamicIcon dynamicIcon2 = new SlickControls.DynamicIcon();
			SlickControls.DynamicIcon dynamicIcon3 = new SlickControls.DynamicIcon();
			this.TLP_Buttons = new System.Windows.Forms.TableLayoutPanel();
			this.B_Manage = new SlickControls.SlickButton();
			this.B_YourPackages = new SlickControls.SlickButton();
			this.B_ManageSingle = new SlickControls.SlickButton();
			this.panel1 = new System.Windows.Forms.Panel();
			this.FLP_Reports = new SlickControls.SmartFlowPanel();
			this.slickScroll1 = new SlickControls.SlickScroll();
			this.slickPictureBox1 = new SlickControls.SlickPictureBox();
			this.TLP_Buttons.SuspendLayout();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.slickPictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// base_Text
			// 
			this.base_Text.Location = new System.Drawing.Point(-2, 3);
			// 
			// TLP_Buttons
			// 
			this.TLP_Buttons.AutoSize = true;
			this.TLP_Buttons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.TLP_Buttons.ColumnCount = 4;
			this.TLP_Buttons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TLP_Buttons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Buttons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Buttons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Buttons.Controls.Add(this.B_Manage, 2, 0);
			this.TLP_Buttons.Controls.Add(this.B_YourPackages, 3, 0);
			this.TLP_Buttons.Controls.Add(this.B_ManageSingle, 1, 0);
			this.TLP_Buttons.Dock = System.Windows.Forms.DockStyle.Top;
			this.TLP_Buttons.Location = new System.Drawing.Point(0, 30);
			this.TLP_Buttons.Margin = new System.Windows.Forms.Padding(0);
			this.TLP_Buttons.Name = "TLP_Buttons";
			this.TLP_Buttons.RowCount = 2;
			this.TLP_Buttons.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Buttons.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Buttons.Size = new System.Drawing.Size(783, 36);
			this.TLP_Buttons.TabIndex = 0;
			// 
			// B_Manage
			// 
			this.B_Manage.AutoSize = true;
			this.B_Manage.ColorShade = null;
			this.B_Manage.Cursor = System.Windows.Forms.Cursors.Hand;
			dynamicIcon1.Name = "I_Cog";
			this.B_Manage.ImageName = dynamicIcon1;
			this.B_Manage.Location = new System.Drawing.Point(555, 3);
			this.B_Manage.Name = "B_Manage";
			this.B_Manage.Size = new System.Drawing.Size(100, 30);
			this.B_Manage.SpaceTriggersClick = true;
			this.B_Manage.TabIndex = 0;
			this.B_Manage.Text = "ManageCompatibilityData";
			this.B_Manage.Click += new System.EventHandler(this.B_Manage_Click);
			// 
			// B_YourPackages
			// 
			this.B_YourPackages.AutoSize = true;
			this.B_YourPackages.ColorShade = null;
			this.B_YourPackages.Cursor = System.Windows.Forms.Cursors.Hand;
			dynamicIcon2.Name = "I_User";
			this.B_YourPackages.ImageName = dynamicIcon2;
			this.B_YourPackages.Location = new System.Drawing.Point(661, 3);
			this.B_YourPackages.Name = "B_YourPackages";
			this.B_YourPackages.Size = new System.Drawing.Size(119, 30);
			this.B_YourPackages.SpaceTriggersClick = true;
			this.B_YourPackages.TabIndex = 0;
			this.B_YourPackages.Text = "YourPackages";
			this.B_YourPackages.Click += new System.EventHandler(this.B_YourPackages_Click);
			// 
			// B_ManageSingle
			// 
			this.B_ManageSingle.AutoSize = true;
			this.B_ManageSingle.ColorShade = null;
			this.B_ManageSingle.Cursor = System.Windows.Forms.Cursors.Hand;
			dynamicIcon3.Name = "I_Edit";
			this.B_ManageSingle.ImageName = dynamicIcon3;
			this.B_ManageSingle.Location = new System.Drawing.Point(449, 3);
			this.B_ManageSingle.Name = "B_ManageSingle";
			this.B_ManageSingle.Size = new System.Drawing.Size(100, 30);
			this.B_ManageSingle.SpaceTriggersClick = true;
			this.B_ManageSingle.TabIndex = 0;
			this.B_ManageSingle.Text = "ManageSinglePackage";
			this.B_ManageSingle.Click += new System.EventHandler(this.B_ManageSingle_Click);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.FLP_Reports);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 66);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(783, 372);
			this.panel1.TabIndex = 2;
			// 
			// FLP_Reports
			// 
			this.FLP_Reports.Location = new System.Drawing.Point(0, 186);
			this.FLP_Reports.Margin = new System.Windows.Forms.Padding(0);
			this.FLP_Reports.Name = "FLP_Reports";
			this.FLP_Reports.Size = new System.Drawing.Size(783, 0);
			this.FLP_Reports.TabIndex = 2;
			// 
			// slickScroll1
			// 
			this.slickScroll1.Dock = System.Windows.Forms.DockStyle.Right;
			this.slickScroll1.LinkedControl = this.FLP_Reports;
			this.slickScroll1.Location = new System.Drawing.Point(773, 66);
			this.slickScroll1.Name = "slickScroll1";
			this.slickScroll1.Size = new System.Drawing.Size(10, 372);
			this.slickScroll1.Style = SlickControls.StyleType.Vertical;
			this.slickScroll1.TabIndex = 3;
			this.slickScroll1.TabStop = false;
			this.slickScroll1.Text = "slickScroll1";
			// 
			// slickPictureBox1
			// 
			this.slickPictureBox1.LoaderSpeed = 1D;
			this.slickPictureBox1.Location = new System.Drawing.Point(375, 203);
			this.slickPictureBox1.Name = "slickPictureBox1";
			this.slickPictureBox1.Size = new System.Drawing.Size(32, 32);
			this.slickPictureBox1.TabIndex = 5;
			this.slickPictureBox1.TabStop = false;
			this.slickPictureBox1.Visible = false;
			// 
			// PC_CompatibilityReport
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.slickPictureBox1);
			this.Controls.Add(this.slickScroll1);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.TLP_Buttons);
			this.LabelBounds = new System.Drawing.Point(-2, 3);
			this.Name = "PC_CompatibilityReport";
			this.Padding = new System.Windows.Forms.Padding(0, 30, 0, 0);
			this.Controls.SetChildIndex(this.base_Text, 0);
			this.Controls.SetChildIndex(this.TLP_Buttons, 0);
			this.Controls.SetChildIndex(this.panel1, 0);
			this.Controls.SetChildIndex(this.slickScroll1, 0);
			this.Controls.SetChildIndex(this.slickPictureBox1, 0);
			this.TLP_Buttons.ResumeLayout(false);
			this.TLP_Buttons.PerformLayout();
			this.panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.slickPictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion
	private System.Windows.Forms.TableLayoutPanel TLP_Buttons;
	private SlickControls.SlickButton B_Manage;
	private SlickControls.SlickButton B_YourPackages;
	private SlickControls.SlickButton B_ManageSingle;
	private System.Windows.Forms.Panel panel1;
	private SlickControls.SlickScroll slickScroll1;
	private SlickControls.SmartFlowPanel FLP_Reports;
	private SlickControls.SlickPictureBox slickPictureBox1;
}
