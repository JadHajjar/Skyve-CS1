using System;
using System.Diagnostics;
using System.IO;

namespace LoadOrderToolTwo.Utilities.IO;
internal class IOUtil
{
	public static Process? Execute(string dir, string exeFile, string args)
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
				FileName = exeFile,
				Arguments = args,
				WindowStyle = ProcessWindowStyle.Normal,
				UseShellExecute = true,
				CreateNoWindow = false,
			};

			var process = new Process { StartInfo = startInfo };

			process.Start();

			return process;
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
			return null;
		}
	}
}
