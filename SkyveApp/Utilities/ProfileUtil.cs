using SkyveApp.Domain.Compatibility.Enums;
using SkyveApp.Domain.Interfaces;
using SlickControls;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Utilities;
internal static class ProfileUtil
{
	internal static DynamicIcon GetIcon(this IProfile profile)
	{
		if (profile.Temporary)
		{
			return "I_TempProfile";
		}

		return profile.Usage.GetIcon();
	}

	internal static DynamicIcon GetIcon(this PackageUsage usage)
	{
		return usage switch
		{
			PackageUsage.CityBuilding => "I_City",
			PackageUsage.AssetCreation => "I_Tools",
			PackageUsage.MapCreation => "I_Map",
			PackageUsage.ScenarioMaking => "I_ScenarioMaking",
			PackageUsage.ThemeMaking => "I_Paint",
			_ => "I_ProfileSettings"
		};
	}
}
