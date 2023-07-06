using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Systems;

using System.Collections.Generic;
using System.Linq;

namespace SkyveApp.Systems.Compatibility;
internal class LoadOrderHelper : ILoadOrderHelper
{
	private readonly CompatibilityManager _compatibilityManager;
	private readonly IModLogicManager _modLogicManager;
	private readonly CompatibilityHelper _compatibilityHelper;
	private readonly IPackageManager _packageManager;

	public LoadOrderHelper(IPackageManager packageManager, ICompatibilityManager compatibilityManager, IModLogicManager modLogicManager, IPackageUtil packageUtil, IPackageNameUtil packageNameUtil, IWorkshopService workshopService, ILocale locale)
	{
		_packageManager = packageManager;
		_compatibilityManager = (CompatibilityManager)compatibilityManager;
		_modLogicManager = modLogicManager;
		_compatibilityHelper = new CompatibilityHelper(_compatibilityManager, packageManager, packageUtil, packageNameUtil, workshopService, locale);
	}

	private List<ModInfo> GetEntities()
	{
		var entities = new List<ModInfo>();

		foreach (var mod in _packageManager.Mods)
		{
			var info = _compatibilityManager.GetPackageInfo(mod);

			var interaction = info?.Interactions?.FirstOrDefault(x => x.Type == SkyveApp.Domain.Enums.InteractionType.RequiredPackages);
			var loadOrder = info?.Interactions?.FirstOrDefault(x => x.Type == SkyveApp.Domain.Enums.InteractionType.LoadAfter);

			entities.Add(new ModInfo(mod
				, (interaction?.Packages.SelectWhereNotNull(x => (IPackageIdentity)new GenericPackageIdentity(x)).ToArray() ?? new IPackageIdentity[0])!
				, (loadOrder?.Packages.SelectWhereNotNull(x => (IPackageIdentity)new GenericPackageIdentity(x)).ToArray() ?? new IPackageIdentity[0])!));
		}

		return entities;
	}

	public IEnumerable<IMod> GetOrderedMods()
	{
		var entities = GetEntities();

		foreach (var entity in entities)
		{
			foreach (var item in entity.RequiredMods)
			{
				Increment(entities, item, entity.Mod);
			}
		}

		foreach (var entity in entities)
		{
			if (entity.LoadAfterMods.Length > 0)
			{
				entity.Order = entity.LoadAfterMods.SelectMany(x => GetEntity(x.Id, entities, entity.Mod)).Min(x => x.Order) - 1;
			}
		}

		return entities.OrderByDescending(x => x.Order).Select(x => x.Mod);
	}

	private void Increment(List<ModInfo> modEntityMap, IPackageIdentity identity, IPackageIdentity original)
	{
		foreach (var entity in GetEntity(identity.Id, modEntityMap, original))
		{
			entity.Order += 100;

			foreach (var item in entity.RequiredMods)
			{
				Increment(modEntityMap, item, identity);
			}
		}
	}

	private IEnumerable<ModInfo> GetEntity(ulong steamId, List<ModInfo> modEntityMap, IPackageIdentity original)
	{
		var indexedPackage = _compatibilityManager.CompatibilityData.Packages.TryGet(steamId);

		foreach (var item in modEntityMap.Where(x => x.Mod.Id == steamId))
		{
			yield return item;
		}

		if (indexedPackage is null)
		{
			yield break;
		}

		foreach (var item in indexedPackage.Group)
		{
			if (item.Key != steamId)
			{
				foreach (var package in _compatibilityHelper.FindPackage(item.Value, true))
				{
					if (original.Id != package.Id)
					{
						foreach (var entity in modEntityMap.Where(x => x.Mod.Id == package.Id))
						{
							yield return entity;
						}
					}
				}
			}
		}

		foreach (var package in _compatibilityHelper.FindPackage(indexedPackage, true))
		{
			if (original.Id != package.Id)
			{
				foreach (var entity in modEntityMap.Where(x => x.Mod.Id == package.Id))
				{
					yield return entity;
				}
			}
		}
	}

	private class ModInfo
	{
		public int Order;
		public IMod Mod;
		public IPackageIdentity[] RequiredMods;
		public IPackageIdentity[] LoadAfterMods;

		public ModInfo(IMod mod, IPackageIdentity[] requiredMods, IPackageIdentity[] afterLoadMods)
		{
			Mod = mod;
			RequiredMods = requiredMods;
			LoadAfterMods = afterLoadMods;
		}
	}
}
