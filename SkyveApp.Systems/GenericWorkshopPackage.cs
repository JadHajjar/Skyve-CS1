using SkyveApp.Domain;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Systems;
public class GenericWorkshopPackage : IPackage
{
	public GenericWorkshopPackage(ulong id)
	{
		Id = id;
	}

    public GenericWorkshopPackage()
    {
        
    }

    public bool IsMod { get; set; }
	public bool IsLocal { get; set; }
	public bool IsBuiltIn { get; set; }
	public ILocalPackageWithContents? LocalPackage { get; set; }
	public IEnumerable<IPackageRequirement> Requirements { get; set; }
	public IEnumerable<ITag> Tags { get; set; }
	public ulong Id { get; set; }
	public string Name { get; set; }
	public string? Url { get; set; }

	public IWorkshopInfo? GetWorkshopInfo()
	{
		return null;
	}
}
