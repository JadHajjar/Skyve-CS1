using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyve.Domain.CS1;
public class NotificationInfo : INotificationInfo
{
	private readonly Action? _action;

	public DateTime Time { get; }
	public string Title { get; }
	public string Description { get; }
	public string Icon { get; }
	public Color? Color { get; }
	public bool HasAction { get; }

	public NotificationInfo(DateTime time, string title, string description, string icon, Color? color = null, Action? action = null)
	{
		Time = time;
		Title = title;
		Description = description;
		Icon = icon;
		Color = color;
		_action = action;
		HasAction = action is not null;
	}

	public void OnClick()
	{
		_action?.Invoke();
	}

	public void OnRightClick()
	{
	}
}
