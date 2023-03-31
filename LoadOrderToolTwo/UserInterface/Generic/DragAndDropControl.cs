using Extensions;

using LoadOrderToolTwo.Utilities;

using SlickControls;

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Generic;

[DefaultEvent("FileSelected")]
internal class DragAndDropControl : SlickControl
{
	private bool isDragActive;
	private bool isDragAvailable;

	public event Action<string>? FileSelected;
	public event Func<string, bool>? ValidFile;

	[Browsable(true)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	[EditorBrowsable(EditorBrowsableState.Always)]
	[Bindable(true)]
	public override string Text { get => base.Text; set => base.Text = value; }

	[Category("Appearance")]
	public string? RegexTest { get; set; }
	[Category("Behavior")]
	public string[]? ValidExtensions { get; set; }
	[Category("Behavior")]
	public string? StartingFolder { get; set; }

	public DragAndDropControl()
	{
		AllowDrop = true;
		Cursor = Cursors.Hand;
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		if (Live)
		{
			Size = UI.Scale(new Size(500, 130), UI.FontScale);
			Margin = Padding = UI.Scale(new Padding(10), UI.UIScale);
			Font = UI.Font(9.75F);
		}
	}

	protected override void OnDragEnter(DragEventArgs drgevent)
	{
		base.OnDragEnter(drgevent);

		isDragActive = true;

		if (drgevent.Data.GetDataPresent(DataFormats.FileDrop) && (ValidFile?.Invoke(((string[])drgevent.Data.GetData(DataFormats.FileDrop)).FirstOrDefault()) ?? false))
		{
				Log.Error($"Drop success for '{((string[])drgevent.Data.GetData(DataFormats.FileDrop)).FirstOrDefault()}'");
			drgevent.Effect = DragDropEffects.Copy;
			isDragAvailable = true;
			Invalidate();
		}
		else
		{
			if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
				Log.Error($"Check failed for '{((string[])drgevent.Data.GetData(DataFormats.FileDrop)).FirstOrDefault()}'");
			else
			Log.Error("Drop is not a file");
			drgevent.Effect = DragDropEffects.None;
			isDragAvailable = false;
			Invalidate();
		}
	}

	protected override void OnDragLeave(EventArgs e)
	{
		base.OnDragLeave(e);

		isDragActive = false;
		Invalidate();
	}

	protected override void OnDragDrop(DragEventArgs drgevent)
	{
		base.OnDragDrop(drgevent);

		var file = ((string[])drgevent.Data.GetData(DataFormats.FileDrop)).FirstOrDefault();

		if (file != null)
		{
			FileSelected?.Invoke(file);
		}

		isDragActive = false;
		Invalidate();
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);

		if (e.Button == MouseButtons.Left)
		{
			using var dialog = new IOSelectionDialog
			{
				ValidExtensions = ValidExtensions
			};

			if (dialog.PromptFile(Program.MainForm, StartingFolder) == DialogResult.OK)
			{
				if (ValidFile?.Invoke(dialog.SelectedPath) ?? false)
				{
					FileSelected?.Invoke(dialog.SelectedPath);
				}
				else
				{
					MessagePrompt.Show(Locale.SelectedFileInvalid, PromptButtons.OK, PromptIcons.Warning, Program.MainForm);
				}
			}
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.SetUp(BackColor);

		var border = (int)(4 * UI.FontScale);
		var color = isDragActive ? FormDesign.Design.ActiveForeColor : HoverState.HasFlag(HoverState.Pressed) ? FormDesign.Design.ActiveColor : HoverState.HasFlag(HoverState.Hovered) ? FormDesign.Design.ActiveColor.MergeColor(FormDesign.Design.ForeColor) : FormDesign.Design.ForeColor;

		if (isDragActive)
		{
			using var brush = new SolidBrush(isDragAvailable ? FormDesign.Design.ActiveColor : FormDesign.Design.RedColor);
			e.Graphics.FillRoundedRectangle(brush, ClientRectangle.Pad((int)(1.5 * UI.FontScale)), border);
		}
		else if (HoverState.HasFlag(HoverState.Hovered))
		{
			using var brush = new SolidBrush(Color.FromArgb(25, color));
			e.Graphics.FillRoundedRectangle(brush, ClientRectangle.Pad((int)(1.5 * UI.FontScale)), border);
		}

		using var pen = new Pen(Color.FromArgb(100, color), (float)(1.5 * UI.FontScale)) { DashStyle = DashStyle.Dash };
		e.Graphics.DrawRoundedRectangle(pen, ClientRectangle.Pad((int)(1.5 * UI.FontScale)), border);

		var text = LocaleHelper.GetGlobalText(Text);
		var size = e.Graphics.Measure(text, Font, Width - Padding.Horizontal);
		var width = (int)size.Width + 3 + Padding.Left + (UI.FontScale >= 2 ? 48 : 24);
		var rect = ClientRectangle.CenterR(width, Math.Max(UI.FontScale >= 2 ? 48 : 24, (int)size.Height + 3));

		using var icon = (UI.FontScale >= 2 ? Properties.Resources.I_DragDrop_48 : Properties.Resources.I_DragDrop).Color(color);

		e.Graphics.DrawImage(icon, rect.Align(icon.Size, ContentAlignment.MiddleLeft));

		e.Graphics.DrawString(text, Font, new SolidBrush(color), rect.Align(size.ToSize(), ContentAlignment.MiddleRight).Pad(-2));
	}
}