using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Domain.Systems;
public interface IModDllManager
{
	void SaveDllCache(); 
	void ClearDllCache();
	void SetDllModCache(string path, bool isMod, Version? version);
	bool? GetDllModCache(string path, out Version? version);
}
