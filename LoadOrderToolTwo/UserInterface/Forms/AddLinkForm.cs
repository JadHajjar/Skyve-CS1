using Extensions;

using LoadOrderToolTwo.Domain.Compatibility;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Forms;
public partial class AddLinkForm : BaseForm
{
	public event Action<IEnumerable<PackageLink>>? LinksReturned;

	public AddLinkForm(List<PackageLink> links)
	{
		InitializeComponent();

		Text = LocaleHelper.GetGlobalText("Links");

		foreach (var link in links)
		{ AddLink(link); }
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		B_AddLink.Margin = UI.Scale(new Padding(10), UI.FontScale);
		B_Apply.Margin = UI.Scale(new Padding(0, 0, 10, 10), UI.FontScale);
	}

	private void B_AddLink_Click(object sender, EventArgs e)
	{
		AddLink(new PackageLink());
	}

	private void AddLink(PackageLink packageLink)
	{
		TLP.SuspendDrawing();

		TLP.RowStyles.Add(new());

		TLP.Controls.Add(new LinkControl(packageLink), 0, TLP.RowStyles.Count - 1);

		TLP.ResumeDrawing();
	}

	protected override void OnDeactivate(EventArgs e)
	{
		base.OnDeactivate(e);

		if (CurrentFormState != FormState.ForcedFocused)
		{
			LinksReturned?.Invoke(GetLinks());
			Close();
		}
	}

	private IEnumerable<PackageLink> GetLinks()
	{
		throw new NotImplementedException();
	}

	private class LinkControl : TableLayoutPanel
	{
		private readonly SlickIcon icon;
		private readonly SlickTextBox tbLink;
		private readonly SlickTextBox tbName;
		private readonly SlickIcon deleteButton;
		private PackageLink link;

		public LinkControl(PackageLink packageLink)
		{
			Dock = DockStyle.Top;
			AutoSize = true;
			AutoSizeMode = AutoSizeMode.GrowAndShrink;

			link = packageLink;
			icon = new SlickIcon();
			tbLink = new SlickTextBox();
			tbName = new SlickTextBox();
			deleteButton = new SlickIcon();

			icon.ImageName = packageLink.Type.GetIcon();
			icon.Size = UI.Scale(new Size(24, 24), UI.FontScale);
			icon.Padding = UI.Scale(new Padding(5), UI.FontScale);
			icon.Margin = UI.Scale(new Padding(10, 3, 3, 3), UI.FontScale);
			icon.Click += Icon_Click;

			tbLink.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			tbLink.ShowLabel = false;
			tbLink.Placeholder = "Link URL";
			tbLink.MinimumSize = new(0, (int)(24 * UI.FontScale));

			tbName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			tbName.ShowLabel = false;
			tbName.Placeholder = "Link Name";
			tbName.MinimumSize = new(0, (int)(24 * UI.FontScale));

			deleteButton.Size = UI.Scale(new Size(24, 24), UI.FontScale);
			deleteButton.Padding = UI.Scale(new Padding(5), UI.FontScale);
			deleteButton.ImageName = "I_Disposable";
			deleteButton.ColorStyle = ColorStyle.Red;
			deleteButton.Margin = UI.Scale(new Padding(3, 3, 10, 3), UI.FontScale);
			deleteButton.Click += (s, e) => Dispose();

			ColumnStyles.Add(new());
			ColumnStyles.Add(new(SizeType.Percent, 60));
			ColumnStyles.Add(new(SizeType.Percent, 40));
			ColumnStyles.Add(new());
			RowStyles.Add(new());
			Controls.Add(icon, 0, 0);
			Controls.Add(tbLink, 1, 0);
			Controls.Add(deleteButton, 3, 0);
			SetColumnSpan(tbLink, 2);
		}

		private void Icon_Click(object sender, EventArgs e)
		{
			var items = Enum.GetValues(typeof(LinkType)).Cast<LinkType>().Select(x => new SlickStripItem(x.ToString(), x.GetIcon(), action: () =>
			{
				link.Type = x;
				icon.ImageName = link.Type.GetIcon();

				if (link.Type == LinkType.Other)
				{
					SetColumnSpan(tbLink, 1);
					Controls.Add(tbName, 2, 0);
				}
				else
				{
					Controls.Remove(tbName);
					SetColumnSpan(tbLink, 2);
				}
			})).ToArray();

			SlickToolStrip.Show(FindForm() as AddLinkForm, icon.PointToScreen(new Point(icon.Width + 5, 0)), items);
		}
	}

	private void B_Apply_Click(object sender, EventArgs e)
	{
		LinksReturned?.Invoke(GetLinks());
		Close();
	}
}
