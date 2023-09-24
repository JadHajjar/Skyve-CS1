using Extensions;

using Skyve.Domain;
using Skyve.Domain.CS1.Utilities;
using Skyve.Domain.Systems;

namespace Skyve.Systems.CS1.Systems;
internal class SettingsService : ISettings
{
	public SessionSettings SessionSettings { get; private set; }
	public FolderSettings FolderSettings { get; private set; }
	IUserSettings ISettings.UserSettings => SessionSettings.UserSettings;
	ISessionSettings ISettings.SessionSettings => SessionSettings;
	IFolderSettings ISettings.FolderSettings => FolderSettings;

	public SettingsService()
	{
		SessionSettings = ISave.Load<SessionSettings>(nameof(SessionSettings) + ".json");
		FolderSettings = ISave.Load<FolderSettings>(nameof(FolderSettings) + ".json");

		CrossIO.CurrentPlatform = FolderSettings.Platform;
	}

	public void ResetFolderSettings()
	{
		FolderSettings = new FolderSettings();
		FolderSettings.Save();
	}

	public void ResetUserSettings()
	{
		SessionSettings.UserSettings = new();
		SessionSettings.Save();
	}
}
