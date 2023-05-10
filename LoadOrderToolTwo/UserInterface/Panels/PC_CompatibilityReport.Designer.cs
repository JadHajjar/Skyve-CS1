namespace LoadOrderToolTwo.UserInterface.Panels;

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
			this.TLP_Main = new System.Windows.Forms.TableLayoutPanel();
			this.TLP_Buttons = new System.Windows.Forms.TableLayoutPanel();
			this.B_Manage = new SlickControls.SlickButton();
			this.B_YourPackages = new SlickControls.SlickButton();
			this.B_ManageSingle = new SlickControls.SlickButton();
			this.PB_Loading = new SlickControls.SlickPictureBox();
			this.TLP_Main.SuspendLayout();
			this.TLP_Buttons.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PB_Loading)).BeginInit();
			this.SuspendLayout();
			// 
			// base_Text
			// 
			this.base_Text.Location = new System.Drawing.Point(-2, 3);
			// 
			// TLP_Main
			// 
			this.TLP_Main.ColumnCount = 2;
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TLP_Main.Controls.Add(this.TLP_Buttons, 0, 0);
			this.TLP_Main.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TLP_Main.Location = new System.Drawing.Point(0, 30);
			this.TLP_Main.Name = "TLP_Main";
			this.TLP_Main.RowCount = 2;
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TLP_Main.Size = new System.Drawing.Size(783, 408);
			this.TLP_Main.TabIndex = 13;
			// 
			// TLP_Buttons
			// 
			this.TLP_Buttons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.TLP_Buttons.AutoSize = true;
			this.TLP_Buttons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.TLP_Buttons.ColumnCount = 3;
			this.TLP_Main.SetColumnSpan(this.TLP_Buttons, 2);
			this.TLP_Buttons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Buttons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Buttons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Buttons.Controls.Add(this.B_Manage, 1, 0);
			this.TLP_Buttons.Controls.Add(this.B_YourPackages, 2, 0);
			this.TLP_Buttons.Controls.Add(this.B_ManageSingle, 0, 0);
			this.TLP_Buttons.Location = new System.Drawing.Point(446, 0);
			this.TLP_Buttons.Margin = new System.Windows.Forms.Padding(0);
			this.TLP_Buttons.Name = "TLP_Buttons";
			this.TLP_Buttons.RowCount = 1;
			this.TLP_Buttons.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Buttons.Size = new System.Drawing.Size(337, 36);
			this.TLP_Buttons.TabIndex = 0;
			this.TLP_Buttons.Visible = false;
			// 
			// B_Manage
			// 
			this.B_Manage.AutoSize = true;
			this.B_Manage.ColorShade = null;
			this.B_Manage.Cursor = System.Windows.Forms.Cursors.Hand;
			dynamicIcon1.Name = "I_Cog";
			this.B_Manage.ImageName = dynamicIcon1;
			this.B_Manage.Location = new System.Drawing.Point(109, 3);
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
			this.B_YourPackages.Location = new System.Drawing.Point(215, 3);
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
			this.B_ManageSingle.Location = new System.Drawing.Point(3, 3);
			this.B_ManageSingle.Name = "B_ManageSingle";
			this.B_ManageSingle.Size = new System.Drawing.Size(100, 30);
			this.B_ManageSingle.SpaceTriggersClick = true;
			this.B_ManageSingle.TabIndex = 0;
			this.B_ManageSingle.Text = "ManageSinglePackage";
			this.B_ManageSingle.Click += new System.EventHandler(this.B_ManageSingle_Click);
			// 
			// PB_Loading
			// 
			this.PB_Loading.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PB_Loading.Location = new System.Drawing.Point(0, 30);
			this.PB_Loading.Name = "PB_Loading";
			this.PB_Loading.Size = new System.Drawing.Size(783, 408);
			this.PB_Loading.TabIndex = 14;
			this.PB_Loading.TabStop = false;
			// 
			// PC_CompatibilityReport
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.TLP_Main);
			this.Controls.Add(this.PB_Loading);
			this.LabelBounds = new System.Drawing.Point(-2, 3);
			this.Name = "PC_CompatibilityReport";
			this.Padding = new System.Windows.Forms.Padding(0, 30, 0, 0);
			this.Controls.SetChildIndex(this.PB_Loading, 0);
			this.Controls.SetChildIndex(this.TLP_Main, 0);
			this.Controls.SetChildIndex(this.base_Text, 0);
			this.TLP_Main.ResumeLayout(false);
			this.TLP_Main.PerformLayout();
			this.TLP_Buttons.ResumeLayout(false);
			this.TLP_Buttons.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PB_Loading)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	private System.Windows.Forms.TableLayoutPanel TLP_Main;
	private System.Windows.Forms.TableLayoutPanel TLP_Buttons;
	private SlickControls.SlickButton B_Manage;
	private SlickControls.SlickButton B_YourPackages;
	private SlickControls.SlickPictureBox PB_Loading;
	private SlickControls.SlickButton B_ManageSingle;
}
