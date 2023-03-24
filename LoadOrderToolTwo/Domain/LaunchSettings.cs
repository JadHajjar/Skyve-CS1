using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadOrderToolTwo.Domain;
public class LaunchSettings
{
	public bool UseCitiesExe { get; set; }
	public bool DebugMono { get; set; }
	public bool ProfilerCities { get; set; }
	public bool NoWorkshop { get; set; }
	public bool ResetAssets { get; set; }
	public bool NoAssets { get; set; }
	public bool NoMods { get; set; }
	public bool LHT { get; set; }
	public string? SaveToLoad { get; set; }
}
