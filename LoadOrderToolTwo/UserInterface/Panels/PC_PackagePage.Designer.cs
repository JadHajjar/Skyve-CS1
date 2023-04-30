using LoadOrderToolTwo.UserInterface.Content;

namespace LoadOrderToolTwo.UserInterface.Panels;

partial class PC_PackagePage
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
			SlickControls.DynamicIcon dynamicIcon4 = new SlickControls.DynamicIcon();
			SlickControls.DynamicIcon dynamicIcon5 = new SlickControls.DynamicIcon();
			SlickControls.DynamicIcon dynamicIcon6 = new SlickControls.DynamicIcon();
			this.P_Content = new System.Windows.Forms.Panel();
			this.slickTabControl1 = new SlickControls.SlickTabControl();
			this.T_Info = new SlickControls.SlickTabControl.Tab();
			this.T_CR = new SlickControls.SlickTabControl.Tab();
			this.T_Profiles = new SlickControls.SlickTabControl.Tab();
			this.TLP_Profiles = new System.Windows.Forms.TableLayoutPanel();
			this.TLP_Top = new System.Windows.Forms.TableLayoutPanel();
			this.PB_Icon = new LoadOrderToolTwo.UserInterface.Content.PackageIcon();
			this.L_Title = new System.Windows.Forms.Label();
			this.P_Back = new System.Windows.Forms.Panel();
			this.P_Info = new LoadOrderToolTwo.UserInterface.Content.PackageDescriptionControl();
			this.B_Folder = new SlickControls.SlickButton();
			this.B_SteamPage = new SlickControls.SlickButton();
			this.B_Redownload = new SlickControls.SlickButton();
			this.P_Content.SuspendLayout();
			this.TLP_Top.SuspendLayout();
			this.SuspendLayout();
			// 
			// base_Text
			// 
			this.base_Text.Location = new System.Drawing.Point(-2, 3);
			this.base_Text.Text = "Back";
			// 
			// P_Content
			// 
			this.P_Content.Controls.Add(this.slickTabControl1);
			this.P_Content.Dock = System.Windows.Forms.DockStyle.Fill;
			this.P_Content.Location = new System.Drawing.Point(0, 130);
			this.P_Content.Name = "P_Content";
			this.P_Content.Size = new System.Drawing.Size(783, 308);
			this.P_Content.TabIndex = 13;
			// 
			// slickTabControl1
			// 
			this.slickTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.slickTabControl1.Location = new System.Drawing.Point(0, 0);
			this.slickTabControl1.Margin = new System.Windows.Forms.Padding(0);
			this.slickTabControl1.Name = "slickTabControl1";
			this.slickTabControl1.Size = new System.Drawing.Size(783, 308);
			this.slickTabControl1.TabIndex = 0;
			this.slickTabControl1.Tabs = new SlickControls.SlickTabControl.Tab[] {
        this.T_Info,
        this.T_CR,
        this.T_Profiles};
			// 
			// T_Info
			// 
			this.T_Info.Cursor = System.Windows.Forms.Cursors.Hand;
			this.T_Info.Dock = System.Windows.Forms.DockStyle.Left;
			this.T_Info.Font = new System.Drawing.Font("Nirmala UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dynamicIcon1.Name = "I_Content";
			this.T_Info.IconName = dynamicIcon1;
			this.T_Info.LinkedControl = null;
			this.T_Info.Location = new System.Drawing.Point(0, 5);
			this.T_Info.Name = "T_Info";
			this.T_Info.Selected = true;
			this.T_Info.Size = new System.Drawing.Size(261, 25);
			this.T_Info.TabIndex = 0;
			this.T_Info.TabStop = false;
			this.T_Info.Text = "tab1";
			// 
			// T_CR
			// 
			this.T_CR.Cursor = System.Windows.Forms.Cursors.Hand;
			this.T_CR.Dock = System.Windows.Forms.DockStyle.Left;
			this.T_CR.Font = new System.Drawing.Font("Nirmala UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dynamicIcon2.Name = "I_CompatibilityReport";
			this.T_CR.IconName = dynamicIcon2;
			this.T_CR.LinkedControl = null;
			this.T_CR.Location = new System.Drawing.Point(261, 5);
			this.T_CR.Name = "T_CR";
			this.T_CR.Selected = false;
			this.T_CR.Size = new System.Drawing.Size(261, 25);
			this.T_CR.TabIndex = 0;
			this.T_CR.TabStop = false;
			this.T_CR.Text = "tab2";
			// 
			// T_Profiles
			// 
			this.T_Profiles.Cursor = System.Windows.Forms.Cursors.Hand;
			this.T_Profiles.Dock = System.Windows.Forms.DockStyle.Left;
			this.T_Profiles.Font = new System.Drawing.Font("Nirmala UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dynamicIcon3.Name = "I_ProfileSettings";
			this.T_Profiles.IconName = dynamicIcon3;
			this.T_Profiles.LinkedControl = this.TLP_Profiles;
			this.T_Profiles.Location = new System.Drawing.Point(522, 5);
			this.T_Profiles.Name = "T_Profiles";
			this.T_Profiles.Selected = false;
			this.T_Profiles.Size = new System.Drawing.Size(261, 25);
			this.T_Profiles.TabIndex = 0;
			this.T_Profiles.TabStop = false;
			this.T_Profiles.Text = "tab3";
			// 
			// TLP_Profiles
			// 
			this.TLP_Profiles.ColumnCount = 2;
			this.TLP_Profiles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TLP_Profiles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TLP_Profiles.Location = new System.Drawing.Point(0, 0);
			this.TLP_Profiles.MaximumSize = new System.Drawing.Size(769, 2147483647);
			this.TLP_Profiles.MinimumSize = new System.Drawing.Size(769, 0);
			this.TLP_Profiles.Name = "TLP_Profiles";
			this.TLP_Profiles.RowCount = 2;
			this.TLP_Profiles.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TLP_Profiles.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TLP_Profiles.Size = new System.Drawing.Size(769, 208);
			this.TLP_Profiles.TabIndex = 16;
			// 
			// TLP_Top
			// 
			this.TLP_Top.ColumnCount = 6;
			this.TLP_Top.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.TLP_Top.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Top.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TLP_Top.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Top.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Top.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Top.Controls.Add(this.PB_Icon, 1, 0);
			this.TLP_Top.Controls.Add(this.L_Title, 2, 0);
			this.TLP_Top.Controls.Add(this.P_Back, 0, 1);
			this.TLP_Top.Controls.Add(this.P_Info, 2, 1);
			this.TLP_Top.Controls.Add(this.B_Folder, 5, 0);
			this.TLP_Top.Controls.Add(this.B_SteamPage, 4, 0);
			this.TLP_Top.Controls.Add(this.B_Redownload, 3, 0);
			this.TLP_Top.Dock = System.Windows.Forms.DockStyle.Top;
			this.TLP_Top.Location = new System.Drawing.Point(0, 30);
			this.TLP_Top.Margin = new System.Windows.Forms.Padding(0);
			this.TLP_Top.Name = "TLP_Top";
			this.TLP_Top.RowCount = 2;
			this.TLP_Top.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TLP_Top.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TLP_Top.Size = new System.Drawing.Size(783, 100);
			this.TLP_Top.TabIndex = 0;
			// 
			// PB_Icon
			// 
			this.PB_Icon.Dock = System.Windows.Forms.DockStyle.Left;
			this.PB_Icon.Location = new System.Drawing.Point(32, 0);
			this.PB_Icon.Margin = new System.Windows.Forms.Padding(0);
			this.PB_Icon.Name = "PB_Icon";
			this.TLP_Top.SetRowSpan(this.PB_Icon, 2);
			this.PB_Icon.Size = new System.Drawing.Size(100, 100);
			this.PB_Icon.TabIndex = 0;
			this.PB_Icon.TabStop = false;
			// 
			// L_Title
			// 
			this.L_Title.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.L_Title.AutoSize = true;
			this.L_Title.Location = new System.Drawing.Point(135, 25);
			this.L_Title.Name = "L_Title";
			this.L_Title.Size = new System.Drawing.Size(63, 25);
			this.L_Title.TabIndex = 1;
			this.L_Title.Text = "label1";
			this.L_Title.UseMnemonic = false;
			// 
			// P_Back
			// 
			this.P_Back.Dock = System.Windows.Forms.DockStyle.Fill;
			this.P_Back.Location = new System.Drawing.Point(0, 50);
			this.P_Back.Margin = new System.Windows.Forms.Padding(0);
			this.P_Back.Name = "P_Back";
			this.P_Back.Size = new System.Drawing.Size(32, 50);
			this.P_Back.TabIndex = 2;
			// 
			// P_Info
			// 
			this.TLP_Top.SetColumnSpan(this.P_Info, 4);
			this.P_Info.Dock = System.Windows.Forms.DockStyle.Fill;
			this.P_Info.Location = new System.Drawing.Point(132, 50);
			this.P_Info.Margin = new System.Windows.Forms.Padding(0);
			this.P_Info.Name = "P_Info";
			this.P_Info.Size = new System.Drawing.Size(651, 50);
			this.P_Info.TabIndex = 3;
			// 
			// B_Folder
			// 
			this.B_Folder.ColorShade = null;
			this.B_Folder.Cursor = System.Windows.Forms.Cursors.Hand;
			dynamicIcon4.Name = "I_Folder";
			this.B_Folder.ImageName = dynamicIcon4;
			this.B_Folder.Location = new System.Drawing.Point(680, 3);
			this.B_Folder.Name = "B_Folder";
			this.B_Folder.Size = new System.Drawing.Size(100, 30);
			this.B_Folder.SpaceTriggersClick = true;
			this.B_Folder.TabIndex = 4;
			this.B_Folder.Click += new System.EventHandler(this.B_Folder_Click);
			// 
			// B_SteamPage
			// 
			this.B_SteamPage.ColorShade = null;
			this.B_SteamPage.Cursor = System.Windows.Forms.Cursors.Hand;
			dynamicIcon5.Name = "I_Steam";
			this.B_SteamPage.ImageName = dynamicIcon5;
			this.B_SteamPage.Location = new System.Drawing.Point(574, 3);
			this.B_SteamPage.Name = "B_SteamPage";
			this.B_SteamPage.Size = new System.Drawing.Size(100, 30);
			this.B_SteamPage.SpaceTriggersClick = true;
			this.B_SteamPage.TabIndex = 5;
			this.B_SteamPage.Click += new System.EventHandler(this.B_SteamPage_Click);
			// 
			// B_Redownload
			// 
			this.B_Redownload.ColorShade = null;
			this.B_Redownload.Cursor = System.Windows.Forms.Cursors.Hand;
			dynamicIcon6.Name = "I_ReDownload";
			this.B_Redownload.ImageName = dynamicIcon6;
			this.B_Redownload.Location = new System.Drawing.Point(468, 3);
			this.B_Redownload.Name = "B_Redownload";
			this.B_Redownload.Size = new System.Drawing.Size(100, 30);
			this.B_Redownload.SpaceTriggersClick = true;
			this.B_Redownload.TabIndex = 6;
			this.B_Redownload.Click += new System.EventHandler(this.B_Redownload_Click);
			// 
			// PC_PackagePage
			// 
			this.Controls.Add(this.P_Content);
			this.Controls.Add(this.TLP_Top);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(58)))), ((int)(((byte)(69)))));
			this.LabelBounds = new System.Drawing.Point(-2, 3);
			this.Name = "PC_PackagePage";
			this.Padding = new System.Windows.Forms.Padding(0, 30, 0, 0);
			this.Text = "Back";
			this.Controls.SetChildIndex(this.base_Text, 0);
			this.Controls.SetChildIndex(this.TLP_Top, 0);
			this.Controls.SetChildIndex(this.P_Content, 0);
			this.P_Content.ResumeLayout(false);
			this.TLP_Top.ResumeLayout(false);
			this.TLP_Top.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion
	private System.Windows.Forms.TableLayoutPanel TLP_Top;
	private PackageIcon PB_Icon;
	private System.Windows.Forms.Label L_Title;
	private System.Windows.Forms.Panel P_Content;
	private System.Windows.Forms.Panel P_Back;
	private PackageDescriptionControl P_Info;
	private SlickControls.SlickButton B_Redownload;
	private SlickControls.SlickButton B_Folder;
	private SlickControls.SlickButton B_SteamPage;
	private SlickControls.SlickTabControl slickTabControl1;
	private SlickControls.SlickTabControl.Tab T_Info;
	internal SlickControls.SlickTabControl.Tab T_CR;
	private SlickControls.SlickTabControl.Tab T_Profiles;
	private System.Windows.Forms.TableLayoutPanel TLP_Profiles;
}
