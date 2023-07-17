using Extensions;

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

	public TroubleshootSystem(IPackageManager packageManager, IPlaysetManager playsetManager, ISettings settings, INotifier notifier)
	{
		ISave.Load(out currentState, "TroubleshootState.json");

		_packageManager = packageManager;
		_playsetManager = (PlaysetManager)playsetManager;
		_settings = settings;
		_notifier = notifier;
	}

	public bool IsInProgress() => currentState is not null;
	public int CurrentStage => currentState?.CurrentStage ?? 0;
	public int TotalStages => currentState?.TotalStages ?? 0;
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
		public int CurrentStage { get; set; }
        public int TotalStages { get; set; }
	}
}
