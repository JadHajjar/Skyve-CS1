using ICities;

namespace KianCommons.IImplict;
internal interface IMod
{
	void OnEnabled();
	void OnDisabled();
}

internal interface IModWithSettings : IUserMod
{
	void OnSettingsUI(UIHelper helper);
}
