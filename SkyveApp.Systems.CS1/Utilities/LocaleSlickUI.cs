using Extensions;

namespace SkyveApp.Systems.CS1.Utilities;
public class LocaleSlickUI : LocaleHelper
{
	private static readonly LocaleSlickUI _instance = new();

	protected LocaleSlickUI() : base($"{nameof(SkyveApp)}.Properties.SlickUI.json") { }

	public static Translation Search => _instance.GetText(nameof(Search));
	public static Translation Tags => _instance.GetText(nameof(Tags));
	public static Translation TaskCompleted => _instance.GetText(nameof(TaskCompleted));
	public static Translation UnexpectedError => _instance.GetText(nameof(UnexpectedError));
	public static Translation Apply => _instance.GetText(nameof(Apply));
	public static Translation Cancel => _instance.GetText(nameof(Cancel));
	public static Translation Download => _instance.GetText(nameof(Download));
	public static Translation Remove => _instance.GetText(nameof(Remove));

	public static void Load() { _ = _instance; }
}