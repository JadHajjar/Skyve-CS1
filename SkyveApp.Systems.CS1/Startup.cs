using Microsoft.Extensions.DependencyInjection;

using SkyveApp.Domain.Systems;
using SkyveApp.Systems.CS1.Managers;
using SkyveApp.Systems.CS1.Systems;
using SkyveApp.Utilities;
using SkyveApp.Utilities.IO;

namespace SkyveApp.Systems.CS1;
public static class Startup
{
	public static IServiceCollection AddCs1SkyveSystems(this IServiceCollection services)
	{
		services.AddSingleton<ILocale, Locale>();
		services.AddSingleton<ICitiesManager, CitiesManager>();
		services.AddSingleton<IPackageManager, PackageManager>();
		services.AddSingleton<ILocationManager, LocationManager>();
		services.AddSingleton<IModLogicManager, ModLogicManager>();
		services.AddSingleton<IPlaysetManager, PlaysetManager>();
		services.AddSingleton<ISettings, SettingsService>();
		services.AddSingleton<ISubscriptionsManager, SubscriptionsManager>();
		services.AddSingleton<IUpdateManager, UpdateManager>();
		services.AddSingleton<IAssetUtil, AssetsUtil>();
		services.AddSingleton<IModUtil, ModsUtil>();
		services.AddSingleton<ColossalOrderUtil>();

		services.AddTransient<ILogUtil, LogUtil>();
		services.AddTransient<IOUtil>();
		services.AddTransient<AssemblyUtil>();
		services.AddTransient<ContentUtil>();

		services.AddSingleton<CentralManager>();

		return services;
	}
}
