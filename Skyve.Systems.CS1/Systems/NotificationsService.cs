using Skyve.Domain;
using Skyve.Domain.Systems;

using System;
using System.Collections.Generic;

namespace Skyve.Systems.CS1.Systems;
internal class NotificationsService : INotificationsService
{
	private readonly List<INotificationInfo> _notifications = new();

	public event Action? OnNewNotification;

	public IEnumerable<INotificationInfo> GetNotifications()
	{
		List<INotificationInfo> notifications;

		lock (this)
		{
			notifications = new(_notifications);
		}

		foreach (var item in notifications)
		{
			yield return item;
		}
	}

	public void SendNotification(INotificationInfo notification)
	{
		lock (this)
		{
			_notifications.Add(notification);
		}

		OnNewNotification?.Invoke();
	}
}
