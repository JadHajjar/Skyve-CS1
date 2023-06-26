using SkyveApp.Domain.Systems;
using SkyveApp.Services;
using SkyveApp.Services.Interfaces;
using SkyveApp.Systems;
using SkyveApp.Utilities;

namespace SkyveApp.Domain.Utilities;

internal class CachedModEnabled : CachedSaveItem<Mod, bool>
{
	private readonly ModsUtil _modUtil = Program.Services.GetService<IModUtil, ModsUtil>();

	public CachedModEnabled(Mod key, bool value) : base(key, value)
	{
	}

	public override bool CurrentValue => _modUtil.IsLocallyEnabled(Key);

	protected override void OnSave()
	{
		_modUtil.SetLocallyEnabled(Key, ValueToSave, false);
	}
}
