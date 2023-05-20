using System;
using System.Linq;

namespace SkyveApp.Domain.Compatibility;

public class CRNAttribute : Attribute // Compatibility Report Notification (CRN)
{
	public NotificationType Notification { get; }
	public StatusAction[] ValidActions { get; }
	public bool Browsable { get; }

	public CRNAttribute(NotificationType notification, bool browsable = true)
	{
		Notification = notification;
		Browsable = browsable;
		ValidActions = new StatusAction[0];
	}

	public CRNAttribute(NotificationType notification, StatusAction[] actions, bool browsable = true)
	{
		Notification = notification;
		Browsable = browsable;
		ValidActions = actions;
	}

	public static NotificationType GetNotification<TEnum>(TEnum enumValue) where TEnum : struct, Enum
	{
		return GetAttribute(enumValue).Notification;
	}

	public static CRNAttribute GetAttribute<TEnum>(TEnum enumValue) where TEnum : struct, Enum
	{
		var memberInfo = enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault() ?? throw new ArgumentException($"Invalid enum value: {enumValue}");

		var crnAttribute = (CRNAttribute?)memberInfo.GetCustomAttributes(typeof(CRNAttribute), false).FirstOrDefault();

		return crnAttribute ?? throw new ArgumentException($"Enum value {enumValue} is missing CRN attribute");
	}
}