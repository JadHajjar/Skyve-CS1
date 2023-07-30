using Extensions;

using Newtonsoft.Json;

using SkyveApp.Domain;
using SkyveApp.Domain.Enums;
using SkyveApp.Domain.Systems;

using System;
using System.Collections.Generic;
using System.Linq;

namespace SkyveApp.Systems.Compatibility.Domain;
public class CompatibilityInfo : ICompatibilityInfo
{
	private IPackage? package;
	private ILocalPackage? localPackage;
	private DtoLocalPackage? dtoPackage;

	[JsonIgnore] public IPackage? Package => dtoPackage ?? localPackage ?? package;
	[JsonIgnore] public ILocalPackage? LocalPackage => dtoPackage ?? localPackage;
	[JsonIgnore] public IndexedPackage? Data { get; }
	public List<ReportItem> ReportItems { get; set; }
	public DtoLocalPackage? DtoPackage { get => dtoPackage ??= localPackage?.CloneTo<ILocalPackage, DtoLocalPackage>(); set => dtoPackage = value; }

	ILocalPackage? ICompatibilityInfo.Package => LocalPackage;
	IPackageCompatibilityInfo? ICompatibilityInfo.Info => Data?.Package;
	IEnumerable<ICompatibilityItem> ICompatibilityInfo.ReportItems => ReportItems.Cast<ICompatibilityItem>();

	[Obsolete("Reserved for DTO", true)]
    public CompatibilityInfo()
    {
		ReportItems = new();
	}

	public CompatibilityInfo(IPackage package, IndexedPackage? packageData)
	{
		this.package = package;
		localPackage = package is ILocalPackage lp ? lp : package.LocalPackage;
		Data = packageData;
		ReportItems = new();
	}

	public void Add(ReportType type, IGenericPackageStatus status, string message, ulong[] packages)
	{
		ReportItems.Add(new ReportItem
		{
			PackageId = Data?.Package.SteamId ?? 0,
			Type = type,
			Status = status,
			Message = message,
			Packages = packages.Select(x => new PseudoPackage(x)).ToArray()
		});
	}

	public void Add(ReportType type, IGenericPackageStatus status, string message, PseudoPackage[] packages)
	{
		ReportItems.Add(new ReportItem
		{
			PackageId = Data?.Package.SteamId ?? 0,
			Type = type,
			Status = status,
			Message = message,
			Packages = packages
		});
	}

	#region DtoLocalPackage
	#nullable disable

	public class DtoLocalPackage : ILocalPackage
	{
		[JsonIgnore] public ILocalPackageWithContents LocalParentPackage { get; set; }
		[JsonIgnore] public ILocalPackage LocalPackage { get; set; }
		public long LocalSize { get; set; }
		public DateTime LocalTime { get; set; }
		public string Folder { get; set; }
		public bool IsMod { get; set; }
		public bool IsLocal { get; set; }
		public bool IsBuiltIn { get; set; }
		public IEnumerable<IPackageRequirement> Requirements { get; set; }
		public string FilePath { get; set; }
		public ulong Id { get; set; }
		public string Name { get; set; }
		public string Url { get; set; }
	}

	#nullable enable
	#endregion
}
