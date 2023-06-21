using CitiesHarmony.API;

using HarmonyLib;

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using static KianCommons.ReflectionHelpers;

namespace KianCommons;
public static class HarmonyUtil
{
	static bool harmonyInstalled_ = false;
	const string errorMessage_ =
				"****** ERRRROOORRRRRR!!!!!!!!!! **************\n" +
				"**********************************************\n" +
				"    HARMONY MOD DEPENDANCY IS NOT INSTALLED!\n\n" +
				"solution:\n" +
				" - exit to desktop.\n" +
				" - unsub harmony mod.\n" +
				" - make sure harmony mod is deleted from the content folder\n" +
				" - resub to harmony mod.\n" +
				" - run the game again.\n" +
				"**********************************************\n" +
				"**********************************************\n";

	internal static void AssertCitiesHarmonyInstalled()
	{
		if (!HarmonyHelper.IsHarmonyInstalled)
		{
			throw new Exception(errorMessage_);
		}
	}

	internal static void InstallHarmony(string harmonyID)
	{
		try
		{
			if (harmonyInstalled_)
			{
				Log.Info("skipping harmony installation because its already installed");
				return;
			}
			AssertCitiesHarmonyInstalled();
			Log.Info("Patching...");
			PatchAll(harmonyID);
			harmonyInstalled_ = true;
			Log.Info("Patched.");
		}
		catch (TypeLoadException ex)
		{
			Log.Exception(new TypeLoadException(errorMessage_, ex));
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
		}
	}

	/// <typeparam name="T">Only install classes with this attribute</typeparam>
	internal static void InstallHarmony<T>(string harmonyID) where T : Attribute
	{
		try
		{
			AssertCitiesHarmonyInstalled();
			Log.Info("Patching...");
			PatchAll(harmonyID, required: typeof(T));
			Log.Info("Patched.");
		}
		catch (TypeLoadException ex)
		{
			Log.Exception(new TypeLoadException(errorMessage_, ex));
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
		}
	}

	internal static void InstallHarmony(
		string harmonyID,
		Type required = null,
		Type forbidden = null)
	{
		try
		{
			AssertCitiesHarmonyInstalled();
			Log.Info("Patching...");
			PatchAll(harmonyID, required: required, forbidden: forbidden);
			Log.Info("Patched.");
		}
		catch (TypeLoadException ex)
		{
			Log.Exception(new TypeLoadException(errorMessage_, ex));
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
		}
	}


	/// <summary>
	/// assertion shall take place in a function that does not reference Harmony.
	/// </summary>
	/// <param name="harmonyID"></param>
	[MethodImpl(MethodImplOptions.NoInlining)]
	static void PatchAll(string harmonyID)
	{
		var harmony = new Harmony(harmonyID);
		harmony.PatchAll();
		harmony.LogPatchedMethods();
	}

	/// <summary>
	/// assertion shall take place in a function that does not reference Harmony.
	/// Only install classes with this attribute
	/// </summary>
	/// <param name="harmonyID"></param>
	[MethodImpl(MethodImplOptions.NoInlining)]
	static void PatchAll(string harmonyID, Type required = null, Type forbidden = null)
	{
		try
		{
			var harmony = new Harmony(harmonyID);
			var assembly = Assembly.GetExecutingAssembly();
			foreach (var type in AccessTools.GetTypesFromAssembly(assembly))
			{
				try
				{
					if (required is not null && !type.HasAttribute(required))
					{
						continue;
					}

					if (forbidden is not null && type.HasAttribute(forbidden))
					{
						continue;
					}

					if (type.HasAttribute<HarmonyPatch>())
					{
						Log.Info($"applying {type.FullName} ...");
					}

					var patchedMethods = harmony.CreateClassProcessor(type).Patch();
					if (!patchedMethods.IsNullorEmpty())
					{
						var strPatchedMethods = patchedMethods
							.Select(item => item.DeclaringType + "." + item.Name)
							.Join(", ");
						Log.Info($"{type.FullName} successfully patched : {strPatchedMethods}");
					}
				}
				catch (Exception ex)
				{
					Log.Exception(new Exception($"{type} failed.", ex));
				}
			}
			harmony.LogPatchedMethods();
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
		}
	}

	public static void LogPatchedMethods(this Harmony harmony)
	{
		foreach (var method in harmony.GetPatchedMethods())
		{
			Log.Info($"harmony({harmony.Id}) patched: {method.DeclaringType.FullName}::{method.Name}", true);
		}
	}

	internal static void UninstallHarmony(string harmonyID)
	{
		AssertCitiesHarmonyInstalled();
		Log.Info("UnPatching...");
		UnpatchAll(harmonyID);
		harmonyInstalled_ = false;
		Log.Info("UnPatched.");
	}

	/// <summary>
	/// assertion shall take place in a function that does not reference Harmony.
	/// </summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	static void UnpatchAll(string harmonyID)
	{
		var harmony = new Harmony(harmonyID);
		harmony.UnpatchAll(harmonyID);
	}

	internal static void ManualPatch<T>(string harmonyID)
	{
		AssertCitiesHarmonyInstalled();
		ManualPatchUnSafe(typeof(T), harmonyID);
	}
	internal static void ManualPatch(Type t, string harmonyID)
	{
		AssertCitiesHarmonyInstalled();
		ManualPatchUnSafe(t, harmonyID);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	static void ManualPatchUnSafe(Type t, string harmonyID)
	{
		try
		{
			var targetMethod =
				InvokeMethod(t, "TargetMethod") as MethodBase;
			Log.Info($"{t.FullName}.TorgetMethod()->{targetMethod}", true);
			Assertion.AssertNotNull(targetMethod, $"{t.FullName}.TargetMethod() returned null");
			var prefix = GetHarmonyMethod(t, "Prefix");
			var postfix = GetHarmonyMethod(t, "Postfix");
			var transpiler = GetHarmonyMethod(t, "Transpiler");
			var finalizer = GetHarmonyMethod(t, "Finalizer");
			var harmony = new Harmony(harmonyID);
			harmony.Patch(original: targetMethod, prefix: prefix, postfix: postfix, transpiler: transpiler, finalizer: finalizer);
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
	}

	public static HarmonyMethod GetHarmonyMethod(Type t, string name)
	{
		var m = GetMethod(t, name, throwOnError: false);
		if (m == null)
		{
			return null;
		}

		Assertion.Assert(m.IsStatic, $"{m}.IsStatic");
		return new HarmonyMethod(m);
	}
}