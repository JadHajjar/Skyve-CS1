using Extensions;

using System;
using System.Windows.Forms;

namespace LoadOrderToolTwo.Utilities;
internal class LocaleSlickUI : LocaleHelper
{
	private static readonly LocaleSlickUI _instance = new();

	protected LocaleSlickUI() : base($"{nameof(LoadOrderToolTwo)}.Properties.SlickUI.json") { }

	public static Translation Search => _instance.GetText(nameof(Search));

	public static void Load() { }
}