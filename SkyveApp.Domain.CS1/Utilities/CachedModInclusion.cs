using SkyveApp.Domain.Systems;
using SkyveApp.Services.Interfaces;
using SkyveApp.Systems;
using SkyveApp.Utilities;

namespace SkyveApp.Domain.Utilities;
internal class CachedModInclusion : CachedSaveItem<Mod, bool>
{
	private readonly ModsUtil _modUtil = Program.Services.GetService<IModUtil, ModsUtil>();

	public CachedModInclusion(Mod key, bool value) : base(key, value)
	{
	}

	public override bool CurrentValue => _modUtil.IsLocallyIncluded(Key);

	protected override void OnSave()
	{
		_modUtil.SetLocallyIncluded(Key, ValueToSave);
	}
}
