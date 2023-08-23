using Extensions;

using Skyve.Domain;
using Skyve.Domain.CS1;
using Skyve.Domain.Enums;
using Skyve.Domain.Systems;
using Skyve.Systems.CS1.Managers;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Skyve.Systems.CS1.Systems;
internal class TroubleshootSystem : ITroubleshootSystem
{
	private TroubleshootState? currentState;
	private readonly IPackageManager _packageManager;
	private readonly IModLogicManager _modLogicManager;
	private readonly PlaysetManager _playsetManager;
	private readonly ISettings _settings;
	private readonly INotifier _notifier;
	private readonly IBulkUtil _bulkUtil;
	private readonly IModUtil _modUtil;

	public event Action? StageChanged;
	public event Action? AskForConfirmation;
	public event Action<List<ILocalPackage>>? PromptResult;

	public bool IsInProgress => currentState is not null;
	public string CurrentAction => LocaleHelper.GetGlobalText(currentState?.Stage.ToString());
	public bool WaitingForGameLaunch => currentState?.Stage is ActionStage.WaitingForGameLaunch;
	public bool WaitingForGameClose => currentState?.Stage is ActionStage.WaitingForGameClose;
	public bool WaitingForPrompt => currentState?.Stage is ActionStage.WaitingForConfirmation;
	public int CurrentStage => currentState?.CurrentStage ?? 0;
	public int TotalStages => currentState?.TotalStages ?? 0;

	public TroubleshootSystem(IPackageManager packageManager, IPlaysetManager playsetManager, ISettings settings, INotifier notifier, ICitiesManager citiesManager, IBulkUtil bulkUtil, IModLogicManager modLogicManager, IModUtil modUtil)
	{
		try
		{ ISave.Load(out currentState, "TroubleshootState.json"); }
		catch { }

		_packageManager = packageManager;
		_playsetManager = (PlaysetManager)playsetManager;
		_modLogicManager = modLogicManager;
		_settings = settings;
		_notifier = notifier;
		_bulkUtil = bulkUtil;
		_modUtil = modUtil;

		citiesManager.MonitorTick += CitiesManager_MonitorTick;
	}

	public void Start(ITroubleshootSettings settings)
	{
		var playset = new Playset();

		_playsetManager.GatherInformation(playset);

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

		var packageToProcess = new List<ILocalPackage>();

		foreach (var item in packages)
		{
			if (CheckPackageValidity(settings, item))
			{
				if (item is IMod mod && _modLogicManager.IsRequired(mod, _modUtil))
				{
					continue;
				}

				if (item.GetPackageInfo()?.Statuses?.Any(x => x.Type is StatusType.StandardMod) == true)
				{
					continue;
				}

				packageToProcess.Add(item);
			}
		}

		currentState.ProcessingItems = currentState.UnprocessedItems = GetItemGroups(packageToProcess.ToList());

		currentState.TotalStages = (int)Math.Ceiling(Math.Log(currentState.UnprocessedItems.Count, 2));

		if (currentState.TotalStages == 0)
		{
			if (packageToProcess.Any())
			{
				PromptResult?.Invoke(packageToProcess);
			}

			currentState = null;

			return;
		}

		_playsetManager.SetCurrentPlayset(Playset.TemporaryPlayset);

		ApplyConfirmation(true);
	}

	private static bool CheckPackageValidity(ITroubleshootSettings settings, ILocalPackage item)
	{
		if (settings.ItemIsCausingIssues)
		{
			return item.IsIncluded() == true;
		}

		if (settings.ItemIsMissing)
		{
			return item.IsIncluded() == false;
		}

		if (settings.NewItemCausingIssues)
		{
			if (!item.IsIncluded())
			{
				return false;
			}

			if (item.LocalTime > DateTime.Today.AddDays(-7))
			{
				return true;
			}
		}

		return false;
	}

	public void Stop(bool keepSettings)
	{
		if (currentState is null)
		{
			return;
		}

		if (!keepSettings)
		{
			_playsetManager.ApplyPlayset(currentState.Playset!, false);
		}

		_playsetManager.CurrentPlayset = _playsetManager.Playsets.FirstOrDefault(x => x.Name == currentState.PlaysetName) ?? Playset.TemporaryPlayset;

		_notifier.OnPlaysetChanged();

		_settings.SessionSettings.CurrentPlayset = currentState.PlaysetName;
		_settings.SessionSettings.Save();

		ISave.Delete("TroubleshootState.json");

		currentState = null;

		StageChanged?.Invoke();
	}

	public void ApplyConfirmation(bool issuePersists)
	{
		if (currentState?.Stage == ActionStage.WaitingForConfirmation)
		{
			NextStage();

			ApplyNextSettings(issuePersists);

			NextStage();
		}
	}

	public void NextStage()
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
	}

	private void ApplyNextSettings(bool issuePersists)
	{
		_playsetManager.ApplyPlayset(currentState!.Playset!, false);

		var lists = SplitGroup(issuePersists ? currentState.ProcessingItems! : currentState.UnprocessedItems!);

		if (lists.processingItems.Count == 1 && lists.unprocessedItems.Count == 0)
		{
			if (lists.processingItems[0].Count > 1)
			{
				lists = SplitGroup(lists.processingItems[0].ToList(x => new List<string> { x }));
			}
			else
			{
				_bulkUtil.SetBulkIncluded(GetPackages(new[] { lists.processingItems[0][0] }), currentState.ItemIsMissing);

				PromptResult?.Invoke(GetPackages(new[] { lists.processingItems[0][0] }).ToList());

				Stop(true);

				return;
			}
		}

		currentState.ProcessingItems = lists.processingItems;
		currentState.UnprocessedItems = lists.unprocessedItems;

		_bulkUtil.SetBulkIncluded(GetPackages(currentState.ProcessingItems.SelectMany(x => x)), currentState.ItemIsMissing);

		currentState.CurrentStage++;
	}

	private (List<List<string>> processingItems, List<List<string>> unprocessedItems) SplitGroup(List<List<string>> list)
	{
		var list1 = new List<List<string>>();
		var list2 = new List<List<string>>();

		foreach (var item in list.OrderByDescending(x => x.Count))
		{
			if (list1.Sum(x => x.Count) > list2.Sum(x => x.Count))
			{
				list2.Add(item);
			}
			else
			{
				list1.Add(item);
			}
		}

		return (list1, list2);
	}

	private List<List<string>> GetItemGroups(List<ILocalPackage> items)
	{
		var groups = new List<List<string>>();

		while (items.Count > 0)
		{
			var item = items[0];

			var list = new List<string>();

			GetPairedItems(items, list, item);

			groups.Add(list);

			items.RemoveAll(x => list.Contains(x.FilePath));
		}

		return groups;
	}

	private void GetPairedItems(List<ILocalPackage> items, List<string> group, ILocalPackage current)
	{
		foreach (var item in items)
		{
			if (group.Contains(item.FilePath))
			{
				continue;
			}

			if (item == current)
			{
				group.Add(item.FilePath);
			}
			else if (!currentState!.ItemIsMissing && currentState!.Mods && AreItemsPaired(item, current))
			{
				GetPairedItems(items, group, item);
			}
		}
	}

	private bool AreItemsPaired(ILocalPackage packageA, ILocalPackage packageB)
	{
		if (packageA != null && packageB != null)
		{
			return packageA.Requirements.Any(x => x.Id == packageB.Id)
				|| packageB.Requirements.Any(x => x.Id == packageA.Id);
		}

		return false;
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

	private void Save()
	{
		ISave.Save(currentState, "TroubleshootState.json");
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

	public class TroubleshootState : ITroubleshootSettings
	{
		public string? PlaysetName { get; set; }
		public Playset? Playset { get; set; }
		public List<List<string>>? UnprocessedItems { get; set; }
		public List<List<string>>? ProcessingItems { get; set; }
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
