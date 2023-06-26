using Extensions;
using SkyveApp.Services;
using SkyveApp.Services.Interfaces;

using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SkyveApp.Utilities.IO;
public class MacAssemblyUtil
{
	public static bool FindImplementation(string[] dllPaths, out string? dllPath, out Version? version)
	{
		var util = Program.Services.GetService<IContentUtil>();

		foreach (var path in dllPaths)
		{
			var cache = util.GetDllModCache(path, out version);

			if (cache == true || (cache is null && CheckDllImplementsInterface(path, out version)))
			{
				util.SetDllModCache(path, true, version);
				dllPath = path;
				return true;
			}

			util.SetDllModCache(path, false, null);
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

			if (CrossIO.CurrentPlatform is Platform.MacOSX && MacOsResolve(dllPath, out version))
			{
				return true;
			}
		}
		catch { }

		return false;
	}

	public static bool MacOsResolve(string dllPath, out Version? version)
	{
#if DEBUG
		Program.Services.GetService<ILogger>().Debug("Resolving " + dllPath);
#endif
		version = null;
		var locationManager = Program.Services.GetService<ILocationManager>();
		var process = Program.Services.GetService<IOUtil>().Execute(CrossIO.Combine(Program.CurrentDirectory, "AssemblyResolver.exe"), string.Join(" ", new string[]
		{
			dllPath,
			locationManager.ManagedDLL,
			locationManager.ModsPath,
			locationManager.WorkshopContentPath
		}.Select(x => $"\"{x}\"")), false, true);

		if (process is null)
		{
#if DEBUG
			Program.Services.GetService<ILogger>().Debug("Process null");
#endif
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
