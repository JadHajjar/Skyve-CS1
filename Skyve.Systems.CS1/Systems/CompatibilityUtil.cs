using Extensions;

using Skyve.Domain;
using Skyve.Domain.Enums;
using Skyve.Systems.Compatibility;
using Skyve.Systems.Compatibility.Domain;
using Skyve.Systems.Compatibility.Domain.Api;

using System;
using System.Linq;

namespace Skyve.Systems.CS1.Systems;
internal class CompatibilityUtil : ICompatibilityUtil
{
	private const ulong MUSIC_MOD_ID = 2474585115;
	private const ulong IMT_MOD_ID = 2140418403;
	private const ulong THEMEMIXER_MOD_ID = 2954236385;
	private const ulong RENDERIT_MOD_ID = 1794015399;
	private const ulong PO_MOD_ID = 1094334744;

	public CompatibilityUtil()
	{
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

	public void PopulatePackageReport(IndexedPackage packageData, CompatibilityInfo info, CompatibilityHelper compatibilityHelper)
	{
		if (packageData.Package.Type is PackageType.CSM && !packageData.Statuses.ContainsKey(StatusType.Reupload))
		{
			compatibilityHelper.HandleStatus(info, new PackageStatus(StatusType.Reupload, StatusAction.Switch) { Packages = new ulong[] { 1558438291 } });
		}

		if (packageData.Package.Type is PackageType.MusicPack or PackageType.ThemeMix or PackageType.IMTMarkings or PackageType.RenderItPreset or PackageType.POFont && !packageData.Interactions.ContainsKey(InteractionType.RequiredPackages))
		{
			compatibilityHelper.HandleInteraction(info, new PackageInteraction(InteractionType.RequiredPackages, StatusAction.SubscribeToPackages)
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
