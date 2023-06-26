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
	event Action? RefreshUI;
	event Action? CompatibilityReportProcessed;
	event Action<Exception>? LoggerFailed;

	void TriggerAutoSave();
	void OnContentLoaded();
	void OnInclusionUpdated();
	void OnInformationUpdated();
	void OnWorkshopInfoUpdated();
	void OnPlaysetUpdated();
	void OnPlaysetChanged();
	void OnRefreshUI();
	void OnCompatibilityReportProcessed();
	void OnLoggerFailed(Exception ex);
}
