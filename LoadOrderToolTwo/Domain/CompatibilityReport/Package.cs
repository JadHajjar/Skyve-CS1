using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadOrderToolTwo.Domain.CompatibilityReport;
public class Package
{
	public ulong SteamId { get; set; }
	public string? Name { get; set; }
	public string? FileName { get; set; }
	public ulong AuthorId { get; set; }
	public int GroupId { get; set; }
    public List<PackageLink>? Links { get; set; }
    public List<ulong>? Successors { get; set; }
	public List<ulong>? Requires { get; set; }
	public List<PackageInteraction>? Interactions { get; set; }

}

public struct PackageLink
{
	public LinkType Type { get; set; }
	public string Url { get; set; }
}

public struct PackageInteraction
{
	public InteractionType Type { get; set; }
	public ulong Package { get; set; }
    public string Note { get; set; }
}

public enum InteractionType
{
}

public enum LinkType
{
	Discord,
	Github,
	Crowdin,
	Donation
}

public class Author
{
	public ulong SteamId { get; set; }
	public string? Name { get; set; }
    public bool Retired { get; set; }
}
