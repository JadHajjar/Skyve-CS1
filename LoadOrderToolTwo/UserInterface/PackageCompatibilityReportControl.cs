using CompatibilityReport.CatalogData;

using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface;
internal class PackageCompatibilityReportControl : TableLayoutPanel
{
	public PackageCompatibilityReportControl(Package package)
	{
		Package = package;
		AutoSize = true;
		AutoSizeMode = AutoSizeMode.GrowAndShrink;
		ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 / 3F));
		ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 / 3F));
		ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 / 3F));
		ColumnCount = 3;
	}

	public Package Package { get; }
	public CompatibilityManager.ReportInfo? Report { get; private set; }

	protected override void OnCreateControl()
	{
		base.OnCreateControl();

		if (DesignMode)
		{
			return;
		}

		Padding = UI.Scale(new Padding(5), UI.FontScale);

		Reset();
	}

	internal void Reset()
	{
		Report = Package.CompatibilityReport;

		RowStyles.Clear();
		RowCount = 0;
		Controls.Clear(true);

		if (Report == null)
		{
			GenerateSection(Locale.CompatibilityReport, Properties.Resources.I_CompatibilityReport, FormDesign.Design.ButtonColor, new CompatibilityMessageControl(this, Enums.ReportType.Note, new CompatibilityManager.ReportMessage(Enums.ReportType.Note, Enums.ReportSeverity.NothingToReport, Locale.CR_NoAvailableReport)));
			return;
		}

		foreach (var item in Report.Messages.GroupBy(x => x.Type).OrderBy(x => x.Key))
		{
			GenerateSection(LocaleHelper.GetGlobalText($"CRT_{item.Key}"), GetTypeIcon(item.Key), GetTypeColor(item), item.Select(x => new CompatibilityMessageControl(this, item.Key, x)).ToArray());
		}
	}

	private Color GetTypeColor(IGrouping<Enums.ReportType, CompatibilityManager.ReportMessage> item)
	{
		return item.Max(x => x.Severity).GetSeverityColor().MergeColor(BackColor, 15);
	}

	private Bitmap GetTypeIcon(Enums.ReportType type)
	{
		return type switch
		{
			Enums.ReportType.Stability => Properties.Resources.I_Stability,
			Enums.ReportType.DlcMissing or Enums.ReportType.RequiredMods => Properties.Resources.I_MissingMod,
			Enums.ReportType.UnneededDependency => Properties.Resources.I_Disposable,
			Enums.ReportType.WorksWhenDisabled => Properties.Resources.I_Malicious,
			Enums.ReportType.Successors => Properties.Resources.I_Upgrade,
			Enums.ReportType.Alternatives => Properties.Resources.I_Alternatives,
			Enums.ReportType.Status => Properties.Resources.I_Statuses,
			Enums.ReportType.Note => Properties.Resources.I_Note,
			Enums.ReportType.Recommendations => Properties.Resources.I_Recommendations,
			Enums.ReportType.Compatibility => Properties.Resources.I_Compatibilities,
			_ => Properties.Resources.I_CompatibilityReport,
		};
	}

	private void GenerateSection(string title, Bitmap image, Color backColor, params Control[] controls)
	{
		if (controls.Length == 0)
		{
			return;
		}

		var tlp = new RoundedTableLayoutPanel
		{
			Dock = DockStyle.Top,
			Padding = UI.Scale(new Padding(5), UI.FontScale),
			Margin = UI.Scale(new Padding(5), UI.FontScale),
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			BackColor = backColor,
			ForeColor = backColor.GetTextColor(),
		};

		var icon = new PictureBox
		{
			Image = image.Color(tlp.ForeColor),
			Enabled = false,
			Anchor = AnchorStyles.Left,
			Size = new Size(32, 32),
			SizeMode = PictureBoxSizeMode.CenterImage,
			Margin = UI.Scale(new Padding(4, 4, 0, 10), UI.FontScale),
		};

		var label = new Label
		{
			Text = title,
			AutoSize = true,
			Anchor = AnchorStyles.Left,
			Font = UI.Font(10.25F, FontStyle.Bold),
			Margin = UI.Scale(new Padding(0, 3, 4, 10), UI.FontScale),
		};

		tlp.ColumnStyles.Add(new ColumnStyle());
		tlp.ColumnStyles.Add(new ColumnStyle());
		tlp.RowStyles.Add(new RowStyle());
		tlp.Controls.Add(icon, 0, 0);
		tlp.Controls.Add(label, 1, 0);

		var i = 1;
		foreach (var item in controls)
		{
			tlp.RowStyles.Add(new RowStyle());
			tlp.Controls.Add(item, 0, i++);
			tlp.SetColumnSpan(item, 2);
		}

		tlp.RowCount = tlp.RowStyles.Count;
		tlp.ColumnCount = tlp.ColumnStyles.Count;

		if (Controls.Count % 3 == 0)
		{
			RowCount++;
			RowStyles.Add(new());
		}

		Controls.Add(tlp, Controls.Count % 3, RowCount - 1);
	}
}
