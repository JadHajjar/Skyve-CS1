using Extensions;

using SkyveApp.Domain.Utilities;
using SkyveApp.Services;
using SkyveApp.UserInterface.Generic;
using SkyveApp.Utilities;
using SkyveApp.Utilities.IO;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Panels;
public partial class PC_HelpAndLogs : PanelContent
{
	public PC_HelpAndLogs() : base(true)
	{
		InitializeComponent();

		if (LocationManager.Platform is Platform.Windows)
		{
			DD_LogFile.StartingFolder = LocationManager.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
		}

		foreach (var button in TLP_HelpLogs.GetControls<SlickButton>())
		{
			if (button != B_ChangeLog)
			{
				SlickTip.SetTo(button, LocaleHelper.GetGlobalText($"{button.Text}_Tip"));
			}
		}

		if (LocationManager.Platform is not Platform.Windows)
		{
			B_CopyLogFile.Visible = B_CopyZip.Visible = B_LotLogCopy.Visible = false;
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

		I_Info.Size = UI.Scale(new Size(24, 24), UI.FontScale);
		TLP_Main.Padding = UI.Scale(new Padding(3, 0, 7, 0), UI.FontScale);
		DD_LogFile.Margin = TLP_Errors.Margin = TLP_LogFolders.Margin = TLP_HelpLogs.Margin = UI.Scale(new Padding(10), UI.UIScale);

		foreach (var button in this.GetControls<SlickButton>())
		{
			if (button is not SlickLabel)
			{
				button.Padding = UI.Scale(new Padding(7), UI.FontScale);
				button.Margin = UI.Scale(new Padding(10, 5, 10, 5), UI.UIScale);
			}
		}

		B_CopyLogFile.Margin = B_LotLogCopy.Margin = B_SaveZip.Margin = UI.Scale(new Padding(10, 0, 10, 10), UI.UIScale);
		slickSpacer1.Height = slickSpacer2.Height = slickSpacer3.Height = slickSpacer4.Height = (int)(1.5 * UI.FontScale);
		slickSpacer1.Margin = slickSpacer2.Margin = slickSpacer3.Margin = slickSpacer4.Margin = UI.Scale(new Padding(5), UI.UIScale);
	}

	protected override void DesignChanged(FormDesign design)
	{
		base.DesignChanged(design);

		BackColor = design.AccentBackColor;
		TLP_Errors.BackColor = TLP_LogFolders.BackColor = TLP_HelpLogs.BackColor = design.BackColor.Tint(Lum: design.Type.If(FormDesignType.Dark, 1, -1));
	}

	protected override bool LoadData()
	{
		var tempName = Path.GetTempFileName();

		File.Copy(LogUtil.GameLogFile, tempName, true);

		var logs = LogUtil.SimplifyLog(tempName, out _);

		this.TryInvoke(() => SetTrace(logs));

		return true;
	}

	private async void B_CopyZip_Click(object sender, EventArgs e)
	{
		B_CopyZip.Loading = true;
		await Task.Run(() =>
		{
			try
			{
				LogUtil.CreateZipFileAndSetToClipboard();
			}
			catch (Exception ex) { ShowPrompt(ex, Locale.FailedToFetchLogs); }
		});
		B_CopyZip.Loading = false;

		B_CopyZip.ImageName = "I_Check";
		await Task.Delay(1500);
		B_CopyZip.ImageName = "I_CopyFile";
	}

	private async void B_SaveZip_Click(object sender, EventArgs e)
	{
		B_SaveZip.Loading = true;

		await Task.Run(() =>
		{
			try
			{
				var folder = LocationManager.Combine(LocationManager.SkyveAppDataPath, "Support Logs");

				Directory.CreateDirectory(folder);

				var fileName = LogUtil.CreateZipFileAndSetToClipboard(folder);

				PlatformUtil.OpenFolder(fileName);
			}
			catch (Exception ex) { ShowPrompt(ex, Locale.FailedToFetchLogs); }
		});

		B_SaveZip.Loading = false;

		B_SaveZip.ImageName = "I_Check";
		await Task.Delay(1500);
		B_SaveZip.ImageName = "I_Log";
	}

	private void DD_LogFile_FileSelected(string obj)
	{
		if (!ExtensionClass.FileExists(obj))
		{
			DD_LogFile.SelectedFile = string.Empty;
			return;
		}

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

	private void B_Donate_Click(object sender, EventArgs e)
	{
		PlatformUtil.OpenUrl("https://www.buymeacoffee.com/tdwsvillage");
	}

	private void slickScroll1_Scroll(object sender, ScrollEventArgs e)
	{
		slickSpacer3.Visible = slickScroll1.Percentage != 0;
	}

	private void B_OpenLog_Click(object sender, EventArgs e)
	{
		IOUtil.Execute(LogUtil.GameLogFile, string.Empty);
	}

	private void B_OpenAppData_Click(object sender, EventArgs e)
	{
		PlatformUtil.OpenFolder(LocationManager.AppDataPath);
	}
}
