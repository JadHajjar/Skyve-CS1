using LoadOrderToolTwo.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadOrderToolTwo.Domain.Utilities;
internal class CachedModInclusion : CachedSaveItem<Mod, bool>
{
	public CachedModInclusion(Mod key, bool value) : base(key, value)
	{ }

	public override bool CurrentValue => ModsUtil.IsLocallyIncluded(Key);

	protected override void OnSave()
	{
		ModsUtil.SetIncluded(Key, ValueToSave);
	}
}
