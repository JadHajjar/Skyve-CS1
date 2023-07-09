using Microsoft.Extensions.DependencyInjection;

using SkyveApp.Domain.Systems;
using SkyveApp.Systems.Compatibility.Domain;
using SkyveApp.Systems.CS1.Managers;
using SkyveApp.Systems.CS1.Systems;
using SkyveApp.Systems.CS1.Utilities;
using SkyveApp.Systems.CS1.Utilities.IO;

namespace SkyveApp.Systems.CS1;
public static class Startup
{
	public static IServiceCollection AddCs1SkyveSystems(this IServiceCollection services)
	{
		services.AddTransient<ICentralManager, CentralManager>();
		services.AddSingleton<ILocale, Locale>();
		services.AddSingleton<ICitiesManager, CitiesManager>();
		services.AddSingleton<ILocationManager, LocationManager>();
		services.AddSingleton<IModLogicManager, ModLogicManager>();
		services.AddSingleton<IPackageManager, PackageManager>();
		services.AddSingleton<IPlaysetManager, PlaysetManager>();
		services.AddSingleton<ISubscriptionsManager, SubscriptionsManager>();
		//services.AddSingleton<IUpdateManager, UpdateManager>();
		services.AddSingleton<ICompatibilityUtil, CompatibilityUtil>();
		services.AddSingleton<ISettings, SettingsService>();
		services.AddTransient<AssemblyUtil>();
		services.AddTransient<MacAssemblyUtil>();
		services.AddSingleton<IAssetUtil, AssetsUtil>();
		services.AddSingleton<ColossalOrderUtil>();
		services.AddSingleton<IContentManager, ContentManager>();
		services.AddSingleton<IModDllManager, ModDllManager>();
		services.AddTransient<ILogUtil, LogUtil>();
		services.AddSingleton<IModUtil, ModsUtil>();
		services.AddSingleton<IOnlinePlaysetUtil, OnlinePlaysetUtil>();
		services.AddTransient<IWorkshopService, WorkshopService>();
		services.AddSingleton<IUserService, UserService>();
		services.AddSingleton<IDlcManager, DlcManager>();
		services.AddSingleton<ITagsService, TagsService>();
		services.AddTransient<IVersionUpdateService, VersionUpdateService>();
		services.AddTransient<IDownloadService, DownloadService>();

		return services;
	}
}
