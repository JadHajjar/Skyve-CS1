using Extensions;

using SlickControls;

using System.Reflection;

namespace SkyveApp.UserInterface.Panels;
internal class PC_LotChangeLog : PC_Changelog
{
	public PC_LotChangeLog() : base(
		Assembly.GetExecutingAssembly(),
		GetChangelogFile(),
		Assembly.GetExecutingAssembly().GetName().Version)
	{
	}

	private static string GetChangelogFile()
	{
		var assembly = Assembly.GetExecutingAssembly();
		var name = $"{nameof(SkyveApp)}.Properties.Changelog.{LocaleHelper.CurrentCulture.IetfLanguageTag}.json";
		using var resource = assembly.GetManifestResourceStream(name);

		if (resource is null)
		{
			return $"{nameof(SkyveApp)}.Properties.Changelog.json";
		}

		return name;
	}
}
