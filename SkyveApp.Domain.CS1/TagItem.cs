using SkyveApp.Domain.Enums;

using SlickControls;

using System.Collections.Generic;

namespace SkyveApp.Domain;
public struct TagItem : ITag
{
	public TagSource Source { get; set; }
	public string Value { get; set; }
	public readonly string Icon => Source switch { TagSource.Workshop => "I_Steam", TagSource.FindIt => "I_Search", _ => "I_Tag" };

	public TagItem(TagSource source, string value)
	{
		Source = source;
		Value = value;
	}

	public override readonly string ToString()
	{
		return Value;
	}

	public override readonly bool Equals(object? obj)
	{
		return obj is TagItem item &&
			   Value == item.Value;
	}

	public override readonly int GetHashCode()
	{
		return -1937169414 + EqualityComparer<string>.Default.GetHashCode(Value);
	}
}
