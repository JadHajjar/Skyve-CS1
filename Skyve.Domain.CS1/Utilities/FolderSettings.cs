using Extensions;

#nullable disable

namespace Skyve.Domain.CS1.Utilities;

[SaveName(nameof(FolderSettings) + ".json")]
public class FolderSettings : ISaveObject, IFolderSettings
{
	public string GamePath { get; set; }
	public string AppDataPath { get; set; }
	public string SteamPath { get; set; }
	public Platform Platform { get; set; }
	public SaveHandler Handler {get; set; }

	void IFolderSettings.Save()
	{
		Handler?.Save(this);
	}
}
#nullable enable