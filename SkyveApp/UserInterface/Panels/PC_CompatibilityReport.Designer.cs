using SkyveApp.Services;

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
			_compatibilityManager.ReportProcessed -= CompatibilityManager_ReportProcessed;
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
			SlickControls.DynamicIcon dynamicIcon4 = new SlickControls.DynamicIcon();
			SlickControls.DynamicIcon dynamicIcon1 = new SlickControls.DynamicIcon();
			SlickControls.DynamicIcon dynamicIcon3 = new SlickControls.DynamicIcon();
			SlickControls.DynamicIcon dynamicIcon2 = new SlickControls.DynamicIcon();
			this.TLP_Buttons = new System.Windows.Forms.TableLayoutPanel();
			this.B_Requests = new SlickControls.SlickButton();
			this.B_Manage = new SlickControls.SlickButton();
			this.B_ManageSingle = new SlickControls.SlickButton();
			this.B_YourPackages = new SlickControls.SlickButton();
			this.tabHeader = new SlickControls.SlickTabHeader();
			this.PB_Loader = new SlickControls.SlickPictureBox();
			this.LC_Items = new SkyveApp.UserInterface.Lists.CompatibilityReportList();
			this.TLP_Buttons.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PB_Loader)).BeginInit();
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
			this.TLP_Buttons.Controls.Add(this.B_Manage, 3, 0);
			this.TLP_Buttons.Controls.Add(this.B_YourPackages, 0, 0);
			this.TLP_Buttons.Controls.Add(this.B_ManageSingle, 1, 0);
			this.TLP_Buttons.Controls.Add(this.B_Requests, 2, 0);
			this.TLP_Buttons.Dock = System.Windows.Forms.DockStyle.Top;
			this.TLP_Buttons.Location = new System.Drawing.Point(0, 30);
			this.TLP_Buttons.Margin = new System.Windows.Forms.Padding(0);
			this.TLP_Buttons.Name = "TLP_Buttons";
			this.TLP_Buttons.RowCount = 2;
			this.TLP_Buttons.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Buttons.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Buttons.Size = new System.Drawing.Size(1312, 36);
			this.TLP_Buttons.TabIndex = 0;
			// 
			// B_Requests
			// 
			this.B_Requests.AutoSize = true;
			this.B_Requests.ColorShade = null;
			this.B_Requests.Cursor = System.Windows.Forms.Cursors.Hand;
			dynamicIcon4.Name = "I_RequestReview";
			this.B_Requests.ImageName = dynamicIcon4;
			this.B_Requests.Location = new System.Drawing.Point(1103, 3);
			this.B_Requests.Name = "B_Requests";
			this.B_Requests.Size = new System.Drawing.Size(100, 30);
			this.B_Requests.SpaceTriggersClick = true;
			this.B_Requests.TabIndex = 2;
			this.B_Requests.Text = "ViewRequests";
			this.B_Requests.Click += new System.EventHandler(this.B_Requests_Click);
			// 
			// B_Manage
			// 
			this.B_Manage.AutoSize = true;
			this.B_Manage.ColorShade = null;
			this.B_Manage.Cursor = System.Windows.Forms.Cursors.Hand;
			dynamicIcon1.Name = "I_Cog";
			this.B_Manage.ImageName = dynamicIcon1;
			this.B_Manage.Location = new System.Drawing.Point(1209, 3);
			this.B_Manage.Name = "B_Manage";
			this.B_Manage.Size = new System.Drawing.Size(100, 30);
			this.B_Manage.SpaceTriggersClick = true;
			this.B_Manage.TabIndex = 3;
			this.B_Manage.Text = "ManageCompatibilityData";
			this.B_Manage.Click += new System.EventHandler(this.B_Manage_Click);
			// 
			// B_ManageSingle
			// 
			this.B_ManageSingle.AutoSize = true;
			this.B_ManageSingle.ColorShade = null;
			this.B_ManageSingle.Cursor = System.Windows.Forms.Cursors.Hand;
			dynamicIcon3.Name = "I_Edit";
			this.B_ManageSingle.ImageName = dynamicIcon3;
			this.B_ManageSingle.Location = new System.Drawing.Point(997, 3);
			this.B_ManageSingle.Name = "B_ManageSingle";
			this.B_ManageSingle.Size = new System.Drawing.Size(100, 30);
			this.B_ManageSingle.SpaceTriggersClick = true;
			this.B_ManageSingle.TabIndex = 1;
			this.B_ManageSingle.Text = "ManageSinglePackage";
			this.B_ManageSingle.Click += new System.EventHandler(this.B_ManageSingle_Click);
			// 
			// B_YourPackages
			// 
			this.B_YourPackages.AutoSize = true;
			this.B_YourPackages.ColorShade = null;
			this.B_YourPackages.Cursor = System.Windows.Forms.Cursors.Hand;
			dynamicIcon2.Name = "I_User";
			this.B_YourPackages.ImageName = dynamicIcon2;
			this.B_YourPackages.Location = new System.Drawing.Point(3, 3);
			this.B_YourPackages.Name = "B_YourPackages";
			this.B_YourPackages.Size = new System.Drawing.Size(119, 30);
			this.B_YourPackages.SpaceTriggersClick = true;
			this.B_YourPackages.TabIndex = 0;
			this.B_YourPackages.Text = "YourPackages";
			this.B_YourPackages.Click += new System.EventHandler(this.B_YourPackages_Click);
			// 
			// tabHeader
			// 
			this.tabHeader.Dock = System.Windows.Forms.DockStyle.Top;
			this.tabHeader.Location = new System.Drawing.Point(0, 66);
			this.tabHeader.Name = "tabHeader";
			this.tabHeader.Padding = new System.Windows.Forms.Padding(0);
			this.tabHeader.Size = new System.Drawing.Size(1312, 60);
			this.tabHeader.TabIndex = 100;
			this.tabHeader.Tabs = new SlickControls.SlickTab[0];
			// 
			// PB_Loader
			// 
			this.PB_Loader.LoaderSpeed = 1D;
			this.PB_Loader.Location = new System.Drawing.Point(640, 392);
			this.PB_Loader.Name = "PB_Loader";
			this.PB_Loader.Size = new System.Drawing.Size(32, 32);
			this.PB_Loader.TabIndex = 102;
			this.PB_Loader.TabStop = false;
			this.PB_Loader.Visible = false;
			// 
			// LC_Items
			// 
			this.LC_Items.AllowDrop = true;
			this.LC_Items.AutoInvalidate = false;
			this.LC_Items.AutoScroll = true;
			this.LC_Items.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LC_Items.HighlightOnHover = true;
			this.LC_Items.ItemHeight = 75;
			this.LC_Items.Location = new System.Drawing.Point(0, 126);
			this.LC_Items.Name = "LC_Items";
			this.LC_Items.SeparateWithLines = true;
			this.LC_Items.Size = new System.Drawing.Size(1312, 691);
			this.LC_Items.TabIndex = 101;
			// 
			// PC_CompatibilityReport
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.PB_Loader);
			this.Controls.Add(this.LC_Items);
			this.Controls.Add(this.tabHeader);
			this.Controls.Add(this.TLP_Buttons);
			this.LabelBounds = new System.Drawing.Point(-2, 3);
			this.Name = "PC_CompatibilityReport";
			this.Padding = new System.Windows.Forms.Padding(0, 30, 0, 0);
			this.Size = new System.Drawing.Size(1312, 817);
			this.Controls.SetChildIndex(this.base_Text, 0);
			this.Controls.SetChildIndex(this.TLP_Buttons, 0);
			this.Controls.SetChildIndex(this.tabHeader, 0);
			this.Controls.SetChildIndex(this.LC_Items, 0);
			this.Controls.SetChildIndex(this.PB_Loader, 0);
			this.TLP_Buttons.ResumeLayout(false);
			this.TLP_Buttons.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PB_Loader)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion
	private System.Windows.Forms.TableLayoutPanel TLP_Buttons;
	private SlickControls.SlickButton B_Manage;
	private SlickControls.SlickButton B_YourPackages;
	private SlickControls.SlickButton B_ManageSingle;
	private SlickControls.SlickTabHeader tabHeader;
	private Lists.CompatibilityReportList LC_Items;
	private SlickControls.SlickPictureBox PB_Loader;
	private SlickControls.SlickButton B_Requests;
}
