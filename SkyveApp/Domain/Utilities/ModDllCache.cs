using System;

namespace SkyveApp.Domain.Utilities;
internal class ModDllCache
{
	public Version? Version { get; set; }
	public DateTime Date { get; set; }
	public bool IsMod { get; set; }
	public string? Path { get; set; }
}
