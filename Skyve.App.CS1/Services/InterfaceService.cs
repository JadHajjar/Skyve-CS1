using Skyve.App.CS1.UserInterface.Panels;
using Skyve.App.Interfaces;
using Skyve.App.UserInterface.Panels;

namespace Skyve.App.CS1.Services;
internal class InterfaceService : IAppInterfaceService
{
	PlaysetSettingsPanel IAppInterfaceService.PlaysetSettingsPanel()
	{
		return new PC_PlaysetSettings();
	}

	PC_Changelog IAppInterfaceService.ChangelogPanel()
	{
		return new PC_SkyveChangeLog();
	}

	PanelContent IAppInterfaceService.UtilitiesPanel()
	{
		return new PC_Utilities();
	}

	INotificationInfo IAppInterfaceService.GetLastVersionNotification()
	{
		return new LastVersionNotification();
	}

	void IInterfaceService.ViewSpecificPackages(List<ILocalPackageWithContents> packages, string title)
	{
		App.Program.MainForm.PushPanel(new PC_ViewSpecificPackages(packages, title));
	}

	void IInterfaceService.OpenOptionsPage()
	{
		App.Program.MainForm.PushPanel<PC_Options>();
	}
}
