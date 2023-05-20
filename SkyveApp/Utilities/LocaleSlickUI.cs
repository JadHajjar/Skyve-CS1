using Extensions;

using SkyveApp.Domain.Steam;

using System;
using System.Windows.Forms;

namespace SkyveApp.Utilities;
internal class LocaleSlickUI : LocaleHelper
{
	private static readonly LocaleSlickUI _instance = new();

	protected LocaleSlickUI() : base($"{nameof(SkyveApp)}.Properties.SlickUI.json") { }

	public static Translation Search => _instance.GetText(nameof(Search));
	public static Translation Tags => _instance.GetText(nameof(Tags));

	public static void Load() { }
}