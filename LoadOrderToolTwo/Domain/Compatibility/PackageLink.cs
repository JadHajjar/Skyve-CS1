using Extensions.Sql;

namespace LoadOrderToolTwo.Domain.Compatibility;

[DynamicSqlClass("PackageLinks")]
public struct PackageLink : IDynamicSql
{
#if API
	[DynamicSqlProperty(Indexer = true), System.Text.Json.Serialization.JsonIgnore]
	public ulong PackageId { get; set; }
#endif

	[DynamicSqlProperty]
	public LinkType Type { get; set; }

	[DynamicSqlProperty]
	public string? Url { get; set; }

	[DynamicSqlProperty]
	public string? Title { get; set; }
}
