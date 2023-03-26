using Extensions;

using SlickControls;

using System;
using System.Diagnostics;
using System.IO;

using static System.Environment;

namespace LoadOrderToolTwo.UserInterface.Panels;
public partial class PC_HelpAndLogs : PanelContent
{
	public PC_HelpAndLogs()
	{
		InitializeComponent();
	}

	private void slickButton1_Click(object sender, EventArgs e)
	{
		Process.Start(Path.Combine(GetFolderPath(SpecialFolder.LocalApplicationData), ISave.AppName, "Logs"));
	}
}
