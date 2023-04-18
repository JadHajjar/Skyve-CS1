using Extensions;

using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.IO;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Generic;

[DefaultEvent("FileSelected")]
internal class DragAndDropControl : SlickControl
{
	private bool isDragActive;
	private bool isDragAvailable;
	private string? _selectedFile;

	public event Action<string>? FileSelected;
	public event Func<object, string, bool>? ValidFile;

	[Browsable(true)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	[EditorBrowsable(EditorBrowsableState.Always)]
	[Bindable(true)]
	public override string Text { get => base.Text; set => base.Text = value; }

	[Category("Behavior"), DefaultValue(null)]
	public string[]? ValidExtensions { get; set; }

	[Category("Behavior"), DefaultValue(null)]
	public string? StartingFolder { get; set; }

	[Category("Behavior"), DefaultValue(null)]
	public string? SelectedFile { get => _selectedFile; set { _selectedFile = value; Invalidate(); } }

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
			Size = UI.Scale(new Size(325, 80), UI.FontScale);
			Padding = UI.Scale(new Padding(10), UI.UIScale);
			Font = UI.Font(9.75F);

			if (Margin == new Padding(3))
			{
				Margin = Padding;
			}
		}
	}

	protected override void OnDragEnter(DragEventArgs drgevent)
	{
		base.OnDragEnter(drgevent);

		isDragActive = true;

		if (drgevent.Data.GetDataPresent(DataFormats.FileDrop) && (ValidFile?.Invoke(this, ((string[])drgevent.Data.GetData(DataFormats.FileDrop)).FirstOrDefault()) ?? true))
		{
			drgevent.Effect = DragDropEffects.Copy;
			isDragAvailable = true;
			Invalidate();
		}
		else
		{
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
			if (LocationManager.Platform is not Platform.Windows)
			{
				var realPath = IOUtil.ToRealPath(file);

				if (LocationManager.FileExists(realPath))
				{
					file = realPath!;
				}
			}

			FileSelected?.Invoke(file);
		}

		isDragActive = false;
		Invalidate();
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);

		if (e.Button == MouseButtons.Middle && !string.IsNullOrWhiteSpace(SelectedFile))
		{
			FileSelected?.Invoke(string.Empty);
		}

		if (e.Button != MouseButtons.Left)
		{
			return;
		}

		var availableWidth = string.IsNullOrWhiteSpace(SelectedFile) ? Width : (Width - (int)(125 * UI.FontScale));

		if (!string.IsNullOrWhiteSpace(SelectedFile))
		{
			using var removeIcon = ImageManager.GetIcon(nameof(Properties.Resources.I_X)).Color(FormDesign.Design.MenuForeColor);

			var fileRect = new Rectangle(0, 0, Width - availableWidth, Height).Pad(Padding.Left);
			var removeRect = fileRect.Align(new Size(removeIcon.Width + Padding.Left, removeIcon.Height + Padding.Top), ContentAlignment.TopRight);

			if (removeRect.Contains(e.Location))
			{
				FileSelected?.Invoke(string.Empty);
				return;
			}
			else if (fileRect.Contains(e.Location))
			{
				PlatformUtil.OpenFolder(SelectedFile);

				return;
			}
		}

		using var dialog = new IOSelectionDialog
		{
			ValidExtensions = ValidExtensions
		};

		if (dialog.PromptFile(Program.MainForm, StartingFolder) == DialogResult.OK)
		{
			if (ValidFile?.Invoke(this, dialog.SelectedPath) ?? true)
			{
				FileSelected?.Invoke(dialog.SelectedPath);
			}
			else
			{
				MessagePrompt.Show(Locale.SelectedFileInvalid, PromptButtons.OK, PromptIcons.Warning, Program.MainForm);
			}
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.SetUp(BackColor);

		var fileHovered = false;
		var border = (int)(4 * UI.FontScale);
		var cursor = PointToClient(MousePosition);
		var availableWidth = string.IsNullOrWhiteSpace(SelectedFile) ? Width : (Width - (int)(125 * UI.FontScale));

		if (!string.IsNullOrWhiteSpace(SelectedFile))
		{
			using var fileIcon = (UI.FontScale >= 2 ? Properties.Resources.I_File_48 : Properties.Resources.I_File).Color(FormDesign.Design.MenuForeColor);
			using var removeIcon = ImageManager.GetIcon(nameof(Properties.Resources.I_X)).Color(FormDesign.Design.MenuForeColor);

			var textSize = e.Graphics.Measure(Path.GetFileNameWithoutExtension(SelectedFile), new Font(Font, FontStyle.Bold), Width - availableWidth - Padding.Horizontal);
			var fileHeight = (int)textSize.Height + 3 + fileIcon.Height + Padding.Top;
			var fileRect = new Rectangle(0, 0, Width - availableWidth, Height).Pad(Padding.Left);
			var fileContentRect = fileRect.CenterR(Math.Max((int)textSize.Width + 3, fileIcon.Width), fileHeight);
			var removeRect = fileRect.Align(new Size(removeIcon.Width + Padding.Left, removeIcon.Height + Padding.Top), ContentAlignment.TopRight);

			e.Graphics.FillRoundedRectangle(new SolidBrush((fileHovered = HoverState.HasFlag(HoverState.Hovered) && fileRect.Contains(cursor)) && !removeRect.Contains(cursor) ? FormDesign.Design.MenuColor.MergeColor(FormDesign.Design.ActiveColor) : FormDesign.Design.MenuColor), fileRect, border);
			e.Graphics.DrawImage(fileIcon, fileContentRect.Align(fileIcon.Size, ContentAlignment.TopCenter));
			e.Graphics.DrawString(Path.GetFileNameWithoutExtension(SelectedFile), new Font(Font, FontStyle.Bold), new SolidBrush(FormDesign.Design.MenuForeColor), fileContentRect, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far });

			if (HoverState.HasFlag(HoverState.Hovered) && removeRect.Contains(cursor))
			{
				e.Graphics.FillRoundedRectangle(new SolidBrush(Color.FromArgb(HoverState.HasFlag(HoverState.Pressed) ? 255 : 100, FormDesign.Design.RedColor)), removeRect, border);
			}

			e.Graphics.DrawImage(removeIcon, removeRect.CenterR(removeIcon.Size));
		}

		var color = fileHovered ? FormDesign.Design.ForeColor : isDragActive ? FormDesign.Design.ActiveForeColor : HoverState.HasFlag(HoverState.Pressed) ? FormDesign.Design.ActiveColor : HoverState.HasFlag(HoverState.Hovered) ? FormDesign.Design.ActiveColor.MergeColor(FormDesign.Design.ForeColor) : FormDesign.Design.ForeColor;

		if (isDragActive)
		{
			using var brush = new SolidBrush(isDragAvailable ? FormDesign.Design.ActiveColor : FormDesign.Design.RedColor);
			e.Graphics.FillRoundedRectangle(brush, ClientRectangle.Pad((int)(1.5 * UI.FontScale)), border);
		}
		else if (HoverState.HasFlag(HoverState.Hovered) && !fileHovered)
		{
			using var brush = new SolidBrush(Color.FromArgb(25, color));
			e.Graphics.FillRoundedRectangle(brush, ClientRectangle.Pad((int)(1.5 * UI.FontScale)), border);
		}

		using var pen = new Pen(Color.FromArgb(100, color), (float)(1.5 * UI.FontScale)) { DashStyle = DashStyle.Dash };
		e.Graphics.DrawRoundedRectangle(pen, ClientRectangle.Pad((int)(1.5 * UI.FontScale)), border);

		var text = LocaleHelper.GetGlobalText(Text);
		var size = e.Graphics.Measure(text, Font, availableWidth - 2 * Padding.Horizontal - (UI.FontScale >= 2 ? 48 : 24));
		var width = (int)size.Width + 3 + Padding.Left + (UI.FontScale >= 2 ? 48 : 24);
		var rect = new Rectangle(Width - availableWidth, 0, availableWidth, Height).CenterR(width, Math.Max(UI.FontScale >= 2 ? 48 : 24, (int)size.Height + 3));

		if (Loading)
		{
			DrawLoader(e.Graphics, rect.Align(new Size(UI.FontScale >= 2 ? 48 : 24, UI.FontScale >= 2 ? 48 : 24), ContentAlignment.MiddleLeft));
		}
		else
		{
			using var icon = (UI.FontScale >= 2 ? Properties.Resources.I_DragDrop_48 : Properties.Resources.I_DragDrop).Color(color);

			e.Graphics.DrawImage(icon, rect.Align(icon.Size, ContentAlignment.MiddleLeft));
		}

		e.Graphics.DrawString(text, Font, new SolidBrush(color), rect.Align(size.ToSize(), ContentAlignment.MiddleRight).Pad(-2));

		if (!Enabled)
		{
			e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(100, BackColor)), ClientRectangle);
		}
	}
}