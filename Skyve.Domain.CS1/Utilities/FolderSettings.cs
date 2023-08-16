using Extensions;

#nullable disable

namespace Skyve.Domain.CS1.Utilities;
public class FolderSettings : ISave, IFolderSettings
{
	public override string Name => nameof(FolderSettings) + ".json";

	public string GamePath { get; set; }
	public string AppDataPath { get; set; }
	public string SteamPath { get; set; }
	public Platform Platform { get; set; }

	void IFolderSettings.Save()
	{
		Save();
	}
}
#nullable enable