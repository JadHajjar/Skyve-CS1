using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.CS1.Utilities;
using SkyveApp.Domain.Systems;

namespace SkyveApp.Systems.CS1.Systems;
public class SettingsService : ISettings
{
	public SessionSettings SessionSettings { get; }
	public FolderSettings FolderSettings { get; }
	IUserSettings ISettings.UserSettings => SessionSettings.UserSettings;
	ISessionSettings ISettings.SessionSettings => SessionSettings;
	IFolderSettings ISettings.FolderSettings => FolderSettings;

	public SettingsService()
	{
		SessionSettings = ISave.Load<SessionSettings>(nameof(SessionSettings) + ".json");
		FolderSettings = ISave.Load<FolderSettings>(nameof(FolderSettings) + ".json");

		CrossIO.CurrentPlatform = FolderSettings.Platform;
	}
}
