using SkyveApp.Domain;
using SkyveApp.Domain.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Services.Interfaces;
public interface IModLogicManager
{
	void Analyze(Mod mod);
	void ApplyRequiredStates();
	bool AreMultipleLOMsPresent();
	IEnumerable<IPackage> GetPackagesThatReference(IPackage package);
	bool IsForbidden(Mod mod);
	bool IsPseudoMod(IPackage package);
	bool IsRequired(Mod mod);
	void ModRemoved(Mod mod);
}
