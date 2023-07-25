using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.CS1;
using SkyveApp.Domain.Systems;
using SkyveApp.Systems.CS1.Managers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Systems.CS1.Systems;
internal class TroubleshootSystem : ITroubleshootSystem
{
	private TroubleshootState? currentState;
	private readonly IPackageManager _packageManager;
	private readonly PlaysetManager _playsetManager;
	private readonly ISettings _settings;
	private readonly INotifier _notifier;

	public TroubleshootSystem(IPackageManager packageManager, IPlaysetManager playsetManager, ISettings settings, INotifier notifier, ICitiesManager citiesManager)
	{
		ISave.Load(out currentState, "TroubleshootState.json");

		_packageManager = packageManager;
		_playsetManager = (PlaysetManager)playsetManager;
		_settings = settings;
		_notifier = notifier;

		citiesManager.MonitorTick += CitiesManager_MonitorTick;
	}

	private void CitiesManager_MonitorTick(bool isAvailable, bool isRunning)
	{
		if (currentState == null)
		{
			return;
		}

		if (currentState.Stage == ActionStage.WaitingForGameLaunch && isRunning)
		{
			NextStage();
		}
		else if (currentState.Stage == ActionStage.WaitingForGameClose && !isRunning && isAvailable)
		{
			NextStage();
		}
	}

	public void NextStage()
	{
		if (currentState is null)
			return;

		switch (currentState.Stage)
		{
			case ActionStage.ApplyingSettings:
				currentState.Stage = ActionStage.WaitingForGameLaunch;
				break;
			case ActionStage.WaitingForGameLaunch:
				currentState.Stage = ActionStage.WaitingForGameClose;
				break;
			case ActionStage.WaitingForGameClose:
				currentState.Stage = ActionStage.WaitingForConfirmation;
				break;
			case ActionStage.WaitingForConfirmation:
				currentState.Stage = ActionStage.ApplyingSettings;
				break;
		}

		Save();
	}

	public bool IsInProgress => currentState is not null;
	public int CurrentStage => currentState?.CurrentStage ?? 0;
	public int TotalStages => currentState?.TotalStages ?? 0;
	public void Start(ITroubleshootSettings settings)
	{
		var playset = new Playset();

		_playsetManager.GatherInformation(playset);

		currentState = new()
		{
			Playset = playset,
			ItemIsCausingIssues = settings.ItemIsCausingIssues,
			ProcessingItems = new(),
			UnprocessedItems = new()
		};

		IEnumerable<ILocalPackage> packages = settings.Mods ? _packageManager.Mods : _packageManager.Assets;

		foreach (var item in packages)
		{
			if (item.IsIncluded() == settings.ItemIsCausingIssues)
			{
				currentState.UnprocessedItems.Add(item.FilePath);
			}
		}

		currentState.TotalStages = (int)Math.Ceiling(Math.Log(currentState.UnprocessedItems.Count, 2));

		Save();
	}

	private void Save()
	{
		ISave.Save(currentState, "TroubleshootState.json");
	}

	public void Stop()
	{
		if (currentState is null)
		{
			return;
		}

		_playsetManager.ApplyPlayset(currentState.Playset!, false);

		_playsetManager.CurrentPlayset = _playsetManager.Playsets.FirstOrDefault(x => x.Name == currentState.PlaysetName);
		
		_notifier.OnPlaysetChanged();
		
		_settings.SessionSettings.CurrentPlayset = currentState.PlaysetName;
		_settings.SessionSettings.Save();

		ISave.Delete("TroubleshootState.json");

		currentState = null;
	}

	public class TroubleshootState
	{
        public string? PlaysetName { get; set; }
        public Playset? Playset { get; set; }
        public List<string>? UnprocessedItems { get; set; }
        public List<string>? ProcessingItems { get; set; }
		public ActionStage Stage { get; set; }
		public int CurrentStage { get; set; }
		public int TotalStages { get; set; }
        public bool ItemIsCausingIssues { get; set; }
    }

	public enum ActionStage
	{
		ApplyingSettings,
		WaitingForGameLaunch,
		WaitingForGameClose,
		WaitingForConfirmation,
	}
}
