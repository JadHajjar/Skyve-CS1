using Microsoft.Extensions.DependencyInjection;

using System;

namespace SkyveApp;

#nullable disable
public static class ServiceCenter
{
	public static IServiceProvider Provider { get; set; }

	public static T Get<T>()
	{
		return Provider.GetService<T>();
	}

	public static T2 Get<T, T2>() where T2 : T
	{
		return (T2)Provider.GetService<T>();
	}
}
#nullable enable