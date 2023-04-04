using LoadOrderToolTwo.Domain.Enums;

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
}
