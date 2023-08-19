namespace Skyve.App.CS1;
internal class LocaleChangelog : LocaleHelper
{
	private static readonly LocaleChangelog _instance = new();

	protected LocaleChangelog() : base($"SkyveApp.Properties.Changelog.json") { }

	public static void Load() { _ = _instance; }
}
