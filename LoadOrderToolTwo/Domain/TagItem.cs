using LoadOrderToolTwo.Domain.Enums;

using System.Collections.Generic;

namespace LoadOrderToolTwo.Domain;
public struct TagItem
{
	public TagSource Source { get; set; }
	public string Value { get; set; }

	public TagItem(TagSource source, string value)
	{
		Source = source;
		Value = value;
	}

	public override string ToString()
	{
		return Value;
	}

	public override bool Equals(object? obj)
	{
		return obj is TagItem item &&
			   Value == item.Value;
	}

	public override int GetHashCode()
	{
		return -1937169414 + EqualityComparer<string>.Default.GetHashCode(Value);
	}
}
