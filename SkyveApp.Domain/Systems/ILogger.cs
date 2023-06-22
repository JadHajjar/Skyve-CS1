using System;

namespace SkyveApp.Domain.Systems;
public interface ILogger
{
	string LogFilePath { get; }

	void Info(string message);
	void Warning(string message);
	void Error(string message);
	void Exception(Exception exception, string message, bool showInPanel = false);

#if DEBUG
	void Debug(string message);
#endif
}
