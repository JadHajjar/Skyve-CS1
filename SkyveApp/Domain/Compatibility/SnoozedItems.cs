namespace SkyveApp.Domain.Compatibility;
internal struct SnoozedItem
{
	public SnoozedItem()
	{

	}

	public SnoozedItem(ReportItem report) : this()
	{
		SteamId = report.PackageId;
		ReportType = (int)report.Type;
		StatusType = report.Status.IntType;
		StatusAction = (int)report.Status.Action;
		;
	}

	public ulong SteamId { get; set; }
	public int ReportType { get; set; }
	public int StatusType { get; set; }
	public int StatusAction { get; set; }

	public override bool Equals(object? obj)
	{
		if (obj is ReportItem report)
		{
			return SteamId == report.PackageId
				&& ReportType == (int)report.Type
				&& StatusType == report.Status.IntType
				&& StatusAction == (int)report.Status.Action;
		}

		return obj is SnoozedItem item &&
			   SteamId == item.SteamId &&
			   ReportType == item.ReportType &&
			   StatusType == item.StatusType &&
			   StatusAction == item.StatusAction;
	}

	public override int GetHashCode()
	{
		var hashCode = -143951897;
		hashCode = (hashCode * -1521134295) + SteamId.GetHashCode();
		hashCode = (hashCode * -1521134295) + ReportType.GetHashCode();
		hashCode = (hashCode * -1521134295) + StatusType.GetHashCode();
		hashCode = (hashCode * -1521134295) + StatusAction.GetHashCode();
		return hashCode;
	}
}
