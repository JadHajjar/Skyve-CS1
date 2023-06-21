using SkyveApp.Domain;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Services.Interfaces;
public interface IColossalOrderUtil
{
	bool IsEnabled(Mod mod);
	void SetEnabled(Mod mod, bool value);
	void SaveSettings();
	void Start();
}
