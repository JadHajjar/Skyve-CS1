﻿namespace SkyveApp.Domain.Compatibility;

public struct ReportItem
{
    public ulong PackageId { get; set; }
	public IGenericPackageStatus Status { get; set; }
	public ReportType Type { get; set; }
	public string Message { get; set; }
	public PseudoPackage[] Packages { get; set; }
}
