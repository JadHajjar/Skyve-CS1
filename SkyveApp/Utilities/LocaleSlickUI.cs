using Extensions;

namespace SkyveApp.Utilities;
internal class LocaleSlickUI : LocaleHelper
{
	private static readonly LocaleSlickUI _instance = new();

	protected LocaleSlickUI() : base($"{nameof(SkyveApp)}.Properties.SlickUI.json") { }

	public static Translation Search => _instance.GetText(nameof(Search));
	public static Translation Tags => _instance.GetText(nameof(Tags));
	public static Translation TaskCompleted => _instance.GetText(nameof(TaskCompleted));
	public static Translation UnexpectedError => _instance.GetText(nameof(UnexpectedError));

	public static void Load() { }
}