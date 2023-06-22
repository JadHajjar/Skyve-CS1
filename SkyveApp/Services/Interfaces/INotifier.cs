using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Services.Interfaces;
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

	void TriggerAutoSave();
	void OnContentLoaded();
	void OnInclusionUpdated();
	void OnInformationUpdated();
	void OnWorkshopInfoUpdated();
}
