using LoadOrderToolTwo.UserInterface.Content;

namespace LoadOrderToolTwo.UserInterface.Panels;

partial class PC_CompatibilityManagement
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
			SlickControls.DynamicIcon dynamicIcon3 = new SlickControls.DynamicIcon();
			SlickControls.DynamicIcon dynamicIcon5 = new SlickControls.DynamicIcon();
			SlickControls.DynamicIcon dynamicIcon6 = new SlickControls.DynamicIcon();
			SlickControls.DynamicIcon dynamicIcon7 = new SlickControls.DynamicIcon();
			SlickControls.DynamicIcon dynamicIcon8 = new SlickControls.DynamicIcon();
			SlickControls.DynamicIcon dynamicIcon9 = new SlickControls.DynamicIcon();
			SlickControls.DynamicIcon dynamicIcon11 = new SlickControls.DynamicIcon();
			SlickControls.DynamicIcon dynamicIcon12 = new SlickControls.DynamicIcon();
			SlickControls.DynamicIcon dynamicIcon13 = new SlickControls.DynamicIcon();
			SlickControls.DynamicIcon dynamicIcon10 = new SlickControls.DynamicIcon();
			SlickControls.DynamicIcon dynamicIcon2 = new SlickControls.DynamicIcon();
			SlickControls.DynamicIcon dynamicIcon4 = new SlickControls.DynamicIcon();
			this.P_Content = new System.Windows.Forms.Panel();
			this.slickTabControl1 = new SlickControls.SlickTabControl();
			this.T_Info = new SlickControls.SlickTabControl.Tab();
			this.TLP_MainInfo = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.P_Links = new SlickControls.RoundedGroupFlowLayoutPanel();
			this.P_Tags = new SlickControls.RoundedGroupFlowLayoutPanel();
			this.CB_BlackListName = new SlickControls.SlickCheckbox();
			this.CB_BlackListId = new SlickControls.SlickCheckbox();
			this.T_Statuses = new SlickControls.SlickTabControl.Tab();
			this.FLP_Statuses = new System.Windows.Forms.FlowLayoutPanel();
			this.B_AddStatus = new SlickControls.SlickButton();
			this.T_Interactions = new SlickControls.SlickTabControl.Tab();
			this.FLP_Interactions = new System.Windows.Forms.FlowLayoutPanel();
			this.B_AddInteraction = new SlickControls.SlickButton();
			this.TLP_Top = new System.Windows.Forms.TableLayoutPanel();
			this.P_Main = new SlickControls.RoundedPanel();
			this.panel1 = new System.Windows.Forms.Panel();
			this.PB_LoadingPackage = new SlickControls.SlickPictureBox();
			this.TLP_Main = new System.Windows.Forms.TableLayoutPanel();
			this.B_Previous = new SlickControls.SlickButton();
			this.B_Skip = new SlickControls.SlickButton();
			this.B_Apply = new SlickControls.SlickButton();
			this.PB_Loading = new SlickControls.SlickPictureBox();
			this.TB_Note = new SlickControls.SlickTextBox();
			this.L_Page = new System.Windows.Forms.Label();
			this.B_ReuseData = new SlickControls.SlickButton();
			this.T_NewLink = new LoadOrderToolTwo.UserInterface.Content.TagControl();
			this.T_NewTag = new LoadOrderToolTwo.UserInterface.Content.TagControl();
			this.DD_Usage = new LoadOrderToolTwo.UserInterface.Dropdowns.PackageUsageDropDown();
			this.DD_Stability = new LoadOrderToolTwo.UserInterface.Dropdowns.PackageStabilityDropDown();
			this.PB_Icon = new LoadOrderToolTwo.UserInterface.Content.PackageIcon();
			this.P_Info = new LoadOrderToolTwo.UserInterface.Content.PackageDescriptionControl();
			this.P_Content.SuspendLayout();
			this.TLP_MainInfo.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.P_Links.SuspendLayout();
			this.P_Tags.SuspendLayout();
			this.FLP_Statuses.SuspendLayout();
			this.FLP_Interactions.SuspendLayout();
			this.TLP_Top.SuspendLayout();
			this.P_Main.SuspendLayout();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PB_LoadingPackage)).BeginInit();
			this.TLP_Main.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PB_Loading)).BeginInit();
			this.SuspendLayout();
			// 
			// base_Text
			// 
			this.base_Text.Text = "Back";
			// 
			// P_Content
			// 
			this.P_Content.Controls.Add(this.slickTabControl1);
			this.P_Content.Dock = System.Windows.Forms.DockStyle.Fill;
			this.P_Content.Location = new System.Drawing.Point(0, 100);
			this.P_Content.Name = "P_Content";
			this.P_Content.Size = new System.Drawing.Size(1055, 528);
			this.P_Content.TabIndex = 13;
			// 
			// slickTabControl1
			// 
			this.slickTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.slickTabControl1.Location = new System.Drawing.Point(0, 0);
			this.slickTabControl1.Margin = new System.Windows.Forms.Padding(0);
			this.slickTabControl1.Name = "slickTabControl1";
			this.slickTabControl1.Size = new System.Drawing.Size(1055, 528);
			this.slickTabControl1.TabIndex = 0;
			this.slickTabControl1.Tabs = new SlickControls.SlickTabControl.Tab[] {
        this.T_Info,
        this.T_Statuses,
        this.T_Interactions};
			// 
			// T_Info
			// 
			this.T_Info.Cursor = System.Windows.Forms.Cursors.Hand;
			this.T_Info.Dock = System.Windows.Forms.DockStyle.Left;
			this.T_Info.FillTab = true;
			this.T_Info.Font = new System.Drawing.Font("Nirmala UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dynamicIcon1.Name = "I_Content";
			this.T_Info.IconName = dynamicIcon1;
			this.T_Info.LinkedControl = this.TLP_MainInfo;
			this.T_Info.Location = new System.Drawing.Point(0, 5);
			this.T_Info.Name = "T_Info";
			this.T_Info.Selected = true;
			this.T_Info.Size = new System.Drawing.Size(351, 25);
			this.T_Info.TabIndex = 0;
			this.T_Info.TabStop = false;
			this.T_Info.Text = "Info";
			// 
			// TLP_MainInfo
			// 
			this.TLP_MainInfo.AutoSize = true;
			this.TLP_MainInfo.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.TLP_MainInfo.ColumnCount = 4;
			this.TLP_MainInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_MainInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TLP_MainInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_MainInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_MainInfo.Controls.Add(this.TB_Note, 0, 2);
			this.TLP_MainInfo.Controls.Add(this.tableLayoutPanel1, 2, 2);
			this.TLP_MainInfo.Controls.Add(this.DD_Usage, 0, 1);
			this.TLP_MainInfo.Controls.Add(this.DD_Stability, 0, 0);
			this.TLP_MainInfo.Controls.Add(this.CB_BlackListName, 2, 0);
			this.TLP_MainInfo.Controls.Add(this.CB_BlackListId, 3, 0);
			this.TLP_MainInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TLP_MainInfo.Location = new System.Drawing.Point(0, 0);
			this.TLP_MainInfo.Name = "TLP_MainInfo";
			this.TLP_MainInfo.RowCount = 4;
			this.TLP_MainInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_MainInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_MainInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_MainInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_MainInfo.Size = new System.Drawing.Size(1055, 498);
			this.TLP_MainInfo.TabIndex = 17;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.TLP_MainInfo.SetColumnSpan(this.tableLayoutPanel1, 2);
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.P_Links, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.P_Tags, 0, 0);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(916, 226);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.TLP_MainInfo.SetRowSpan(this.tableLayoutPanel1, 2);
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(136, 269);
			this.tableLayoutPanel1.TabIndex = 17;
			// 
			// P_Links
			// 
			this.P_Links.AddOutline = true;
			this.P_Links.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.P_Links.AutoSize = true;
			this.P_Links.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.P_Links.Controls.Add(this.T_NewLink);
			dynamicIcon3.Name = "I_Link";
			this.P_Links.ImageName = dynamicIcon3;
			this.P_Links.Location = new System.Drawing.Point(3, 154);
			this.P_Links.Name = "P_Links";
			this.P_Links.Padding = new System.Windows.Forms.Padding(9, 54, 9, 9);
			this.P_Links.Size = new System.Drawing.Size(130, 112);
			this.P_Links.TabIndex = 20;
			this.P_Links.Text = "Links";
			// 
			// P_Tags
			// 
			this.P_Tags.AddOutline = true;
			this.P_Tags.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.P_Tags.AutoSize = true;
			this.P_Tags.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.P_Tags.Controls.Add(this.T_NewTag);
			dynamicIcon5.Name = "I_Tag";
			this.P_Tags.ImageName = dynamicIcon5;
			this.P_Tags.Info = "";
			this.P_Tags.Location = new System.Drawing.Point(3, 3);
			this.P_Tags.Name = "P_Tags";
			this.P_Tags.Padding = new System.Windows.Forms.Padding(9, 87, 9, 9);
			this.P_Tags.Size = new System.Drawing.Size(130, 145);
			this.P_Tags.TabIndex = 19;
			this.P_Tags.Text = "GlobalTags";
			// 
			// CB_BlackListName
			// 
			this.CB_BlackListName.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.CB_BlackListName.AutoSize = false;
			this.CB_BlackListName.Checked = false;
			this.CB_BlackListName.CheckedText = null;
			this.CB_BlackListName.Cursor = System.Windows.Forms.Cursors.Hand;
			this.CB_BlackListName.DefaultValue = false;
			this.CB_BlackListName.EnterTriggersClick = false;
			this.CB_BlackListName.Location = new System.Drawing.Point(702, 14);
			this.CB_BlackListName.Name = "CB_BlackListName";
			this.CB_BlackListName.Size = new System.Drawing.Size(209, 33);
			this.CB_BlackListName.SpaceTriggersClick = true;
			this.CB_BlackListName.TabIndex = 17;
			this.CB_BlackListName.Text = "BlackListName";
			this.CB_BlackListName.UncheckedText = null;
			// 
			// CB_BlackListId
			// 
			this.CB_BlackListId.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.CB_BlackListId.AutoSize = false;
			this.CB_BlackListId.Checked = false;
			this.CB_BlackListId.CheckedText = null;
			this.CB_BlackListId.Cursor = System.Windows.Forms.Cursors.Hand;
			this.CB_BlackListId.DefaultValue = false;
			this.CB_BlackListId.EnterTriggersClick = false;
			this.CB_BlackListId.Location = new System.Drawing.Point(917, 14);
			this.CB_BlackListId.Name = "CB_BlackListId";
			this.CB_BlackListId.Size = new System.Drawing.Size(135, 33);
			this.CB_BlackListId.SpaceTriggersClick = true;
			this.CB_BlackListId.TabIndex = 17;
			this.CB_BlackListId.Text = "BlackListId";
			this.CB_BlackListId.UncheckedText = null;
			// 
			// T_Statuses
			// 
			this.T_Statuses.Cursor = System.Windows.Forms.Cursors.Hand;
			this.T_Statuses.Dock = System.Windows.Forms.DockStyle.Left;
			this.T_Statuses.Font = new System.Drawing.Font("Nirmala UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dynamicIcon6.Name = "I_Statuses";
			this.T_Statuses.IconName = dynamicIcon6;
			this.T_Statuses.LinkedControl = this.FLP_Statuses;
			this.T_Statuses.Location = new System.Drawing.Point(351, 5);
			this.T_Statuses.Name = "T_Statuses";
			this.T_Statuses.Selected = false;
			this.T_Statuses.Size = new System.Drawing.Size(351, 25);
			this.T_Statuses.TabIndex = 0;
			this.T_Statuses.TabStop = false;
			this.T_Statuses.Text = "Statuses";
			// 
			// FLP_Statuses
			// 
			this.FLP_Statuses.AutoSize = true;
			this.FLP_Statuses.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.FLP_Statuses.Controls.Add(this.B_AddStatus);
			this.FLP_Statuses.Location = new System.Drawing.Point(0, 0);
			this.FLP_Statuses.Name = "FLP_Statuses";
			this.FLP_Statuses.Size = new System.Drawing.Size(106, 36);
			this.FLP_Statuses.TabIndex = 17;
			this.FLP_Statuses.ControlAdded += new System.Windows.Forms.ControlEventHandler(this.FLP_Statuses_ControlAdded);
			this.FLP_Statuses.ControlRemoved += new System.Windows.Forms.ControlEventHandler(this.FLP_Statuses_ControlAdded);
			// 
			// B_AddStatus
			// 
			this.B_AddStatus.ColorShade = null;
			this.B_AddStatus.Cursor = System.Windows.Forms.Cursors.Hand;
			dynamicIcon7.Name = "I_Add";
			this.B_AddStatus.ImageName = dynamicIcon7;
			this.B_AddStatus.LargeImage = true;
			this.B_AddStatus.Location = new System.Drawing.Point(3, 3);
			this.B_AddStatus.Name = "B_AddStatus";
			this.B_AddStatus.Size = new System.Drawing.Size(100, 30);
			this.B_AddStatus.SpaceTriggersClick = true;
			this.B_AddStatus.TabIndex = 0;
			this.B_AddStatus.Text = "AddStatus";
			this.B_AddStatus.Click += new System.EventHandler(this.B_AddStatus_Click);
			// 
			// T_Interactions
			// 
			this.T_Interactions.Cursor = System.Windows.Forms.Cursors.Hand;
			this.T_Interactions.Dock = System.Windows.Forms.DockStyle.Left;
			this.T_Interactions.Font = new System.Drawing.Font("Nirmala UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dynamicIcon8.Name = "I_Switch";
			this.T_Interactions.IconName = dynamicIcon8;
			this.T_Interactions.LinkedControl = this.FLP_Interactions;
			this.T_Interactions.Location = new System.Drawing.Point(702, 5);
			this.T_Interactions.Name = "T_Interactions";
			this.T_Interactions.Selected = false;
			this.T_Interactions.Size = new System.Drawing.Size(351, 25);
			this.T_Interactions.TabIndex = 0;
			this.T_Interactions.TabStop = false;
			this.T_Interactions.Text = "Interactions";
			// 
			// FLP_Interactions
			// 
			this.FLP_Interactions.AutoSize = true;
			this.FLP_Interactions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.FLP_Interactions.Controls.Add(this.B_AddInteraction);
			this.FLP_Interactions.Location = new System.Drawing.Point(0, 0);
			this.FLP_Interactions.Name = "FLP_Interactions";
			this.FLP_Interactions.Size = new System.Drawing.Size(106, 36);
			this.FLP_Interactions.TabIndex = 18;
			this.FLP_Interactions.ControlAdded += new System.Windows.Forms.ControlEventHandler(this.FLP_Statuses_ControlAdded);
			this.FLP_Interactions.ControlRemoved += new System.Windows.Forms.ControlEventHandler(this.FLP_Statuses_ControlAdded);
			// 
			// B_AddInteraction
			// 
			this.B_AddInteraction.ColorShade = null;
			this.B_AddInteraction.Cursor = System.Windows.Forms.Cursors.Hand;
			dynamicIcon9.Name = "I_Add";
			this.B_AddInteraction.ImageName = dynamicIcon9;
			this.B_AddInteraction.LargeImage = true;
			this.B_AddInteraction.Location = new System.Drawing.Point(3, 3);
			this.B_AddInteraction.Name = "B_AddInteraction";
			this.B_AddInteraction.Size = new System.Drawing.Size(100, 30);
			this.B_AddInteraction.SpaceTriggersClick = true;
			this.B_AddInteraction.TabIndex = 1;
			this.B_AddInteraction.Text = "AddInteraction";
			this.B_AddInteraction.Click += new System.EventHandler(this.B_AddInteraction_Click);
			// 
			// TLP_Top
			// 
			this.TLP_Top.ColumnCount = 2;
			this.TLP_Top.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Top.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TLP_Top.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.TLP_Top.Controls.Add(this.PB_Icon, 0, 0);
			this.TLP_Top.Controls.Add(this.P_Info, 1, 0);
			this.TLP_Top.Dock = System.Windows.Forms.DockStyle.Top;
			this.TLP_Top.Location = new System.Drawing.Point(0, 0);
			this.TLP_Top.Margin = new System.Windows.Forms.Padding(0);
			this.TLP_Top.Name = "TLP_Top";
			this.TLP_Top.RowCount = 2;
			this.TLP_Top.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TLP_Top.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TLP_Top.Size = new System.Drawing.Size(1055, 100);
			this.TLP_Top.TabIndex = 0;
			// 
			// P_Main
			// 
			this.P_Main.AddOutline = true;
			this.TLP_Main.SetColumnSpan(this.P_Main, 5);
			this.P_Main.Controls.Add(this.panel1);
			this.P_Main.Controls.Add(this.PB_LoadingPackage);
			this.P_Main.Dock = System.Windows.Forms.DockStyle.Fill;
			this.P_Main.Location = new System.Drawing.Point(3, 3);
			this.P_Main.Name = "P_Main";
			this.P_Main.Size = new System.Drawing.Size(1055, 628);
			this.P_Main.TabIndex = 14;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.P_Content);
			this.panel1.Controls.Add(this.TLP_Top);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(1055, 628);
			this.panel1.TabIndex = 17;
			// 
			// PB_LoadingPackage
			// 
			this.PB_LoadingPackage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PB_LoadingPackage.Location = new System.Drawing.Point(0, 0);
			this.PB_LoadingPackage.Name = "PB_LoadingPackage";
			this.PB_LoadingPackage.Size = new System.Drawing.Size(1055, 628);
			this.PB_LoadingPackage.TabIndex = 18;
			this.PB_LoadingPackage.TabStop = false;
			// 
			// TLP_Main
			// 
			this.TLP_Main.ColumnCount = 5;
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Main.Controls.Add(this.B_ReuseData, 3, 1);
			this.TLP_Main.Controls.Add(this.P_Main, 0, 0);
			this.TLP_Main.Controls.Add(this.B_Previous, 0, 1);
			this.TLP_Main.Controls.Add(this.B_Skip, 2, 1);
			this.TLP_Main.Controls.Add(this.B_Apply, 4, 1);
			this.TLP_Main.Controls.Add(this.L_Page, 1, 1);
			this.TLP_Main.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TLP_Main.Location = new System.Drawing.Point(0, 30);
			this.TLP_Main.Name = "TLP_Main";
			this.TLP_Main.RowCount = 2;
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Main.Size = new System.Drawing.Size(1061, 670);
			this.TLP_Main.TabIndex = 15;
			// 
			// B_Previous
			// 
			this.B_Previous.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.B_Previous.ColorShade = null;
			this.B_Previous.Cursor = System.Windows.Forms.Cursors.Hand;
			dynamicIcon11.Name = "I_ArrowLeft";
			this.B_Previous.ImageName = dynamicIcon11;
			this.B_Previous.Location = new System.Drawing.Point(3, 637);
			this.B_Previous.Name = "B_Previous";
			this.B_Previous.Size = new System.Drawing.Size(100, 30);
			this.B_Previous.SpaceTriggersClick = true;
			this.B_Previous.TabIndex = 15;
			this.B_Previous.Text = "Previous";
			this.B_Previous.Click += new System.EventHandler(this.B_Previous_Click);
			// 
			// B_Skip
			// 
			this.B_Skip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.B_Skip.ColorShade = null;
			this.B_Skip.Cursor = System.Windows.Forms.Cursors.Hand;
			dynamicIcon12.Name = "I_ArrowRight";
			this.B_Skip.ImageName = dynamicIcon12;
			this.B_Skip.Location = new System.Drawing.Point(115, 637);
			this.B_Skip.Name = "B_Skip";
			this.B_Skip.Size = new System.Drawing.Size(100, 30);
			this.B_Skip.SpaceTriggersClick = true;
			this.B_Skip.TabIndex = 15;
			this.B_Skip.Text = "Skip";
			this.B_Skip.Click += new System.EventHandler(this.B_Skip_Click);
			// 
			// B_Apply
			// 
			this.B_Apply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.B_Apply.ColorShade = null;
			this.B_Apply.Cursor = System.Windows.Forms.Cursors.Hand;
			dynamicIcon13.Name = "I_Ok";
			this.B_Apply.ImageName = dynamicIcon13;
			this.B_Apply.Location = new System.Drawing.Point(842, 637);
			this.B_Apply.Name = "B_Apply";
			this.B_Apply.Size = new System.Drawing.Size(216, 30);
			this.B_Apply.SpaceTriggersClick = true;
			this.B_Apply.TabIndex = 15;
			this.B_Apply.Text = "ApplyContinue";
			this.B_Apply.Click += new System.EventHandler(this.B_Apply_Click);
			// 
			// PB_Loading
			// 
			this.PB_Loading.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PB_Loading.Location = new System.Drawing.Point(0, 30);
			this.PB_Loading.Name = "PB_Loading";
			this.PB_Loading.Size = new System.Drawing.Size(1061, 670);
			this.PB_Loading.TabIndex = 16;
			this.PB_Loading.TabStop = false;
			// 
			// TB_Note
			// 
			this.TLP_MainInfo.SetColumnSpan(this.TB_Note, 2);
			this.TB_Note.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.TB_Note.LabelText = "Note";
			this.TB_Note.Location = new System.Drawing.Point(3, 344);
			this.TB_Note.MultiLine = true;
			this.TB_Note.Name = "TB_Note";
			this.TB_Note.Placeholder = "NoteInfo";
			this.TLP_MainInfo.SetRowSpan(this.TB_Note, 2);
			this.TB_Note.SelectedText = "";
			this.TB_Note.SelectionLength = 0;
			this.TB_Note.SelectionStart = 0;
			this.TB_Note.Size = new System.Drawing.Size(693, 151);
			this.TB_Note.TabIndex = 19;
			// 
			// L_Page
			// 
			this.L_Page.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.L_Page.AutoSize = true;
			this.L_Page.Location = new System.Drawing.Point(109, 637);
			this.L_Page.Name = "L_Page";
			this.L_Page.Size = new System.Drawing.Size(0, 30);
			this.L_Page.TabIndex = 16;
			// 
			// B_ReuseData
			// 
			this.B_ReuseData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.B_ReuseData.ColorShade = null;
			this.B_ReuseData.Cursor = System.Windows.Forms.Cursors.Hand;
			dynamicIcon10.Name = "I_Refresh";
			this.B_ReuseData.ImageName = dynamicIcon10;
			this.B_ReuseData.Location = new System.Drawing.Point(620, 637);
			this.B_ReuseData.Name = "B_ReuseData";
			this.B_ReuseData.Size = new System.Drawing.Size(216, 30);
			this.B_ReuseData.SpaceTriggersClick = true;
			this.B_ReuseData.TabIndex = 17;
			this.B_ReuseData.Text = "ReuseData";
			this.B_ReuseData.Click += new System.EventHandler(this.B_ReuseData_Click);
			// 
			// T_NewLink
			// 
			dynamicIcon2.Name = "I_Add";
			this.T_NewLink.ImageName = dynamicIcon2;
			this.T_NewLink.Location = new System.Drawing.Point(12, 57);
			this.T_NewLink.Name = "T_NewLink";
			this.T_NewLink.Size = new System.Drawing.Size(106, 43);
			this.T_NewLink.TabIndex = 0;
			this.T_NewLink.Click += new System.EventHandler(this.T_NewLink_Click);
			// 
			// T_NewTag
			// 
			dynamicIcon4.Name = "I_Add";
			this.T_NewTag.ImageName = dynamicIcon4;
			this.T_NewTag.Location = new System.Drawing.Point(12, 90);
			this.T_NewTag.Name = "T_NewTag";
			this.T_NewTag.Size = new System.Drawing.Size(106, 43);
			this.T_NewTag.TabIndex = 0;
			this.T_NewTag.Click += new System.EventHandler(this.T_NewTag_Click);
			// 
			// DD_Usage
			// 
			this.DD_Usage.Cursor = System.Windows.Forms.Cursors.Hand;
			this.DD_Usage.Location = new System.Drawing.Point(3, 65);
			this.DD_Usage.Name = "DD_Usage";
			this.DD_Usage.Size = new System.Drawing.Size(375, 54);
			this.DD_Usage.TabIndex = 17;
			this.DD_Usage.Text = "Usage";
			// 
			// DD_Stability
			// 
			this.DD_Stability.Cursor = System.Windows.Forms.Cursors.Hand;
			this.DD_Stability.Location = new System.Drawing.Point(3, 3);
			this.DD_Stability.Name = "DD_Stability";
			this.DD_Stability.Size = new System.Drawing.Size(375, 56);
			this.DD_Stability.TabIndex = 0;
			this.DD_Stability.Text = "Stability";
			// 
			// PB_Icon
			// 
			this.PB_Icon.Dock = System.Windows.Forms.DockStyle.Left;
			this.PB_Icon.HalfColor = false;
			this.PB_Icon.Location = new System.Drawing.Point(0, 0);
			this.PB_Icon.Margin = new System.Windows.Forms.Padding(0);
			this.PB_Icon.Name = "PB_Icon";
			this.TLP_Top.SetRowSpan(this.PB_Icon, 2);
			this.PB_Icon.Size = new System.Drawing.Size(100, 100);
			this.PB_Icon.TabIndex = 0;
			this.PB_Icon.TabStop = false;
			// 
			// P_Info
			// 
			this.P_Info.Dock = System.Windows.Forms.DockStyle.Fill;
			this.P_Info.Location = new System.Drawing.Point(105, 0);
			this.P_Info.Margin = new System.Windows.Forms.Padding(5, 0, 0, 0);
			this.P_Info.Name = "P_Info";
			this.TLP_Top.SetRowSpan(this.P_Info, 2);
			this.P_Info.Size = new System.Drawing.Size(950, 100);
			this.P_Info.TabIndex = 3;
			// 
			// PC_CompatibilityManagement
			// 
			this.Controls.Add(this.TLP_Main);
			this.Controls.Add(this.PB_Loading);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(58)))), ((int)(((byte)(69)))));
			this.Name = "PC_CompatibilityManagement";
			this.Padding = new System.Windows.Forms.Padding(0, 30, 0, 0);
			this.Size = new System.Drawing.Size(1061, 700);
			this.Text = "Back";
			this.Controls.SetChildIndex(this.PB_Loading, 0);
			this.Controls.SetChildIndex(this.base_Text, 0);
			this.Controls.SetChildIndex(this.TLP_Main, 0);
			this.P_Content.ResumeLayout(false);
			this.TLP_MainInfo.ResumeLayout(false);
			this.TLP_MainInfo.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.P_Links.ResumeLayout(false);
			this.P_Tags.ResumeLayout(false);
			this.FLP_Statuses.ResumeLayout(false);
			this.FLP_Interactions.ResumeLayout(false);
			this.TLP_Top.ResumeLayout(false);
			this.P_Main.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PB_LoadingPackage)).EndInit();
			this.TLP_Main.ResumeLayout(false);
			this.TLP_Main.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PB_Loading)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion
	private System.Windows.Forms.TableLayoutPanel TLP_Top;
	private PackageIcon PB_Icon;
	private System.Windows.Forms.Panel P_Content;
	private PackageDescriptionControl P_Info;
	private SlickControls.SlickTabControl slickTabControl1;
	private SlickControls.SlickTabControl.Tab T_Info;
	internal SlickControls.SlickTabControl.Tab T_Statuses;
	private SlickControls.SlickTabControl.Tab T_Interactions;
	private SlickControls.RoundedPanel P_Main;
	private System.Windows.Forms.TableLayoutPanel TLP_Main;
	private SlickControls.SlickButton B_Previous;
	private SlickControls.SlickButton B_Skip;
	private SlickControls.SlickButton B_Apply;
	private SlickControls.SlickPictureBox PB_Loading;
	private System.Windows.Forms.TableLayoutPanel TLP_MainInfo;
	private Dropdowns.PackageStabilityDropDown DD_Stability;
	private Dropdowns.PackageUsageDropDown DD_Usage;
	private SlickControls.RoundedGroupFlowLayoutPanel P_Tags;
	private TagControl T_NewTag;
	private SlickControls.RoundedGroupFlowLayoutPanel P_Links;
	private TagControl T_NewLink;
	private System.Windows.Forms.FlowLayoutPanel FLP_Statuses;
	private System.Windows.Forms.FlowLayoutPanel FLP_Interactions;
	private SlickControls.SlickButton B_AddStatus;
	private SlickControls.SlickButton B_AddInteraction;
	private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	private SlickControls.SlickCheckbox CB_BlackListName;
	private SlickControls.SlickCheckbox CB_BlackListId;
	private System.Windows.Forms.Panel panel1;
	private SlickControls.SlickPictureBox PB_LoadingPackage;
	private SlickControls.SlickTextBox TB_Note;
	private System.Windows.Forms.Label L_Page;
	private SlickControls.SlickButton B_ReuseData;
}
