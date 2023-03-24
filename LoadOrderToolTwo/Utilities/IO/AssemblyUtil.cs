using LoadOrderToolTwo.Utilities.Managers;

using Mono.Cecil;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LoadOrderToolTwo.Utilities.IO;
public static class AssemblyUtil
{
	public static AssemblyDefinition? ReadAssemblyDefinition(string dllpath)
	{
		try
		{
			var r = new AssemblyResolver();
			r.AddSearchDirectory(LocationManager.ManagedDLL);
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
				Log.Info("Assembly Definition at " + dllpath + " failed to load.", false);
			}

			return asm;
		}
		catch (Exception ex)
		{
			Log.Info("Assembly Definition at " + dllpath + " failed to load.\n" + ex.Message, false);
			return null;
		}
	}

	public static bool FindImplementation(string[] dllPaths, string fullInterfaceName, out string? dllPath, out Version? version)
	{
		try
		{
			foreach (var path in dllPaths)
			{
				if (CheckDllImplementsInterface(path, fullInterfaceName, out version))
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

	public static bool CheckDllImplementsInterface(string dllPath, string interfaceName, out Version? version)
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

			var asm = ReadAssemblyDefinition(dllPath);
			var userMod = asm?.GetExportedImplementation(interfaceName);

			version = userMod?.Module?.Assembly?.Name?.Version;

			return userMod != null;
		}
		catch { }

		return false;
	}


	public static TypeDefinition GetExportedImplementation(this AssemblyDefinition asm, string fullInterfaceName)
	{
		return asm.MainModule.Types.FirstOrDefault(t =>
			!t.IsAbstract && t.IsPublic && t.ImplementsInterface(fullInterfaceName));
	}

	public static bool ImplementsInterface(this TypeDefinition type, string fullInterfaceName)
	{
		return type.GetAllInterfaces().Any(i => i.FullName == fullInterfaceName);
	}

	public static IEnumerable<TypeReference> GetAllInterfaces(this TypeDefinition? type)
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
				ex.Log(false);
				type = null;
			}
		}
	}

	public static byte[]? ReadBytesFromGetManifestResource(string name)
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
			Log.Exception(ex);
			return null;
		}
	}

	internal static Assembly? ResolveInterface(object sender, ResolveEventArgs args)
	{
		var name = new AssemblyName(args.Name).Name + ".dll";
		var managedPath = Path.Combine(LocationManager.ManagedDLL, name);

		if (File.Exists(managedPath))
		{
			return Assembly.LoadFrom(managedPath);
		}

		return null;
	}

	internal static Assembly? ReflectionResolveInterface(object sender, ResolveEventArgs args)
	{
		var name = new AssemblyName(args.Name).Name + ".dll";
		var path = Path.Combine(Directory.GetParent(args.RequestingAssembly.Location).FullName, name);
		var managedPath = Path.Combine(LocationManager.ManagedDLL, name);

		if (File.Exists(path))
		{
			return Assembly.ReflectionOnlyLoadFrom(path);
		}

		if (File.Exists(managedPath))
		{
			return Assembly.ReflectionOnlyLoadFrom(managedPath);
		}

		return null;
	}

	internal static string[] kIgnoreAssemblies = new string[]
	{
		"2.5.29.31",
		"2.5.29.32",
		"2.5.29.35",
		"2.5.29.17",
		"1.3.6.1.5.5.7.1.1",
		"MoneyPanel"
	};

	internal static Assembly? CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
	{
		Log.Called();
		var name0 = args.Name;
		if (kIgnoreAssemblies.Contains(name0))
		{
			return null;
		}

		var assemblies = AppDomain.CurrentDomain.GetAssemblies();
		Assembly? ret = null;
		foreach (var asm in assemblies)
		{
			var name = asm.GetName().Name;
			if (name0.StartsWith(name))
			{
				// get latest assembly
				if (ret == null || ret.GetName().Version < asm.GetName().Version)
				{
					ret = asm;
				}
			}
		}
		if (ret != null)
		{
			Log.Info($"Assembly '{name0}' resolved to '{ret}'", false);
		}
		else
		{
			if (name0 == "Mono.Runtime")
			{
				Log.Info($"[harmless] Assembly resolution failure. No assembly named '{name0}' was found.", false);
			}
			else
			{
				Log.Error($"Assembly resolution failure. No assembly named '{name0}' was found.");
			}
		}
		return ret;
	}
}
