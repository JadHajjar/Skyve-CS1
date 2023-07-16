using SkyveApp.UserInterface.Generic;


namespace SkyveApp.UserInterface.Panels;

partial class PC_Troubleshoot
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PC_Troubleshoot));
			this.TLP_New = new System.Windows.Forms.TableLayoutPanel();
			this.bigSelectionOptionControl2 = new SkyveApp.UserInterface.Generic.BigSelectionOptionControl();
			this.bigSelectionOptionControl1 = new SkyveApp.UserInterface.Generic.BigSelectionOptionControl();
			this.B_Cancel = new SlickControls.SlickButton();
			this.L_Title = new System.Windows.Forms.Label();
			this.TLP_New.SuspendLayout();
			this.SuspendLayout();
			// 
			// base_Text
			// 
			this.base_Text.Text = "TroubleshootIssues";
			// 
			// TLP_New
			// 
			this.TLP_New.ColumnCount = 3;
			this.TLP_New.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TLP_New.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_New.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TLP_New.Controls.Add(this.bigSelectionOptionControl2, 1, 1);
			this.TLP_New.Controls.Add(this.bigSelectionOptionControl1, 1, 2);
			this.TLP_New.Controls.Add(this.B_Cancel, 2, 4);
			this.TLP_New.Controls.Add(this.L_Title, 0, 0);
			this.TLP_New.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TLP_New.Location = new System.Drawing.Point(0, 30);
			this.TLP_New.Name = "TLP_New";
			this.TLP_New.RowCount = 5;
			this.TLP_New.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 35F));
			this.TLP_New.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_New.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_New.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_New.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 65F));
			this.TLP_New.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.TLP_New.Size = new System.Drawing.Size(1182, 789);
			this.TLP_New.TabIndex = 0;
			// 
			// bigSelectionOptionControl2
			// 
			this.bigSelectionOptionControl2.Cursor = System.Windows.Forms.Cursors.Hand;
			this.bigSelectionOptionControl2.FromScratch = false;
			dynamicIcon1.Name = "I_Wrench";
			this.bigSelectionOptionControl2.ImageName = dynamicIcon1;
			this.bigSelectionOptionControl2.Location = new System.Drawing.Point(516, 169);
			this.bigSelectionOptionControl2.Name = "bigSelectionOptionControl2";
			this.bigSelectionOptionControl2.Size = new System.Drawing.Size(150, 150);
			this.bigSelectionOptionControl2.TabIndex = 4;
			this.bigSelectionOptionControl2.Text = "TroubleshootCaused";
			// 
			// bigSelectionOptionControl1
			// 
			this.bigSelectionOptionControl1.Cursor = System.Windows.Forms.Cursors.Hand;
			this.bigSelectionOptionControl1.FromScratch = false;
			dynamicIcon2.Name = "I_Search";
			this.bigSelectionOptionControl1.ImageName = dynamicIcon2;
			this.bigSelectionOptionControl1.Location = new System.Drawing.Point(516, 325);
			this.bigSelectionOptionControl1.Name = "bigSelectionOptionControl1";
			this.bigSelectionOptionControl1.Size = new System.Drawing.Size(150, 150);
			this.bigSelectionOptionControl1.TabIndex = 5;
			this.bigSelectionOptionControl1.Text = "TroubleshootMissing";
			// 
			// B_Cancel
			// 
			this.B_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.B_Cancel.AutoSize = true;
			this.B_Cancel.ColorShade = null;
			this.B_Cancel.ColorStyle = Extensions.ColorStyle.Red;
			this.B_Cancel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_Cancel.Image = ((System.Drawing.Image)(resources.GetObject("B_Cancel.Image")));
			this.B_Cancel.Location = new System.Drawing.Point(1038, 733);
			this.B_Cancel.Margin = new System.Windows.Forms.Padding(10);
			this.B_Cancel.Name = "B_Cancel";
			this.B_Cancel.Padding = new System.Windows.Forms.Padding(10, 15, 10, 15);
			this.B_Cancel.Size = new System.Drawing.Size(134, 46);
			this.B_Cancel.SpaceTriggersClick = true;
			this.B_Cancel.TabIndex = 14;
			this.B_Cancel.Text = "Cancel";
			this.B_Cancel.Click += new System.EventHandler(this.B_Cancel_Click);
			// 
			// label1
			// 
			this.L_Title.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.L_Title.AutoSize = true;
			this.TLP_New.SetColumnSpan(this.L_Title, 3);
			this.L_Title.Location = new System.Drawing.Point(555, 68);
			this.L_Title.Name = "label1";
			this.L_Title.Size = new System.Drawing.Size(71, 30);
			this.L_Title.TabIndex = 15;
			this.L_Title.Text = "label1";
			// 
			// PC_Troubleshoot
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.TLP_New);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(58)))), ((int)(((byte)(69)))));
			this.Name = "PC_Troubleshoot";
			this.Padding = new System.Windows.Forms.Padding(0, 30, 0, 0);
			this.Size = new System.Drawing.Size(1182, 819);
			this.Text = "TroubleshootIssues";
			this.Controls.SetChildIndex(this.TLP_New, 0);
			this.Controls.SetChildIndex(this.base_Text, 0);
			this.TLP_New.ResumeLayout(false);
			this.TLP_New.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion
	private System.Windows.Forms.TableLayoutPanel TLP_New;
	private SlickControls.SlickButton B_Cancel;
	private BigSelectionOptionControl bigSelectionOptionControl2;
	private BigSelectionOptionControl bigSelectionOptionControl1;
	private System.Windows.Forms.Label L_Title;
}
