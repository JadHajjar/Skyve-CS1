using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Domain.Systems;
public interface ITroubleshootSystem
{
	int CurrentStage { get; }
	bool IsInProgress { get; }
	int TotalStages { get; }
	string CurrentAction { get; }
	bool CanSkip { get; }

	event Action? StageChanged;
	event Action? AskForConfirmation;
	event Action<List<ILocalPackage>>? PromptResult;

	Task ApplyConfirmation(bool issuePersists);
	Task NextStage();
	Task Start(ITroubleshootSettings settings);
	Task Stop();
}
