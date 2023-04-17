using Extensions;

using LoadOrderToolTwo.Utilities.Managers;

using System;
using System.Diagnostics;
using System.IO;

namespace LoadOrderToolTwo.Utilities.IO;
internal static class IOUtil
{
	public static Process? Execute(string exeFile, string args, bool useShellExecute = true, bool createNoWindow = false)
	{
		try
		{
			exeFile = exeFile.FormatPath();

			if (!LocationManager.FileExists(exeFile))
			{
				Log.Error("Execute failed, could not find file: " + exeFile);

				return null;
			}

			Log.Info($"Executing: {exeFile} {args}");

			var startInfo = new ProcessStartInfo
			{
				WorkingDirectory = Path.GetDirectoryName(exeFile).FormatPath(),
				FileName = exeFile,
				Arguments = args,
				WindowStyle = ProcessWindowStyle.Normal,
				UseShellExecute = useShellExecute && LocationManager.Platform is not Platform.MacOSX,
				CreateNoWindow = createNoWindow || LocationManager.Platform is not Platform.MacOSX,
			};

			var process = new Process { StartInfo = startInfo };

			try
			{ process.Start(); }
			catch
			{
				if (LocationManager.Platform is not Platform.MacOSX)
				{
					throw;
				}
			}

			return process;
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
			return null;
		}
	}

	public static void RunBatch(string command)
	{
		try
		{
			File.WriteAllText(LocationManager.Combine(Program.CurrentDirectory, "batch.bat"), command);

			Execute(LocationManager.Combine(Program.CurrentDirectory, "batch.bat"), "", true, true);
		}
		catch (Exception ex) { Log.Exception(ex, "Failed to start batch to restart the tool after update"); }
	}

	public static void WaitForUpdate()
	{
		RunBatch($"@echo off\r\nsetlocal\r\n\r\nset FILENAME=LoadOrderToolTwo.exe\r\nset MAX_WAIT_TIME_SECONDS=15\r\n\r\nset FILE_LAST_WRITE_TIME=\r\nfor /f \"usebackq\" %%i in (`dir /b /a-d \"%FILENAME%\"`) do (\r\n  set FILE_LAST_WRITE_TIME=%%~ti\r\n)\r\n\r\nif not defined FILE_LAST_WRITE_TIME (\r\n  echo Error: File \"%FILENAME%\" not found.\r\n  exit /b 1\r\n)\r\n\r\necho {Locale.UpdatingLot}\r\nset WAIT_START_TIME=%TIME%\r\n:WAIT_LOOP\r\nping -n 2 127.0.0.1 > nul\r\nfor /f \"usebackq\" %%i in (`dir /b /a-d \"%FILENAME%\"`) do (\r\n  if not \"%%~ti\"==\"%FILE_LAST_WRITE_TIME%\" (\r\n    echo File \"%FILENAME%\" has changed. Launching...\r\n    start \"\" \"%FILENAME%\"\r\n    exit /b 0\r\n  start %FILENAME%\r\n)\r\n)\r\nset WAIT_CURRENT_TIME=%TIME%\r\nset /a WAIT_TIME_SECONDS=(1%WAIT_CURRENT_TIME:~6,2%-1%WAIT_START_TIME:~6,2%)+(1%WAIT_CURRENT_TIME:~3,2%-1%WAIT_START_TIME:~3,2%)*60+(1%WAIT_CURRENT_TIME:~0,2%-1%WAIT_START_TIME:~0,2%)\r\nif %WAIT_TIME_SECONDS% gtr %MAX_WAIT_TIME_SECONDS% (\r\n  echo Error: Wait time exceeded %MAX_WAIT_TIME_SECONDS% seconds.\r\n  exit /b 1\r\n  start %FILENAME%\r\n)\r\ngoto WAIT_LOOP\r\n");
	}

	public static string? ToRealPath(string? path)
	{
		if (path is null)
		{
			return null;
		}

		if (LocationManager.Platform is not Platform.Windows && char.IsLetter(path[0]))
		{
			return path.Substring(2).FormatPath();
		}

		return path.FormatPath();
	}
}
