using Extensions;
using Extensions.Sql;

#if API
using System.Text.Json.Serialization;
#else
using Newtonsoft.Json;
#endif
using System;
using System.Collections.Generic;
using System.Linq;

namespace SkyveApp.Domain.Utilities;
[DynamicSqlClass("UserDataRequests")]
public class RequestLog : IDynamicSql
{
	[JsonIgnore]
	[DynamicSqlProperty(Indexer = true, PrimaryKey = true)]
	public string? UserId { get; set; }
	[DynamicSqlProperty(PrimaryKey = true)]
    public string? BaseUrl { get; set; }
	[DynamicSqlProperty()]
	public int RequestCounts { get; set; }
	[DynamicSqlProperty()]
	public long TotalRequestSizesInKb { get; set; }
}

[DynamicSqlClass("UserData")]
#if API
public class RequestData : IDynamicSql
#else
public class RequestData : ISave, IDynamicSql
#endif
{
	[DynamicSqlProperty(PrimaryKey = true)]
    public string? UserId { get; set; }
	[DynamicSqlProperty()]
    public long TotalUpTime { get; set; }
	[DynamicSqlProperty()]
    public int TotalPackages { get; set; }
	[DynamicSqlProperty()]
	public int TotalMods { get; set; }
	[DynamicSqlProperty()]
	public int TotalAssets { get; set; }
	[DynamicSqlProperty()]
	public int IncludedMods { get; set; }
	[DynamicSqlProperty()]
	public int IncludedAssets { get; set; }
	[DynamicSqlProperty()]
	public long TotalModSizes { get; set; }
	[DynamicSqlProperty()]
	public long TotalAssetSizes { get; set; }
	[DynamicSqlProperty()]
	public long TotalIncludedModSizes { get; set; }
	[DynamicSqlProperty()]
	public long TotalIncludedAssetSizes { get; set; }
	public List<RequestLog>? Logs { get; set; }
}
