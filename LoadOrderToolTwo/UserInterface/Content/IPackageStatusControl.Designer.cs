namespace LoadOrderToolTwo.UserInterface.Content;

partial class IPackageStatusControl<T>
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
			this.topIcon1 = new SlickControls.TopIcon();
			this.roundedTableLayoutPanel1 = new SlickControls.RoundedTableLayoutPanel();
			this.DD_Action = new LoadOrderToolTwo.UserInterface.Dropdowns.PackageActionDropDown();
			this.slickTextBox1 = new SlickControls.SlickTextBox();
			((System.ComponentModel.ISupportInitialize)(this.topIcon1)).BeginInit();
			this.roundedTableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// topIcon1
			// 
			this.topIcon1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.topIcon1.AnimatedValue = 0;
			this.topIcon1.Color = SlickControls.TopIcon.IconStyle.Close;
			this.topIcon1.Location = new System.Drawing.Point(367, 3);
			this.topIcon1.Name = "topIcon1";
			this.topIcon1.Size = new System.Drawing.Size(16, 16);
			this.topIcon1.TabIndex = 0;
			this.topIcon1.TabStop = false;
			// 
			// roundedTableLayoutPanel1
			// 
			this.roundedTableLayoutPanel1.AutoSize = true;
			this.roundedTableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.roundedTableLayoutPanel1.ColumnCount = 2;
			this.roundedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.roundedTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.roundedTableLayoutPanel1.Controls.Add(this.slickTextBox1, 0, 2);
			this.roundedTableLayoutPanel1.Controls.Add(this.topIcon1, 1, 0);
			this.roundedTableLayoutPanel1.Controls.Add(this.DD_Action, 0, 1);
			this.roundedTableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.roundedTableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.roundedTableLayoutPanel1.Name = "roundedTableLayoutPanel1";
			this.roundedTableLayoutPanel1.RowCount = 2;
			this.roundedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.roundedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.roundedTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.roundedTableLayoutPanel1.Size = new System.Drawing.Size(386, 229);
			this.roundedTableLayoutPanel1.TabIndex = 1;
			// 
			// DD_Action
			// 
			this.DD_Action.Cursor = System.Windows.Forms.Cursors.Hand;
			this.DD_Action.Location = new System.Drawing.Point(3, 25);
			this.DD_Action.Name = "DD_Action";
			this.DD_Action.Size = new System.Drawing.Size(225, 44);
			this.DD_Action.TabIndex = 1;
			this.DD_Action.Text = "Recommended Action";
			// 
			// slickTextBox1
			// 
			this.roundedTableLayoutPanel1.SetColumnSpan(this.slickTextBox1, 2);
			this.slickTextBox1.Dock = System.Windows.Forms.DockStyle.Top;
			this.slickTextBox1.LabelText = "Note";
			this.slickTextBox1.Location = new System.Drawing.Point(3, 75);
			this.slickTextBox1.MultiLine = true;
			this.slickTextBox1.Name = "slickTextBox1";
			this.slickTextBox1.Placeholder = "NoteInfo";
			this.slickTextBox1.SelectedText = "";
			this.slickTextBox1.SelectionLength = 0;
			this.slickTextBox1.SelectionStart = 0;
			this.slickTextBox1.Size = new System.Drawing.Size(380, 151);
			this.slickTextBox1.TabIndex = 19;
			// 
			// IPackageStatusControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Controls.Add(this.roundedTableLayoutPanel1);
			this.Name = "IPackageStatusControl";
			this.Size = new System.Drawing.Size(386, 179);
			((System.ComponentModel.ISupportInitialize)(this.topIcon1)).EndInit();
			this.roundedTableLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion
	private SlickControls.TopIcon topIcon1;
	private SlickControls.RoundedTableLayoutPanel roundedTableLayoutPanel1;
	private Dropdowns.PackageActionDropDown DD_Action;
	private SlickControls.SlickTextBox slickTextBox1;
}
