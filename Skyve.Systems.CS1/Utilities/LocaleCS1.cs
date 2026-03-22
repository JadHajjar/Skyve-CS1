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

	public static Translation TotalSubbedSize => _instance.GetText(nameof(TotalSubbedSize));
	public static Translation TotalLocalModsSize => _instance.GetText(nameof(TotalLocalModsSize));
	public static Translation TotalSavesSize => _instance.GetText(nameof(TotalSavesSize));
	public static Translation TotalOtherSize => _instance.GetText(nameof(TotalOtherSize));
	public static Translation TotalCitiesSize => _instance.GetText(nameof(TotalCitiesSize));
	public static Translation DiskStatus => _instance.GetText(nameof(DiskStatus));
}