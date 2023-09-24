using Skyve.Systems;

using System;

namespace Skyve.Domain.CS1;
public class KnownPackage
{
	public KnownPackage(ILocalPackage x)
	{
		Folder = x.Folder;
		UpdateTime = x.LocalTime;
	}

	public KnownPackage()
	{

	}

	public string? Folder { get; set; }
	public DateTime UpdateTime { get; set; }
}
