using SkyveApp.Utilities;

namespace SkyveApp.Domain.Utilities;

internal class CachedModEnabled : CachedSaveItem<Mod, bool>
{
	public CachedModEnabled(Mod key, bool value) : base(key, value)
	{ }

	public override bool CurrentValue => ModsUtil.IsLocallyEnabled(Key);

	protected override void OnSave()
	{
		ModsUtil.SetLocallyEnabled(Key, ValueToSave, false);
	}
}
