using Extensions;

using Skyve.Domain.Systems;

namespace Skyve.Systems.CS1.Utilities;
public class LocaleCS1 : LocaleHelper, ILocale
{
	private static readonly LocaleCS1 _instance = new();

	public static void Load() { _ = _instance; }

	public Translation Get(string key)
	{
		return GetGlobalText(key);
	}

	public LocaleCS1() : base($"Skyve.Systems.CS1.Properties.LocaleCS1.json") { }
}