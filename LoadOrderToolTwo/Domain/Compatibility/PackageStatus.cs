using Extensions.Sql;

namespace LoadOrderToolTwo.Domain.Compatibility;

[DynamicSqlClass("PackageStatuses")]
public struct PackageStatus : IPackageStatus, IDynamicSql
{
#if API
	[DynamicSqlProperty(PrimaryKey = true, Indexer = true), System.Text.Json.Serialization.JsonIgnore]
	public ulong PackageId { get; set; }
#endif

	[DynamicSqlProperty(PrimaryKey = true)]
	public StatusType Type { get; set; }

	[DynamicSqlProperty]
	public InteractionAction Action { get; set; }

	public ulong[]? Packages { get; set; }

	[DynamicSqlProperty]
	public string? Note { get; set; }

#if API
	[DynamicSqlProperty(ColumnName = nameof(Packages)), System.Text.Json.Serialization.JsonIgnore]
	public string? PackageList { get => Packages is null ? null : string.Join(',', Packages); set => Packages = value?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(ulong.Parse).ToArray(); }
#else
	public NotificationType Notification
	{
		get
		{
			var type = CRNAttribute.GetNotification(Type);
			var action = CRNAttribute.GetNotification(Action);

			return type > action ? type : action;
		}
	}
#endif
}
