﻿using Extensions;

namespace SkyveApp.Utilities;
internal class LocaleCRNotes : LocaleHelper
{
	private static readonly LocaleCRNotes _instance = new();

	protected LocaleCRNotes() : base($"{nameof(SkyveApp)}.Properties.CompatibilityNotes.json") { }

	public static Translation Get(string value)
	{
		return _instance.GetText(value);
	}

	public static void Load() { }
}