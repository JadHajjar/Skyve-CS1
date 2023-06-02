using HarmonyLib;

using KianCommons;
using KianCommons.Patches;

using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SkyveMod.Patches.ContentManager;
/// <summary>
/// Options panel Skips plugins with no user mod to avoid errors.
/// </summary>
// [HarmonyPatch(typeof(OptionsMainPanel), "AddUserMods")]
public static class OptionsMainPanel_AddUserMods_Patch
{
	public static IEnumerable<CodeInstruction> Transpiler(ILGenerator il, IEnumerable<CodeInstruction> instructions)
	{
		try
		{
			var codes = instructions.ToCodeList();
			var iBrMoveNext = codes.Search(c => c.opcode == OpCodes.Br);
			codes[iBrMoveNext].Branches(out var lblContinue);
			var iLdlen = codes.Search(c => c.opcode == OpCodes.Ldlen);

			// if Instances.Lenght == 0 then continue; 
			codes.InsertInstructions(iLdlen + 1, new[] {
					new CodeInstruction(OpCodes.Dup), //duplicate Instances.Lenght
                    new CodeInstruction(OpCodes.Ldc_I4_0),
					new CodeInstruction(OpCodes.Beq, lblContinue),
				});
			return codes;
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
			throw ex;
		}
	}
}
