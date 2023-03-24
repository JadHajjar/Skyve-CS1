using System;

namespace LoadOrderToolTwo.Domain.Utilities;

public struct PublishedFileId : IEquatable<PublishedFileId>
{
	public bool IsValid => this != invalid && AsUInt64 != 0;

	public static readonly PublishedFileId invalid = new PublishedFileId(ulong.MaxValue);

	public PublishedFileId(ulong value)
	{
		AsUInt64 = value;
	}

	public ulong AsUInt64 { get; }

	public static bool operator ==(PublishedFileId x, PublishedFileId y)
	{
		return x.AsUInt64 == y.AsUInt64;
	}

	public static bool operator !=(PublishedFileId x, PublishedFileId y)
	{
		return x.AsUInt64 != y.AsUInt64;
	}

	public bool Equals(PublishedFileId other)
	{
		return this == other;
	}

	public override bool Equals(object obj)
	{
		return obj is PublishedFileId && this == (PublishedFileId)obj;
	}

	public override int GetHashCode()
	{
		return AsUInt64.GetHashCode();
	}

	public override string ToString()
	{
		return AsUInt64.ToString();
	}

	// Note: this type is marked as 'beforefieldinit'.
	static PublishedFileId()
	{
	}
}
