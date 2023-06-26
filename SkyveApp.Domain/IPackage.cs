using System.Collections.Generic;

namespace SkyveApp.Domain;
public interface IPackage : IPackageIdentity
{
	bool IsMod { get; }
	bool IsLocal { get; }
	bool IsBuiltIn { get; }
	ILocalPackageWithContents? LocalPackage { get; }
	IEnumerable<IPackageRequirement> Requirements { get; }
	IEnumerable<ITag> Tags { get; }
}
