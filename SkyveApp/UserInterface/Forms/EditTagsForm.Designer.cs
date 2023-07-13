namespace SkyveApp.UserInterface.Forms;

partial class EditTagsForm
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

	#region Windows Form Designer generated code

	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
			SlickControls.DynamicIcon dynamicIcon5 = new SlickControls.DynamicIcon();
			SlickControls.DynamicIcon dynamicIcon6 = new SlickControls.DynamicIcon();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.B_Apply = new SlickControls.SlickButton();
			this.FLP_Tags = new SlickControls.SmartFlowPanel();
			this.addTagControl = new SkyveApp.UserInterface.Content.TagControl();
			this.panel1 = new System.Windows.Forms.Panel();
			this.smartFlowPanel1 = new SlickControls.SmartFlowPanel();
			this.slickScroll1 = new SlickControls.SlickScroll();
			this.base_P_Content.SuspendLayout();
			this.base_P_Container.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.base_PB_Icon)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.FLP_Tags.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// base_P_Content
			// 
			this.base_P_Content.AutoScroll = true;
			this.base_P_Content.Controls.Add(this.tableLayoutPanel1);
			this.base_P_Content.Location = new System.Drawing.Point(1, 43);
			this.base_P_Content.Size = new System.Drawing.Size(477, 554);
			// 
			// base_P_Controls
			// 
			this.base_P_Controls.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(235)))), ((int)(((byte)(243)))));
			this.base_P_Controls.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(58)))), ((int)(((byte)(69)))));
			this.base_P_Controls.Location = new System.Drawing.Point(1, 43);
			// 
			// base_P_Top_Spacer
			// 
			this.base_P_Top_Spacer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(235)))), ((int)(((byte)(243)))));
			this.base_P_Top_Spacer.Size = new System.Drawing.Size(477, 6);
			// 
			// base_P_Container
			// 
			this.base_P_Container.Size = new System.Drawing.Size(479, 598);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.B_Apply, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.FLP_Tags, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(477, 554);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// B_Apply
			// 
			this.B_Apply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.B_Apply.ColorShade = null;
			this.B_Apply.Cursor = System.Windows.Forms.Cursors.Hand;
			dynamicIcon5.Name = "I_Ok";
			this.B_Apply.ImageName = dynamicIcon5;
			this.B_Apply.Location = new System.Drawing.Point(355, 521);
			this.B_Apply.Name = "B_Apply";
			this.B_Apply.Size = new System.Drawing.Size(119, 30);
			this.B_Apply.SpaceTriggersClick = true;
			this.B_Apply.TabIndex = 100;
			this.B_Apply.Text = "Apply";
			this.B_Apply.Click += new System.EventHandler(this.B_Apply_Click);
			// 
			// FLP_Tags
			// 
			this.FLP_Tags.Controls.Add(this.addTagControl);
			this.FLP_Tags.Dock = System.Windows.Forms.DockStyle.Top;
			this.FLP_Tags.Location = new System.Drawing.Point(3, 3);
			this.FLP_Tags.Name = "FLP_Tags";
			this.FLP_Tags.Size = new System.Drawing.Size(471, 48);
			this.FLP_Tags.TabIndex = 0;
			// 
			// addTagControl
			// 
			this.addTagControl.Display = false;
			dynamicIcon6.Name = "I_Add";
			this.addTagControl.ImageName = dynamicIcon6;
			this.addTagControl.Location = new System.Drawing.Point(3, 3);
			this.addTagControl.Name = "addTagControl";
			this.addTagControl.Size = new System.Drawing.Size(104, 42);
			this.addTagControl.TabIndex = 0;
			this.addTagControl.TagInfo = null;
			this.addTagControl.Click += new System.EventHandler(this.B_AddLink_Click);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.slickScroll1);
			this.panel1.Controls.Add(this.smartFlowPanel1);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(3, 57);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(471, 458);
			this.panel1.TabIndex = 101;
			// 
			// smartFlowPanel1
			// 
			this.smartFlowPanel1.Location = new System.Drawing.Point(0, 0);
			this.smartFlowPanel1.Name = "smartFlowPanel1";
			this.smartFlowPanel1.Size = new System.Drawing.Size(200, 0);
			this.smartFlowPanel1.TabIndex = 0;
			// 
			// slickScroll1
			// 
			this.slickScroll1.Dock = System.Windows.Forms.DockStyle.Right;
			this.slickScroll1.LinkedControl = this.smartFlowPanel1;
			this.slickScroll1.Location = new System.Drawing.Point(459, 0);
			this.slickScroll1.Name = "slickScroll1";
			this.slickScroll1.Size = new System.Drawing.Size(12, 458);
			this.slickScroll1.Style = SlickControls.StyleType.Vertical;
			this.slickScroll1.TabIndex = 1;
			this.slickScroll1.TabStop = false;
			this.slickScroll1.Text = "slickScroll1";
			// 
			// EditTagsForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(490, 609);
			this.MaximizedBounds = new System.Drawing.Rectangle(0, 0, 2560, 1380);
			this.Name = "EditTagsForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "AddLinkForm";
			this.base_P_Content.ResumeLayout(false);
			this.base_P_Container.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.base_PB_Icon)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.FLP_Tags.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

	}

	#endregion
	private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	private SlickControls.SlickButton B_Apply;
	private SlickControls.SmartFlowPanel FLP_Tags;
	private Content.TagControl addTagControl;
	private System.Windows.Forms.Panel panel1;
	private SmartFlowPanel smartFlowPanel1;
	private SlickScroll slickScroll1;
}