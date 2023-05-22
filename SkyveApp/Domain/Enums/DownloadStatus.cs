namespace SkyveApp.Domain.Enums;

public enum DownloadStatus
{
	None,
	OK,
	Unknown,
	OutOfDate,
	NotDownloaded,
	PartiallyDownloaded,
	Removed,
}

public enum DownloadStatusFilter
{
	Any,
	AnyIssue,
	//None,
	OK,
	Unknown,
	OutOfDate,
	NotDownloaded,
	PartiallyDownloaded,
	Removed,
}