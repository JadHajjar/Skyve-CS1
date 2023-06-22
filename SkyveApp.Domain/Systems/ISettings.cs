namespace SkyveApp.Domain.Systems;
public interface ISettings
{
	ISessionSettings SessionSettings { get; }
	IFolderSettings FolderSettings { get; }
}
