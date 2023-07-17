using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Domain.Systems;
public interface IIOUtil
{
	Process? Execute(string exeFile, string args, bool useShellExecute = true, bool createNoWindow = false);
	void RunBatch(string command);
	string? ToRealPath(string? path);
	void WaitForUpdate();
}
