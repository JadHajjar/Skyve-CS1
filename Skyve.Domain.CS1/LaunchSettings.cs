using Newtonsoft.Json;

namespace Skyve.Domain.CS1;
public class LaunchSettings
{
	public bool UseCitiesExe { get; set; }
	public bool DebugMono { get; set; }
	public bool UnityProfiler { get; set; }
	public bool NoWorkshop { get; set; }
	public bool ResetAssets { get; set; }
	public bool NoAssets { get; set; }
	public bool NoMods { get; set; }
	public bool LHT { get; set; }
	public string? SaveToLoad { get; set; }
	public bool LoadSaveGame { get; set; }
	public bool DevUi { get; set; }
	public string? MapToLoad { get; set; }
	public bool StartNewGame { get; set; }
	public string? CustomArgs { get; set; }
	public bool NewAsset { get; set; }
	public bool LoadAsset { get; set; }
	[JsonIgnore] public bool RefreshWorkshop { get; set; }
}
