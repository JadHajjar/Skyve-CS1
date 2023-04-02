using LoadOrderToolTwo.UserInterface.Generic;
using LoadOrderToolTwo.UserInterface.Profiles;
using LoadOrderToolTwo.Utilities.Managers;

namespace LoadOrderToolTwo.UserInterface.Panels;

partial class PC_Profiles
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
			ProfileManager.ProfileChanged -= ProfileManager_ProfileChanged;
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PC_Profiles));
			this.TLP_ProfileName = new SlickControls.RoundedTableLayoutPanel();
			this.I_ProfileIcon = new SlickControls.SlickIcon();
			this.L_CurrentProfile = new System.Windows.Forms.Label();
			this.B_EditName = new SlickControls.SlickIcon();
			this.TB_Name = new SlickControls.SlickTextBox();
			this.B_Save = new SlickControls.SlickIcon();
			this.TLP_Main = new System.Windows.Forms.TableLayoutPanel();
			this.I_TempProfile = new SlickControls.SlickIcon();
			this.L_TempProfile = new System.Windows.Forms.Label();
			this.P_Options = new SlickControls.RoundedPanel();
			this.P_ScrollPanel = new System.Windows.Forms.Panel();
			this.slickScroll = new SlickControls.SlickScroll();
			this.FLP_Options = new System.Windows.Forms.FlowLayoutPanel();
			this.TLP_GeneralSettings = new SlickControls.RoundedGroupTableLayoutPanel();
			this.L_ProfileUsage = new System.Windows.Forms.Label();
			this.T_ProfileUsage = new LoadOrderToolTwo.UserInterface.Generic.ThreeOptionToggle();
			this.CB_AutoSave = new SlickControls.SlickCheckbox();
			this.TLP_LaunchSettings = new SlickControls.RoundedGroupTableLayoutPanel();
			this.CB_LoadSave = new SlickControls.SlickCheckbox();
			this.CB_LHT = new SlickControls.SlickCheckbox();
			this.CB_NoWorkshop = new SlickControls.SlickCheckbox();
			this.CB_NoAssets = new SlickControls.SlickCheckbox();
			this.CB_NoMods = new SlickControls.SlickCheckbox();
			this.CB_DebugMono = new SlickControls.SlickCheckbox();
			this.CB_UseCitiesExe = new SlickControls.SlickCheckbox();
			this.CB_UnityProfiler = new SlickControls.SlickCheckbox();
			this.DD_SaveFile = new LoadOrderToolTwo.UserInterface.Generic.DragAndDropControl();
			this.TLP_LSM = new SlickControls.RoundedGroupTableLayoutPanel();
			this.CB_SkipFile = new SlickControls.SlickCheckbox();
			this.CB_LoadEnabled = new SlickControls.SlickCheckbox();
			this.CB_LoadUsed = new SlickControls.SlickCheckbox();
			this.DD_SkipFile = new LoadOrderToolTwo.UserInterface.Generic.DragAndDropControl();
			this.B_ViewProfiles = new SlickControls.SlickButton();
			this.I_Info = new SlickControls.SlickIcon();
			this.L_Info = new System.Windows.Forms.Label();
			this.B_TempProfile = new SlickControls.SlickButton();
			this.B_NewProfile = new SlickControls.SlickButton();
			this.TLP_New = new System.Windows.Forms.TableLayoutPanel();
			this.newProfileOptionControl1 = new LoadOrderToolTwo.UserInterface.Profiles.NewProfileOptionControl();
			this.newProfileOptionControl2 = new LoadOrderToolTwo.UserInterface.Profiles.NewProfileOptionControl();
			this.B_Cancel = new SlickControls.SlickButton();
			this.DAD_NewProfile = new LoadOrderToolTwo.UserInterface.Generic.DragAndDropControl();
			this.TLP_ProfileName.SuspendLayout();
			this.TLP_Main.SuspendLayout();
			this.P_Options.SuspendLayout();
			this.P_ScrollPanel.SuspendLayout();
			this.FLP_Options.SuspendLayout();
			this.TLP_GeneralSettings.SuspendLayout();
			this.TLP_LaunchSettings.SuspendLayout();
			this.TLP_LSM.SuspendLayout();
			this.TLP_New.SuspendLayout();
			this.SuspendLayout();
			// 
			// TLP_ProfileName
			// 
			this.TLP_ProfileName.AutoSize = true;
			this.TLP_ProfileName.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.TLP_ProfileName.ColumnCount = 5;
			this.TLP_Main.SetColumnSpan(this.TLP_ProfileName, 2);
			this.TLP_ProfileName.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_ProfileName.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_ProfileName.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_ProfileName.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_ProfileName.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_ProfileName.Controls.Add(this.I_ProfileIcon, 0, 0);
			this.TLP_ProfileName.Controls.Add(this.L_CurrentProfile, 1, 0);
			this.TLP_ProfileName.Controls.Add(this.B_EditName, 3, 0);
			this.TLP_ProfileName.Controls.Add(this.TB_Name, 2, 0);
			this.TLP_ProfileName.Controls.Add(this.B_Save, 4, 0);
			this.TLP_ProfileName.Location = new System.Drawing.Point(10, 10);
			this.TLP_ProfileName.Margin = new System.Windows.Forms.Padding(10);
			this.TLP_ProfileName.Name = "TLP_ProfileName";
			this.TLP_ProfileName.Padding = new System.Windows.Forms.Padding(5);
			this.TLP_ProfileName.RowCount = 1;
			this.TLP_ProfileName.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_ProfileName.Size = new System.Drawing.Size(441, 48);
			this.TLP_ProfileName.TabIndex = 13;
			// 
			// I_ProfileIcon
			// 
			this.I_ProfileIcon.ActiveColor = null;
			this.I_ProfileIcon.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.I_ProfileIcon.Cursor = System.Windows.Forms.Cursors.Hand;
			this.I_ProfileIcon.Enabled = false;
			this.I_ProfileIcon.Image = global::LoadOrderToolTwo.Properties.Resources.I_ProfileSettings;
			this.I_ProfileIcon.Location = new System.Drawing.Point(8, 8);
			this.I_ProfileIcon.Name = "I_ProfileIcon";
			this.I_ProfileIcon.Size = new System.Drawing.Size(32, 32);
			this.I_ProfileIcon.TabIndex = 2;
			this.I_ProfileIcon.TabStop = false;
			// 
			// L_CurrentProfile
			// 
			this.L_CurrentProfile.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.L_CurrentProfile.AutoSize = true;
			this.L_CurrentProfile.Location = new System.Drawing.Point(46, 12);
			this.L_CurrentProfile.Name = "L_CurrentProfile";
			this.L_CurrentProfile.Size = new System.Drawing.Size(55, 23);
			this.L_CurrentProfile.TabIndex = 0;
			this.L_CurrentProfile.Text = "label1";
			// 
			// B_EditName
			// 
			this.B_EditName.ActiveColor = null;
			this.B_EditName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.B_EditName.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_EditName.Image = ((System.Drawing.Image)(resources.GetObject("B_EditName.Image")));
			this.B_EditName.Location = new System.Drawing.Point(363, 8);
			this.B_EditName.Name = "B_EditName";
			this.B_EditName.Size = new System.Drawing.Size(32, 32);
			this.B_EditName.TabIndex = 1;
			this.B_EditName.TabStop = false;
			this.B_EditName.Click += new System.EventHandler(this.B_EditName_Click);
			// 
			// TB_Name
			// 
			this.TB_Name.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.TB_Name.Image = global::LoadOrderToolTwo.Properties.Resources.I_Ok;
			this.TB_Name.LabelText = "ProfileName";
			this.TB_Name.Location = new System.Drawing.Point(107, 12);
			this.TB_Name.Name = "TB_Name";
			this.TB_Name.Placeholder = "RenameProfile";
			this.TB_Name.SelectedText = "";
			this.TB_Name.SelectionLength = 0;
			this.TB_Name.SelectionStart = 0;
			this.TB_Name.Size = new System.Drawing.Size(250, 23);
			this.TB_Name.TabIndex = 3;
			this.TB_Name.Visible = false;
			this.TB_Name.IconClicked += new System.EventHandler(this.TB_Name_IconClicked);
			this.TB_Name.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TB_Name_KeyDown);
			this.TB_Name.Leave += new System.EventHandler(this.TB_Name_Leave);
			this.TB_Name.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.TB_Name_PreviewKeyDown);
			// 
			// B_Save
			// 
			this.B_Save.ActiveColor = null;
			this.B_Save.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.B_Save.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_Save.Image = global::LoadOrderToolTwo.Properties.Resources.I_Save;
			this.B_Save.Location = new System.Drawing.Point(401, 8);
			this.B_Save.Name = "B_Save";
			this.B_Save.Size = new System.Drawing.Size(32, 32);
			this.B_Save.TabIndex = 1;
			this.B_Save.TabStop = false;
			this.B_Save.Click += new System.EventHandler(this.B_Save_Click);
			// 
			// TLP_Main
			// 
			this.TLP_Main.ColumnCount = 5;
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.TLP_Main.Controls.Add(this.I_TempProfile, 0, 1);
			this.TLP_Main.Controls.Add(this.L_TempProfile, 1, 1);
			this.TLP_Main.Controls.Add(this.TLP_ProfileName, 0, 0);
			this.TLP_Main.Controls.Add(this.P_Options, 0, 3);
			this.TLP_Main.Controls.Add(this.B_ViewProfiles, 4, 0);
			this.TLP_Main.Controls.Add(this.I_Info, 0, 2);
			this.TLP_Main.Controls.Add(this.L_Info, 1, 2);
			this.TLP_Main.Controls.Add(this.B_TempProfile, 3, 0);
			this.TLP_Main.Controls.Add(this.B_NewProfile, 2, 0);
			this.TLP_Main.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TLP_Main.Location = new System.Drawing.Point(0, 30);
			this.TLP_Main.Name = "TLP_Main";
			this.TLP_Main.RowCount = 4;
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TLP_Main.Size = new System.Drawing.Size(1410, 547);
			this.TLP_Main.TabIndex = 14;
			// 
			// I_TempProfile
			// 
			this.I_TempProfile.ActiveColor = null;
			this.I_TempProfile.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.I_TempProfile.ColorStyle = Extensions.ColorStyle.Yellow;
			this.I_TempProfile.Cursor = System.Windows.Forms.Cursors.Hand;
			this.I_TempProfile.Enabled = false;
			this.I_TempProfile.Image = global::LoadOrderToolTwo.Properties.Resources.I_Warning;
			this.I_TempProfile.Location = new System.Drawing.Point(18, 73);
			this.I_TempProfile.Margin = new System.Windows.Forms.Padding(18, 3, 3, 3);
			this.I_TempProfile.Name = "I_TempProfile";
			this.I_TempProfile.Selected = true;
			this.I_TempProfile.Size = new System.Drawing.Size(32, 32);
			this.I_TempProfile.TabIndex = 2;
			this.I_TempProfile.TabStop = false;
			// 
			// L_TempProfile
			// 
			this.L_TempProfile.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.L_TempProfile.AutoSize = true;
			this.TLP_Main.SetColumnSpan(this.L_TempProfile, 4);
			this.L_TempProfile.Location = new System.Drawing.Point(56, 78);
			this.L_TempProfile.Margin = new System.Windows.Forms.Padding(3, 10, 10, 10);
			this.L_TempProfile.Name = "L_TempProfile";
			this.L_TempProfile.Size = new System.Drawing.Size(55, 23);
			this.L_TempProfile.TabIndex = 15;
			this.L_TempProfile.Text = "label1";
			this.L_TempProfile.UseMnemonic = false;
			// 
			// P_Options
			// 
			this.TLP_Main.SetColumnSpan(this.P_Options, 5);
			this.P_Options.Controls.Add(this.P_ScrollPanel);
			this.P_Options.Dock = System.Windows.Forms.DockStyle.Fill;
			this.P_Options.Location = new System.Drawing.Point(3, 157);
			this.P_Options.Name = "P_Options";
			this.P_Options.Size = new System.Drawing.Size(1404, 387);
			this.P_Options.TabIndex = 16;
			// 
			// P_ScrollPanel
			// 
			this.P_ScrollPanel.Controls.Add(this.slickScroll);
			this.P_ScrollPanel.Controls.Add(this.FLP_Options);
			this.P_ScrollPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.P_ScrollPanel.Location = new System.Drawing.Point(0, 0);
			this.P_ScrollPanel.Margin = new System.Windows.Forms.Padding(5);
			this.P_ScrollPanel.Name = "P_ScrollPanel";
			this.P_ScrollPanel.Size = new System.Drawing.Size(1404, 387);
			this.P_ScrollPanel.TabIndex = 16;
			// 
			// slickScroll
			// 
			this.slickScroll.Dock = System.Windows.Forms.DockStyle.Right;
			this.slickScroll.LinkedControl = this.FLP_Options;
			this.slickScroll.Location = new System.Drawing.Point(1396, 0);
			this.slickScroll.Name = "slickScroll";
			this.slickScroll.Size = new System.Drawing.Size(8, 387);
			this.slickScroll.Style = SlickControls.StyleType.Vertical;
			this.slickScroll.TabIndex = 16;
			this.slickScroll.TabStop = false;
			this.slickScroll.Text = "slickScroll1";
			// 
			// FLP_Options
			// 
			this.FLP_Options.AutoSize = true;
			this.FLP_Options.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.FLP_Options.Controls.Add(this.TLP_GeneralSettings);
			this.FLP_Options.Controls.Add(this.TLP_LaunchSettings);
			this.FLP_Options.Controls.Add(this.TLP_LSM);
			this.FLP_Options.Location = new System.Drawing.Point(0, 0);
			this.FLP_Options.Name = "FLP_Options";
			this.FLP_Options.Size = new System.Drawing.Size(1336, 262);
			this.FLP_Options.TabIndex = 18;
			// 
			// TLP_GeneralSettings
			// 
			this.TLP_GeneralSettings.AddOutline = true;
			this.TLP_GeneralSettings.AutoSize = true;
			this.TLP_GeneralSettings.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.TLP_GeneralSettings.ColumnCount = 2;
			this.TLP_GeneralSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_GeneralSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_GeneralSettings.Controls.Add(this.L_ProfileUsage, 1, 0);
			this.TLP_GeneralSettings.Controls.Add(this.T_ProfileUsage, 1, 1);
			this.TLP_GeneralSettings.Controls.Add(this.CB_AutoSave, 0, 1);
			this.TLP_GeneralSettings.Image = ((System.Drawing.Image)(resources.GetObject("TLP_GeneralSettings.Image")));
			this.TLP_GeneralSettings.Location = new System.Drawing.Point(3, 3);
			this.TLP_GeneralSettings.Name = "TLP_GeneralSettings";
			this.TLP_GeneralSettings.Padding = new System.Windows.Forms.Padding(7, 38, 7, 7);
			this.TLP_GeneralSettings.RowCount = 2;
			this.TLP_GeneralSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_GeneralSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_GeneralSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.TLP_GeneralSettings.Size = new System.Drawing.Size(501, 114);
			this.TLP_GeneralSettings.TabIndex = 17;
			// 
			// L_ProfileUsage
			// 
			this.L_ProfileUsage.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.L_ProfileUsage.AutoSize = true;
			this.L_ProfileUsage.Location = new System.Drawing.Point(310, 38);
			this.L_ProfileUsage.Name = "L_ProfileUsage";
			this.L_ProfileUsage.Size = new System.Drawing.Size(55, 23);
			this.L_ProfileUsage.TabIndex = 21;
			this.L_ProfileUsage.Text = "label1";
			// 
			// T_ProfileUsage
			// 
			this.T_ProfileUsage.Cursor = System.Windows.Forms.Cursors.Hand;
			this.T_ProfileUsage.Image1 = "I_City";
			this.T_ProfileUsage.Image2 = "I_Tools";
			this.T_ProfileUsage.Location = new System.Drawing.Point(185, 64);
			this.T_ProfileUsage.Name = "T_ProfileUsage";
			this.T_ProfileUsage.Option1 = "GamePlay";
			this.T_ProfileUsage.Option2 = "EditorPlay";
			this.T_ProfileUsage.OptionStyle1 = Extensions.ColorStyle.Active;
			this.T_ProfileUsage.OptionStyle2 = Extensions.ColorStyle.Active;
			this.T_ProfileUsage.Size = new System.Drawing.Size(306, 40);
			this.T_ProfileUsage.TabIndex = 20;
			this.T_ProfileUsage.SelectedValueChanged += new System.EventHandler(this.T_ProfileUsage_SelectedValueChanged);
			// 
			// CB_AutoSave
			// 
			this.CB_AutoSave.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.CB_AutoSave.AutoSize = true;
			this.CB_AutoSave.Checked = false;
			this.CB_AutoSave.CheckedText = null;
			this.CB_AutoSave.Cursor = System.Windows.Forms.Cursors.Hand;
			this.CB_AutoSave.DefaultValue = false;
			this.CB_AutoSave.EnterTriggersClick = false;
			this.CB_AutoSave.Location = new System.Drawing.Point(10, 69);
			this.CB_AutoSave.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
			this.CB_AutoSave.Name = "CB_AutoSave";
			this.CB_AutoSave.Size = new System.Drawing.Size(152, 30);
			this.CB_AutoSave.SpaceTriggersClick = true;
			this.CB_AutoSave.TabIndex = 0;
			this.CB_AutoSave.Text = "AutoSave";
			this.CB_AutoSave.UncheckedText = null;
			this.CB_AutoSave.CheckChanged += new System.EventHandler(this.ValueChanged);
			// 
			// TLP_LaunchSettings
			// 
			this.TLP_LaunchSettings.AddOutline = true;
			this.TLP_LaunchSettings.AutoSize = true;
			this.TLP_LaunchSettings.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.TLP_LaunchSettings.ColumnCount = 3;
			this.TLP_LaunchSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_LaunchSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TLP_LaunchSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_LaunchSettings.Controls.Add(this.CB_LoadSave, 0, 5);
			this.TLP_LaunchSettings.Controls.Add(this.CB_LHT, 0, 0);
			this.TLP_LaunchSettings.Controls.Add(this.CB_NoWorkshop, 0, 1);
			this.TLP_LaunchSettings.Controls.Add(this.CB_NoAssets, 0, 2);
			this.TLP_LaunchSettings.Controls.Add(this.CB_NoMods, 0, 3);
			this.TLP_LaunchSettings.Controls.Add(this.CB_DebugMono, 2, 0);
			this.TLP_LaunchSettings.Controls.Add(this.CB_UseCitiesExe, 2, 1);
			this.TLP_LaunchSettings.Controls.Add(this.CB_UnityProfiler, 2, 2);
			this.TLP_LaunchSettings.Controls.Add(this.DD_SaveFile, 1, 5);
			this.TLP_LaunchSettings.Image = global::LoadOrderToolTwo.Properties.Resources.I_Launch;
			this.TLP_LaunchSettings.Location = new System.Drawing.Point(510, 3);
			this.TLP_LaunchSettings.Name = "TLP_LaunchSettings";
			this.TLP_LaunchSettings.Padding = new System.Windows.Forms.Padding(7, 38, 7, 7);
			this.TLP_LaunchSettings.RowCount = 6;
			this.TLP_LaunchSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_LaunchSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_LaunchSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_LaunchSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_LaunchSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_LaunchSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_LaunchSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.TLP_LaunchSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.TLP_LaunchSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.TLP_LaunchSettings.Size = new System.Drawing.Size(473, 256);
			this.TLP_LaunchSettings.TabIndex = 1;
			// 
			// CB_LoadSave
			// 
			this.CB_LoadSave.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.CB_LoadSave.AutoSize = true;
			this.CB_LoadSave.Checked = false;
			this.CB_LoadSave.CheckedText = null;
			this.CB_LoadSave.Cursor = System.Windows.Forms.Cursors.Hand;
			this.CB_LoadSave.DefaultValue = false;
			this.CB_LoadSave.EnterTriggersClick = false;
			this.CB_LoadSave.Location = new System.Drawing.Point(10, 202);
			this.CB_LoadSave.Name = "CB_LoadSave";
			this.CB_LoadSave.Size = new System.Drawing.Size(25, 25);
			this.CB_LoadSave.SpaceTriggersClick = true;
			this.CB_LoadSave.TabIndex = 7;
			this.CB_LoadSave.UncheckedText = null;
			this.CB_LoadSave.CheckChanged += new System.EventHandler(this.ValueChanged);
			// 
			// CB_LHT
			// 
			this.CB_LHT.AutoSize = true;
			this.CB_LHT.Checked = false;
			this.CB_LHT.CheckedText = null;
			this.TLP_LaunchSettings.SetColumnSpan(this.CB_LHT, 2);
			this.CB_LHT.Cursor = System.Windows.Forms.Cursors.Hand;
			this.CB_LHT.DefaultValue = false;
			this.CB_LHT.EnterTriggersClick = false;
			this.CB_LHT.Location = new System.Drawing.Point(10, 48);
			this.CB_LHT.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
			this.CB_LHT.Name = "CB_LHT";
			this.CB_LHT.Size = new System.Drawing.Size(162, 30);
			this.CB_LHT.SpaceTriggersClick = true;
			this.CB_LHT.TabIndex = 0;
			this.CB_LHT.Text = "LHT";
			this.CB_LHT.UncheckedText = null;
			this.CB_LHT.CheckChanged += new System.EventHandler(this.ValueChanged);
			// 
			// CB_NoWorkshop
			// 
			this.CB_NoWorkshop.AutoSize = true;
			this.CB_NoWorkshop.Checked = false;
			this.CB_NoWorkshop.CheckedText = null;
			this.TLP_LaunchSettings.SetColumnSpan(this.CB_NoWorkshop, 2);
			this.CB_NoWorkshop.Cursor = System.Windows.Forms.Cursors.Hand;
			this.CB_NoWorkshop.DefaultValue = false;
			this.CB_NoWorkshop.EnterTriggersClick = false;
			this.CB_NoWorkshop.Location = new System.Drawing.Point(10, 81);
			this.CB_NoWorkshop.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.CB_NoWorkshop.Name = "CB_NoWorkshop";
			this.CB_NoWorkshop.Size = new System.Drawing.Size(216, 30);
			this.CB_NoWorkshop.SpaceTriggersClick = true;
			this.CB_NoWorkshop.TabIndex = 4;
			this.CB_NoWorkshop.Text = "NoWorkshop";
			this.CB_NoWorkshop.UncheckedText = null;
			this.CB_NoWorkshop.CheckChanged += new System.EventHandler(this.ValueChanged);
			// 
			// CB_NoAssets
			// 
			this.CB_NoAssets.AutoSize = true;
			this.CB_NoAssets.Checked = false;
			this.CB_NoAssets.CheckedText = null;
			this.TLP_LaunchSettings.SetColumnSpan(this.CB_NoAssets, 2);
			this.CB_NoAssets.Cursor = System.Windows.Forms.Cursors.Hand;
			this.CB_NoAssets.DefaultValue = false;
			this.CB_NoAssets.EnterTriggersClick = false;
			this.CB_NoAssets.Location = new System.Drawing.Point(10, 114);
			this.CB_NoAssets.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.CB_NoAssets.Name = "CB_NoAssets";
			this.CB_NoAssets.Size = new System.Drawing.Size(173, 30);
			this.CB_NoAssets.SpaceTriggersClick = true;
			this.CB_NoAssets.TabIndex = 5;
			this.CB_NoAssets.Text = "NoAssets";
			this.CB_NoAssets.UncheckedText = null;
			this.CB_NoAssets.CheckChanged += new System.EventHandler(this.ValueChanged);
			// 
			// CB_NoMods
			// 
			this.CB_NoMods.AutoSize = true;
			this.CB_NoMods.Checked = false;
			this.CB_NoMods.CheckedText = null;
			this.TLP_LaunchSettings.SetColumnSpan(this.CB_NoMods, 2);
			this.CB_NoMods.Cursor = System.Windows.Forms.Cursors.Hand;
			this.CB_NoMods.DefaultValue = false;
			this.CB_NoMods.EnterTriggersClick = false;
			this.CB_NoMods.Location = new System.Drawing.Point(10, 147);
			this.CB_NoMods.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.CB_NoMods.Name = "CB_NoMods";
			this.CB_NoMods.Size = new System.Drawing.Size(168, 30);
			this.CB_NoMods.SpaceTriggersClick = true;
			this.CB_NoMods.TabIndex = 6;
			this.CB_NoMods.Text = "NoMods";
			this.CB_NoMods.UncheckedText = null;
			this.CB_NoMods.CheckChanged += new System.EventHandler(this.ValueChanged);
			// 
			// CB_DebugMono
			// 
			this.CB_DebugMono.AutoSize = true;
			this.CB_DebugMono.Checked = false;
			this.CB_DebugMono.CheckedText = null;
			this.CB_DebugMono.Cursor = System.Windows.Forms.Cursors.Hand;
			this.CB_DebugMono.DefaultValue = false;
			this.CB_DebugMono.EnterTriggersClick = false;
			this.CB_DebugMono.Location = new System.Drawing.Point(232, 48);
			this.CB_DebugMono.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
			this.CB_DebugMono.Name = "CB_DebugMono";
			this.CB_DebugMono.Size = new System.Drawing.Size(168, 30);
			this.CB_DebugMono.SpaceTriggersClick = true;
			this.CB_DebugMono.TabIndex = 6;
			this.CB_DebugMono.Text = "UseDebugMono";
			this.CB_DebugMono.UncheckedText = null;
			this.CB_DebugMono.CheckChanged += new System.EventHandler(this.ValueChanged);
			// 
			// CB_UseCitiesExe
			// 
			this.CB_UseCitiesExe.AutoSize = true;
			this.CB_UseCitiesExe.Checked = false;
			this.CB_UseCitiesExe.CheckedText = null;
			this.CB_UseCitiesExe.Cursor = System.Windows.Forms.Cursors.Hand;
			this.CB_UseCitiesExe.DefaultValue = false;
			this.CB_UseCitiesExe.EnterTriggersClick = false;
			this.CB_UseCitiesExe.Location = new System.Drawing.Point(232, 81);
			this.CB_UseCitiesExe.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.CB_UseCitiesExe.Name = "CB_UseCitiesExe";
			this.CB_UseCitiesExe.Size = new System.Drawing.Size(231, 30);
			this.CB_UseCitiesExe.SpaceTriggersClick = true;
			this.CB_UseCitiesExe.TabIndex = 6;
			this.CB_UseCitiesExe.Text = "LaunchThroughCities";
			this.CB_UseCitiesExe.UncheckedText = null;
			this.CB_UseCitiesExe.CheckChanged += new System.EventHandler(this.ValueChanged);
			// 
			// CB_UnityProfiler
			// 
			this.CB_UnityProfiler.AutoSize = true;
			this.CB_UnityProfiler.Checked = false;
			this.CB_UnityProfiler.CheckedText = null;
			this.CB_UnityProfiler.Cursor = System.Windows.Forms.Cursors.Hand;
			this.CB_UnityProfiler.DefaultValue = false;
			this.CB_UnityProfiler.EnterTriggersClick = false;
			this.CB_UnityProfiler.Location = new System.Drawing.Point(232, 114);
			this.CB_UnityProfiler.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.CB_UnityProfiler.Name = "CB_UnityProfiler";
			this.CB_UnityProfiler.Size = new System.Drawing.Size(166, 30);
			this.CB_UnityProfiler.SpaceTriggersClick = true;
			this.CB_UnityProfiler.TabIndex = 6;
			this.CB_UnityProfiler.Text = "UnityProfilerMode";
			this.CB_UnityProfiler.UncheckedText = null;
			this.CB_UnityProfiler.CheckChanged += new System.EventHandler(this.ValueChanged);
			// 
			// DD_SaveFile
			// 
			this.DD_SaveFile.AllowDrop = true;
			this.TLP_LaunchSettings.SetColumnSpan(this.DD_SaveFile, 2);
			this.DD_SaveFile.Cursor = System.Windows.Forms.Cursors.Hand;
			this.DD_SaveFile.Location = new System.Drawing.Point(41, 183);
			this.DD_SaveFile.Name = "DD_SaveFile";
			this.DD_SaveFile.Size = new System.Drawing.Size(293, 63);
			this.DD_SaveFile.TabIndex = 8;
			this.DD_SaveFile.Text = "SaveFileInfo";
			this.DD_SaveFile.ValidExtensions = new string[] {
        ".crp"};
			this.DD_SaveFile.FileSelected += new System.Action<string>(this.DD_SaveFile_FileSelected);
			this.DD_SaveFile.ValidFile += new System.Func<string, bool>(this.DD_SaveFile_ValidFile);
			// 
			// TLP_LSM
			// 
			this.TLP_LSM.AddOutline = true;
			this.TLP_LSM.AutoSize = true;
			this.TLP_LSM.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.TLP_LSM.ColumnCount = 2;
			this.TLP_LSM.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_LSM.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_LSM.Controls.Add(this.CB_SkipFile, 0, 2);
			this.TLP_LSM.Controls.Add(this.CB_LoadEnabled, 0, 0);
			this.TLP_LSM.Controls.Add(this.CB_LoadUsed, 0, 1);
			this.TLP_LSM.Controls.Add(this.DD_SkipFile, 1, 2);
			this.TLP_LSM.Image = global::LoadOrderToolTwo.Properties.Resources.I_LSM;
			this.TLP_LSM.Location = new System.Drawing.Point(989, 3);
			this.TLP_LSM.Name = "TLP_LSM";
			this.TLP_LSM.Padding = new System.Windows.Forms.Padding(7, 38, 7, 7);
			this.TLP_LSM.RowCount = 3;
			this.TLP_LSM.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_LSM.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_LSM.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_LSM.Size = new System.Drawing.Size(344, 190);
			this.TLP_LSM.TabIndex = 18;
			// 
			// CB_SkipFile
			// 
			this.CB_SkipFile.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.CB_SkipFile.AutoSize = true;
			this.CB_SkipFile.Checked = false;
			this.CB_SkipFile.CheckedText = null;
			this.CB_SkipFile.Cursor = System.Windows.Forms.Cursors.Hand;
			this.CB_SkipFile.DefaultValue = false;
			this.CB_SkipFile.EnterTriggersClick = false;
			this.CB_SkipFile.Location = new System.Drawing.Point(10, 136);
			this.CB_SkipFile.Name = "CB_SkipFile";
			this.CB_SkipFile.Size = new System.Drawing.Size(25, 25);
			this.CB_SkipFile.SpaceTriggersClick = true;
			this.CB_SkipFile.TabIndex = 8;
			this.CB_SkipFile.UncheckedText = null;
			this.CB_SkipFile.CheckChanged += new System.EventHandler(this.LsmSettingsChanged);
			// 
			// CB_LoadEnabled
			// 
			this.CB_LoadEnabled.AutoSize = true;
			this.CB_LoadEnabled.Checked = false;
			this.CB_LoadEnabled.CheckedText = null;
			this.TLP_LSM.SetColumnSpan(this.CB_LoadEnabled, 2);
			this.CB_LoadEnabled.Cursor = System.Windows.Forms.Cursors.Hand;
			this.CB_LoadEnabled.DefaultValue = false;
			this.CB_LoadEnabled.EnterTriggersClick = false;
			this.CB_LoadEnabled.Location = new System.Drawing.Point(10, 48);
			this.CB_LoadEnabled.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
			this.CB_LoadEnabled.Name = "CB_LoadEnabled";
			this.CB_LoadEnabled.Size = new System.Drawing.Size(136, 30);
			this.CB_LoadEnabled.SpaceTriggersClick = true;
			this.CB_LoadEnabled.TabIndex = 4;
			this.CB_LoadEnabled.Text = "LoadEnabled";
			this.CB_LoadEnabled.UncheckedText = null;
			this.CB_LoadEnabled.CheckChanged += new System.EventHandler(this.LsmSettingsChanged);
			// 
			// CB_LoadUsed
			// 
			this.CB_LoadUsed.AutoSize = true;
			this.CB_LoadUsed.Checked = false;
			this.CB_LoadUsed.CheckedText = null;
			this.TLP_LSM.SetColumnSpan(this.CB_LoadUsed, 2);
			this.CB_LoadUsed.Cursor = System.Windows.Forms.Cursors.Hand;
			this.CB_LoadUsed.DefaultValue = false;
			this.CB_LoadUsed.EnterTriggersClick = false;
			this.CB_LoadUsed.Location = new System.Drawing.Point(10, 81);
			this.CB_LoadUsed.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.CB_LoadUsed.Name = "CB_LoadUsed";
			this.CB_LoadUsed.Size = new System.Drawing.Size(114, 30);
			this.CB_LoadUsed.SpaceTriggersClick = true;
			this.CB_LoadUsed.TabIndex = 5;
			this.CB_LoadUsed.Text = "LoadUsed";
			this.CB_LoadUsed.UncheckedText = null;
			this.CB_LoadUsed.CheckChanged += new System.EventHandler(this.LsmSettingsChanged);
			// 
			// DD_SkipFile
			// 
			this.DD_SkipFile.AllowDrop = true;
			this.DD_SkipFile.Cursor = System.Windows.Forms.Cursors.Hand;
			this.DD_SkipFile.Location = new System.Drawing.Point(41, 117);
			this.DD_SkipFile.Name = "DD_SkipFile";
			this.DD_SkipFile.Size = new System.Drawing.Size(293, 63);
			this.DD_SkipFile.TabIndex = 8;
			this.DD_SkipFile.Text = "SkipFileInfo";
			this.DD_SkipFile.ValidExtensions = new string[] {
        ".txt"};
			this.DD_SkipFile.FileSelected += new System.Action<string>(this.DD_SkipFile_FileSelected);
			this.DD_SkipFile.ValidFile += new System.Func<string, bool>(this.DD_SkipFile_ValidFile);
			// 
			// B_ViewProfiles
			// 
			this.B_ViewProfiles.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.B_ViewProfiles.ColorShade = null;
			this.B_ViewProfiles.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_ViewProfiles.Image = global::LoadOrderToolTwo.Properties.Resources.I_Import;
			this.B_ViewProfiles.Location = new System.Drawing.Point(1300, 19);
			this.B_ViewProfiles.Margin = new System.Windows.Forms.Padding(10);
			this.B_ViewProfiles.Name = "B_ViewProfiles";
			this.B_ViewProfiles.Padding = new System.Windows.Forms.Padding(10, 15, 10, 15);
			this.B_ViewProfiles.Size = new System.Drawing.Size(100, 30);
			this.B_ViewProfiles.SpaceTriggersClick = true;
			this.B_ViewProfiles.TabIndex = 14;
			this.B_ViewProfiles.Text = "ViewProfiles";
			this.B_ViewProfiles.Click += new System.EventHandler(this.B_LoadProfiles_Click);
			// 
			// I_Info
			// 
			this.I_Info.ActiveColor = null;
			this.I_Info.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.I_Info.ColorStyle = Extensions.ColorStyle.Icon;
			this.I_Info.Cursor = System.Windows.Forms.Cursors.Hand;
			this.I_Info.Enabled = false;
			this.I_Info.Image = global::LoadOrderToolTwo.Properties.Resources.I_Info;
			this.I_Info.Location = new System.Drawing.Point(18, 116);
			this.I_Info.Margin = new System.Windows.Forms.Padding(18, 3, 3, 3);
			this.I_Info.Name = "I_Info";
			this.I_Info.Selected = true;
			this.I_Info.Size = new System.Drawing.Size(32, 32);
			this.I_Info.TabIndex = 2;
			this.I_Info.TabStop = false;
			// 
			// L_Info
			// 
			this.L_Info.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.L_Info.AutoSize = true;
			this.TLP_Main.SetColumnSpan(this.L_Info, 4);
			this.L_Info.Location = new System.Drawing.Point(56, 121);
			this.L_Info.Margin = new System.Windows.Forms.Padding(3, 10, 10, 10);
			this.L_Info.Name = "L_Info";
			this.L_Info.Size = new System.Drawing.Size(55, 23);
			this.L_Info.TabIndex = 15;
			this.L_Info.Text = "label1";
			this.L_Info.UseMnemonic = false;
			// 
			// B_TempProfile
			// 
			this.B_TempProfile.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.B_TempProfile.ColorShade = null;
			this.B_TempProfile.ColorStyle = Extensions.ColorStyle.Green;
			this.B_TempProfile.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_TempProfile.Image = global::LoadOrderToolTwo.Properties.Resources.I_TempProfile;
			this.B_TempProfile.Location = new System.Drawing.Point(1180, 19);
			this.B_TempProfile.Margin = new System.Windows.Forms.Padding(10);
			this.B_TempProfile.Name = "B_TempProfile";
			this.B_TempProfile.Padding = new System.Windows.Forms.Padding(10, 15, 10, 15);
			this.B_TempProfile.Size = new System.Drawing.Size(100, 30);
			this.B_TempProfile.SpaceTriggersClick = true;
			this.B_TempProfile.TabIndex = 14;
			this.B_TempProfile.Text = "TemporaryProfile";
			this.B_TempProfile.Click += new System.EventHandler(this.B_TempProfile_Click);
			// 
			// B_NewProfile
			// 
			this.B_NewProfile.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.B_NewProfile.ColorShade = null;
			this.B_NewProfile.ColorStyle = Extensions.ColorStyle.Green;
			this.B_NewProfile.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_NewProfile.Image = global::LoadOrderToolTwo.Properties.Resources.I_Add;
			this.B_NewProfile.Location = new System.Drawing.Point(1060, 19);
			this.B_NewProfile.Margin = new System.Windows.Forms.Padding(10);
			this.B_NewProfile.Name = "B_NewProfile";
			this.B_NewProfile.Padding = new System.Windows.Forms.Padding(10, 15, 10, 15);
			this.B_NewProfile.Size = new System.Drawing.Size(100, 30);
			this.B_NewProfile.SpaceTriggersClick = true;
			this.B_NewProfile.TabIndex = 14;
			this.B_NewProfile.Text = "AddProfile";
			this.B_NewProfile.Click += new System.EventHandler(this.B_NewProfile_Click);
			// 
			// TLP_New
			// 
			this.TLP_New.ColumnCount = 3;
			this.TLP_New.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TLP_New.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_New.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TLP_New.Controls.Add(this.newProfileOptionControl1, 1, 1);
			this.TLP_New.Controls.Add(this.newProfileOptionControl2, 1, 2);
			this.TLP_New.Controls.Add(this.B_Cancel, 2, 4);
			this.TLP_New.Controls.Add(this.DAD_NewProfile, 0, 3);
			this.TLP_New.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TLP_New.Location = new System.Drawing.Point(0, 30);
			this.TLP_New.Name = "TLP_New";
			this.TLP_New.RowCount = 5;
			this.TLP_New.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TLP_New.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_New.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_New.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_New.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TLP_New.Size = new System.Drawing.Size(1410, 547);
			this.TLP_New.TabIndex = 16;
			this.TLP_New.Visible = false;
			// 
			// newProfileOptionControl1
			// 
			this.newProfileOptionControl1.Cursor = System.Windows.Forms.Cursors.Hand;
			this.newProfileOptionControl1.FromScratch = true;
			this.newProfileOptionControl1.Location = new System.Drawing.Point(630, 87);
			this.newProfileOptionControl1.Name = "newProfileOptionControl1";
			this.newProfileOptionControl1.Size = new System.Drawing.Size(150, 150);
			this.newProfileOptionControl1.TabIndex = 0;
			this.newProfileOptionControl1.Click += new System.EventHandler(this.NewProfile_Click);
			// 
			// newProfileOptionControl2
			// 
			this.newProfileOptionControl2.Cursor = System.Windows.Forms.Cursors.Hand;
			this.newProfileOptionControl2.FromScratch = false;
			this.newProfileOptionControl2.Location = new System.Drawing.Point(630, 243);
			this.newProfileOptionControl2.Name = "newProfileOptionControl2";
			this.newProfileOptionControl2.Size = new System.Drawing.Size(150, 110);
			this.newProfileOptionControl2.TabIndex = 0;
			this.newProfileOptionControl2.Click += new System.EventHandler(this.CopyProfile_Click);
			// 
			// B_Cancel
			// 
			this.B_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.B_Cancel.ColorShade = null;
			this.B_Cancel.ColorStyle = Extensions.ColorStyle.Red;
			this.B_Cancel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_Cancel.Image = global::LoadOrderToolTwo.Properties.Resources.I_Disposable;
			this.B_Cancel.Location = new System.Drawing.Point(1300, 507);
			this.B_Cancel.Margin = new System.Windows.Forms.Padding(10);
			this.B_Cancel.Name = "B_Cancel";
			this.B_Cancel.Padding = new System.Windows.Forms.Padding(10, 15, 10, 15);
			this.B_Cancel.Size = new System.Drawing.Size(100, 30);
			this.B_Cancel.SpaceTriggersClick = true;
			this.B_Cancel.TabIndex = 14;
			this.B_Cancel.Text = "Cancel";
			this.B_Cancel.Click += new System.EventHandler(this.B_Cancel_Click);
			// 
			// DAD_NewProfile
			// 
			this.DAD_NewProfile.AllowDrop = true;
			this.DAD_NewProfile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DAD_NewProfile.Cursor = System.Windows.Forms.Cursors.Hand;
			this.DAD_NewProfile.Location = new System.Drawing.Point(3, 394);
			this.DAD_NewProfile.Name = "DAD_NewProfile";
			this.TLP_New.SetRowSpan(this.DAD_NewProfile, 2);
			this.DAD_NewProfile.Size = new System.Drawing.Size(150, 150);
			this.DAD_NewProfile.TabIndex = 15;
			this.DAD_NewProfile.Text = "DropNewProfile";
			this.DAD_NewProfile.ValidExtensions = new string[] {
        ".json"};
			this.DAD_NewProfile.FileSelected += new System.Action<string>(this.DAD_NewProfile_FileSelected);
			this.DAD_NewProfile.ValidFile += new System.Func<string, bool>(this.DAD_NewProfile_ValidFile);
			// 
			// PC_Profiles
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.TLP_New);
			this.Controls.Add(this.TLP_Main);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(58)))), ((int)(((byte)(69)))));
			this.Name = "PC_Profiles";
			this.Padding = new System.Windows.Forms.Padding(0, 30, 0, 0);
			this.Size = new System.Drawing.Size(1410, 577);
			this.Controls.SetChildIndex(this.TLP_Main, 0);
			this.Controls.SetChildIndex(this.TLP_New, 0);
			this.Controls.SetChildIndex(this.base_Text, 0);
			this.TLP_ProfileName.ResumeLayout(false);
			this.TLP_ProfileName.PerformLayout();
			this.TLP_Main.ResumeLayout(false);
			this.TLP_Main.PerformLayout();
			this.P_Options.ResumeLayout(false);
			this.P_ScrollPanel.ResumeLayout(false);
			this.P_ScrollPanel.PerformLayout();
			this.FLP_Options.ResumeLayout(false);
			this.FLP_Options.PerformLayout();
			this.TLP_GeneralSettings.ResumeLayout(false);
			this.TLP_GeneralSettings.PerformLayout();
			this.TLP_LaunchSettings.ResumeLayout(false);
			this.TLP_LaunchSettings.PerformLayout();
			this.TLP_LSM.ResumeLayout(false);
			this.TLP_LSM.PerformLayout();
			this.TLP_New.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	private SlickControls.RoundedTableLayoutPanel TLP_ProfileName;
	private System.Windows.Forms.Label L_CurrentProfile;
	private SlickControls.SlickIcon B_EditName;
	private SlickControls.SlickIcon I_ProfileIcon;
	private System.Windows.Forms.TableLayoutPanel TLP_Main;
	private SlickControls.SlickButton B_ViewProfiles;
	private SlickControls.SlickButton B_NewProfile;
	private System.Windows.Forms.Label L_TempProfile;
	private SlickControls.RoundedPanel P_Options;
	private SlickControls.SlickScroll slickScroll;
	private SlickControls.RoundedGroupTableLayoutPanel TLP_LaunchSettings;
	private SlickControls.SlickCheckbox CB_LHT;
	private SlickControls.SlickCheckbox CB_NoWorkshop;
	private SlickControls.SlickCheckbox CB_NoAssets;
	private SlickControls.SlickCheckbox CB_NoMods;
	private SlickControls.SlickIcon I_TempProfile;
	private SlickControls.RoundedGroupTableLayoutPanel TLP_GeneralSettings;
	private SlickControls.SlickCheckbox CB_AutoSave;
	private System.Windows.Forms.FlowLayoutPanel FLP_Options;
	private System.Windows.Forms.Panel P_ScrollPanel;
	private SlickControls.SlickTextBox TB_Name;
	private System.Windows.Forms.TableLayoutPanel TLP_New;
	private NewProfileOptionControl newProfileOptionControl1;
	private NewProfileOptionControl newProfileOptionControl2;
	private SlickControls.SlickButton B_Cancel;
	private ThreeOptionToggle T_ProfileUsage;
	private System.Windows.Forms.Label L_ProfileUsage;
	private SlickControls.RoundedGroupTableLayoutPanel TLP_LSM;
	private SlickControls.SlickCheckbox CB_LoadEnabled;
	private SlickControls.SlickCheckbox CB_LoadUsed;
	private SlickControls.SlickIcon B_Save;
	private SlickControls.SlickIcon I_Info;
	private System.Windows.Forms.Label L_Info;
	private SlickControls.SlickButton B_TempProfile;
	private SlickControls.SlickCheckbox CB_LoadSave;
	private SlickControls.SlickCheckbox CB_SkipFile;
	private SlickControls.SlickCheckbox CB_DebugMono;
	private SlickControls.SlickCheckbox CB_UseCitiesExe;
	private SlickControls.SlickCheckbox CB_UnityProfiler;
	private DragAndDropControl DD_SaveFile;
	private DragAndDropControl DD_SkipFile;
	private DragAndDropControl DAD_NewProfile;
}
