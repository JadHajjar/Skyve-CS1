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

	void Start(ITroubleshootSettings settings);
	void Stop();
}
