using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Compatibility;
using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.CompatibilityReport;
internal class PackageCompatibilityReportControl : TableLayoutPanel
{
	public PackageCompatibilityReportControl(IPackage package)
	{
		Package = package;
		AutoSize = true;
		AutoSizeMode = AutoSizeMode.GrowAndShrink;
		ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 / 3F));
		ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 / 3F));
		ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 / 3F));
		ColumnCount = 3;

		CentralManager.PackageInformationUpdated += CentralManager_PackageInformationUpdated;
	}

	private void CentralManager_PackageInformationUpdated()
	{
		this.TryInvoke(Reset);
	}

	public IPackage Package { get; }
	public CompatibilityInfo? Report { get; private set; }

	protected override void Dispose(bool disposing)
	{
		CentralManager.PackageInformationUpdated -= CentralManager_PackageInformationUpdated;
		base.Dispose(disposing);
	}

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
		Report = Package.GetCompatibilityInfo();

		RowStyles.Clear();
		RowCount = 0;
		Controls.Clear(true);

		if (Report == null)
		{
			GenerateSection(Locale.CompatibilityReport, IconManager.GetLargeIcon("I_CompatibilityReport"), FormDesign.Design.ButtonColor, new CompatibilityMessageControl(this, ReportType.Note, new Domain.Compatibility.ReportMessage { Type = ReportType.Note, Message = Locale.CR_NoAvailableReport }));
			return;
		}

		foreach (var item in Report.ReportMessages.GroupBy(x => x.Type).OrderBy(x => x.Key))
		{
			GenerateSection(LocaleHelper.GetGlobalText($"CRT_{item.Key}"), GetTypeIcon(item.Key), GetTypeColor(item), item.Select(x => new CompatibilityMessageControl(this, item.Key, x)).ToArray());
		}
	}

	private Color GetTypeColor(IGrouping<ReportType, ReportMessage> item)
	{
		return item.Max(x => x.Status.Notification).GetColor().MergeColor(BackColor, 15);
	}

	private DynamicIcon GetTypeIcon(ReportType type)
	{
		return type switch
		{
			ReportType.Stability => "I_Stability",
			ReportType.DlcMissing or ReportType.RequiredMods => "I_MissingMod",
			ReportType.UnneededDependency => "I_Disposable",
			ReportType.WorksWhenDisabled => "I_Malicious",
			ReportType.Successors => "I_Upgrade",
			ReportType.Alternatives => "I_Alternatives",
			ReportType.Status => "I_Statuses",
			ReportType.Note => "I_Note",
			ReportType.Recommendations => "I_Recommendations",
			ReportType.Compatibility => "I_Compatibilities",
			_ => "I_CompatibilityReport",
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
