using Extensions;

using LoadOrderToolTwo.Utilities.Managers;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LoadOrderToolTwo.Utilities.IO;
internal static class PlatformUtil
{
	public static void OpenUrl(string? url)
	{
		try
		{
			if (url is null or "")
			{
				return;
			}

			if (CentralManager.SessionSettings.UserSettings.OpenLinksInBrowser)
			{
				Process.Start(url);
				return;
			}

			var domain = Regex.Match(url, @"^(?:https?:\/\/)?(?:[^@\n]+@)?(?:www\.)?([^:\/\n?]+)").Groups[1].Value;

			if (domain.Contains("store.steam", StringComparison.OrdinalIgnoreCase))
			{
				SteamUtil.ExecuteSteam("steam://store/" + Regex.Match(url, "\\d+$").Value);
			}
			else if (domain.Contains("steam", StringComparison.OrdinalIgnoreCase))
			{
				SteamUtil.ExecuteSteam("steam://url/CommunityFilePage/" + Regex.Match(url, "\\d{8,20}").Value);
			}
			else
			{
				Process.Start(url);
			}
		}
		catch (Exception ex) { Log.Exception(ex, $"Failed to open the URL: '{url}'"); }
	}

	public static void OpenFolder(string? folder)
	{
		try
		{
			if (folder is null or "")
			{
				return;
			}

			folder = folder.FormatPath();

			if (LocationManager.FileExists(folder))
			{
				OpenFileInFolder(folder);
				return;
			}

			if (!Directory.Exists(folder))
			{
				Log.Error($"Failed to open the folder: '{folder}' | Directory not found");
				return;
			}

			_ = (char.IsLetter(folder[0]) ? Platform.Windows : LocationManager.Platform) switch
			{
				Platform.MacOSX => Process.Start("/bin/zsh", $"-c \" open -R '{Directory.EnumerateFiles(folder).FirstOrDefault()?.FormatPath() ?? folder}' \""),
				Platform.Linux => Process.Start("/usr/bin/bash", $"-c \" xdg-open '{folder}' \""),
				//Platform.Linux => Process.Start("/usr/bin/xdg-open", folder),
				Platform.Windows or _ => Process.Start(folder),
			};
		}
		catch (Exception ex) { Log.Exception(ex, $"Failed to open the folder: '{folder}'"); }
	}

	private static void OpenFileInFolder(string file)
	{
		try
		{
			_ = (char.IsLetter(file[0]) ? Platform.Windows : LocationManager.Platform) switch
			{
				Platform.MacOSX => Process.Start("/bin/zsh", $"-c \" open -R '{file}' \""),
				Platform.Linux => Process.Start("/usr/bin/bash", $"-c \" xdg-open '{Path.GetDirectoryName(file).FormatPath()}' \""),
				//Platform.Linux => Process.Start("/usr/bin/xdg-open", Path.GetDirectoryName(file).FormatPath()),
				Platform.Windows or _ => Process.Start("explorer.exe", $"/select, \"{file}\""),
			};
		}
		catch (Exception ex) { Log.Exception(ex, $"Failed to open the file: '{file}'"); }
	}

	public static void SetFileInClipboard(string path)
	{
		if (LocationManager.Platform is not Platform.Windows)
		{
			if (path[0] is 'c' or 'C')
			{
				var file = LocationManager.Combine(LocationManager.LotAppDataPath, "Support Logs", Path.GetFileName(path));

				ExtensionClass.CopyFile(path, file, true);

				path = file;
			}

			path = $"file://{IOUtil.ToRealPath(path)}";
		}

		Clipboard.SetData(DataFormats.FileDrop, new[] { path });
	}
}
