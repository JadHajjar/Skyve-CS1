using SkyveApp.Domain.CS1;
using SkyveApp.Systems.CS1.Utilities;

using System.IO;
using System.Reflection.Emit;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Panels;
public partial class PC_Troubleshoot : PanelContent
{
	private readonly TroubleshootSettings _settings = new();

	public PC_Troubleshoot()
	{
		InitializeComponent();

		L_Title.Text = Locale.TroubleshootSelection;
		L_ModAssetTitle.Text = Locale.TroubleshootModOrAsset;
		B_Mods.Text = Locale.Mod.Plural;
		B_Assets.Text = Locale.Asset.Plural;
	}

	protected override void DesignChanged(FormDesign design)
	{
		base.DesignChanged(design);

		L_Title.ForeColor =L_ModAssetTitle.ForeColor= design.ActiveColor;
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		L_Title.Font= L_ModAssetTitle.Font = UI.Font(10.5F, System.Drawing.FontStyle.Bold);
		B_Cancel.Font = UI.Font(9.75F);
	}

	private void B_Cancel_Click(object sender, EventArgs e)
	{
		PushBack();
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (keyData == Keys.Escape)
		{
			PushBack();
			return true;
		}

		return base.ProcessCmdKey(ref msg, keyData);
	}

	private void B_Caused_Load(object sender, EventArgs e)
	{
		TLP_ModAsset.Show();
		TLP_New.Hide();

		_settings.ItemIsCausingIssues = true;
	}

	private void B_Missing_Load(object sender, EventArgs e)
	{
		TLP_ModAsset.Show();
		TLP_New.Hide();

		_settings.ItemIsMissing = true;
	}

	private void B_New_Click(object sender, EventArgs e)
	{
		TLP_ModAsset.Show();
		TLP_New.Hide();

		_settings.NewItemCausingIssues = true;
	}

	private void B_Mods_Click(object sender, EventArgs e)
	{
		_settings.Mods = true;

		ServiceCenter.Get<ITroubleshootSystem>().Start(_settings);
	}

	private void B_Assets_Click(object sender, EventArgs e)
	{
		ServiceCenter.Get<ITroubleshootSystem>().Start(_settings);
	}

	private class TroubleshootSettings : ITroubleshootSettings
	{
		public bool Mods { get; set; }
		public bool ItemIsCausingIssues { get; set; }
		public bool ItemIsMissing { get; set; }
		public bool NewItemCausingIssues { get; set; }
	}
}
