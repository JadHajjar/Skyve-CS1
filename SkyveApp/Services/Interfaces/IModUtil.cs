using SkyveApp.Domain;
using SkyveApp.Domain.Enums;
using SkyveApp.Domain.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Services.Interfaces;
public interface IModUtil
{
	Mod FindMod(string? folder);
	Mod? GetMod(Package package);
	DownloadStatus GetStatus(IPackage mod, out string reason);
	bool IsEnabled(Mod mod);
	bool IsIncluded(Mod mod);
	bool IsLocallyEnabled(Mod mod);
	bool IsLocallyIncluded(Mod mod);
	void SavePendingValues();
	void SetEnabled(Mod mod, bool value, bool save = true);
	void SetIncluded(Mod mod, bool value);
	void SetLocallyEnabled(Mod mod, bool value, bool save);
	void SetLocallyIncluded(Mod mod, bool value);
}
