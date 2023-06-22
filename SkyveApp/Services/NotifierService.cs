using SkyveApp.Domain.Utilities;
using SkyveApp.Services.Interfaces;

using System;

namespace SkyveApp.Services;
internal class NotifierService : INotifier
{
	public event Action? ContentLoaded;
	public event Action? WorkshopInfoUpdated;
	public event Action? PackageInformationUpdated;
	public event Action? PackageInclusionUpdated;
	public event Action? AutoSaveRequested;

	private readonly DelayedAction _delayedPackageInformationUpdated;
	private readonly DelayedAction _delayedPackageInclusionUpdated;
	private readonly DelayedAction _delayedWorkshopInfoUpdated;
	private readonly DelayedAction _delayedContentLoaded;
	private readonly DelayedAction _delayedAutoSaveRequested;

	public NotifierService()
	{
		_delayedContentLoaded = new(300, () => ContentLoaded?.Invoke());
		_delayedPackageInformationUpdated = new(300, () => PackageInformationUpdated?.Invoke());
		_delayedPackageInclusionUpdated = new(300, () => PackageInclusionUpdated?.Invoke());
		_delayedWorkshopInfoUpdated = new(300, () => WorkshopInfoUpdated?.Invoke());
		_delayedAutoSaveRequested = new(300, () => AutoSaveRequested?.Invoke());
	}

	public bool IsContentLoaded { get; private set; }
	public bool BulkUpdating { get; set; }
	public bool ApplyingProfile { get; set; }
	public bool ProfilesLoaded { get; set; }

	public void OnContentLoaded()
	{
		IsContentLoaded = true;

		_delayedContentLoaded.Run();
	}

	public void OnWorkshopInfoUpdated()
	{
		if (IsContentLoaded)
		{
			_delayedWorkshopInfoUpdated.Run();
		}
	}

	public void OnInformationUpdated()
	{
		if (IsContentLoaded)
		{
			_delayedPackageInformationUpdated.Run();
		}
	}

	public void OnInclusionUpdated()
	{
		if (IsContentLoaded)
		{
			_delayedPackageInclusionUpdated.Run();
			_delayedPackageInformationUpdated.Run();
		}
	}

	public void TriggerAutoSave()
	{
		_delayedAutoSaveRequested.Run();
	}
}
