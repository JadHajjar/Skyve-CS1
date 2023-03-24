using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Utilities;

using System;
using System.Collections.Generic;
using System.IO;

namespace LoadOrderToolTwo.Utilities.Managers;
internal class ModLogicManager
{
    private const ulong CompatibilityReport_STEAM_ID = 2881031511;
    private const string HARMONY_ASSEMBLY = "CitiesHarmony.dll";
    private const string PATCH_ASSEMBLY = "PatchLoaderMod.dll";
    private const string LOM_ASSEMBLY = "LoadOrderModTwo.dll";
    private const string LOM1_ASSEMBLY = "LoadOrderMod.dll";

    private static readonly ModCollection _modCollection = new();

    internal static void Analyze(Mod mod)
    {
        if (mod.SteamId == CompatibilityReport_STEAM_ID)
        {
            CompatibilityManager.LoadCompatibilityReport(mod.Package);
        }

        switch (Path.GetFileName(mod.FileName))
        {
            case HARMONY_ASSEMBLY:
            case PATCH_ASSEMBLY:
            case LOM_ASSEMBLY:
				_modCollection.AddMod(mod);
                break;
        }
    }

    internal static bool IsRequired(Mod mod)
	{
		var list = _modCollection.GetCollection(mod);

        if (list is null)
            return false;

        foreach (var modItem in list)
        {
            if (modItem != mod && modItem.IsIncluded && modItem.IsEnabled)
            {
                return false;
            }
        }

        return true;
    }

    internal static bool IsForbidden(Mod mod)
	{
		return Path.GetFileName(mod.FileName) switch
		{
			LOM1_ASSEMBLY => true,
			_ => false,
		};
	}

    internal static void ModRemoved(Mod mod)
    {
        _modCollection.RemoveMod(mod);
    }
}
