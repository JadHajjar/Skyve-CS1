using Extensions;

using SkyveApp.Utilities.Managers;

using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SkyveApp.Utilities.IO;
internal class MacAssemblyUtil
{
	public static bool FindImplementation(string[] dllPaths, out string? dllPath, out Version? version)
	{
		foreach (var path in dllPaths)
		{
			var cache = ContentUtil.GetDllModCache(path, out version);

			if (cache == true || (cache is null && CheckDllImplementsInterface(path, out version)))
			{
				ContentUtil.SetDllModCache(path, true, version);
				dllPath = path;
				return true;
			}

			ContentUtil.SetDllModCache(path, false, null);
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
				case "SkyveInjections.dll":
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
		var process = IOUtil.Execute(LocationManager.Combine(Program.CurrentDirectory, "AssemblyResolver.exe"), string.Join(" ", new string[]
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
