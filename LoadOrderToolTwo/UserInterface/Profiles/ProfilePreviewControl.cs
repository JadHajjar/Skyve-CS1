using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Drawing;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Profiles;
internal class ProfilePreviewControl : SlickControl
{
	private Rectangle LoadRect;
	private Rectangle MergeRect;
	private Rectangle ExcludeRect;
	private Rectangle DisposeRect;

	public ProfilePreviewControl(Profile profile)
	{
		Profile = profile;
	}

	public Profile Profile { get; }
	public bool Merging { get; private set; }
	public bool Excluding { get; private set; }

	public event Action<Profile>? LoadProfile;
	public event Action<Profile>? MergeProfile;
	public event Action<Profile>? ExcludeProfile;
	public event Action<Profile>? DisposeProfile;

	protected override void UIChanged()
	{
		base.UIChanged();

		Padding = UI.Scale(new Padding(5), UI.FontScale);
		Margin = UI.Scale(new Padding(5), UI.FontScale);
		Size = UI.Scale(new Size(350, 112), UI.FontScale);
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);

		if (e.Button != MouseButtons.Left)
		{
			return;
		}

		var starRect = ClientRectangle.Align(UI.Scale(new Size(32, 32), UI.FontScale), ContentAlignment.TopRight);

		if (!Profile.Temporary && starRect.Contains(e.Location))
		{
			Profile.IsFavorite = !Profile.IsFavorite;
			ProfileManager.Save(Profile);
			return;
		}

		if (LoadRect.Contains(e.Location))
		{
			LoadProfile?.Invoke(Profile);
			return;
		}

		else if (MergeRect.Contains(e.Location))
		{
			Loading = true;
			Merging = true;
			MergeProfile?.Invoke(Profile);
			return;
		}

		else if (ExcludeRect.Contains(e.Location))
		{
			Loading = true;
			Excluding = true;
			ExcludeProfile?.Invoke(Profile);
			return;
		}

		else if (DisposeRect.Contains(e.Location) && MessagePrompt.Show(string.Format(Locale.ConfirmDeleteProfile, Profile.Name), PromptButtons.YesNo, PromptIcons.Hand, Program.MainForm) == DialogResult.Yes)
		{
			DisposeProfile?.Invoke(Profile);

			Dispose();
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);

		var starRect = ClientRectangle.Align(UI.Scale(new Size(32, 32), UI.FontScale), ContentAlignment.TopRight);

		if (LoadRect.Contains(e.Location))
		{
			SlickTip.SetTo(this, Locale.ProfileReplace);
		}

		else if (!Profile.Temporary && starRect.Contains(e.Location))
		{
			SlickTip.SetTo(this, Profile.IsFavorite ? Locale.UnFavoriteThisProfile : Locale.FavoriteThisProfile);
		}

		else if (MergeRect.Contains(e.Location))
		{
			SlickTip.SetTo(this, Locale.ProfileMerge);
		}

		else if (ExcludeRect.Contains(e.Location))
		{
			SlickTip.SetTo(this, Locale.ProfileExclude);
		}

		else if (DisposeRect.Contains(e.Location))
		{
			SlickTip.SetTo(this, Locale.ProfileDelete);
		}

		else
		{
			SlickTip.SetTo(this, Profile.Name);
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.SetUp(BackColor);

		var back = FormDesign.Design.BackColor.Tint(Lum: FormDesign.Design.Type.If(FormDesignType.Dark, 6, -6));

		if (Profile.Temporary)
		{
			back = back.MergeColor(FormDesign.Design.YellowColor, 65);
		}
		else if (Profile.IsMissingItems)
		{
			back = back.MergeColor(FormDesign.Design.RedColor, 90);
		}

		using var backBrush = ClientRectangle.Gradient(Color.FromArgb(220, back), 0.5F);
		e.Graphics.FillRoundedRectangle(backBrush, ClientRectangle.Pad(1), Padding.Left);

		var titleHeight = Math.Max(24, (int)e.Graphics.Measure(Profile.Name, UI.Font(9.75F, FontStyle.Bold), Width - (24 + 2 * Padding.Horizontal + (int)(32 * UI.FontScale))).Height);
		var iconRectangle = new Rectangle(Padding.Left, Padding.Top + (titleHeight - 24) / 2, 24, 24);

		if (Loading)
		{
			DrawLoader(e.Graphics, iconRectangle);
		}
		else
		{
			using var image = Profile.GetIcon();

			e.Graphics.DrawImage(image.Color(FormDesign.Design.IconColor), iconRectangle);
		}

		e.Graphics.DrawString(Profile.Name, UI.Font(9.75F, FontStyle.Bold), new SolidBrush(FormDesign.Design.ForeColor), new Rectangle(0, 0, Width, titleHeight).Pad(24 + Padding.Left * 2, Padding.Top, Padding.Horizontal + (int)(32 * UI.FontScale), 0), new StringFormat { LineAlignment = StringAlignment.Center });

		var y = titleHeight + Padding.Vertical;

		var hovered = false;

		hovered |= DrawButton(e, Locale.LoadProfile, ImageManager.GetIcon("I_Import"), ClientRectangle, ContentAlignment.BottomRight, ColorStyle.Active, false, out var loadRect);

		LoadRect = loadRect;

		if (!Profile.Temporary)
		{
			y = DrawValue(e, y, Profile.Mods.Count.ToString(), Profile.Mods.Count == 1 ? Locale.ModIncluded : Locale.ModIncludedPlural);
			y = DrawValue(e, y, Profile.Assets.Count.ToString(), Profile.Assets.Count == 1 ? Locale.AssetIncluded : Locale.AssetIncludedPlural);

			y += Padding.Top;

			using var star = Profile.IsFavorite ? Properties.Resources.I_StarFilled.Color(FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.RedColor, 70)) : Properties.Resources.I_Star.Color(ForeColor);

			var starRect = ClientRectangle.Align(UI.Scale(new Size(32, 32), UI.FontScale), ContentAlignment.TopRight);

			if (starRect.Contains(PointToClient(Cursor.Position)))
			{
				hovered = true;

				e.Graphics.FillRoundedRectangle(new SolidBrush(Color.FromArgb(20, ForeColor)), starRect, Padding.Left);
			}

			e.Graphics.DrawImage(star, starRect.CenterR(star.Size));

			hovered |= DrawButton(e, string.Empty, ImageManager.GetIcon("I_Merge"), ClientRectangle.Pad(0, 0, 0, loadRect.Height + Padding.Bottom), ContentAlignment.BottomRight, ColorStyle.Yellow, Merging, out var mergeRect);
			hovered |= DrawButton(e, string.Empty, ImageManager.GetIcon("I_Exclude"), ClientRectangle.Pad(0, 0, 0, loadRect.Height + Padding.Bottom).Pad(0, 0, mergeRect.Width + Padding.Left, 0), ContentAlignment.BottomRight, ColorStyle.Yellow, Excluding, out var excludeRect);
			hovered |= DrawButton(e, string.Empty, ImageManager.GetIcon("I_Disposable"), ClientRectangle, ContentAlignment.BottomLeft, ColorStyle.Red, false, out var disposeRect);

			MergeRect = mergeRect;
			ExcludeRect = excludeRect;
			DisposeRect = disposeRect;

			if (Profile.IsMissingItems)
			{
				e.Graphics.DrawString(Locale.IncludesItemsYouDoNotHave, Font, new SolidBrush(FormDesign.Design.RedColor), new Rectangle(DisposeRect.Right, LoadRect.Y, LoadRect.Left - DisposeRect.Right, LoadRect.Height), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
			}
		}

		Cursor = hovered ? Cursors.Hand : Cursors.Default;

		Height = y + loadRect.Height + Padding.Bottom;
	}

	private bool DrawButton(PaintEventArgs e, string text, Bitmap icon, Rectangle rectangle, ContentAlignment alignment, ColorStyle style, bool load, out Rectangle rect)
	{
		using (icon)
		{
			var size = SlickButton.GetSize(e.Graphics, icon, text, Font, UI.Scale(new Padding(7), UI.UIScale));
			rect = rectangle.Pad(Padding).Align(size, alignment);
			var hovered = rect.Contains(PointToClient(Cursor.Position));

			SlickButton.DrawButton(e, rect.Location, rect.Size, text, Font, Color.Empty, Color.Empty, icon, UI.Scale(new Padding(7), UI.UIScale), Enabled, hovered ? HoverState & ~HoverState.Focused : HoverState.Normal, style, null, load ? this : null);

			return hovered;
		}
	}

	private int DrawValue(PaintEventArgs e, int targetHeight, string value, string descriptor, int x = 0)
	{
		var valueSize = e.Graphics.Measure(value, UI.Font(8.25F, FontStyle.Bold), Width - Padding.Horizontal).ToSize();
		var descriptorSize = e.Graphics.Measure(descriptor, UI.Font(8.25F), Width - valueSize.Width - Padding.Right + 3).ToSize();

		e.Graphics.DrawString(value, UI.Font(8.25F, FontStyle.Bold), new SolidBrush(FormDesign.Design.LabelColor), new Rectangle(x + Padding.Left, targetHeight, valueSize.Width + 3, valueSize.Height));

		e.Graphics.DrawString(descriptor, UI.Font(8.25F), new SolidBrush(FormDesign.Design.LabelColor), new Rectangle(x + valueSize.Width + Padding.Left, targetHeight, Width - valueSize.Width - Padding.Right + 3, Height));

		return targetHeight + Math.Max(valueSize.Height, descriptorSize.Height) + Padding.Bottom;
	}
}
