using System.Collections.Generic;
using SkyveApp.Domain.Compatibility.Api;

namespace SkyveApp.Domain.Compatibility;
public class CompatibilityData
{
	public List<CrPackage>? Packages { get; set; }
	public List<Author>? Authors { get; set; }
	public List<ulong>? BlackListedIds { get; set; }
	public List<string>? BlackListedNames { get; set; }
}
