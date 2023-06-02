using Extensions.Sql;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Domain.Compatibility.Api;
[DynamicSqlClass("UserProfiles")]
public class UserProfile : IDynamicSql
{
    [DynamicSqlProperty(PrimaryKey = true, Identity = true)]
    public int ProfileId { get; set; }
	[DynamicSqlProperty(Indexer = true)]
	public ulong AuthorId { get; set; }
    [DynamicSqlProperty]
    public string? Name { get; set; }
    [DynamicSqlProperty]
    public int ModCount { get; set; }
    [DynamicSqlProperty]
    public int AssetCount { get; set; }
    [DynamicSqlProperty]
    public DateTime DateCreated { get; set; }
	[DynamicSqlProperty]
	public DateTime DateModified { get; set; }
	[DynamicSqlProperty]
	public bool Public { get; set; }
	[DynamicSqlProperty]
	public byte[]? Banner { get; set; }
	[DynamicSqlProperty]
	public int? Color { get; set; }

	public UserProfileContent[]? Contents { get; set; }
}
