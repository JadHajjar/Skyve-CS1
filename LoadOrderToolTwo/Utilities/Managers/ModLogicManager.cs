using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Utilities;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LoadOrderToolTwo.Utilities.Managers;
internal class ModLogicManager
{
	private const ulong CompatibilityReport_STEAM_ID = 2881031511;
	private const string HARMONY_ASSEMBLY = "CitiesHarmony.dll";
	private const string PATCH_ASSEMBLY = "PatchLoaderMod.dll";
	private const string LOM_ASSEMBLY = "LoadOrderModTwo.dll";
	private const string LOM1_ASSEMBLY = "LoadOrderMod.dll";

	private static readonly ModCollection _modCollection = new(GetGroupInfo());

	private static Dictionary<string, CollectionInfo> GetGroupInfo()
	{
		return new()
		{
			[HARMONY_ASSEMBLY] = new() { Required = true },
			[PATCH_ASSEMBLY] = new() { Required = true },
			[LOM_ASSEMBLY] = new() { Required = true },
			[LOM1_ASSEMBLY] = new() { Required = false },
		};
	}

	internal static readonly ulong[] BlackList = new ulong[]
	{
		2620852727,
		2448824112,
	};

	internal static void Analyze(Mod mod)
	{
		if (mod.SteamId == CompatibilityReport_STEAM_ID)
		{
			CompatibilityManager.LoadCompatibilityReport(mod.Package);
		}

		_modCollection.CheckAndAdd(mod);

		if (IsForbidden(mod))
		{
			mod.IsIncluded = false;
			mod.IsEnabled = false;
		}

		if (IsPseudoMod(mod))
		{
			mod.IsPseudoMod = true;

			if (CentralManager.SessionSettings.UserSettings.HidePseudoMods)
			{
				mod.IsIncluded = true;
				mod.IsEnabled = true;
			}
		}
	}

	internal static bool IsRequired(Mod mod)
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

	internal static bool IsForbidden(Mod mod)
	{
		var list = _modCollection.GetCollection(mod, out var collection);

		if (!(collection?.Forbidden ?? false) || list is null)
		{
			return false;
		}

		return true;
	}

	internal static void ModRemoved(Mod mod)
	{
		_modCollection.RemoveMod(mod);
	}

	internal static void ApplyRequiredStates()
	{
		foreach (var item in _modCollection.Collections)
		{
			if (item.Any(x => x.IsIncluded && x.IsEnabled))
				continue;

			item[0].IsIncluded = true;
			item[0].IsEnabled = true;
		}
	}

	internal static bool IsPseudoMod(Mod mod)
	{
		if (File.Exists(Path.Combine(mod.Folder, "ThemeMix.xml")))
			return true;

		return false;
	}

	internal static bool AreMultipleLOMsPresent()
	{
		return (_modCollection.GetCollection(LOM1_ASSEMBLY, out _)?.Count ?? 0) + (_modCollection.GetCollection(LOM_ASSEMBLY, out _)?.Count ?? 0) > 1;
	}

	internal static bool IsBlackListed(ulong steamId)
	{
		return BlackList.Contains(steamId);
	}
}
