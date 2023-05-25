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
			SlickControls.DynamicIcon dynamicIcon1 = new SlickControls.DynamicIcon();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.TLP_Info = new SlickControls.RoundedGroupTableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// base_Text
			// 
			this.base_Text.Text = "Review Request Info";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 149F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.TLP_Info, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.label1, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.label2, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.label3, 1, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(5, 30);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 5;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(773, 403);
			this.tableLayoutPanel1.TabIndex = 2;
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
			dynamicIcon1.Name = "I_Info";
			this.TLP_Info.ImageName = dynamicIcon1;
			this.TLP_Info.Location = new System.Drawing.Point(3, 3);
			this.TLP_Info.Name = "TLP_Info";
			this.TLP_Info.Padding = new System.Windows.Forms.Padding(9, 54, 9, 9);
			this.TLP_Info.RowCount = 3;
			this.tableLayoutPanel1.SetRowSpan(this.TLP_Info, 5);
			this.TLP_Info.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Info.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Info.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Info.Size = new System.Drawing.Size(143, 63);
			this.TLP_Info.TabIndex = 0;
			this.TLP_Info.Text = "Info";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(152, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(122, 30);
			this.label1.TabIndex = 1;
			this.label1.Text = "Description";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(152, 30);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(71, 30);
			this.label2.TabIndex = 1;
			this.label2.Text = "label1";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(152, 60);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(195, 30);
			this.label3.TabIndex = 1;
			this.label3.Text = "Proposed Changes";
			// 
			// PC_ReviewSingleRequest
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "PC_ReviewSingleRequest";
			this.Text = "Review Request Info";
			this.Controls.SetChildIndex(this.base_Text, 0);
			this.Controls.SetChildIndex(this.tableLayoutPanel1, 0);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	private SlickControls.RoundedGroupTableLayoutPanel TLP_Info;
	private System.Windows.Forms.Label label1;
	private System.Windows.Forms.Label label2;
	private System.Windows.Forms.Label label3;
}
