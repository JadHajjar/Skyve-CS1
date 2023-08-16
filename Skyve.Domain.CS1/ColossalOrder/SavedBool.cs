namespace Skyve.Domain.CS1.ColossalOrder;

public class SavedBool : SavedValue
{
	private bool m_Value;

	public SavedBool(string name, string fileName) : base(name, fileName, false)
	{
		m_Value = false;
	}

	public SavedBool(string name, string fileName, bool def) : base(name, fileName, false)
	{
		m_Value = def;
	}

	public SavedBool(string name, string fileName, bool def, bool autoUpdate) : base(name, fileName, autoUpdate)
	{
		m_Value = def;
	}

	public bool value
	{
		get
		{
			if (m_AutoUpdate || !m_Synced)
			{
				Sync();
			}

			return m_Value;
		}
		set
		{
			m_Value = value;
			settingsFile?.SetValue(m_Name, m_Value);
		}
	}

	protected override void SyncImpl()
	{
		if (settingsFile != null)
		{
			m_Exists = settingsFile.GetValue(m_Name, ref m_Value);
		}
	}

	public static implicit operator bool(SavedBool s)
	{
		return s.value;
	}

	public override string ToString()
	{
		return value.ToString();
	}
}
