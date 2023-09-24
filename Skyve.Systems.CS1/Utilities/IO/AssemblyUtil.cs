using Extensions;

using Mono.Cecil;

using Skyve.Domain.Systems;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Skyve.Systems.CS1.Utilities.IO;
internal class AssemblyUtil
{
	private readonly ILocationManager _locationManager;
	private readonly IModDllManager _contentUtil;
	private readonly ILogger _logger;

	public AssemblyUtil(ILocationManager locationManager, IModDllManager contentUtil, ILogger logger)
	{
		_locationManager = locationManager;
		_contentUtil = contentUtil;
		_logger = logger;
	}

	public AssemblyDefinition? ReadAssemblyDefinition(string dllpath)
	{
		try
		{
			var r = new AssemblyResolver();
			r.AddSearchDirectory(_locationManager.ManagedDLL);
			r.AddSearchDirectory(Path.GetDirectoryName(dllpath));
			var readerParameters = new ReaderParameters
			{
				ReadWrite = false,
				InMemory = true,
				AssemblyResolver = r,
			};
			r.ReaderParameters = readerParameters;
			var asm = AssemblyDefinition.ReadAssembly(dllpath, readerParameters);

			if (asm == null)
			{
				_logger.Info("Assembly Definition at " + dllpath + " failed to load.");
			}

			return asm;
		}
		catch (Exception ex)
		{
			_logger.Info("Assembly Definition at " + dllpath + " failed to load.\n" + ex.Message);
			return null;
		}
	}

	public bool FindImplementation(string[] dllPaths, out string? dllPath, out Version? version)
	{
		foreach (var path in dllPaths)
		{
			var cache = _contentUtil.GetDllModCache(path, out version);

			if (cache == true || cache is null && CheckDllImplementsInterface(path, "ICities.IUserMod", out version))
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

	public bool CheckDllImplementsInterface(string dllPath, string interfaceName, out Version? version)
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

			var asm = ReadAssemblyDefinition(dllPath);
			var userMod = asm is null ? null : GetExportedImplementation(asm, interfaceName);

			version = userMod?.Module?.Assembly?.Name?.Version;

			return userMod != null;
		}
		catch { }

		return false;
	}

	public TypeDefinition GetExportedImplementation(AssemblyDefinition asm, string fullInterfaceName)
	{
		return asm.MainModule.Types.FirstOrDefault(t =>
			!t.IsAbstract && t.IsPublic && ImplementsInterface(t, fullInterfaceName));
	}

	public bool ImplementsInterface(TypeDefinition type, string fullInterfaceName)
	{
		return GetAllInterfaces(type).Any(i => i.FullName == fullInterfaceName);
	}

	public IEnumerable<TypeReference> GetAllInterfaces(TypeDefinition? type)
	{
		while (type != null)
		{
			foreach (var i in type.Interfaces)
			{
				yield return i.InterfaceType;
			}

			try
			{
				type = type.BaseType?.Resolve();
			}
			catch (AssemblyResolutionException)
			{
				type = null;
			}
			catch (Exception ex)
			{
				_logger.Exception(ex, "");
				type = null;
			}
		}
	}

	public byte[]? ReadBytesFromGetManifestResource(string name)
	{
		try
		{
			using var stream = typeof(AssemblyUtil).Assembly.GetManifestResourceStream(name);
			var array = new byte[stream.Length];
			stream.Read(array, 0, array.Length);
			return array;
		}
		catch (Exception ex)
		{
			_logger.Exception(ex, "");
			return null;
		}
	}

	public Assembly? ResolveInterface(object sender, ResolveEventArgs args)
	{
		var name = new AssemblyName(args.Name).Name + ".dll";
		var managedPath = CrossIO.Combine(_locationManager.ManagedDLL, name);

		return File.Exists(managedPath) ? Assembly.LoadFrom(managedPath) : null;
	}

	public Assembly? ReflectionResolveInterface(object sender, ResolveEventArgs args)
	{
		var name = new AssemblyName(args.Name).Name + ".dll";
		var path = CrossIO.Combine(Directory.GetParent(args.RequestingAssembly.Location).FullName, name);
		var managedPath = CrossIO.Combine(_locationManager.ManagedDLL, name);

		if (File.Exists(path))
		{
			return Assembly.ReflectionOnlyLoadFrom(path);
		}

		return File.Exists(managedPath) ? Assembly.ReflectionOnlyLoadFrom(managedPath) : null;
	}

	public string[] kIgnoreAssemblies = new string[]
	{
		"2.5.29.31",
		"2.5.29.32",
		"2.5.29.35",
		"2.5.29.17",
		"1.3.6.1.5.5.7.1.1",
		"MoneyPanel"
	};
}
