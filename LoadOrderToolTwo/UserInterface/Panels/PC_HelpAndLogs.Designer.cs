namespace LoadOrderToolTwo.UserInterface.Panels;

partial class PC_HelpAndLogs
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
			this.slickScroll1 = new SlickControls.SlickScroll();
			this.TLP_Main = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.TLP_HelpLogs = new SlickControls.RoundedGroupTableLayoutPanel();
			this.B_SaveZip = new SlickControls.SlickButton();
			this.B_CopyZip = new SlickControls.SlickButton();
			this.B_Discord = new SlickControls.SlickButton();
			this.B_Guide = new SlickControls.SlickButton();
			this.slickSpacer1 = new SlickControls.SlickSpacer();
			this.TLP_LogFolders = new SlickControls.RoundedGroupTableLayoutPanel();
			this.B_LotLogCopy = new SlickControls.SlickButton();
			this.B_LotLog = new SlickControls.SlickButton();
			this.slickSpacer2 = new SlickControls.SlickSpacer();
			this.B_OpenLogFolder = new SlickControls.SlickButton();
			this.B_CopyLogFile = new SlickControls.SlickButton();
			this.DD_LogFile = new LoadOrderToolTwo.UserInterface.Generic.DragAndDropControl();
			this.TLP_Errors = new SlickControls.RoundedGroupTableLayoutPanel();
			this.I_Info = new SlickControls.SlickIcon();
			this.L_Info = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.B_ChangeLog = new SlickControls.SlickButton();
			this.TLP_Main.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.TLP_HelpLogs.SuspendLayout();
			this.TLP_LogFolders.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// slickScroll1
			// 
			this.slickScroll1.Dock = System.Windows.Forms.DockStyle.Right;
			this.slickScroll1.LinkedControl = this.TLP_Main;
			this.slickScroll1.Location = new System.Drawing.Point(1158, 30);
			this.slickScroll1.Name = "slickScroll1";
			this.slickScroll1.Size = new System.Drawing.Size(8, 702);
			this.slickScroll1.Style = SlickControls.StyleType.Vertical;
			this.slickScroll1.TabIndex = 17;
			this.slickScroll1.TabStop = false;
			this.slickScroll1.Text = "slickScroll1";
			// 
			// TLP_Main
			// 
			this.TLP_Main.AutoSize = true;
			this.TLP_Main.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.TLP_Main.ColumnCount = 3;
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 66.66667F));
			this.TLP_Main.Controls.Add(this.tableLayoutPanel1, 0, 0);
			this.TLP_Main.Controls.Add(this.DD_LogFile, 1, 0);
			this.TLP_Main.Controls.Add(this.TLP_Errors, 1, 2);
			this.TLP_Main.Controls.Add(this.I_Info, 1, 1);
			this.TLP_Main.Controls.Add(this.L_Info, 2, 1);
			this.TLP_Main.Location = new System.Drawing.Point(0, 0);
			this.TLP_Main.MaximumSize = new System.Drawing.Size(1100, 0);
			this.TLP_Main.Name = "TLP_Main";
			this.TLP_Main.RowCount = 3;
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.TLP_Main.Size = new System.Drawing.Size(1100, 436);
			this.TLP_Main.TabIndex = 13;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.TLP_HelpLogs, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.TLP_LogFolders, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.TLP_Main.SetRowSpan(this.tableLayoutPanel1, 3);
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(351, 436);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// TLP_HelpLogs
			// 
			this.TLP_HelpLogs.AddOutline = true;
			this.TLP_HelpLogs.AutoSize = true;
			this.TLP_HelpLogs.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.TLP_HelpLogs.ColumnCount = 1;
			this.TLP_HelpLogs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TLP_HelpLogs.Controls.Add(this.B_ChangeLog, 0, 2);
			this.TLP_HelpLogs.Controls.Add(this.B_SaveZip, 0, 5);
			this.TLP_HelpLogs.Controls.Add(this.B_CopyZip, 0, 4);
			this.TLP_HelpLogs.Controls.Add(this.B_Discord, 0, 0);
			this.TLP_HelpLogs.Controls.Add(this.B_Guide, 0, 1);
			this.TLP_HelpLogs.Controls.Add(this.slickSpacer1, 0, 3);
			this.TLP_HelpLogs.Dock = System.Windows.Forms.DockStyle.Top;
			this.TLP_HelpLogs.Location = new System.Drawing.Point(3, 3);
			this.TLP_HelpLogs.Name = "TLP_HelpLogs";
			this.TLP_HelpLogs.Padding = new System.Windows.Forms.Padding(7, 38, 7, 7);
			this.TLP_HelpLogs.RowCount = 6;
			this.TLP_HelpLogs.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_HelpLogs.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_HelpLogs.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_HelpLogs.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_HelpLogs.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_HelpLogs.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_HelpLogs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.TLP_HelpLogs.Size = new System.Drawing.Size(345, 222);
			this.TLP_HelpLogs.TabIndex = 0;
			this.TLP_HelpLogs.Text = "HelpSupport";
			// 
			// B_SaveZip
			// 
			this.B_SaveZip.AutoSize = true;
			this.B_SaveZip.ColorShade = null;
			this.B_SaveZip.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_SaveZip.Dock = System.Windows.Forms.DockStyle.Top;
			this.B_SaveZip.Location = new System.Drawing.Point(10, 198);
			this.B_SaveZip.Name = "B_SaveZip";
			this.B_SaveZip.Size = new System.Drawing.Size(325, 14);
			this.B_SaveZip.SpaceTriggersClick = true;
			this.B_SaveZip.TabIndex = 3;
			this.B_SaveZip.Text = "LogZipFile";
			this.B_SaveZip.Click += new System.EventHandler(this.B_SaveZip_Click);
			// 
			// B_CopyZip
			// 
			this.B_CopyZip.AutoSize = true;
			this.B_CopyZip.ColorShade = null;
			this.B_CopyZip.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_CopyZip.Dock = System.Windows.Forms.DockStyle.Top;
			this.B_CopyZip.Location = new System.Drawing.Point(10, 178);
			this.B_CopyZip.Name = "B_CopyZip";
			this.B_CopyZip.Size = new System.Drawing.Size(325, 14);
			this.B_CopyZip.SpaceTriggersClick = true;
			this.B_CopyZip.TabIndex = 2;
			this.B_CopyZip.Text = "LogZipCopy";
			this.B_CopyZip.Click += new System.EventHandler(this.B_CopyZip_Click);
			// 
			// B_Discord
			// 
			this.B_Discord.AutoSize = true;
			this.B_Discord.ColorShade = null;
			this.B_Discord.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_Discord.Dock = System.Windows.Forms.DockStyle.Top;
			this.B_Discord.Location = new System.Drawing.Point(10, 41);
			this.B_Discord.Name = "B_Discord";
			this.B_Discord.Size = new System.Drawing.Size(325, 30);
			this.B_Discord.SpaceTriggersClick = true;
			this.B_Discord.TabIndex = 0;
			this.B_Discord.Text = "JoinDiscord";
			this.B_Discord.Click += new System.EventHandler(this.B_Discord_Click);
			// 
			// B_Guide
			// 
			this.B_Guide.AutoSize = true;
			this.B_Guide.ColorShade = null;
			this.B_Guide.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_Guide.Dock = System.Windows.Forms.DockStyle.Top;
			this.B_Guide.Location = new System.Drawing.Point(10, 77);
			this.B_Guide.Name = "B_Guide";
			this.B_Guide.Size = new System.Drawing.Size(325, 30);
			this.B_Guide.SpaceTriggersClick = true;
			this.B_Guide.TabIndex = 1;
			this.B_Guide.Text = "OpenGuide";
			this.B_Guide.Click += new System.EventHandler(this.B_Guide_Click);
			// 
			// slickSpacer1
			// 
			this.slickSpacer1.Dock = System.Windows.Forms.DockStyle.Top;
			this.slickSpacer1.Location = new System.Drawing.Point(10, 149);
			this.slickSpacer1.Name = "slickSpacer1";
			this.slickSpacer1.Size = new System.Drawing.Size(325, 23);
			this.slickSpacer1.TabIndex = 2;
			this.slickSpacer1.TabStop = false;
			this.slickSpacer1.Text = "slickSpacer1";
			// 
			// TLP_LogFolders
			// 
			this.TLP_LogFolders.AddOutline = true;
			this.TLP_LogFolders.AutoSize = true;
			this.TLP_LogFolders.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.TLP_LogFolders.ColumnCount = 1;
			this.TLP_LogFolders.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TLP_LogFolders.Controls.Add(this.B_LotLogCopy, 0, 4);
			this.TLP_LogFolders.Controls.Add(this.B_LotLog, 0, 3);
			this.TLP_LogFolders.Controls.Add(this.slickSpacer2, 0, 2);
			this.TLP_LogFolders.Controls.Add(this.B_OpenLogFolder, 0, 0);
			this.TLP_LogFolders.Controls.Add(this.B_CopyLogFile, 0, 1);
			this.TLP_LogFolders.Dock = System.Windows.Forms.DockStyle.Top;
			this.TLP_LogFolders.Location = new System.Drawing.Point(3, 231);
			this.TLP_LogFolders.Name = "TLP_LogFolders";
			this.TLP_LogFolders.Padding = new System.Windows.Forms.Padding(7, 38, 7, 7);
			this.TLP_LogFolders.RowCount = 5;
			this.TLP_LogFolders.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_LogFolders.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_LogFolders.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_LogFolders.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_LogFolders.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_LogFolders.Size = new System.Drawing.Size(345, 202);
			this.TLP_LogFolders.TabIndex = 1;
			this.TLP_LogFolders.Text = "LogFolders";
			// 
			// B_LotLogCopy
			// 
			this.B_LotLogCopy.AutoSize = true;
			this.B_LotLogCopy.ColorShade = null;
			this.B_LotLogCopy.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_LotLogCopy.Dock = System.Windows.Forms.DockStyle.Top;
			this.B_LotLogCopy.Location = new System.Drawing.Point(10, 178);
			this.B_LotLogCopy.Name = "B_LotLogCopy";
			this.B_LotLogCopy.Size = new System.Drawing.Size(325, 14);
			this.B_LotLogCopy.SpaceTriggersClick = true;
			this.B_LotLogCopy.TabIndex = 3;
			this.B_LotLogCopy.Text = "CopyLOTLogFile";
			this.B_LotLogCopy.Click += new System.EventHandler(this.B_LotLogCopy_Click);
			// 
			// B_LotLog
			// 
			this.B_LotLog.AutoSize = true;
			this.B_LotLog.ColorShade = null;
			this.B_LotLog.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_LotLog.Dock = System.Windows.Forms.DockStyle.Top;
			this.B_LotLog.Location = new System.Drawing.Point(10, 158);
			this.B_LotLog.Name = "B_LotLog";
			this.B_LotLog.Size = new System.Drawing.Size(325, 14);
			this.B_LotLog.SpaceTriggersClick = true;
			this.B_LotLog.TabIndex = 2;
			this.B_LotLog.Text = "OpenLOTLogFolder";
			this.B_LotLog.Click += new System.EventHandler(this.B_LotLog_Click);
			// 
			// slickSpacer2
			// 
			this.slickSpacer2.Dock = System.Windows.Forms.DockStyle.Top;
			this.slickSpacer2.Location = new System.Drawing.Point(10, 129);
			this.slickSpacer2.Name = "slickSpacer2";
			this.slickSpacer2.Size = new System.Drawing.Size(325, 23);
			this.slickSpacer2.TabIndex = 18;
			this.slickSpacer2.TabStop = false;
			this.slickSpacer2.Text = "slickSpacer2";
			// 
			// B_OpenLogFolder
			// 
			this.B_OpenLogFolder.AutoSize = true;
			this.B_OpenLogFolder.ColorShade = null;
			this.B_OpenLogFolder.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_OpenLogFolder.Dock = System.Windows.Forms.DockStyle.Top;
			this.B_OpenLogFolder.Location = new System.Drawing.Point(10, 41);
			this.B_OpenLogFolder.Name = "B_OpenLogFolder";
			this.B_OpenLogFolder.Size = new System.Drawing.Size(325, 38);
			this.B_OpenLogFolder.SpaceTriggersClick = true;
			this.B_OpenLogFolder.TabIndex = 0;
			this.B_OpenLogFolder.Text = "OpenLogFolder";
			this.B_OpenLogFolder.Click += new System.EventHandler(this.B_OpenLogFolder_Click);
			// 
			// B_CopyLogFile
			// 
			this.B_CopyLogFile.AutoSize = true;
			this.B_CopyLogFile.ColorShade = null;
			this.B_CopyLogFile.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_CopyLogFile.Dock = System.Windows.Forms.DockStyle.Top;
			this.B_CopyLogFile.Location = new System.Drawing.Point(10, 85);
			this.B_CopyLogFile.Name = "B_CopyLogFile";
			this.B_CopyLogFile.Size = new System.Drawing.Size(325, 38);
			this.B_CopyLogFile.SpaceTriggersClick = true;
			this.B_CopyLogFile.TabIndex = 1;
			this.B_CopyLogFile.Text = "CopyLogFile";
			this.B_CopyLogFile.Click += new System.EventHandler(this.B_CopyLogFile_Click);
			// 
			// DD_LogFile
			// 
			this.DD_LogFile.AllowDrop = true;
			this.TLP_Main.SetColumnSpan(this.DD_LogFile, 2);
			this.DD_LogFile.Cursor = System.Windows.Forms.Cursors.Hand;
			this.DD_LogFile.Dock = System.Windows.Forms.DockStyle.Top;
			this.DD_LogFile.Location = new System.Drawing.Point(354, 3);
			this.DD_LogFile.Name = "DD_LogFile";
			this.DD_LogFile.Size = new System.Drawing.Size(743, 150);
			this.DD_LogFile.TabIndex = 1;
			this.DD_LogFile.Text = "LogFileDrop";
			this.DD_LogFile.ValidExtensions = new string[] {
        ".txt",
        ".log"};
			this.DD_LogFile.FileSelected += new System.Action<string>(this.DD_LogFile_FileSelected);
			this.DD_LogFile.ValidFile += new System.Func<string, bool>(this.DD_LogFile_ValidFile);
			// 
			// TLP_Errors
			// 
			this.TLP_Errors.AddOutline = true;
			this.TLP_Errors.AutoSize = true;
			this.TLP_Errors.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.TLP_Errors.ColumnCount = 1;
			this.TLP_Main.SetColumnSpan(this.TLP_Errors, 2);
			this.TLP_Errors.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TLP_Errors.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.TLP_Errors.Dock = System.Windows.Forms.DockStyle.Top;
			this.TLP_Errors.Location = new System.Drawing.Point(354, 202);
			this.TLP_Errors.Name = "TLP_Errors";
			this.TLP_Errors.Padding = new System.Windows.Forms.Padding(7, 38, 7, 7);
			this.TLP_Errors.RowCount = 1;
			this.TLP_Errors.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Errors.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.TLP_Errors.Size = new System.Drawing.Size(743, 45);
			this.TLP_Errors.TabIndex = 2;
			this.TLP_Errors.Text = "ErrorsInLog";
			// 
			// I_Info
			// 
			this.I_Info.ActiveColor = null;
			this.I_Info.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.I_Info.ColorStyle = Extensions.ColorStyle.Icon;
			this.I_Info.Cursor = System.Windows.Forms.Cursors.Hand;
			this.I_Info.Enabled = false;
			this.I_Info.Image = global::LoadOrderToolTwo.Properties.Resources.I_Info;
			this.I_Info.Location = new System.Drawing.Point(361, 161);
			this.I_Info.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.I_Info.Name = "I_Info";
			this.I_Info.Selected = true;
			this.I_Info.Size = new System.Drawing.Size(32, 32);
			this.I_Info.TabIndex = 18;
			this.I_Info.TabStop = false;
			// 
			// L_Info
			// 
			this.L_Info.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.L_Info.AutoSize = true;
			this.L_Info.Location = new System.Drawing.Point(399, 166);
			this.L_Info.Margin = new System.Windows.Forms.Padding(3, 10, 10, 10);
			this.L_Info.Name = "L_Info";
			this.L_Info.Size = new System.Drawing.Size(55, 23);
			this.L_Info.TabIndex = 19;
			this.L_Info.Text = "label1";
			this.L_Info.UseMnemonic = false;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.TLP_Main);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(5, 30);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(1161, 702);
			this.panel1.TabIndex = 16;
			// 
			// B_ChangeLog
			// 
			this.B_ChangeLog.AutoSize = true;
			this.B_ChangeLog.ColorShade = null;
			this.B_ChangeLog.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_ChangeLog.Dock = System.Windows.Forms.DockStyle.Top;
			this.B_ChangeLog.Location = new System.Drawing.Point(10, 113);
			this.B_ChangeLog.Name = "B_ChangeLog";
			this.B_ChangeLog.Size = new System.Drawing.Size(325, 30);
			this.B_ChangeLog.SpaceTriggersClick = true;
			this.B_ChangeLog.TabIndex = 20;
			this.B_ChangeLog.Text = "OpenChangelog";
			this.B_ChangeLog.Click += new System.EventHandler(this.B_ChangeLog_Click);
			// 
			// PC_HelpAndLogs
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.slickScroll1);
			this.Controls.Add(this.panel1);
			this.Name = "PC_HelpAndLogs";
			this.Size = new System.Drawing.Size(1171, 737);
			this.Controls.SetChildIndex(this.base_Text, 0);
			this.Controls.SetChildIndex(this.panel1, 0);
			this.Controls.SetChildIndex(this.slickScroll1, 0);
			this.TLP_Main.ResumeLayout(false);
			this.TLP_Main.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.TLP_HelpLogs.ResumeLayout(false);
			this.TLP_HelpLogs.PerformLayout();
			this.TLP_LogFolders.ResumeLayout(false);
			this.TLP_LogFolders.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion
	private Generic.DragAndDropControl DD_LogFile;
	private SlickControls.SlickScroll slickScroll1;
	private System.Windows.Forms.TableLayoutPanel TLP_Main;
	private SlickControls.RoundedGroupTableLayoutPanel TLP_Errors;
	private SlickControls.RoundedGroupTableLayoutPanel TLP_HelpLogs;
	private SlickControls.SlickButton B_Discord;
	private SlickControls.SlickButton B_Guide;
	private System.Windows.Forms.Panel panel1;
	private SlickControls.SlickButton B_SaveZip;
	private SlickControls.SlickButton B_CopyZip;
	private SlickControls.SlickSpacer slickSpacer1;
	private SlickControls.SlickIcon I_Info;
	private System.Windows.Forms.Label L_Info;
	private SlickControls.RoundedGroupTableLayoutPanel TLP_LogFolders;
	private SlickControls.SlickButton B_LotLogCopy;
	private SlickControls.SlickButton B_LotLog;
	private SlickControls.SlickSpacer slickSpacer2;
	private SlickControls.SlickButton B_OpenLogFolder;
	private SlickControls.SlickButton B_CopyLogFile;
	private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	private SlickControls.SlickButton B_ChangeLog;
}
