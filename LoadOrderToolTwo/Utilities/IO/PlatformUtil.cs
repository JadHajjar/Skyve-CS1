using Extensions;

using LoadOrderToolTwo.Utilities.Managers;

using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace LoadOrderToolTwo.Utilities.IO;
internal static class PlatformUtil
{
	public static void OpenUrl(string? url)
	{
		try
		{
			if (string.IsNullOrEmpty(url))
			{ return; }

			var domain = Regex.Match(url, @"^(?:https?:\/\/)?(?:[^@\n]+@)?(?:www\.)?([^:\/\n?]+)").Groups[1].Value;

			if (domain.Contains("store.steam", StringComparison.OrdinalIgnoreCase))
			{
				Process.Start("steam://store/" + url);
			}
			else if (domain.Contains("steam", StringComparison.OrdinalIgnoreCase))
			{
				Process.Start("steam://url/CommunityFilePage/" + url);
				//	Process.Start("steam://openurl/" + url);
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
			if (string.IsNullOrEmpty(folder))
			{ return; }

			if (File.Exists(folder))
			{
				OpenFileInFolder(folder);
				return;
			}

			if (!Directory.Exists(folder))
			{
				Log.Error($"Failed to open the folder: '{folder}' | Directory not found");
				return;
			}

			_ = LocationManager.Platform switch
			{
				Platform.MacOSX => Process.Start("open", $"-R \"{folder}\""),
				Platform.Linux => Process.Start("xdg-open", folder),
				Platform.Windows or _ => Process.Start(folder),
			};
		}
		catch (Exception ex) { Log.Exception(ex, $"Failed to open the folder: '{folder}'"); }
	}

	private static void OpenFileInFolder(string? file)
	{
		try
		{
			_ = LocationManager.Platform switch
			{
				Platform.MacOSX => Process.Start("explorer.exe", $"/select, \"{file}\""),
				Platform.Linux => Process.Start("xdg-open", Path.GetDirectoryName(file)),
				Platform.Windows or _ => Process.Start(file),
			};
		}
		catch (Exception ex) { Log.Exception(ex, $"Failed to open the file: '{file}'"); }
	}
}
