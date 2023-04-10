using Extensions;

using System;
using System.Drawing;

namespace LoadOrderToolTwo.Domain.Utilities;
internal class SessionSettings : ISave
{
	public override string Name => nameof(SessionSettings) + ".json";

	public bool FirstTimeSetupCompleted { get; set; }
	public string? CurrentProfile { get; set; }
	public Rectangle? LastWindowsBounds { get; set; }
	public bool WindowWasMaximized { get; set; }
    public bool SubscribeInfoShown { get; set; }
	public bool FpsBoosterLogWarning { get; set; }
	public string? LastVersionNotification { get; set; }

	public UserSettings UserSettings { get; set; } = new();
}
