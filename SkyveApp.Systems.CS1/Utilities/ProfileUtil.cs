using SkyveApp.Domain;
using SkyveApp.Domain.Enums;

using SlickControls;

namespace SkyveApp.Utilities;
public static class ProfileUtil
{
	public static DynamicIcon GetIcon(this IPlayset profile)
	{
		if (profile.Temporary)
		{
			return "I_TempProfile";
		}

		return profile.Usage.GetIcon();
	}

	public static DynamicIcon GetIcon(this PackageUsage usage)
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
