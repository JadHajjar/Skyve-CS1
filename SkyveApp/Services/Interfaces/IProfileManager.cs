using SkyveApp.Domain;
using SkyveApp.Domain.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Services.Interfaces;
public interface IProfileManager
{
	bool ApplyingProfile { get; }
	Profile CurrentProfile { get; }
	IEnumerable<Profile> Profiles { get; }
	bool ProfilesLoaded { get; }

	event Action<Profile>? ProfileChanged;
	event Action? ProfileUpdated;

	void ConvertLegacyProfiles();
	void CreateShortcut(Profile item);
	Task<bool> DeleteOnlineProfile(IProfile profile);
	void DeleteProfile(Profile profile);
	Task<bool> DownloadProfile(IProfile item);
	Task<bool> DownloadProfile(string link);
	void ExcludeProfile(Profile profile);
	void GatherInformation(Profile? profile);
	string GetFileName(Profile profile);
	void MergeProfile(Profile profile);
	bool Save(Profile? profile, bool forced = false);
	void SaveLsmSettings(Profile profile);
	void SetProfile(Profile profile);
	Task<bool> SetVisibility(Profile profile, bool @public);
	Task Share(Profile item);
	string ToLocalPath(string? relativePath);
	string ToRelativePath(string? localPath);
	void TriggerAutoSave();
}
