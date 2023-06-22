namespace SkyveApp.Domain.Systems;
public interface IModLogicManager
{
	void Analyze(IMod mod);
	void ApplyRequiredStates();
	bool IsForbidden(IMod mod);
	bool IsPseudoMod(IPackage package);
	bool IsRequired(IMod mod);
	void ModRemoved(IMod mod);
}
