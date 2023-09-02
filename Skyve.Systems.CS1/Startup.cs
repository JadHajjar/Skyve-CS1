using Microsoft.Extensions.DependencyInjection;

using Skyve.Domain.Systems;
using Skyve.Systems.Compatibility.Domain;
using Skyve.Systems.CS1.Managers;
using Skyve.Systems.CS1.Systems;
using Skyve.Systems.CS1.Utilities;
using Skyve.Systems.CS1.Utilities.IO;

namespace Skyve.Systems.CS1;
public static class Startup
{
	public static IServiceCollection AddCs1SkyveSystems(this IServiceCollection services)
	{
		services.AddTransient<ICentralManager, CentralManager>();
		services.AddSingleton<ICitiesManager, CitiesManager>();
		services.AddSingleton<ILocationManager, LocationManager>();
		services.AddSingleton<IModLogicManager, ModLogicManager>();
		services.AddSingleton<IPackageManager, PackageManager>();
		services.AddSingleton<IPlaysetManager, PlaysetManager>();
		services.AddSingleton<ISubscriptionsManager, SubscriptionsManager>();
		services.AddSingleton<IUpdateManager, UpdateManager>();
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
		services.AddSingleton<ITroubleshootSystem, TroubleshootSystem>();
		services.AddSingleton<INotificationsService, NotificationsService>();

		LocaleCS1.Load();

		return services;
	}
}
