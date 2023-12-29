using Skyve.Systems;

namespace Skyve.Domain.CS1.Steam;

internal class SteamPackageRequirement : IPackageRequirement
{

	public SteamPackageRequirement(ulong id, bool optional)
	{
		Id = id;
		IsOptional = optional;
		Url = $"https://steamcommunity.com/workshop/filedetails/?id={Id}";
	}

	public bool IsOptional { get; }
	public ulong Id { get; }
	public string? Url { get; }
	public string Name => this.GetWorkshopInfo()?.Name ?? Id.ToString();
}