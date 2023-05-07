namespace LoadOrderToolTwo.Domain.Compatibility;

public struct ReportMessage
{
	public IPackageStatus Status { get; set; }
	public ReportType Type { get; set; }
	public string Message { get; set; }
}