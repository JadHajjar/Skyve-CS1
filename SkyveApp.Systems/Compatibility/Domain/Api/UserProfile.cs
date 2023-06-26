using Extensions.Sql;

using SkyveApp.Domain;
using SkyveApp.Domain.Enums;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace SkyveApp.Systems.Compatibility.Domain.Api;

[DynamicSqlClass("UserProfiles")]
public class UserProfile : IDynamicSql
#if !API
	, ICustomPlayset
#endif
{
	[DynamicSqlProperty(PrimaryKey = true, Identity = true)]
	public int ProfileId { get; set; }
	[DynamicSqlProperty(Indexer = true, ColumnName = "AuthorId")]
	public ulong Author { get; set; }
	[DynamicSqlProperty]
	public string? Name { get; set; }
	[DynamicSqlProperty]
	public int ModCount { get; set; }
	[DynamicSqlProperty]
	public int AssetCount { get; set; }
	[DynamicSqlProperty]
	public DateTime DateCreated { get; set; }
	[DynamicSqlProperty]
	public DateTime DateUpdated { get; set; }
	[DynamicSqlProperty]
	public bool Public { get; set; }
	[DynamicSqlProperty]
	public byte[]? Banner { get; set; }
	[DynamicSqlProperty]
	public int? Color { get; set; }
	[DynamicSqlProperty]
	public int Downloads { get; set; }
	[DynamicSqlProperty(ColumnName = "Usage")]
	public int? ProfileUsage { get; set; }

	public UserProfileContent[]? Contents { get; set; }

#if !API
	private Bitmap? _banner;
	public bool IsFavorite { get; set; }
	public bool IsMissingItems => false;
	public DateTime LastEditDate => DateUpdated;
	public DateTime LastUsed => DateUpdated;
	public PackageUsage Usage => (PackageUsage)(ProfileUsage ?? -1);
	public bool Temporary => false;
	Color? ICustomPlayset.Color { get => Color == null ? null : System.Drawing.Color.FromArgb(Color.Value); set { } }
	//public IEnumerable<IPackage> Packages => Contents?.Select(x => x.IsMod ? new Playset.Mod(x) : new Playset.Asset(x)) ?? Enumerable.Empty<Interfaces.IPackage>();
	Bitmap? ICustomPlayset.Banner
	{
		get
		{
			if (_banner is not null)
			{
				return _banner;
			}

			if (Banner is null || Banner.Length == 0)
			{
				return null;
			}

			using var ms = new MemoryStream(Banner);

			return _banner = new Bitmap(ms);
		}
	}
	IUser? IPlayset.Author { get; }
	string? IPlayset.BannerUrl { get; }
	DateTime IPlayset.DateUsed { get; }
	IEnumerable<IPackage> IPlayset.Packages { get; }
#endif
}
