using static Extensions.LocaleHelper;

namespace SkyveApp.Systems;
public interface ILocale
{
	public Translation Get(string key);
}
