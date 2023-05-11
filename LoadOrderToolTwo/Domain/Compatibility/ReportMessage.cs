namespace LoadOrderToolTwo.Domain.Compatibility;

public struct ReportMessage
{
	public IGenericPackageStatus Status { get; set; }
	public ReportType Type { get; set; }
	public string Message { get; set; }
}