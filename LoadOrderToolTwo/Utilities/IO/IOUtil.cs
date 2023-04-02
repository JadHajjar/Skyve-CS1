using Extensions;

using LoadOrderToolTwo.Utilities.Managers;

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace LoadOrderToolTwo.Utilities.IO;
internal static class IOUtil
{
	public static Process? Execute(string dir, string exeFile, string args, bool useShellExecute = true, bool createNoWindow = false)
	{
		try
		{
			if (!File.Exists(Path.Combine(dir, exeFile)))
			{
				return null;
			}

			var startInfo = new ProcessStartInfo
			{
				WorkingDirectory = dir,
				FileName =Path.Combine(dir, exeFile),
				Arguments = args,
				WindowStyle = ProcessWindowStyle.Normal,
				UseShellExecute = useShellExecute && LocationManager.Platform is not Platform.MacOSX,
				CreateNoWindow = createNoWindow || LocationManager.Platform is not Platform.MacOSX,
			};

			var process = new Process { StartInfo = startInfo };

			try
			{ process.Start(); }
			catch { if (LocationManager.Platform is not Platform.MacOSX) throw; }

			return process;
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
			return null;
		}
	}

	public static string? ToRealPath(string? path)
	{
		if (path is null)
			return null;

		if (LocationManager.Platform is not Platform.Linux)
			return path;

		return path
			.PathReplace(LocationManager.VirtualAppDataPath, LocationManager.AppDataPath)
			.PathReplace(LocationManager.VirtualWorkshopContentPath, LocationManager.WorkshopContentPath)
			.PathReplace(LocationManager.VirtualGamePath, LocationManager.GamePath)
			.Replace("/", "\\");
	}

	public static string? ToVirtualPath(string? path)
	{
		if (path is null)
			return null;

		if (LocationManager.Platform is not Platform.Linux)
			return path;

		return path
			.PathReplace(LocationManager.AppDataPath, LocationManager.VirtualAppDataPath)
			.PathReplace(LocationManager.WorkshopContentPath, LocationManager.VirtualWorkshopContentPath)
			.PathReplace(LocationManager.GamePath, LocationManager.VirtualGamePath)
			.Replace('\\', '/');
	}
}
