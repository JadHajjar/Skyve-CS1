using Extensions;

using SkyveApp.Domain.Systems;

using System;

namespace SkyveApp.Systems;
internal class NotifierSystem : INotifier
{
	public event Action? ContentLoaded;
	public event Action? WorkshopInfoUpdated;
	public event Action? PackageInformationUpdated;
	public event Action? PackageInclusionUpdated;
	public event Action? AutoSaveRequested;
	public event Action? PlaysetUpdated;
	public event Action? PlaysetChanged;
	public event Action? RefreshUI;
	public event Action<Exception>? LoggerFailed;
	public event Action? CompatibilityReportProcessed;
	public event Action? WorkshopPackagesInfoLoaded;
	public event Action? WorkshopUsersInfoLoaded;
	public event Action? CompatibilityDataLoaded;

	private readonly DelayedAction _delayedPackageInformationUpdated;
	private readonly DelayedAction _delayedPackageInclusionUpdated;
	private readonly DelayedAction _delayedWorkshopInfoUpdated;
	private readonly DelayedAction _delayedContentLoaded;
	private readonly DelayedAction _delayedAutoSaveRequested;
	private readonly DelayedAction _delayedImageLoaded;

	public NotifierSystem()
	{
		_delayedContentLoaded = new(300, () => ContentLoaded?.Invoke());
		_delayedPackageInformationUpdated = new(300, () => PackageInformationUpdated?.Invoke());
		_delayedPackageInclusionUpdated = new(300, () => PackageInclusionUpdated?.Invoke());
		_delayedWorkshopInfoUpdated = new(300, () => WorkshopInfoUpdated?.Invoke());
		_delayedAutoSaveRequested = new(300, () => AutoSaveRequested?.Invoke());
		_delayedImageLoaded = new(300, () => RefreshUI?.Invoke());
	}

	public bool IsContentLoaded { get; private set; }
	public bool BulkUpdating { get; set; }
	public bool ApplyingPlayset { get; set; }
	public bool PlaysetsLoaded { get; set; }

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

	public void OnRefreshUI(bool now = false)
	{
		if (now)
		{
			RefreshUI?.Invoke();
		}
		else
		{
			_delayedImageLoaded.Run();
		}
	}

	public void OnPlaysetUpdated()
	{
		PlaysetUpdated?.Invoke();
	}

	public void OnPlaysetChanged()
	{
		PlaysetChanged?.Invoke();
	}

	public void OnLoggerFailed(Exception ex)
	{
		LoggerFailed?.Invoke(ex);
	}

	public void OnCompatibilityReportProcessed()
	{
		CompatibilityReportProcessed?.Invoke();
	}

	public void OnWorkshopPackagesInfoLoaded()
	{
		WorkshopPackagesInfoLoaded?.Invoke();
	}

	public void OnWorkshopUsersInfoLoaded()
	{
		WorkshopUsersInfoLoaded?.Invoke();
	}

	public void OnCompatibilityDataLoaded()
	{
		CompatibilityDataLoaded?.Invoke();
	}
}
