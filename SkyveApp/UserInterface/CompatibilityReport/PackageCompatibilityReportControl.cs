using Extensions;

using SkyveApp.Domain.Compatibility;
using SkyveApp.Domain.Compatibility.Enums;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Utilities;
using SkyveApp.Utilities.Managers;

using SlickControls;

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.CompatibilityReport;
internal class PackageCompatibilityReportControl : TableLayoutPanel
{
	private readonly TableLayoutPanel[] _panels;
	private int controlCount;

	public PackageCompatibilityReportControl(IPackage package)
	{
		Package = package;
		AutoSize = true;
		AutoSizeMode = AutoSizeMode.GrowAndShrink;
		ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 / 3F));
		ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 / 3F));
		ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 / 3F));
		ColumnCount = 3;

		_panels = new TableLayoutPanel[ColumnCount];
		for (var i = 0; i < ColumnCount; i++)
		{
			_panels[i] = new()
			{
				Dock = DockStyle.Top,
				AutoSize = true,
				AutoSizeMode = AutoSizeMode.GrowAndShrink
			};

			_panels[i].ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			_panels[i].ColumnCount = 1;

			Controls.Add(_panels[i], i, 0);
		}

		CompatibilityManager.ReportProcessed += CentralManager_PackageInformationUpdated;
	}

	private void CentralManager_PackageInformationUpdated()
	{
		this.TryInvoke(Reset);
	}

	public IPackage Package { get; }
	public CompatibilityInfo? Report { get; private set; }

	protected override void Dispose(bool disposing)
	{
		CompatibilityManager.ReportProcessed -= CentralManager_PackageInformationUpdated;
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
		try
		{
			this.SuspendDrawing();

			lock (this)
			{
				Report = Package.GetCompatibilityInfo(true);

				for (var i = 0; i < _panels.Length; i++)
				{
					_panels[i].Controls.Clear(true);
					_panels[i].RowStyles.Clear();
				}

				controlCount = 0;

				foreach (var item in Report.ReportItems.GroupBy(x => x.Type).OrderBy(x => x.Key is not ReportType.Stability).ThenByDescending(x => x.Max(y => y.Status.Notification)).ThenByDescending(x => x.Sum(y => y.Packages.Length)))
				{
					var controls = item.ToList(x => new CompatibilityMessageControl(this, item.Key, x));

					GenerateSection(LocaleHelper.GetGlobalText($"CRT_{item.Key}"), GetTypeIcon(item.Key), GetTypeColor(item), controls);
				}

				ColumnStyles[2].Width = controlCount > 2 ? 100/3F : 0;
			}
		}
		finally
		{
			this.ResumeDrawing();
		}
	}

	private Color GetTypeColor(IGrouping<ReportType, ReportItem> item)
	{
		return item.Max(x => x.Status.Notification).GetColor().MergeColor(BackColor, 15);
	}

	private DynamicIcon GetTypeIcon(ReportType type)
	{
		return type switch
		{
			ReportType.Stability => "I_Stability",
			ReportType.DlcMissing or ReportType.RequiredPackages => "I_MissingMod",
			ReportType.Ambiguous => "I_Malicious",
			ReportType.Successors => "I_Upgrade",
			ReportType.Alternatives => "I_Alternatives",
			ReportType.Status => "I_Statuses",
			ReportType.OptionalPackages => "I_Recommendations",
			ReportType.Compatibility => "I_Compatibilities",
			_ => "I_CompatibilityReport",
		};
	}

	private void GenerateSection(string title, Bitmap image, Color backColor, List<CompatibilityMessageControl> controls)
	{
		if (controls.Count == 0)
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

		_panels[controlCount % 3].Controls.Add(tlp);

		controlCount++;
	}
}
