using SkyveApp.Domain;
using SkyveApp.Domain.Compatibility.Enums;
using SkyveApp.Domain.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Services.Interfaces;
public interface IProfileManager
{
	Profile CurrentProfile { get; }
	IEnumerable<Profile> Profiles { get; }

	event Action<Profile>? ProfileChanged;
	event Action? ProfileUpdated;

	void AddProfile(Profile newProfile);
	Profile? ConvertLegacyProfile(string profilePath, bool removeLegacyFile = true);
	void ConvertLegacyProfiles();
	void CreateShortcut(Profile item);
	Task<bool> DeleteOnlineProfile(IProfile profile);
	void DeleteProfile(Profile profile);
	Task<bool> DownloadProfile(IProfile item);
	Task<bool> DownloadProfile(string link);
	void ExcludeProfile(Profile profile);
	void GatherInformation(Profile? profile);
	Asset GetAsset(Profile.Asset asset);
	string GetFileName(Profile profile);
	List<Package> GetInvalidPackages(PackageUsage usage);
	Mod GetMod(Profile.Mod mod);
	string GetNewProfileName();
	Profile? ImportProfile(string obj);
	bool IsPackageIncludedInProfile(IPackage ipackage, Profile profile);
	void MergeProfile(Profile profile);
	bool RenameProfile(Profile profile, string text);
	bool Save(Profile? profile, bool forced = false);
	void SaveLsmSettings(Profile profile);
	void SetIncludedFor(IPackage ipackage, Profile profile, bool value);
	void SetIncludedForAll<T>(T item, bool value) where T : IPackage;
	void SetProfile(Profile profile);
	Task<bool> SetVisibility(Profile profile, bool @public);
	Task Share(Profile item);
	string ToLocalPath(string? relativePath);
	string ToRelativePath(string? localPath);
}
