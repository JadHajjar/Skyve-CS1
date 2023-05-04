namespace LoadOrderToolTwo.UserInterface.Forms;

partial class RatingInfoForm
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
			SlickControls.DynamicIcon dynamicIcon1 = new SlickControls.DynamicIcon();
			SlickControls.DynamicIcon dynamicIcon2 = new SlickControls.DynamicIcon();
			this.TLP_Main = new System.Windows.Forms.TableLayoutPanel();
			this.slickIcon1 = new SlickControls.SlickIcon();
			this.B_Ok = new SlickControls.SlickButton();
			this.L_Title = new System.Windows.Forms.Label();
			this.L_1 = new System.Windows.Forms.Label();
			this.L_2 = new System.Windows.Forms.Label();
			this.L_3 = new System.Windows.Forms.Label();
			this.L_4 = new System.Windows.Forms.Label();
			this.PB_1 = new SlickControls.SlickPictureBox();
			this.PB_2 = new SlickControls.SlickPictureBox();
			this.PB_3 = new SlickControls.SlickPictureBox();
			this.PB_4 = new SlickControls.SlickPictureBox();
			this.base_P_Container.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.base_PB_Icon)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.base_B_Close)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.base_B_Max)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.base_B_Min)).BeginInit();
			this.TLP_Main.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PB_1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PB_2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PB_3)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PB_4)).BeginInit();
			this.SuspendLayout();
			// 
			// base_P_Container
			// 
			this.base_P_Container.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(203)))), ((int)(((byte)(145)))));
			this.base_P_Container.Controls.Add(this.TLP_Main);
			this.base_P_Container.Size = new System.Drawing.Size(608, 405);
			// 
			// TLP_Main
			// 
			this.TLP_Main.ColumnCount = 2;
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TLP_Main.Controls.Add(this.L_1, 1, 2);
			this.TLP_Main.Controls.Add(this.slickIcon1, 0, 1);
			this.TLP_Main.Controls.Add(this.B_Ok, 1, 7);
			this.TLP_Main.Controls.Add(this.L_Title, 1, 1);
			this.TLP_Main.Controls.Add(this.L_2, 1, 3);
			this.TLP_Main.Controls.Add(this.L_3, 1, 4);
			this.TLP_Main.Controls.Add(this.L_4, 1, 5);
			this.TLP_Main.Controls.Add(this.PB_1, 0, 2);
			this.TLP_Main.Controls.Add(this.PB_2, 0, 3);
			this.TLP_Main.Controls.Add(this.PB_3, 0, 4);
			this.TLP_Main.Controls.Add(this.PB_4, 0, 5);
			this.TLP_Main.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TLP_Main.Location = new System.Drawing.Point(1, 1);
			this.TLP_Main.Name = "TLP_Main";
			this.TLP_Main.RowCount = 8;
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Main.Size = new System.Drawing.Size(606, 403);
			this.TLP_Main.TabIndex = 0;
			// 
			// slickIcon1
			// 
			this.slickIcon1.ActiveColor = null;
			this.slickIcon1.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.slickIcon1.Cursor = System.Windows.Forms.Cursors.Hand;
			this.slickIcon1.Enabled = false;
			dynamicIcon1.Name = "I_VoteFilled";
			this.slickIcon1.ImageName = dynamicIcon1;
			this.slickIcon1.Location = new System.Drawing.Point(3, 34);
			this.slickIcon1.Name = "slickIcon1";
			this.slickIcon1.Selected = true;
			this.slickIcon1.Size = new System.Drawing.Size(150, 150);
			this.slickIcon1.TabIndex = 0;
			// 
			// B_Ok
			// 
			this.B_Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.B_Ok.ColorShade = null;
			this.B_Ok.Cursor = System.Windows.Forms.Cursors.Hand;
			dynamicIcon2.Name = "I_Ok";
			this.B_Ok.ImageName = dynamicIcon2;
			this.B_Ok.LargeImage = true;
			this.B_Ok.Location = new System.Drawing.Point(503, 370);
			this.B_Ok.Name = "B_Ok";
			this.B_Ok.Size = new System.Drawing.Size(100, 30);
			this.B_Ok.SpaceTriggersClick = true;
			this.B_Ok.TabIndex = 2;
			this.B_Ok.Text = "Ok";
			this.B_Ok.Click += new System.EventHandler(this.B_Ok_Click);
			// 
			// L_Title
			// 
			this.L_Title.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.L_Title.AutoSize = true;
			this.L_Title.Location = new System.Drawing.Point(159, 102);
			this.L_Title.Name = "L_Title";
			this.L_Title.Size = new System.Drawing.Size(38, 13);
			this.L_Title.TabIndex = 3;
			this.L_Title.Text = "label1";
			this.L_Title.UseMnemonic = false;
			// 
			// L_1
			// 
			this.L_1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.L_1.AutoSize = true;
			this.L_1.Location = new System.Drawing.Point(159, 199);
			this.L_1.Name = "L_1";
			this.L_1.Size = new System.Drawing.Size(38, 13);
			this.L_1.TabIndex = 3;
			this.L_1.Text = "label1";
			this.L_1.UseMnemonic = false;
			// 
			// L_2
			// 
			this.L_2.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.L_2.AutoSize = true;
			this.L_2.Location = new System.Drawing.Point(159, 236);
			this.L_2.Name = "L_2";
			this.L_2.Size = new System.Drawing.Size(38, 13);
			this.L_2.TabIndex = 3;
			this.L_2.Text = "label1";
			this.L_2.UseMnemonic = false;
			// 
			// L_3
			// 
			this.L_3.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.L_3.AutoSize = true;
			this.L_3.Location = new System.Drawing.Point(159, 273);
			this.L_3.Name = "L_3";
			this.L_3.Size = new System.Drawing.Size(38, 13);
			this.L_3.TabIndex = 3;
			this.L_3.Text = "label1";
			this.L_3.UseMnemonic = false;
			// 
			// L_4
			// 
			this.L_4.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.L_4.AutoSize = true;
			this.L_4.Location = new System.Drawing.Point(159, 310);
			this.L_4.Name = "L_4";
			this.L_4.Size = new System.Drawing.Size(38, 13);
			this.L_4.TabIndex = 3;
			this.L_4.Text = "label1";
			this.L_4.UseMnemonic = false;
			// 
			// PB_1
			// 
			this.PB_1.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.PB_1.Location = new System.Drawing.Point(53, 190);
			this.PB_1.Name = "PB_1";
			this.PB_1.Size = new System.Drawing.Size(100, 31);
			this.PB_1.TabIndex = 4;
			this.PB_1.TabStop = false;
			this.PB_1.Paint += new System.Windows.Forms.PaintEventHandler(this.PB_1_Paint);
			// 
			// PB_2
			// 
			this.PB_2.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.PB_2.Location = new System.Drawing.Point(53, 227);
			this.PB_2.Name = "PB_2";
			this.PB_2.Size = new System.Drawing.Size(100, 31);
			this.PB_2.TabIndex = 4;
			this.PB_2.TabStop = false;
			this.PB_2.Paint += new System.Windows.Forms.PaintEventHandler(this.PB_2_Paint);
			// 
			// PB_3
			// 
			this.PB_3.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.PB_3.Location = new System.Drawing.Point(53, 264);
			this.PB_3.Name = "PB_3";
			this.PB_3.Size = new System.Drawing.Size(100, 31);
			this.PB_3.TabIndex = 4;
			this.PB_3.TabStop = false;
			this.PB_3.Paint += new System.Windows.Forms.PaintEventHandler(this.PB_3_Paint);
			// 
			// PB_4
			// 
			this.PB_4.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.PB_4.Location = new System.Drawing.Point(53, 301);
			this.PB_4.Name = "PB_4";
			this.PB_4.Size = new System.Drawing.Size(100, 31);
			this.PB_4.TabIndex = 4;
			this.PB_4.TabStop = false;
			this.PB_4.Paint += new System.Windows.Forms.PaintEventHandler(this.PB_4_Paint);
			// 
			// RatingInfoForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(619, 416);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(58)))), ((int)(((byte)(69)))));
			this.MaximizedBounds = new System.Drawing.Rectangle(0, 0, 1920, 1032);
			this.Name = "RatingInfoForm";
			this.Text = "RatingInfoForm";
			this.base_P_Container.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.base_PB_Icon)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.base_B_Close)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.base_B_Max)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.base_B_Min)).EndInit();
			this.TLP_Main.ResumeLayout(false);
			this.TLP_Main.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PB_1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PB_2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PB_3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PB_4)).EndInit();
			this.ResumeLayout(false);

	}

	#endregion

	private System.Windows.Forms.TableLayoutPanel TLP_Main;
	private SlickControls.SlickIcon slickIcon1;
	private SlickControls.SlickButton B_Ok;
	private System.Windows.Forms.Label L_Title;
	private System.Windows.Forms.Label L_1;
	private System.Windows.Forms.Label L_2;
	private System.Windows.Forms.Label L_3;
	private System.Windows.Forms.Label L_4;
	private SlickControls.SlickPictureBox PB_1;
	private SlickControls.SlickPictureBox PB_2;
	private SlickControls.SlickPictureBox PB_3;
	private SlickControls.SlickPictureBox PB_4;
}