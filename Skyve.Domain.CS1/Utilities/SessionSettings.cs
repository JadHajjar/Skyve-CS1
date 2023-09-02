using Extensions;

using Newtonsoft.Json;

using System.Drawing;

namespace Skyve.Domain.CS1.Utilities;
public class SessionSettings : ISave, ISessionSettings
{
	public override string Name => nameof(SessionSettings) + ".json";

	public bool FirstTimeSetupCompleted { get; set; }
	[JsonProperty("CurrentProfile")]
	public string? CurrentPlayset { get; set; }
	public Rectangle? LastWindowsBounds { get; set; }
	public bool WindowWasMaximized { get; set; }
	public bool SubscribeFirstTimeShown { get; set; }
	public bool CleanupFirstTimeShown { get; set; }
	public bool FpsBoosterLogWarning { get; set; }
	public string? LastVersionNotification { get; set; }
	public int LastVersioningNumber { get; set; }
	public bool DashboardFirstTimeShown { get; set; }

	public UserSettings UserSettings { get; set; } = new();

	void ISessionSettings.Save()
	{
		Save();
	}
}
