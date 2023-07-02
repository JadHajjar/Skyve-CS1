using Microsoft.Extensions.DependencyInjection;

using SkyveApp.Domain.Systems;
using SkyveApp.Systems.Compatibility;

using System;
using System.Windows.Forms;

namespace SkyveApp.Systems;
public static class SystemsProgram
{
	public static Form? MainForm { get; set; }

	public static IServiceCollection AddSkyveSystems(this IServiceCollection services)
	{
		services.AddTransient<IBulkUtil, BulkUtil>();
		services.AddSingleton<IImageService, ImageSystem>();
		services.AddSingleton<IIOUtil, IOUtil>();
		services.AddSingleton<ILogger, LoggerSystem>();
		services.AddSingleton<INotifier, NotifierSystem>();
		services.AddTransient<IPackageNameUtil, PackageNameUtil>();
		services.AddTransient<IPackageUtil, PackageUtil>();
		services.AddTransient<SkyveApiUtil>();
		services.AddSingleton<ICompatibilityManager, CompatibilityManager>();

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
