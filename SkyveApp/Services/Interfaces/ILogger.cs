using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Services.Interfaces;
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
