namespace SkyveApp.Domain.Systems;
public interface IColossalOrderModUtil
{
	bool IsEnabled(IMod mod);
	void SetEnabled(IMod mod, bool value);
	void SaveSettings();
}
