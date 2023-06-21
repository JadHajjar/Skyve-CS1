using SkyveApp.Domain.Compatibility;
using SkyveApp.Domain.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Services.Interfaces;
public interface ICompatibilityUtil
{
	ulong GetFinalSuccessor(ulong steamId);
	IndexedPackage? GetPackageData(IPackage package);
	void HandleInteraction(CompatibilityInfo info, IndexedPackageInteraction interaction);
	void HandleStatus(CompatibilityInfo info, IndexedPackageStatus status);
}
