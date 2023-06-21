using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Services.Interfaces;
public interface ICitiesManager
{
	event CitiesManager.MonitorTickDelegate? MonitorTick;

	bool CitiesAvailable();
	bool IsRunning();
	void Kill();
	void Launch();
	void RunStub();
}
