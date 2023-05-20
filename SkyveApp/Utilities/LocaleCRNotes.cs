using Extensions;

using SkyveApp.Domain.Steam;

using System;
using System.Windows.Forms;

namespace SkyveApp.Utilities;
internal class LocaleCRNotes : LocaleHelper
{
	private static readonly LocaleCRNotes _instance = new();

	protected LocaleCRNotes() : base($"{nameof(SkyveApp)}.Properties.CompatibilityNotes.json") { }

	public static Translation Get(string value) => _instance.GetText(value);

	public static void Load() { }
}