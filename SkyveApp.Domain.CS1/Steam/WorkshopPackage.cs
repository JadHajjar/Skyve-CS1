using SkyveApp.Domain.CS1.Enums;
using SkyveApp.Domain.Systems;
using SkyveApp.Systems;

using System.Collections.Generic;
using System.Linq;

namespace SkyveApp.Domain.CS1.Steam;
public class WorkshopPackage : IPackage
{
	private readonly IWorkshopInfo? _info;

	public WorkshopPackage(ulong id)
	{
		Id = id;
		Url = $"https://steamcommunity.com/workshop/filedetails/?id={Id}";
	}

	public WorkshopPackage(IWorkshopInfo info) : this(info.Id)
	{
		_info = info;
	}

	public ulong Id { get; }
	public string Url { get; }
	public bool IsLocal { get; }
	public bool IsBuiltIn { get; }
	public string Name => GetInfo()?.Name ?? ServiceCenter.Get<ILocale>().Get("UnknownPackage");
	public bool IsMod => GetInfo()?.IsMod ?? false;
	public ILocalPackageWithContents? LocalPackage => ServiceCenter.Get<IPackageManager>().GetPackageById(this);
	public IEnumerable<IPackageRequirement> Requirements => GetInfo()?.Requirements ?? Enumerable.Empty<IPackageRequirement>();
	public IEnumerable<ITag> Tags => GetInfo()?.Tags.Select(x => (ITag)new TagItem(TagSource.Workshop, x)) ?? Enumerable.Empty<ITag>();

	private IWorkshopInfo? GetInfo()
	{
		return _info ?? this.GetWorkshopInfo();
	}
}
