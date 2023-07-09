using SkyveApp.Domain.CS1;
using SkyveApp.Domain.CS1.Enums;
using SkyveApp.Systems.CS1.Utilities;
using SkyveApp.UserInterface.Content;

using System.Windows.Forms;

namespace SkyveApp.UserInterface.Forms;
public partial class EditTagsForm : BaseForm
{
	public List<ILocalPackage> Packages { get; }

	public EditTagsForm(IEnumerable<ILocalPackage> packages)
	{
		InitializeComponent();

		Packages = packages.ToList();
		Text = LocaleHelper.GetGlobalText("Tags");

		foreach (var link in Packages.SelectMany(x => x.GetTags().Where(x => x.IsCustom)).Distinct(x => x.Value))
		{
			AddTag(link);
		}

		//L_MultipleWarning.Visible = Packages.Count > 1;
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

		AddTag(new TagItem(TagSource.FindIt, prompt.Input));
	}

	private void TagControl_Click(object sender, EventArgs e)
	{
		(sender as Control)!.Dispose();
	}

	private void AddTag(ITag tag)
	{
		var control = new TagControl { TagInfo = tag };
		control.Click += TagControl_Click;
		FLP_Tags.Controls.Add(control);
		addTagControl.SendToBack();
	}

	private IEnumerable<string?> GetLinks()
	{
		return FLP_Tags.Controls.OfType<TagControl>().Select(x => x.TagInfo?.Value?.Replace(' ', '-'));
	}

	private void B_Apply_Click(object sender, EventArgs e)
	{
		DialogResult = DialogResult.OK;
		foreach (var package in Packages)
		{
			ServiceCenter.Get<ITagsService>().SetTags(package, GetLinks()!);
		}
		Close();
		Program.MainForm?.TryInvoke(() => Program.MainForm.Invalidate(true));
	}
}
