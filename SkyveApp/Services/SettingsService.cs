using Extensions;

using SkyveApp.Domain.Utilities;
using SkyveApp.Services.Interfaces;

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

    public SettingsService()
	{
		SessionSettings = ISave.Load<SessionSettings>(nameof(SessionSettings) + ".json");
		FolderSettings = ISave.Load<FolderSettings>(nameof(FolderSettings) + ".json");
	}
}
