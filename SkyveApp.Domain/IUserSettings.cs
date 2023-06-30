namespace SkyveApp.Domain;

public interface IUserSettings
{
	bool ShowAllReferencedPackages { get; }
	bool HidePseudoMods { get; }
	bool LinkModAssets { get; }
	bool AdvancedIncludeEnable { get; }
	bool AssumeInternetConnectivity { get; }
}
