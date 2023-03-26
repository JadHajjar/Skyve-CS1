using System;

namespace LoadOrderToolTwo.Domain;
internal class KnownPackage
{
	public KnownPackage(Package x)
	{
		Folder = x.Folder;
		UpdateTime = x.ServerTime;
	}

	public KnownPackage()
	{

	}

	public string? Folder { get; set; }
	public DateTime UpdateTime { get; set; }
}
