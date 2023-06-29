using System;

namespace SkyveApp.Domain.CS1.Utilities;
internal class ModDllCache
{
	public Version? Version { get; set; }
	public DateTime Date { get; set; }
	public bool IsMod { get; set; }
	public string? Path { get; set; }
}
