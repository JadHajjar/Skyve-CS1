using Skyve.App.CS1.UserInterface.Panels;
using Skyve.App.Interfaces;

namespace Skyve.App.CS1.Services;
internal class InterfaceService : IInterfaceService
{
	PlaysetSettingsPanel IInterfaceService.PlaysetSettingsPanel()
	{
		return new PC_PlaysetSettings();
	}

	PC_Changelog IInterfaceService.ChangelogPanel()
	{
		return new PC_SkyveChangeLog();
	}

	PanelContent IInterfaceService.UtilitiesPanel()
	{
		return new PC_Utilities();
	}

	public INotificationInfo GetLastVersionNotification()
	{
		return new LastVersionNotification();
	}
}
