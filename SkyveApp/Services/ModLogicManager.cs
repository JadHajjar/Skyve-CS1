using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Compatibility.Enums;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Domain.Utilities;
using SkyveApp.Services.Interfaces;
using SkyveApp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SkyveApp.Services;
internal class ModLogicManager : IModLogicManager
{
    private const string HARMONY_ASSEMBLY = "CitiesHarmony.dll";
    private const string PATCH_ASSEMBLY = "PatchLoaderMod.dll";
    private const string Skyve_ASSEMBLY = "SkyveMod.dll";
    private const string LOM2_ASSEMBLY = "LoadOrderModTwo.dll";
    private const string LOM1_ASSEMBLY = "LoadOrderMod.dll";

    private readonly ModCollection _modCollection = new(GetGroupInfo());

    private static Dictionary<string, CollectionInfo> GetGroupInfo()
    {
        return new(StringComparer.OrdinalIgnoreCase)
        {
            [HARMONY_ASSEMBLY] = new() { Required = true },
            [PATCH_ASSEMBLY] = new() { Required = true },
            [Skyve_ASSEMBLY] = new() { Required = true },
            [LOM2_ASSEMBLY] = new() { Required = false },
            [LOM1_ASSEMBLY] = new() { Required = false },
        };
    }

    private readonly ISettings _settings;

	public ModLogicManager(ISettings settings)
	{
		_settings = settings;
	}

	public void Analyze(Mod mod)
    {
        _modCollection.CheckAndAdd(mod);

        if (IsForbidden(mod))
        {
            mod.IsIncluded = false;
            mod.IsEnabled = false;
        }
        else if (IsPseudoMod(mod) && _settings.SessionSettings.UserSettings.HidePseudoMods)
        {
            mod.IsIncluded = true;
            mod.IsEnabled = true;
        }
    }

	public bool IsRequired(Mod mod)
    {
        var list = _modCollection.GetCollection(mod, out var collection);

        if (!(collection?.Required ?? false) || list is null)
        {
            return false;
        }

        foreach (var modItem in list)
        {
            if (modItem != mod && modItem.IsIncluded && modItem.IsEnabled)
            {
                return false;
            }
        }

        return true;
    }

	public bool IsForbidden(Mod mod)
    {
        var list = _modCollection.GetCollection(mod, out var collection);

        if (!(collection?.Forbidden ?? false) || list is null)
        {
            return false;
        }

        return true;
    }

	public void ModRemoved(Mod mod)
    {
        _modCollection.RemoveMod(mod);
    }

	public void ApplyRequiredStates()
    {
        foreach (var item in _modCollection.Collections)
        {
            if (item.Any(x => x.IsIncluded && x.IsEnabled))
            {
                continue;
            }

            item[0].IsIncluded = true;
            item[0].IsEnabled = true;
        }
    }

	public bool IsPseudoMod(IPackage package)
    {
        if (CrossIO.FileExists(CrossIO.Combine(package.Folder, "ThemeMix.xml")))
        {
            return true;
        }

        if (package.GetCompatibilityInfo().Data?.Package.Type is not null and not PackageType.GenericPackage and not PackageType.MusicPack and not PackageType.CSM and not PackageType.ContentPackage)
        {
            return true;
        }

        return false;
    }

	public bool AreMultipleLOMsPresent()
    {
        return (_modCollection.GetCollection(Skyve_ASSEMBLY, out _)?.Count ?? 0) + (_modCollection.GetCollection(LOM1_ASSEMBLY, out _)?.Count ?? 0) + (_modCollection.GetCollection(LOM2_ASSEMBLY, out _)?.Count ?? 0) > 1;
    }
}
