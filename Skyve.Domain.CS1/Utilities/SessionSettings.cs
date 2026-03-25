using Extensions;

using Newtonsoft.Json;

using System.Drawing;

namespace Skyve.Domain.CS1.Utilities;
[SaveName(nameof(SessionSettings) + ".json")]
public class SessionSettings : ISaveObject, ISessionSettings
{
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
	public SaveHandler? Handler { get; set; }

	void ISessionSettings.Save()
	{
		Handler?.Save(this);
	}
}
