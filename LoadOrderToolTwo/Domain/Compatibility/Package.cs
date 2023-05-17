using Extensions.Sql;

using System;
using System.Collections.Generic;
using System.Linq;

namespace LoadOrderToolTwo.Domain.Compatibility;
[DynamicSqlClass("Packages")]
public class Package : IDynamicSql
{
	[DynamicSqlProperty(PrimaryKey = true)]
	public ulong SteamId { get; set; }
	[DynamicSqlProperty]
	public string? Name { get; set; }
	[DynamicSqlProperty]
	public string? FileName { get; set; }
	[DynamicSqlProperty]
	public ulong AuthorId { get; set; }
	[DynamicSqlProperty]
	public string? Note { get; set; }
	[DynamicSqlProperty]
	public DateTime ReviewDate { get; set; }
	[DynamicSqlProperty]
	public PackageStability Stability { get; set; }
	[DynamicSqlProperty]
	public PackageUsage Usage { get; set; } = (PackageUsage)(-1);
	[DynamicSqlProperty]
	public PackageType Type { get; set; }

#if API
	[DynamicSqlProperty(ColumnName = nameof(RequiredDLCs)), System.Text.Json.Serialization.JsonIgnore]
	public string? RequiredDLCsList { get => RequiredDLCs is null ? null : string.Join(',', RequiredDLCs); set => RequiredDLCs = value?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(uint.Parse).ToArray(); }
#endif

	public uint[]? RequiredDLCs { get; set; }
	public List<string>? Tags { get; set; }
	public List<PackageLink>? Links { get; set; }
	public List<PackageStatus>? Statuses { get; set; }
	public List<PackageInteraction>? Interactions { get; set; }
}
