using SkyveApp.Domain;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Services.Interfaces;
public interface IUpdateManager
{
	bool IsFirstTime();
	bool IsPackageKnown(Package package);
}
