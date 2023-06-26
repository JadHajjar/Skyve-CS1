using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Systems;
using SkyveApp.Domain.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Services;
internal class SettingsService : ISettings
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
