using Extensions;

using System.Drawing;

namespace LoadOrderToolTwo.Domain.Utilities;
internal class SessionSettings : ISave
{
	public override string Name => nameof(SessionSettings) + ".json";

	public bool FirstTimeSetupCompleted { get; set; }
	public string? CurrentProfile { get; set; }
	public Rectangle? LastWindowsBounds { get; set; }
	public bool WindowWasMaximized { get; set; }
	public UserSettings UserSettings { get; set; } = new();
}
