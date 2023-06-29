namespace SkyveApp.Domain;
public interface ISessionSettings
{
	string? CurrentPlayset { get; }
	bool FirstTimeSetupCompleted { get; }

	void Save();
}
