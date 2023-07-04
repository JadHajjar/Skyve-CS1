using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Systems;

using System;
using System.Collections.Generic;
using System.Linq;

namespace SkyveApp.Systems.Compatibility;
internal class LoadOrderHelper : ILoadOrderHelper
{
	private readonly ICompatibilityManager _compatibilityManager;
	private readonly IModLogicManager _modLogicManager;
	private readonly IPackageManager _packageManager;

	public LoadOrderHelper(IPackageManager packageManager, ICompatibilityManager compatibilityManager, IModLogicManager modLogicManager)
	{
		_packageManager = packageManager;
		_compatibilityManager = compatibilityManager;
		_modLogicManager = modLogicManager;
	}

	private List<ModInfo> GetEntities()
	{
		var entities = new List<ModInfo>();

		foreach (var mod in _packageManager.Mods)
		{
			var info = _compatibilityManager.GetPackageInfo(mod);

			if (info == null || !(info.Interactions?.Any(x => x.Type == SkyveApp.Domain.Enums.InteractionType.RequiredPackages) ?? false))
			{
				entities.Add(new ModInfo(mod,new IMod[0]));

				continue;
			}

			var interaction = info.Interactions.First(x => x.Type == SkyveApp.Domain.Enums.InteractionType.RequiredPackages);

			entities.Add(new ModInfo(mod, interaction.Packages.SelectWhereNotNull(x => _packageManager.GetPackageById(new GenericPackageIdentity(x))?.Mod).ToArray()!));
		}

		return entities;
	}

	public IEnumerable<IMod> GetOrderedMods()
	{
		var entities = GetEntities();

		// Create a dictionary to map each mod to its required mods
		var modEntityMap = entities.ToDictionary(x => x.Mod);

		// Populate the requiredModMap
		foreach (var entity in entities)
		{
			foreach (var item in entity.RequiredMods)
			{
				Increment(modEntityMap, modEntityMap[item]);
			}
		}

		return entities.OrderByDescending(x => x.Order).Select(x => x.Mod);
	}

	private void Increment(Dictionary<IMod, ModInfo> modEntityMap, ModInfo entity)
	{
		entity.Order++;

		foreach (var item in entity.RequiredMods)
		{
			Increment(modEntityMap, modEntityMap[item]);
		}
	}

	private void Visit(IMod mod, Dictionary<IMod, HashSet<IMod>> requiredModMap, HashSet<IMod> visited, List<IMod> orderedMods)
	{
		if (!visited.Contains(mod))
		{
			visited.Add(mod);

			if (requiredModMap.ContainsKey(mod))
			{
				foreach (var requiredMod in requiredModMap[mod])
				{
					Visit(requiredMod, requiredModMap, visited, orderedMods);
				}
			}

			orderedMods.Add(mod);
		}
	}

	private class ModInfo
	{
		public IMod Mod;
		public int Order;
		public IMod[] RequiredMods;

		public ModInfo(IMod mod, IMod[] requiredMods)
		{
			Mod = mod;
			RequiredMods = requiredMods;
		}
	}
}
