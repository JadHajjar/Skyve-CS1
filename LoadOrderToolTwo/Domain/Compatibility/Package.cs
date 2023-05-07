using Extensions.Sql;

using System;
using System.Collections.Generic;

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
	public int GroupId { get; set; }
	[DynamicSqlProperty]
	public string? Note { get; set; }
	[DynamicSqlProperty]
	public DateTime ReviewDate { get; set; }
	[DynamicSqlProperty]
	public PackageStability Stability { get; set; }
	[DynamicSqlProperty]
	public PackageUsage Usage { get; set; }
	public List<string>? Tags { get; set; }
	public List<PackageLink>? Links { get; set; }
	public List<PackageStatus>? Statuses { get; set; }
	public List<PackageInteraction>? Interactions { get; set; }
}
