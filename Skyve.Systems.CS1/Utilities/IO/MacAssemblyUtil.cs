using Extensions;

using Skyve.Domain.Systems;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Skyve.Systems.CS1.Utilities.IO;
internal class MacAssemblyUtil
{
	private readonly ILocationManager _locationManager;
	private readonly IModDllManager _contentUtil;
	private readonly IIOUtil _iOUtil;
	private readonly ILogger _logger;

	public MacAssemblyUtil(ILocationManager locationManager, IModDllManager contentUtil, ILogger logger, IIOUtil iOUtil)
	{
		_locationManager = locationManager;
		_contentUtil = contentUtil;
		_logger = logger;
		_iOUtil = iOUtil;
	}

	public bool FindImplementation(string[] dllPaths, out string? dllPath, out Version? version)
	{
		foreach (var path in dllPaths)
		{
			var cache = _contentUtil.GetDllModCache(path, out version);

			if (cache == true || cache is null && CheckDllImplementsInterface(path, out version))
			{
				_contentUtil.SetDllModCache(path, true, version);
				dllPath = path;
				return true;
			}

			_contentUtil.SetDllModCache(path, false, null);
		}

		dllPath = null;
		version = null;

		return false;
	}

	public bool CheckDllImplementsInterface(string dllPath, out Version? version)
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

	public bool MacOsResolve(string dllPath, out Version? version)
	{
#if DEBUG
		_logger.Debug("Resolving " + dllPath);
#endif
		version = null;
		var process = _iOUtil.Execute(CrossIO.Combine(Application.StartupPath, "AssemblyResolver.exe"), string.Join(" ", new string[]
		{
			dllPath,
			_locationManager.ManagedDLL,
			_locationManager.ModsPath,
			_locationManager.WorkshopContentPath
		}.Select(x => $"\"{x}\"")), false, true);

		if (process is null)
		{
#if DEBUG
			_logger.Debug("Process null");
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
