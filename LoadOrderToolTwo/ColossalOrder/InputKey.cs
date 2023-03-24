namespace LoadOrderToolTwo.ColossalOrder;

public struct InputKey
{
	private int m_Encoded;

	public static implicit operator int(InputKey value)
	{
		return value.m_Encoded;
	}

	public static implicit operator InputKey(int value)
	{
		return new InputKey
		{
			m_Encoded = value
		};
	}

	public override string ToString()
	{
		return SavedInputKey.ToString(m_Encoded);
	}
}
