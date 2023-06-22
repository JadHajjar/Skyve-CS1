namespace SkyveApp.Domain;
public interface ILocalPackage : IPackage, ILocalPackageIdentity
{
	bool IsIncluded { get; set; }
	long LocalSize { get; }
	string Folder { get; }
	bool IsLocal { get; }
}
