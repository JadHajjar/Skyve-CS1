using Extensions;

#nullable disable

namespace LoadOrderToolTwo.Domain.Utilities;
internal class FolderSettings : ISave
{
	public override string Name => nameof(FolderSettings) + ".json";

	public string GamePath { get; set; }
	public string AppDataPath { get; set; }
	public string SteamPath { get; set; }
	public string VirtualGamePath { get; set; }
	public string VirtualAppDataPath { get; set; }
	public Platform Platform { get; set; }
}
#nullable enable