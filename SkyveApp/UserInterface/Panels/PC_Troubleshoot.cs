using SkyveApp.Domain.CS1;
using SkyveApp.Systems.CS1.Utilities;

using System.IO;
using System.Reflection.Emit;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Panels;
public partial class PC_Troubleshoot : PanelContent
{
	public PC_Troubleshoot()
	{
		InitializeComponent();

		L_Title.Text = Locale.TroubleshootSelection;
	}

	protected override void DesignChanged(FormDesign design)
	{
		base.DesignChanged(design);

		L_Title.ForeColor = design.ActiveColor;
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		L_Title.Font = UI.Font(10.5F, System.Drawing.FontStyle.Bold);
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
}
