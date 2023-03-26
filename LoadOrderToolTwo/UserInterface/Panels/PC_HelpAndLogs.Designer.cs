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
			this.slickButton1 = new SlickControls.SlickButton();
			this.SuspendLayout();
			// 
			// slickButton1
			// 
			this.slickButton1.ColorShade = null;
			this.slickButton1.Cursor = System.Windows.Forms.Cursors.Hand;
			this.slickButton1.Location = new System.Drawing.Point(24, 51);
			this.slickButton1.Name = "slickButton1";
			this.slickButton1.Size = new System.Drawing.Size(184, 38);
			this.slickButton1.SpaceTriggersClick = true;
			this.slickButton1.TabIndex = 13;
			this.slickButton1.Text = "Open Log Folder";
			this.slickButton1.Click += new System.EventHandler(this.slickButton1_Click);
			// 
			// PC_HelpAndLogs
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.slickButton1);
			this.Name = "PC_HelpAndLogs";
			this.Controls.SetChildIndex(this.base_Text, 0);
			this.Controls.SetChildIndex(this.slickButton1, 0);
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	private SlickControls.SlickButton slickButton1;
}
