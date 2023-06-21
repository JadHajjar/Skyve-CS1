using SkyveApp.Domain;

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
	void DeleteProfile(Profile profile);
	void ExcludeProfile(Profile profile);
	void GatherInformation(Profile? profile);
	void MergeProfile(Profile profile);
	bool Save(Profile? profile, bool forced = false);
	void SaveLsmSettings(Profile profile);
	void SetProfile(Profile profile);
	void TriggerAutoSave();
}
