using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Domain.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;

namespace SkyveApp.Utilities.Managers;
internal class ModLogicManager
{
	private const string HARMONY_ASSEMBLY = "CitiesHarmony.dll";
	private const string PATCH_ASSEMBLY = "PatchLoaderMod.dll";
	private const string Skyve_ASSEMBLY = "SkyveMod.dll";
	private const string LOM2_ASSEMBLY = "LoadOrderModTwo.dll";
	private const string LOM1_ASSEMBLY = "LoadOrderMod.dll";

	private static readonly ModCollection _modCollection = new(GetGroupInfo());

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

	internal static void Analyze(Mod mod)
	{
		_modCollection.CheckAndAdd(mod);

		if (IsForbidden(mod))
		{
			mod.IsIncluded = false;
			mod.IsEnabled = false;
		}
		else if (IsPseudoMod(mod) && CentralManager.SessionSettings.UserSettings.HidePseudoMods)
		{
			mod.IsIncluded = true;
			mod.IsEnabled = true;
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
			{
				continue;
			}

			item[0].IsIncluded = true;
			item[0].IsEnabled = true;
		}
	}

	internal static bool IsPseudoMod(IPackage package)
	{
		if (LocationManager.FileExists(LocationManager.Combine(package.Folder, "ThemeMix.xml")))
		{
			return true;
		}

		if (package.GetCompatibilityInfo().Data?.Package.Type is not null and not Domain.Compatibility.PackageType.GenericPackage and not Domain.Compatibility.PackageType.MusicPack and not Domain.Compatibility.PackageType.CSM and not Domain.Compatibility.PackageType.ContentPackage)
		{
			return true;
		}

		return false;
	}

	internal static bool AreMultipleLOMsPresent()
	{
		return (_modCollection.GetCollection(Skyve_ASSEMBLY, out _)?.Count ?? 0) + (_modCollection.GetCollection(LOM1_ASSEMBLY, out _)?.Count ?? 0) + (_modCollection.GetCollection(LOM2_ASSEMBLY, out _)?.Count ?? 0) > 1;
	}

	internal static IEnumerable<IPackage> GetPackagesThatReference(IPackage package)
	{
		var packages = CentralManager.SessionSettings.UserSettings.ShowAllReferencedPackages ? CentralManager.Packages.ToList() : CentralManager.Packages.AllWhere(x => x.IsIncluded);

		foreach (var p in packages)
		{
			var cr = CompatibilityUtil.GetPackageData(p);

			if (cr is null)
			{
				//if (p.RequiredPackages is not null)
				//{
				//	foreach (var item in p.RequiredPackages)
				//	{
				//		if (CompatibilityUtil.GetFinalSuccessor(item) == package.SteamId)
				//		{
				//			yield return p;

				//			continue;
				//		}
				//	}
				//}

				continue;
			}

			if (cr.Interactions.ContainsKey(Domain.Compatibility.InteractionType.RequiredPackages))
			{
				foreach (var item in cr.Interactions[Domain.Compatibility.InteractionType.RequiredPackages].SelectMany(x => x.Interaction.Packages))
				{
					if (CompatibilityUtil.GetFinalSuccessor(item) == package.SteamId)
					{
						yield return p;

						continue;
					}
				}
			}

			if (cr.Interactions.ContainsKey(Domain.Compatibility.InteractionType.OptionalPackages))
			{
				foreach (var item in cr.Interactions[Domain.Compatibility.InteractionType.OptionalPackages].SelectMany(x => x.Interaction.Packages))
				{
					if (CompatibilityUtil.GetFinalSuccessor(item) == package.SteamId)
					{
						yield return p;

						continue;
					}
				}
			}
		}
	}
}
