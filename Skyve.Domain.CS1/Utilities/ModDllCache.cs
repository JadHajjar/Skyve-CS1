using System;

namespace Skyve.Domain.CS1.Utilities;
public class ModDllCache
{
	public Version? Version { get; set; }
	public DateTime Date { get; set; }
	public bool IsMod { get; set; }
	public string? Path { get; set; }
}
