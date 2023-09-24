using Extensions;

using Skyve.Domain.Systems;
using Skyve.Systems;

using System;
using System.Drawing;

namespace Skyve.Domain.CS1.Notifications;

public class IncorrectLocationSettingsNotification : INotificationInfo
{
	public IncorrectLocationSettingsNotification()
	{
		Time = DateTime.Now;
		Title = Locale.IncorrectFolderSettings;
		Description = Locale.IncorrectFolderSettingsInfo;
		Icon = "I_Hazard";
		Color = FormDesign.Design.RedColor;
		HasAction = true;
	}

	public DateTime Time { get; }
	public string Title { get; }
	public string? Description { get; }
	public string Icon { get; }
	public Color? Color { get; }
	public bool HasAction { get; }

	public void OnClick()
	{
		ServiceCenter.Get<IInterfaceService>().OpenOptionsPage();
	}

	public void OnRightClick()
	{
	}
}
