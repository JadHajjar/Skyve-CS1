namespace LoadOrderToolTwo.UserInterface.Content;

partial class IPackageStatusControl<T, TBase>
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
			this.CloseIcon = new SlickControls.TopIcon();
			this.P_Main = new SlickControls.RoundedTableLayoutPanel();
			this.TB_Note = new SlickControls.SlickTextBox();
			this.DD_Action = new LoadOrderToolTwo.UserInterface.Dropdowns.PackageActionDropDown();
			this.I_Note = new SlickControls.SlickIcon();
			this.label1 = new System.Windows.Forms.Label();
			this.I_AddPackage = new SlickControls.SlickIcon();
			this.slickSpacer1 = new SlickControls.SlickSpacer();
			this.P_Packages = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.CloseIcon)).BeginInit();
			this.P_Main.SuspendLayout();
			this.SuspendLayout();
			// 
			// CloseIcon
			// 
			this.CloseIcon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseIcon.AnimatedValue = 0;
			this.CloseIcon.Color = SlickControls.TopIcon.IconStyle.Close;
			this.CloseIcon.Cursor = System.Windows.Forms.Cursors.Hand;
			this.CloseIcon.Location = new System.Drawing.Point(367, 3);
			this.CloseIcon.Name = "CloseIcon";
			this.CloseIcon.Size = new System.Drawing.Size(16, 16);
			this.CloseIcon.TabIndex = 0;
			this.CloseIcon.TabStop = false;
			this.CloseIcon.Click += new System.EventHandler(this.CloseIcon_Click);
			// 
			// P_Main
			// 
			this.P_Main.AutoSize = true;
			this.P_Main.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.P_Main.ColumnCount = 2;
			this.P_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.P_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.P_Main.Controls.Add(this.TB_Note, 0, 2);
			this.P_Main.Controls.Add(this.CloseIcon, 1, 0);
			this.P_Main.Controls.Add(this.DD_Action, 0, 1);
			this.P_Main.Controls.Add(this.I_Note, 1, 1);
			this.P_Main.Controls.Add(this.label1, 0, 4);
			this.P_Main.Controls.Add(this.I_AddPackage, 1, 4);
			this.P_Main.Controls.Add(this.slickSpacer1, 0, 3);
			this.P_Main.Controls.Add(this.P_Packages, 0, 5);
			this.P_Main.Dock = System.Windows.Forms.DockStyle.Top;
			this.P_Main.Location = new System.Drawing.Point(0, 0);
			this.P_Main.Name = "P_Main";
			this.P_Main.RowCount = 6;
			this.P_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.P_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.P_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.P_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.P_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.P_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.P_Main.Size = new System.Drawing.Size(386, 286);
			this.P_Main.TabIndex = 1;
			// 
			// TB_Note
			// 
			this.P_Main.SetColumnSpan(this.TB_Note, 2);
			this.TB_Note.Dock = System.Windows.Forms.DockStyle.Top;
			this.TB_Note.LabelText = "Note";
			this.TB_Note.Location = new System.Drawing.Point(3, 75);
			this.TB_Note.MultiLine = true;
			this.TB_Note.Name = "TB_Note";
			this.TB_Note.Placeholder = "NoteInfo";
			this.TB_Note.SelectedText = "";
			this.TB_Note.SelectionLength = 0;
			this.TB_Note.SelectionStart = 0;
			this.TB_Note.Size = new System.Drawing.Size(380, 151);
			this.TB_Note.TabIndex = 19;
			this.TB_Note.Visible = false;
			// 
			// DD_Action
			// 
			this.DD_Action.Cursor = System.Windows.Forms.Cursors.Hand;
			this.DD_Action.Dock = System.Windows.Forms.DockStyle.Top;
			this.DD_Action.Location = new System.Drawing.Point(3, 25);
			this.DD_Action.Name = "DD_Action";
			this.DD_Action.Size = new System.Drawing.Size(300, 44);
			this.DD_Action.TabIndex = 1;
			this.DD_Action.Text = "Recommended Action";
			// 
			// I_Note
			// 
			this.I_Note.ActiveColor = null;
			this.I_Note.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.I_Note.Cursor = System.Windows.Forms.Cursors.Hand;
			dynamicIcon1.Name = "I_Note";
			this.I_Note.ImageName = dynamicIcon1;
			this.I_Note.Location = new System.Drawing.Point(309, 25);
			this.I_Note.Name = "I_Note";
			this.I_Note.Size = new System.Drawing.Size(74, 44);
			this.I_Note.TabIndex = 20;
			this.I_Note.Click += new System.EventHandler(this.I_Note_Click);
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 254);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(35, 13);
			this.label1.TabIndex = 21;
			this.label1.Text = "label1";
			// 
			// I_AddPackage
			// 
			this.I_AddPackage.ActiveColor = null;
			this.I_AddPackage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.I_AddPackage.Cursor = System.Windows.Forms.Cursors.Hand;
			dynamicIcon2.Name = "I_Add";
			this.I_AddPackage.ImageName = dynamicIcon2;
			this.I_AddPackage.Location = new System.Drawing.Point(309, 239);
			this.I_AddPackage.Name = "I_AddPackage";
			this.I_AddPackage.Size = new System.Drawing.Size(74, 44);
			this.I_AddPackage.TabIndex = 20;
			this.I_AddPackage.Click += new System.EventHandler(this.I_AddPackage_Click);
			// 
			// slickSpacer1
			// 
			this.P_Main.SetColumnSpan(this.slickSpacer1, 2);
			this.slickSpacer1.Dock = System.Windows.Forms.DockStyle.Top;
			this.slickSpacer1.Location = new System.Drawing.Point(3, 232);
			this.slickSpacer1.Name = "slickSpacer1";
			this.slickSpacer1.Size = new System.Drawing.Size(380, 1);
			this.slickSpacer1.TabIndex = 22;
			this.slickSpacer1.TabStop = false;
			this.slickSpacer1.Text = "slickSpacer1";
			// 
			// P_Packages
			// 
			this.P_Packages.AutoSize = true;
			this.P_Packages.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.P_Main.SetColumnSpan(this.P_Packages, 2);
			this.P_Packages.Dock = System.Windows.Forms.DockStyle.Top;
			this.P_Packages.Location = new System.Drawing.Point(0, 286);
			this.P_Packages.Margin = new System.Windows.Forms.Padding(0);
			this.P_Packages.Name = "P_Packages";
			this.P_Packages.Size = new System.Drawing.Size(386, 0);
			this.P_Packages.TabIndex = 23;
			// 
			// IPackageStatusControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Controls.Add(this.P_Main);
			this.Name = "IPackageStatusControl";
			this.Size = new System.Drawing.Size(386, 340);
			((System.ComponentModel.ISupportInitialize)(this.CloseIcon)).EndInit();
			this.P_Main.ResumeLayout(false);
			this.P_Main.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion
	private SlickControls.TopIcon CloseIcon;
	private SlickControls.RoundedTableLayoutPanel P_Main;
	private Dropdowns.PackageActionDropDown DD_Action;
	private SlickControls.SlickTextBox TB_Note;
	private SlickControls.SlickIcon I_Note;
	private SlickControls.SlickIcon I_AddPackage;
	private System.Windows.Forms.Label label1;
	private SlickControls.SlickSpacer slickSpacer1;
	private System.Windows.Forms.Panel P_Packages;
}
