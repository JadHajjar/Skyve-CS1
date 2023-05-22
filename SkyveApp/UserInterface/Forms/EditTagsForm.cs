using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Compatibility;
using SkyveApp.Domain.Interfaces;
using SkyveApp.UserInterface.Content;
using SkyveApp.Utilities;
using SkyveApp.Utilities.Managers;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Forms;
public partial class EditTagsForm : BaseForm
{
	public IPackage Package { get; }

	public EditTagsForm(IPackage package)
	{
		InitializeComponent();

		Package = package;
		Text = LocaleHelper.GetGlobalText("Tags");

		foreach (var link in package.Tags.Where(x => x.Source == Domain.Enums.TagSource.FindIt))
		{ AddTag(link); }
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		FLP_Tags.Padding = UI.Scale(new Padding(10), UI.FontScale);
		B_Apply.Margin = UI.Scale(new Padding(0, 0, 10, 10), UI.FontScale);
	}

	private void B_AddLink_Click(object sender, EventArgs e)
	{
		var prompt = MessagePrompt.ShowInput(Locale.AddCustomTag, "");

		if (prompt.DialogResult != DialogResult.OK)
		{
			return;
		}

		if (string.IsNullOrWhiteSpace(prompt.Input) || FLP_Tags.Controls.Any(x => x.Text.Equals(prompt.Input, StringComparison.CurrentCultureIgnoreCase)))
		{
			return;
		}

		AddTag(new(Domain.Enums.TagSource.FindIt, prompt.Input));
	}

	private void TagControl_Click(object sender, EventArgs e)
	{
		(sender as Control)!.Dispose();
	}

	private void AddTag(TagItem tag)
	{
		var control = new TagControl { TagInfo = tag };
		control.Click += TagControl_Click;
		FLP_Tags.Controls.Add(control);
		addTagControl.SendToBack();
	}

	private IEnumerable<string?> GetLinks()
	{
		return FLP_Tags.Controls.OfType<TagControl>().Select(x => x.TagInfo.Value?.Replace(' ', '-'));
	}

	private void B_Apply_Click(object sender, EventArgs e)
	{
		DialogResult = DialogResult.OK;
		AssetsUtil.SetFindItTag(Package, GetLinks().WhereNotEmpty().ListStrings(" "));							
		Close();
		Program.MainForm?.TryInvoke(() => Program.MainForm.Invalidate(true));
	}
}
