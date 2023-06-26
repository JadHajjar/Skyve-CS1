using SkyveApp.Domain.Systems;
using SkyveApp.Systems;
using SkyveApp.Utilities;

using System.Collections.Generic;
using System.Linq;

namespace SkyveApp.Domain.Steam;
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
	public string Name => GetWorkshopInfo()?.Title ?? Locale.UnknownPackage;
	public bool IsMod => GetWorkshopInfo()?.IsMod ?? false;
	public ILocalPackage? LocalPackage => Program.Services.GetService<IContentManager>().GetPackageById(Id);
	public IEnumerable<IPackageRequirement> Requirements => GetWorkshopInfo()?.Requirements ?? Enumerable.Empty<IPackageRequirement>();
	public IEnumerable<ITag> Tags => GetWorkshopInfo()?.Tags.Select(x => (ITag)new TagItem(Enums.TagSource.Workshop, x)) ?? Enumerable.Empty<ITag>();

	public IWorkshopInfo? GetWorkshopInfo()
	{
		return _info ?? SteamUtil.GetItem(Id);
	}
}
