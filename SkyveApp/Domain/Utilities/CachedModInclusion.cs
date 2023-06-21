using SkyveApp.Services.Interfaces;
using SkyveApp.Utilities;

namespace SkyveApp.Domain.Utilities;
internal class CachedModInclusion : CachedSaveItem<Mod, bool>
{
	private readonly IModUtil _modUtil;

	public CachedModInclusion(Mod key, bool value) : base(key, value)
	{
		_modUtil = Program.Services.GetService<IModUtil>();
	}

	public override bool CurrentValue => _modUtil.IsLocallyIncluded(Key);

	protected override void OnSave()
	{
		_modUtil.SetLocallyIncluded(Key, ValueToSave);
	}
}
