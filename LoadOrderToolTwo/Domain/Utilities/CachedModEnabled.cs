using LoadOrderToolTwo.Utilities;

namespace LoadOrderToolTwo.Domain.Utilities;

internal class CachedModEnabled : CachedSaveItem<Mod, bool>
{
	public CachedModEnabled(Mod key, bool value) : base(key, value)
	{ }

	public override bool CurrentValue => ModsUtil.IsLocallyEnabled(Key);

	protected override void OnSave()
	{
		ModsUtil.SetEnabled(Key, ValueToSave, false);
	}
}
