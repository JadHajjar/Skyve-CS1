using ColossalFramework.Plugins;

using KianCommons;

using Mono.Cecil;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using static ColossalFramework.Plugins.PluginManager;
using static KianCommons.ReflectionHelpers;

namespace SkyveInjections.Injections;
public static class LoadingApproach
{
	public static Assembly ExistingAssembly(string dllPath)
	{
		Log.Info(ThisMethod + " dllPath=" + dllPath);
		var asm = ReadAssemblyDefinition(dllPath);
		var asmName = new AssemblyName
		{
			Name = asm.Name.Name,
			Version = asm.Name.Version,
		};
		var fullname = asmName.FullName;

		foreach (var asm2 in AppDomain.CurrentDomain.GetAssemblies())
		{
			if (fullname == asm2.GetName().FullName)
			{
				return asm2;
			}
		}
		return null;
	}

	public static AssemblyDefinition ReadAssemblyDefinition(string dllpath)
	{
		try
		{
			var r = MyAssemblyResolver.CreateDefault();
			var asm = AssemblyDefinition.ReadAssembly(dllpath, r.ReaderParameters);
			if (asm == null)
			{
				Log.Info("Assembly Definition at " + dllpath + " failed to load.");
			}

			return asm;
		}
		catch (Exception ex)
		{
			Log.Info("Assembly Definition at " + dllpath + " failed to load.\n" + ex.Message);
			return null;
		}
	}

	public static IEnumerable<TypeDefinition> GetAllCecilTypes(string path)
	{
		var asm = ReadAssemblyDefinition(path);
		foreach (var type in asm.MainModule.Types)
		{
			yield return type;
			foreach (var nestedType in GetNestedCecilTypesRecursive(type))
			{
				yield return nestedType;
			}
		}
	}

	public static IEnumerable<TypeDefinition> GetNestedCecilTypesRecursive(TypeDefinition type)
	{
		foreach (var nestedType in type.NestedTypes)
		{
			yield return nestedType;
			foreach (var nestedType2 in GetNestedCecilTypesRecursive(nestedType))
			{
				yield return nestedType2;
			}
		}
	}

	public static string GetAssemblyLocation(Assembly assembly)
	{
		var name = assembly.FullName;
		foreach (var pair in m_AssemblyLocations)
		{
			var roName = pair.Key.FullName;
			var path = pair.Value;
			if (name == roName)
			{
				return pair.Value;
			}
		}
		return null;
	}

	private static readonly Dictionary<Assembly, string> m_AssemblyLocations =
		GetFieldValue(PluginManager.instance, "m_AssemblyLocations") as Dictionary<Assembly, string>;


	public static IEnumerable<Type> GetExportedTypes(this PluginInfo plugin)
	{
		foreach (var asm in plugin.GetAssemblies())
		{
			foreach (var type in asm.GetExportedTypes())
			{
				yield return type;
			}
		}
	}

	public static IEnumerable<Type> GetAllTypes(this PluginInfo plugin)
	{
		foreach (var asm in plugin.GetAssemblies())
		{
			foreach (var type in asm.GetTypes())
			{
				yield return type;
			}
		}
	}

	public static Type GetImplementationOf(this Assembly asm, Type type)
	{
		foreach (var type2 in asm.GetExportedTypes())
		{
			if (type2.IsClass && !type2.IsAbstract)
			{
				if (type2.GetInterfaces().Contains(type))
				{
					return type2;
				}
			}
		}
		return null;
	}

	public static Type GetImplementationOf(this PluginInfo plugin, Type type)
	{
		foreach (var asm in plugin.GetAssemblies())
		{
			var ret = asm.GetImplementationOf(type);
			if (ret is not null)
			{
				return ret;
			}
		}
		return null;
	}

	public static void CallStaticConstructor(this Type type)
	{
		RuntimeHelpers.RunClassConstructor(type.TypeHandle);
	}

	public static object Instanciate(this Type type)
	{
		var c = type.GetConstructor(new Type[] { });
		return c.Invoke(null);
	}

	public static T CreateImplementation<T>(this PluginInfo plugin)
	{
		return (T)plugin.GetImplementationOf(typeof(T)).Instanciate();
	}
}
