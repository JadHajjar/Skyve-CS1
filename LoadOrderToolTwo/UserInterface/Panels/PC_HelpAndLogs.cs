using Extensions;

using LoadOrderToolTwo.Domain.Utilities;
using LoadOrderToolTwo.UserInterface.Generic;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.IO;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Panels;
public partial class PC_HelpAndLogs : PanelContent
{
	public PC_HelpAndLogs() : base(true)
	{
		InitializeComponent();

		DD_LogFile.StartingFolder = LocationManager.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

		foreach (var button in TLP_HelpLogs.GetControls<SlickButton>())
		{
			if (button != B_ChangeLog)
			{
				SlickTip.SetTo(button, LocaleHelper.GetGlobalText($"{button.Text}_Tip"));
			}
		}
	}

	protected override void LocaleChanged()
	{
		Text = Locale.HelpLogs;
		L_Info.Text = Locale.DefaultLogViewInfo;
	}

	public override Color GetTopBarColor()
	{
		return FormDesign.Design.AccentBackColor;
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		TLP_Errors.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_Errors));
		TLP_LogFolders.Image = B_OpenLogFolder.Image = B_LotLog.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_Folder));
		TLP_HelpLogs.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_AskHelp));
		B_CopyZip.Image = B_CopyLogFile.Image = B_LotLogCopy.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_CopyFile));
		B_SaveZip.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_Log));
		B_Discord.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_Discord));
		B_ChangeLog.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_Versions));
		B_Guide.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_Guide));

		TLP_Main.Padding = UI.Scale(new Padding(3, 0, 7, 0), UI.FontScale);
		DD_LogFile.Margin = TLP_Errors.Margin = TLP_LogFolders.Margin = TLP_HelpLogs.Margin = UI.Scale(new Padding(10), UI.UIScale);

		foreach (var button in this.GetControls<SlickButton>())
		{
			button.Padding = UI.Scale(new Padding(7), UI.FontScale);
			button.Margin = UI.Scale(new Padding(10), UI.UIScale);
		}

		B_CopyLogFile.Margin = B_LotLogCopy.Margin = B_SaveZip.Margin = UI.Scale(new Padding(10, 0, 10, 10), UI.UIScale);
		slickSpacer1.Height = slickSpacer2.Height = (int)(1.5 * UI.FontScale);
		slickSpacer1.Margin = slickSpacer2.Margin = UI.Scale(new Padding(5), UI.UIScale);
	}

	protected override void DesignChanged(FormDesign design)
	{
		base.DesignChanged(design);

		BackColor = design.AccentBackColor;
		TLP_Errors.BackColor = TLP_LogFolders.BackColor = TLP_HelpLogs.BackColor = design.BackColor.Tint(Lum: design.Type.If(FormDesignType.Dark, 1, -1));
	}

	protected override bool LoadData()
	{
		var logs = LogUtil.SimplifyLog(LogUtil.GameLogFile, out _);

		this.TryInvoke(() => SetTrace(logs));

		return true;
	}

	private void B_CopyZip_Click(object sender, EventArgs e)
	{
		LogUtil.CreateZipFileAndSetToClipboard();
	}

	private void B_SaveZip_Click(object sender, EventArgs e)
	{
		var folder = LocationManager.Combine(LocationManager.LotAppDataPath, "Support Logs");

		Directory.CreateDirectory(folder);

		var fileName = LogUtil.CreateZipFileAndSetToClipboard(folder);

		try
		{
			PlatformUtil.OpenFolder(fileName);
		}
		catch { }
	}

	private void DD_LogFile_FileSelected(string obj)
	{
		DD_LogFile.Loading = true;

		new BackgroundAction("Simplifying Log", () =>
		{
			var logs = LogUtil.SimplifyLog(obj, out var simpleLog);

			this.TryInvoke(() => SetTrace(logs));

			var simpleLogFile = Path.ChangeExtension(obj, "small" + Path.GetExtension(obj));
			File.WriteAllText(simpleLogFile, simpleLog);

			DD_LogFile.SelectedFile = simpleLogFile;
			DD_LogFile.Loading = false;
		}).Run();
	}

	private void SetTrace(List<LogTrace> logs)
	{
		TLP_Errors.Controls.Clear(true);
		TLP_Errors.Controls.Add(new LogTraceControl(logs));
		TLP_Errors.Visible = logs.Count > 0;
	}

	private bool DD_LogFile_ValidFile(object sender, string arg)
	{
		return arg.ToLower().EndsWith(".log") || arg.ToLower().EndsWith(".txt");
	}

	private void B_OpenLogFolder_Click(object sender, EventArgs e)
	{
		PlatformUtil.OpenFolder(Path.GetDirectoryName(LogUtil.GameLogFile));
	}

	private void B_CopyLogFile_Click(object sender, EventArgs e)
	{
		PlatformUtil.SetFileInClipboard(LogUtil.GameLogFile);
	}

	private void B_LotLog_Click(object sender, EventArgs e)
	{
		PlatformUtil.OpenFolder(Path.GetDirectoryName(Log.LogFilePath));
	}

	private void B_LotLogCopy_Click(object sender, EventArgs e)
	{
		PlatformUtil.SetFileInClipboard(Log.LogFilePath);
	}

	private void B_Discord_Click(object sender, EventArgs e)
	{
		PlatformUtil.OpenUrl("https://discord.gg/E4k8ZEtRxd");
	}

	private void B_Guide_Click(object sender, EventArgs e)
	{
		PlatformUtil.OpenUrl("https://bit.ly/40x93vk");
	}

	private void B_ChangeLog_Click(object sender, EventArgs e)
	{
		Form.PushPanel<PC_LotChangeLog>(null);
	}
}
