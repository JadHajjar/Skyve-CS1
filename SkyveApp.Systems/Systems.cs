using Microsoft.Extensions.DependencyInjection;

using SkyveApp.Domain.Systems;
using SkyveApp.Systems.Compatibility;

using System;

namespace SkyveApp.Systems;
public static class SystemsProgram
{
	public static IServiceCollection AddSkyveSystems(this IServiceCollection services)
	{
		services.AddSingleton<ILogger, LoggerSystem>();
		services.AddSingleton<INotifier, NotifierSystem>();
		services.AddSingleton<IImageService, ImageSystem>();
		services.AddSingleton<ICompatibilityManager, CompatibilityManager>();

		services.AddTransient<IBulkUtil, BulkUtil>();
		services.AddTransient<IPackageUtil, PackageUtil>();
		services.AddTransient<IContentUtil, ContentUtil>();

		return services;
	}

	public static T GetService<T>(this IServiceProvider provider)
	{
		return provider.GetService<T>();
	}

	public static T2 GetService<T, T2>(this IServiceProvider provider) where T2 : T
	{
		return (T2)provider.GetService<T>()!;
	}
}
