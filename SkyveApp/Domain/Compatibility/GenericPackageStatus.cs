using Newtonsoft.Json;
using SkyveApp.Domain.Compatibility.Enums;
using System;

namespace SkyveApp.Domain.Compatibility;
public class GenericPackageStatus : IGenericPackageStatus
{
	public GenericPackageStatus()
	{

	}

	public GenericPackageStatus(IGenericPackageStatus status)
	{
		if (status is not null)
		{
			Action = status.Action;
			Packages = status.Packages;
			Note = status.Note;
			IntType = status.IntType;
			Type = status.GetType().FullName;
		}
	}

	public StatusAction Action { get; set; }
	public ulong[]? Packages { get; set; }
	public string? Note { get; set; }
	[JsonIgnore] public NotificationType Notification { get; }
	public int IntType { get; set; }
	public string? Type { get; set; }

	public IGenericPackageStatus ToGenericPackage()
	{
		var instance = (IGenericPackageStatus)Activator.CreateInstance(typeof(GenericPackageStatus).Assembly.GetType(Type));

		instance.Action = Action;
		instance.Packages = Packages;
		instance.Note = Note;
		instance.IntType = IntType;

		return instance;
	}
}
