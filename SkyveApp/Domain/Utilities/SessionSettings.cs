using Extensions;

using System.Drawing;

namespace SkyveApp.Domain.Utilities;
public class SessionSettings : ISave
{
	public override string Name => nameof(SessionSettings) + ".json";

	public bool FirstTimeSetupCompleted { get; set; }
	public string? CurrentProfile { get; set; }
	public Rectangle? LastWindowsBounds { get; set; }
	public bool WindowWasMaximized { get; set; }
	public bool SubscribeFirstTimeShown { get; set; }
	public bool CleanupFirstTimeShown { get; set; }
	public bool FpsBoosterLogWarning { get; set; }
	public string? LastVersionNotification { get; set; }

	public UserSettings UserSettings { get; set; } = new();
}
