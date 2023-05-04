using LoadOrderToolTwo.UserInterface.Dropdowns;
using LoadOrderToolTwo.UserInterface.Generic;
using LoadOrderToolTwo.Utilities.Managers;

namespace LoadOrderToolTwo.UserInterface.Panels;

partial class PC_ProfileList
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
			this.TLP_Main = new System.Windows.Forms.TableLayoutPanel();
			this.T_ProfileUsage = new LoadOrderToolTwo.UserInterface.Generic.ThreeOptionToggle();
			this.slickSpacer2 = new SlickControls.SlickSpacer();
			this.slickSpacer1 = new SlickControls.SlickSpacer();
			this.TB_Search = new SlickControls.SlickTextBox();
			this.DD_Sorting = new LoadOrderToolTwo.UserInterface.Dropdowns.ProfileSortingDropDown();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.L_Counts = new System.Windows.Forms.Label();
			this.L_FilterCount = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.slickScroll1 = new SlickControls.SlickScroll();
			this.FLP_Profiles = new System.Windows.Forms.FlowLayoutPanel();
			this.TLP_Main.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// base_Text
			// 
			this.base_Text.Location = new System.Drawing.Point(-2, 3);
			this.base_Text.Text = "Mods";
			// 
			// TLP_Main
			// 
			this.TLP_Main.ColumnCount = 3;
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TLP_Main.Controls.Add(this.T_ProfileUsage, 1, 0);
			this.TLP_Main.Controls.Add(this.slickSpacer2, 0, 1);
			this.TLP_Main.Controls.Add(this.slickSpacer1, 0, 3);
			this.TLP_Main.Controls.Add(this.TB_Search, 0, 0);
			this.TLP_Main.Controls.Add(this.DD_Sorting, 2, 0);
			this.TLP_Main.Controls.Add(this.tableLayoutPanel3, 0, 2);
			this.TLP_Main.Controls.Add(this.panel1, 0, 4);
			this.TLP_Main.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TLP_Main.Location = new System.Drawing.Point(0, 30);
			this.TLP_Main.Name = "TLP_Main";
			this.TLP_Main.RowCount = 5;
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TLP_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.TLP_Main.Size = new System.Drawing.Size(895, 486);
			this.TLP_Main.TabIndex = 0;
			// 
			// T_ProfileUsage
			// 
			this.T_ProfileUsage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.T_ProfileUsage.Cursor = System.Windows.Forms.Cursors.Hand;
			this.T_ProfileUsage.Image1 = "I_City";
			this.T_ProfileUsage.Image2 = "I_Tools";
			this.T_ProfileUsage.Location = new System.Drawing.Point(284, 7);
			this.T_ProfileUsage.Margin = new System.Windows.Forms.Padding(7);
			this.T_ProfileUsage.Name = "T_ProfileUsage";
			this.T_ProfileUsage.Option1 = "GamePlay";
			this.T_ProfileUsage.Option2 = "EditorPlay";
			this.T_ProfileUsage.OptionStyle1 = Extensions.ColorStyle.Active;
			this.T_ProfileUsage.OptionStyle2 = Extensions.ColorStyle.Active;
			this.T_ProfileUsage.Size = new System.Drawing.Size(306, 40);
			this.T_ProfileUsage.TabIndex = 21;
			this.T_ProfileUsage.SelectedValueChanged += new System.EventHandler(this.FilterChanged);
			// 
			// slickSpacer2
			// 
			this.TLP_Main.SetColumnSpan(this.slickSpacer2, 3);
			this.slickSpacer2.Dock = System.Windows.Forms.DockStyle.Top;
			this.slickSpacer2.Location = new System.Drawing.Point(0, 54);
			this.slickSpacer2.Margin = new System.Windows.Forms.Padding(0);
			this.slickSpacer2.Name = "slickSpacer2";
			this.slickSpacer2.Size = new System.Drawing.Size(895, 2);
			this.slickSpacer2.TabIndex = 8;
			this.slickSpacer2.TabStop = false;
			this.slickSpacer2.Text = "slickSpacer2";
			// 
			// slickSpacer1
			// 
			this.TLP_Main.SetColumnSpan(this.slickSpacer1, 3);
			this.slickSpacer1.Dock = System.Windows.Forms.DockStyle.Top;
			this.slickSpacer1.Location = new System.Drawing.Point(0, 86);
			this.slickSpacer1.Margin = new System.Windows.Forms.Padding(0);
			this.slickSpacer1.Name = "slickSpacer1";
			this.slickSpacer1.Size = new System.Drawing.Size(895, 2);
			this.slickSpacer1.TabIndex = 7;
			this.slickSpacer1.TabStop = false;
			this.slickSpacer1.Text = "slickSpacer1";
			// 
			// TB_Search
			// 
			this.TB_Search.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			dynamicIcon1.Name = "I_Search";
			this.TB_Search.ImageName = dynamicIcon1;
			this.TB_Search.LabelText = "Search";
			this.TB_Search.Location = new System.Drawing.Point(3, 3);
			this.TB_Search.Name = "TB_Search";
			this.TB_Search.Placeholder = "SearchProfiles";
			this.TB_Search.SelectedText = "";
			this.TB_Search.SelectionLength = 0;
			this.TB_Search.SelectionStart = 0;
			this.TB_Search.ShowLabel = false;
			this.TB_Search.Size = new System.Drawing.Size(140, 48);
			this.TB_Search.TabIndex = 0;
			this.TB_Search.TextChanged += new System.EventHandler(this.FilterChanged);
			this.TB_Search.IconClicked += new System.EventHandler(this.TB_Search_IconClicked);
			// 
			// DD_Sorting
			// 
			this.DD_Sorting.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DD_Sorting.Cursor = System.Windows.Forms.Cursors.Hand;
			this.DD_Sorting.Font = new System.Drawing.Font("Nirmala UI", 15F);
			this.DD_Sorting.Location = new System.Drawing.Point(604, 7);
			this.DD_Sorting.Margin = new System.Windows.Forms.Padding(7);
			this.DD_Sorting.Name = "DD_Sorting";
			this.DD_Sorting.Padding = new System.Windows.Forms.Padding(7);
			this.DD_Sorting.Size = new System.Drawing.Size(284, 40);
			this.DD_Sorting.TabIndex = 4;
			this.DD_Sorting.SelectedItemChanged += new System.EventHandler(this.DD_Sorting_SelectedItemChanged);
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.AutoSize = true;
			this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel3.ColumnCount = 2;
			this.TLP_Main.SetColumnSpan(this.tableLayoutPanel3, 3);
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel3.Controls.Add(this.L_Counts, 1, 0);
			this.tableLayoutPanel3.Controls.Add(this.L_FilterCount, 0, 0);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Top;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 56);
			this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 1;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel3.Size = new System.Drawing.Size(895, 30);
			this.tableLayoutPanel3.TabIndex = 6;
			// 
			// L_Counts
			// 
			this.L_Counts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.L_Counts.AutoSize = true;
			this.L_Counts.Location = new System.Drawing.Point(821, 0);
			this.L_Counts.Name = "L_Counts";
			this.L_Counts.Size = new System.Drawing.Size(71, 30);
			this.L_Counts.TabIndex = 1;
			this.L_Counts.Text = "label1";
			// 
			// L_FilterCount
			// 
			this.L_FilterCount.AutoSize = true;
			this.L_FilterCount.Location = new System.Drawing.Point(3, 0);
			this.L_FilterCount.Name = "L_FilterCount";
			this.L_FilterCount.Size = new System.Drawing.Size(71, 30);
			this.L_FilterCount.TabIndex = 2;
			this.L_FilterCount.Text = "label1";
			// 
			// panel1
			// 
			this.TLP_Main.SetColumnSpan(this.panel1, 3);
			this.panel1.Controls.Add(this.slickScroll1);
			this.panel1.Controls.Add(this.FLP_Profiles);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 88);
			this.panel1.Margin = new System.Windows.Forms.Padding(0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(895, 398);
			this.panel1.TabIndex = 14;
			// 
			// slickScroll1
			// 
			this.slickScroll1.Dock = System.Windows.Forms.DockStyle.Right;
			this.slickScroll1.LinkedControl = this.FLP_Profiles;
			this.slickScroll1.Location = new System.Drawing.Point(885, 0);
			this.slickScroll1.Name = "slickScroll1";
			this.slickScroll1.Size = new System.Drawing.Size(10, 398);
			this.slickScroll1.Style = SlickControls.StyleType.Vertical;
			this.slickScroll1.TabIndex = 0;
			this.slickScroll1.TabStop = false;
			this.slickScroll1.Text = "slickScroll1";
			// 
			// FLP_Profiles
			// 
			this.FLP_Profiles.AutoSize = true;
			this.FLP_Profiles.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.FLP_Profiles.Location = new System.Drawing.Point(100, 28);
			this.FLP_Profiles.Name = "FLP_Profiles";
			this.FLP_Profiles.Size = new System.Drawing.Size(0, 0);
			this.FLP_Profiles.TabIndex = 1;
			// 
			// PC_ProfileList
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.TLP_Main);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(58)))), ((int)(((byte)(69)))));
			this.LabelBounds = new System.Drawing.Point(-2, 3);
			this.Name = "PC_ProfileList";
			this.Padding = new System.Windows.Forms.Padding(0, 30, 0, 0);
			this.Size = new System.Drawing.Size(895, 516);
			this.Text = "Mods";
			this.Controls.SetChildIndex(this.base_Text, 0);
			this.Controls.SetChildIndex(this.TLP_Main, 0);
			this.TLP_Main.ResumeLayout(false);
			this.TLP_Main.PerformLayout();
			this.tableLayoutPanel3.ResumeLayout(false);
			this.tableLayoutPanel3.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	private System.Windows.Forms.TableLayoutPanel TLP_Main;
	private SlickControls.SlickTextBox TB_Search;
	private ProfileSortingDropDown DD_Sorting;
	private SlickControls.SlickSpacer slickSpacer1;
	private SlickControls.SlickSpacer slickSpacer2;
	private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
	private System.Windows.Forms.Label L_Counts;
	private System.Windows.Forms.Panel panel1;
	private System.Windows.Forms.FlowLayoutPanel FLP_Profiles;
	private SlickControls.SlickScroll slickScroll1;
	private ThreeOptionToggle T_ProfileUsage;
	private System.Windows.Forms.Label L_FilterCount;
}
