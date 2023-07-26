using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.CS1;
using SkyveApp.Domain.Systems;
using SkyveApp.Systems.CS1.Managers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkyveApp.Systems.CS1.Systems;
internal class TroubleshootSystem : ITroubleshootSystem
{
	private TroubleshootState? currentState;
	private readonly IPackageManager _packageManager;
	private readonly PlaysetManager _playsetManager;
	private readonly ISettings _settings;
	private readonly INotifier _notifier;
	private readonly IBulkUtil _bulkUtil;
	private readonly IModLogicManager _modLogicManager;
	private readonly IModUtil _modUtil;

	public event Action? StageChanged;
	public event Action? AskForConfirmation;

	public bool IsInProgress => currentState is not null;
	public string CurrentAction => LocaleHelper.GetGlobalText(currentState?.Stage.ToString());
	public bool CanSkip => currentState?.Stage is ActionStage.WaitingForGameClose or ActionStage.WaitingForGameLaunch;
	public int CurrentStage => currentState?.CurrentStage ?? 0;
	public int TotalStages => currentState?.TotalStages ?? 0;

	public TroubleshootSystem(IPackageManager packageManager, IPlaysetManager playsetManager, ISettings settings, INotifier notifier, ICitiesManager citiesManager, IBulkUtil bulkUtil)
	{
		ISave.Load(out currentState, "TroubleshootState.json");

		_packageManager = packageManager;
		_playsetManager = (PlaysetManager)playsetManager;
		_settings = settings;
		_notifier = notifier;
		_bulkUtil = bulkUtil;

		citiesManager.MonitorTick += CitiesManager_MonitorTick;
	}

	private async void CitiesManager_MonitorTick(bool isAvailable, bool isRunning)
	{
		if (currentState == null)
		{
			return;
		}

		if (currentState.Stage == ActionStage.WaitingForGameLaunch && isRunning)
		{
			await NextStage();
		}
		else if (currentState.Stage == ActionStage.WaitingForGameClose && !isRunning && isAvailable)
		{
			await NextStage();
		}
	}

	public async Task ApplyConfirmation(bool issuePersists)
	{
		if (currentState?.Stage == ActionStage.WaitingForConfirmation)
		{
			await NextStage();

			ApplyNextSettings(issuePersists);

			await NextStage();
		}
	}

	public async Task NextStage()
	{
		if (currentState is null)
		{
			return;
		}

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

		StageChanged?.Invoke();

		if (currentState.Stage is ActionStage.WaitingForConfirmation)
		{
			AskForConfirmation?.Invoke();
		}

		await Task.CompletedTask;
	}

	private void ApplyNextSettings(bool issuePersists)
	{
		if (currentState!.ItemIsCausingIssues)
		{
			_playsetManager.ApplyPlayset(currentState.Playset!, false);

			if (!issuePersists)
			{
				currentState.UnprocessedItems = currentState.ProcessingItems;
			}

			currentState.ProcessingItems = new();

			var itemsToTake = (int)Math.Floor(currentState.UnprocessedItems!.Count / 2F);

			for (var i = 0; i < itemsToTake; i++)
			{
				currentState.ProcessingItems.Add(currentState.UnprocessedItems[0]);
				currentState.UnprocessedItems.RemoveAt(0);
			}

			_bulkUtil.SetBulkIncluded(GetPackages(currentState.ProcessingItems), false);

			currentState.CurrentStage++;
		}
	}

	public async Task Start(ITroubleshootSettings settings)
	{
		var playset = new Playset();

		_playsetManager.GatherInformation(playset);

		_playsetManager.SetCurrentPlayset(Playset.TemporaryPlayset);

		currentState = new()
		{
			Stage = ActionStage.WaitingForConfirmation,
			Playset = playset,
			Mods = settings.Mods,
			ItemIsCausingIssues = settings.ItemIsCausingIssues,
			ItemIsMissing = settings.ItemIsMissing,
			NewItemCausingIssues = settings.NewItemCausingIssues,
			ProcessingItems = new(),
			UnprocessedItems = new()
		};

		IEnumerable<ILocalPackage> packages = settings.Mods ? _packageManager.Mods : _packageManager.Assets;

		foreach (var item in packages)
		{
			if (item.IsIncluded() == settings.ItemIsCausingIssues)
			{
				if (item is not IMod mod || !_modLogicManager.IsRequired(mod, _modUtil))
				{
					currentState.UnprocessedItems.Add(item.FilePath);
				}
			}
		}

		currentState.TotalStages = (int)Math.Ceiling(Math.Log(currentState.UnprocessedItems.Count, 2));

		await ApplyConfirmation(true);
	}

	private void Save()
	{
		ISave.Save(currentState, "TroubleshootState.json");
	}

	public async Task Stop()
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

		await Task.CompletedTask;
	}

	private IEnumerable<ILocalPackage> GetPackages(IEnumerable<string> packagePaths)
	{
		IEnumerable<ILocalPackage> packages = currentState!.Mods ? _packageManager.Mods : _packageManager.Assets;

		foreach (var package in packages)
		{
			if (packagePaths.Contains(package.FilePath))
			{
				yield return package;
			}
		}
	}

	public class TroubleshootState : ITroubleshootSettings
	{
		public string? PlaysetName { get; set; }
		public Playset? Playset { get; set; }
		public List<string>? UnprocessedItems { get; set; }
		public List<string>? ProcessingItems { get; set; }
		public ActionStage Stage { get; set; }
		public int CurrentStage { get; set; }
		public int TotalStages { get; set; }
		public bool ItemIsCausingIssues { get; set; }
		public bool ItemIsMissing { get; set; }
		public bool NewItemCausingIssues { get; set; }
		public bool Mods { get; set; }
	}

	public enum ActionStage
	{
		ApplyingSettings,
		WaitingForGameLaunch,
		WaitingForGameClose,
		WaitingForConfirmation,
	}
}
