using System;

namespace SkyveApp.Domain.Systems;
public interface INotifier
{
	bool BulkUpdating { get; set; }
	bool IsContentLoaded { get; }
	bool ApplyingProfile { get; set; }
	bool ProfilesLoaded { get; set; }

	event Action? ContentLoaded;
	event Action? WorkshopInfoUpdated;
	event Action? PackageInformationUpdated;
	event Action? PackageInclusionUpdated;
	event Action? AutoSaveRequested;
	event Action? PlaysetUpdated;
	event Action? PlaysetChanged;

	void TriggerAutoSave();
	void OnContentLoaded();
	void OnInclusionUpdated();
	void OnInformationUpdated();
	void OnWorkshopInfoUpdated();
}
