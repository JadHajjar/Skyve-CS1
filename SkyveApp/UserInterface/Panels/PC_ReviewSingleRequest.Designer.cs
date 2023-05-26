namespace SkyveApp.UserInterface.Panels;

partial class PC_ReviewSingleRequest
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
			SlickControls.DynamicIcon dynamicIcon5 = new SlickControls.DynamicIcon();
			SlickControls.DynamicIcon dynamicIcon4 = new SlickControls.DynamicIcon();
			SlickControls.DynamicIcon dynamicIcon3 = new SlickControls.DynamicIcon();
			SlickControls.DynamicIcon dynamicIcon2 = new SlickControls.DynamicIcon();
			SlickControls.DynamicIcon dynamicIcon1 = new SlickControls.DynamicIcon();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.TLP_Info = new SlickControls.RoundedGroupTableLayoutPanel();
			this.roundedGroupTableLayoutPanel1 = new SlickControls.RoundedGroupTableLayoutPanel();
			this.B_ManagePackage = new SlickControls.SlickButton();
			this.B_ApplyChanges = new SlickControls.SlickButton();
			this.B_DeleteRequest = new SlickControls.SlickButton();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.roundedGroupTableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// base_Text
			// 
			this.base_Text.Text = "Review Request Info";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.label3, 0, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(205, 30);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 6;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(573, 403);
			this.tableLayoutPanel1.TabIndex = 2;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(108, 25);
			this.label1.TabIndex = 1;
			this.label1.Text = "Description";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(3, 25);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(63, 25);
			this.label2.TabIndex = 1;
			this.label2.Text = "label1";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(3, 50);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(170, 25);
			this.label3.TabIndex = 1;
			this.label3.Text = "Proposed Changes";
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 1;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Controls.Add(this.roundedGroupTableLayoutPanel1, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.TLP_Info, 0, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Left;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(5, 30);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 2;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.Size = new System.Drawing.Size(200, 403);
			this.tableLayoutPanel2.TabIndex = 3;
			// 
			// TLP_Info
			// 
			this.TLP_Info.AddOutline = true;
			this.TLP_Info.AutoSize = true;
			this.TLP_Info.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.TLP_Info.ColumnCount = 1;
			this.TLP_Info.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TLP_Info.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.TLP_Info.Dock = System.Windows.Forms.DockStyle.Top;
			dynamicIcon5.Name = "I_Info";
			this.TLP_Info.ImageName = dynamicIcon5;
			this.TLP_Info.Location = new System.Drawing.Point(3, 3);
			this.TLP_Info.Name = "TLP_Info";
			this.TLP_Info.Padding = new System.Windows.Forms.Padding(8, 48, 8, 8);
			this.TLP_Info.RowCount = 3;
			this.TLP_Info.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Info.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Info.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Info.Size = new System.Drawing.Size(194, 56);
			this.TLP_Info.TabIndex = 1;
			this.TLP_Info.Text = "Info";
			// 
			// roundedGroupTableLayoutPanel1
			// 
			this.roundedGroupTableLayoutPanel1.AddOutline = true;
			this.roundedGroupTableLayoutPanel1.AutoSize = true;
			this.roundedGroupTableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.roundedGroupTableLayoutPanel1.ColumnCount = 1;
			this.roundedGroupTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.roundedGroupTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.roundedGroupTableLayoutPanel1.Controls.Add(this.B_DeleteRequest, 0, 2);
			this.roundedGroupTableLayoutPanel1.Controls.Add(this.B_ApplyChanges, 0, 1);
			this.roundedGroupTableLayoutPanel1.Controls.Add(this.B_ManagePackage, 0, 0);
			this.roundedGroupTableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			dynamicIcon4.Name = "I_Actions";
			this.roundedGroupTableLayoutPanel1.ImageName = dynamicIcon4;
			this.roundedGroupTableLayoutPanel1.Location = new System.Drawing.Point(3, 65);
			this.roundedGroupTableLayoutPanel1.Name = "roundedGroupTableLayoutPanel1";
			this.roundedGroupTableLayoutPanel1.Padding = new System.Windows.Forms.Padding(8, 48, 8, 8);
			this.roundedGroupTableLayoutPanel1.RowCount = 3;
			this.roundedGroupTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.roundedGroupTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.roundedGroupTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.roundedGroupTableLayoutPanel1.Size = new System.Drawing.Size(194, 164);
			this.roundedGroupTableLayoutPanel1.TabIndex = 2;
			this.roundedGroupTableLayoutPanel1.Text = "Actions";
			// 
			// B_ManagePackage
			// 
			this.B_ManagePackage.ColorShade = null;
			this.B_ManagePackage.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_ManagePackage.Dock = System.Windows.Forms.DockStyle.Top;
			dynamicIcon3.Name = "I_Link";
			this.B_ManagePackage.ImageName = dynamicIcon3;
			this.B_ManagePackage.Location = new System.Drawing.Point(11, 51);
			this.B_ManagePackage.Name = "B_ManagePackage";
			this.B_ManagePackage.Size = new System.Drawing.Size(172, 30);
			this.B_ManagePackage.SpaceTriggersClick = true;
			this.B_ManagePackage.TabIndex = 0;
			this.B_ManagePackage.Text = "ManagePackage";
			// 
			// B_ApplyChanges
			// 
			this.B_ApplyChanges.ColorShade = null;
			this.B_ApplyChanges.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_ApplyChanges.Dock = System.Windows.Forms.DockStyle.Top;
			dynamicIcon2.Name = "I_Link";
			this.B_ApplyChanges.ImageName = dynamicIcon2;
			this.B_ApplyChanges.Location = new System.Drawing.Point(11, 87);
			this.B_ApplyChanges.Name = "B_ApplyChanges";
			this.B_ApplyChanges.Size = new System.Drawing.Size(172, 30);
			this.B_ApplyChanges.SpaceTriggersClick = true;
			this.B_ApplyChanges.TabIndex = 1;
			this.B_ApplyChanges.Text = "ApplyRequestedChanges";
			// 
			// B_DeleteRequest
			// 
			this.B_DeleteRequest.ColorShade = null;
			this.B_DeleteRequest.ColorStyle = Extensions.ColorStyle.Red;
			this.B_DeleteRequest.Cursor = System.Windows.Forms.Cursors.Hand;
			this.B_DeleteRequest.Dock = System.Windows.Forms.DockStyle.Top;
			dynamicIcon1.Name = "I_Disposable";
			this.B_DeleteRequest.ImageName = dynamicIcon1;
			this.B_DeleteRequest.Location = new System.Drawing.Point(11, 123);
			this.B_DeleteRequest.Name = "B_DeleteRequest";
			this.B_DeleteRequest.Size = new System.Drawing.Size(172, 30);
			this.B_DeleteRequest.SpaceTriggersClick = true;
			this.B_DeleteRequest.TabIndex = 2;
			this.B_DeleteRequest.Text = "DeleteRequest";
			// 
			// PC_ReviewSingleRequest
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this.tableLayoutPanel2);
			this.Name = "PC_ReviewSingleRequest";
			this.Text = "Review Request Info";
			this.Controls.SetChildIndex(this.base_Text, 0);
			this.Controls.SetChildIndex(this.tableLayoutPanel2, 0);
			this.Controls.SetChildIndex(this.tableLayoutPanel1, 0);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.roundedGroupTableLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	private System.Windows.Forms.Label label1;
	private System.Windows.Forms.Label label2;
	private System.Windows.Forms.Label label3;
	private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
	private SlickControls.RoundedGroupTableLayoutPanel roundedGroupTableLayoutPanel1;
	private SlickControls.RoundedGroupTableLayoutPanel TLP_Info;
	private SlickControls.SlickButton B_DeleteRequest;
	private SlickControls.SlickButton B_ApplyChanges;
	private SlickControls.SlickButton B_ManagePackage;
}
