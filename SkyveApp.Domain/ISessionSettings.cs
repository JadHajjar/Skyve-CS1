namespace SkyveApp.Domain;
public interface ISessionSettings
{
	string? CurrentPlayset { get; }
	bool FirstTimeSetupCompleted { get; set; }

	void Save();
}
