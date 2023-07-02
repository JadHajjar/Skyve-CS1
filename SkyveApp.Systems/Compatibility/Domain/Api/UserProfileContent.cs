using Extensions.Sql;

using SkyveApp.Domain;

namespace SkyveApp.Systems.Compatibility.Domain.Api;

[DynamicSqlClass("UserProfileContents")]
public class UserProfileContent : IDynamicSql, IPlaysetEntry
{
	[DynamicSqlProperty(Indexer = true)]
	public int ProfileId { get; set; }
	[DynamicSqlProperty]
	public string? RelativePath { get; set; }
	[DynamicSqlProperty]
	public ulong SteamId { get; set; }
	[DynamicSqlProperty]
	public bool IsMod { get; set; }
	[DynamicSqlProperty]
	public bool Enabled { get; set; }

	string ILocalPackageIdentity.FilePath => RelativePath;
	ulong IPackageIdentity.Id => SteamId;
	string IPackageIdentity.Name => RelativePath;
	string? IPackageIdentity.Url { get; }
}