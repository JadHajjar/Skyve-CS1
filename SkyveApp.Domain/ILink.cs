using SkyveApp.Domain.Enums;

namespace SkyveApp.Domain;
public interface ILink
{
	public LinkType Type { get; }
	public string? Url { get; }
	public string? Title { get; }
}