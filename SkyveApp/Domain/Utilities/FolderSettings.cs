using Extensions;

#nullable disable

namespace SkyveApp.Domain.Utilities;
internal class FolderSettings : ISave
{
	public override string Name => nameof(FolderSettings) + ".json";

	public string GamePath { get; set; }
	public string AppDataPath { get; set; }
	public string SteamPath { get; set; }
	public Platform Platform { get; set; }
}
#nullable enable