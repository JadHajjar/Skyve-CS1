using Extensions;

using LoadOrderToolTwo.Utilities.Managers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LoadOrderToolTwo.Utilities.IO;
internal class MacAssemblyUtil
{
	public static bool FindImplementation(string[] dllPaths, out string? dllPath, out Version? version)
	{
		try
		{
			foreach (var path in dllPaths)
			{
				if (CheckDllImplementsInterface(path, out version))
				{
					dllPath = path;
					return true;
				}
			}
		}
		finally
		{
		}

		dllPath = null;
		version = null;

		return false;
	}

	public static bool CheckDllImplementsInterface(string dllPath, out Version? version)
	{
		version = null;

		try
		{
			switch (Path.GetFileName(dllPath)) // Ignore known non-mod files
			{
				case "CitiesHarmony.API.dll":
				case "UnifiedUILib.dll":
				case "MoveItIntegration.dll":
				case "System.Xml.Linq.dll":
					return false;
			}

			if (LocationManager.Platform is Platform.MacOSX && MacOsResolve(dllPath, out version))
			{
				return true;
			}
		}
		catch { }

		return false;
	}

	public static bool MacOsResolve(string dllPath, out Version? version)
	{
		version = null;
		var process = IOUtil.Execute(LocationManager.Combine(LocationManager.CurrentDirectory, "AssemblyResolver.exe"), string.Join(" ", new string[]
		{
			dllPath,
			LocationManager.ManagedDLL,
			LocationManager.ModsPath,
			LocationManager.WorkshopContentPath
		}.Select(x => $"\"{x}\"")), false, true);

		if (process is null)
		{
			return false;
		}

		process.WaitForExit();

		var code = process.ExitCode;

		if (code == 1)
		{
			version = AssemblyName.GetAssemblyName(dllPath).Version;
			return true;
		}

		return false;
	}
}
