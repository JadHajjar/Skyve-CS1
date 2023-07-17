using System;

namespace SkyveApp.Domain;
public interface ILocalPackage : IPackage, ILocalPackageIdentity
{
	new ILocalPackageWithContents LocalParentPackage { get; }
	long LocalSize { get; }
	DateTime LocalTime { get; }
	string Folder { get; }
}
