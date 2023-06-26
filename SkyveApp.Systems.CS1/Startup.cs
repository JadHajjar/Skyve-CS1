using Microsoft.Extensions.DependencyInjection;
using SkyveApp.Domain.Systems;
using SkyveApp.Services;
using SkyveApp.Utilities.IO;
using SkyveApp.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkyveApp.Systems.CS1.Managers;

namespace SkyveApp.Systems.CS1;
public static class Startup
{
	public static IServiceCollection AddCs1SkyveSystems(this IServiceCollection services)
	{
		services.AddSingleton<ILocale, Locale>();
		services.AddSingleton<ICitiesManager, CitiesManager>();
		services.AddSingleton<IContentManager, ContentManager>();
		services.AddSingleton<ILocationManager, LocationManager>();
		services.AddSingleton<IModLogicManager, ModLogicManager>();
		services.AddSingleton<IPlaysetManager, ProfileManager>();
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
