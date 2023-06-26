using System;

namespace SkyveApp.Domain;
internal class KnownPackage
{
	public KnownPackage(ILocalPackage x)
	{
		Folder = x.Folder;
		UpdateTime = x.GetWorkshopInfo()?.ServerTime ?? default;
	}

	public KnownPackage()
	{

	}

	public string? Folder { get; set; }
	public DateTime UpdateTime { get; set; }
}
