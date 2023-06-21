using SkyveApp.Domain.Utilities;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Services.Interfaces;
public interface ILogUtil
{
	string GameDataPath { get; }
	string GameLogFile { get; }

	string CreateZipFileAndSetToClipboard(string? folder = null);
	void CreateZipToStream(Stream fileStream);
	List<LogTrace> SimplifyLog(string log, out string simpleLog);
}
