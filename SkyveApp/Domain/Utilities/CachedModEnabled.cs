using SkyveApp.Services;
using SkyveApp.Services.Interfaces;
using SkyveApp.Utilities;

namespace SkyveApp.Domain.Utilities;

internal class CachedModEnabled : CachedSaveItem<Mod, bool>
{
	private readonly IModUtil _modUtil;

	public CachedModEnabled(Mod key, bool value) : base(key, value)
	{
		_modUtil = Program.Services.GetService<IModUtil>();
	}

	public override bool CurrentValue => _modUtil.IsLocallyEnabled(Key);

	protected override void OnSave()
	{
		_modUtil.SetLocallyEnabled(Key, ValueToSave, false);
	}
}
