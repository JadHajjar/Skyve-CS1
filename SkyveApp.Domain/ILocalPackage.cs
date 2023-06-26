using System;

namespace SkyveApp.Domain;
public interface ILocalPackage : IPackage, ILocalPackageIdentity
{
	long LocalSize { get; }
	DateTime LocalTime { get; }
	string Folder { get; }
}
