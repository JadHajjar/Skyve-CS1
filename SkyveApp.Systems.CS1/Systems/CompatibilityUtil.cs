using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Enums;
using SkyveApp.Systems.Compatibility;
using SkyveApp.Systems.Compatibility.Domain;
using SkyveApp.Systems.Compatibility.Domain.Api;

using System;
using System.Linq;

namespace SkyveApp.Systems.CS1.Systems;
internal class CompatibilityUtil : ICompatibilityUtil
{
	private const ulong MUSIC_MOD_ID = 2474585115;
	private const ulong IMT_MOD_ID = 2140418403;
	private const ulong THEMEMIXER_MOD_ID = 2954236385;
	private const ulong RENDERIT_MOD_ID = 1794015399;
	private const ulong PO_MOD_ID = 1094334744;

	private readonly CompatibilityHelper _compatibilityHelper;

	public CompatibilityUtil(CompatibilityHelper compatibilityHelper)
	{
		_compatibilityHelper = compatibilityHelper;
	}

	public DateTime MinimumModDate { get; } = new DateTime(2023, 6, 12);

	public void PopulateAutomaticPackageInfo(CompatibilityPackageData info, IPackage package, IWorkshopInfo? workshopInfo)
	{
		if (package.Name?.Contains("theme mix", StringComparison.InvariantCultureIgnoreCase) ?? false)
		{
			info.Type = PackageType.ThemeMix;
		}

		if (workshopInfo?.Requirements?.Any(x => x.Id == MUSIC_MOD_ID) ?? false)
		{
			info.Statuses!.Add(new(StatusType.MusicCanBeCopyrighted));
		}
	}

	public void PopulatePackageReport(IndexedPackage packageData, CompatibilityInfo info)
	{
		if (packageData.Package.Type is PackageType.CSM && !packageData.Statuses.ContainsKey(StatusType.Reupload))
		{
			_compatibilityHelper.HandleStatus(info, new PackageStatus(StatusType.Reupload, StatusAction.Switch) { Packages = new ulong[] { 1558438291 } });
		}

		if (packageData.Package.Type is PackageType.MusicPack or PackageType.ThemeMix or PackageType.IMTMarkings or PackageType.RenderItPreset or PackageType.POFont && !packageData.Interactions.ContainsKey(InteractionType.RequiredPackages))
		{
			_compatibilityHelper.HandleInteraction(info, new PackageInteraction(InteractionType.RequiredPackages, StatusAction.SubscribeToPackages)
			{
				Packages = new ulong[]
				{
					packageData.Package.Type switch
					{
						PackageType.MusicPack => MUSIC_MOD_ID,
						PackageType.ThemeMix => THEMEMIXER_MOD_ID,
						PackageType.IMTMarkings => IMT_MOD_ID,
						PackageType.RenderItPreset => RENDERIT_MOD_ID,
						PackageType.POFont => PO_MOD_ID,
						_ => 0
					}
				}
			});
		}
	}
}
