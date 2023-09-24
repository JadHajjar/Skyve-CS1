using Extensions;

using Skyve.Domain;
using Skyve.Domain.CS1.Utilities;
using Skyve.Domain.Enums;
using Skyve.Domain.Systems;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Skyve.Systems.CS1.Managers;
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

	public void Analyze(IMod mod, IModUtil modUtil)
	{
		_modCollection.CheckAndAdd(mod);

		if (IsForbidden(mod))
		{
			modUtil.SetIncluded(mod, false);
			modUtil.SetEnabled(mod, false);
		}
		else if (IsPseudoMod(mod) && _settings.UserSettings.HidePseudoMods)
		{
			modUtil.SetIncluded(mod, true);
			modUtil.SetEnabled(mod, true);
		}
	}

	public bool IsRequired(IMod mod, IModUtil modUtil)
	{
		var list = _modCollection.GetCollection(mod, out var collection);

		if (!(collection?.Required ?? false) || list is null)
		{
			return false;
		}

		foreach (var modItem in list)
		{
			if (modItem != mod && modUtil.IsIncluded(modItem) && modUtil.IsEnabled(modItem))
			{
				return false;
			}
		}

		return true;
	}

	public bool IsForbidden(IMod mod)
	{
		var list = _modCollection.GetCollection(mod, out var collection);

		if (!(collection?.Forbidden ?? false) || list is null)
		{
			return false;
		}

		return true;
	}

	public void ModRemoved(IMod mod)
	{
		_modCollection.RemoveMod(mod);
	}

	public void ApplyRequiredStates(IModUtil modUtil)
	{
		foreach (var item in _modCollection.Collections)
		{
			if (item.Any(mod => modUtil.IsIncluded(mod) && modUtil.IsEnabled(mod)))
			{
				continue;
			}

			modUtil.SetIncluded(item[0], true);
			modUtil.SetEnabled(item[0], true);
		}
	}

	public bool IsPseudoMod(IPackage package)
	{
		if (!package.IsLocal && package is ILocalPackage localPackage && CrossIO.FileExists(CrossIO.Combine(localPackage.Folder, "ThemeMix.xml")))
		{
			return true;
		}

		if (package.GetPackageInfo()?.Type is not null and not PackageType.GenericPackage and not PackageType.MusicPack and not PackageType.CSM and not PackageType.ContentPackage)
		{
			return true;
		}

		return false;
	}

	public bool AreMultipleSkyvesPresent(out List<ILocalPackageWithContents> skyveInstances)
	{
		skyveInstances = new();

		skyveInstances.AddRange(_modCollection.GetCollection(Skyve_ASSEMBLY, out _)?.ToList(x => x.LocalParentPackage) ?? new());
		skyveInstances.AddRange(_modCollection.GetCollection(LOM1_ASSEMBLY, out _)?.ToList(x => x.LocalParentPackage) ?? new());
		skyveInstances.AddRange(_modCollection.GetCollection(LOM2_ASSEMBLY, out _)?.ToList(x => x.LocalParentPackage) ?? new());

		return skyveInstances.Count > 1;
	}
}
