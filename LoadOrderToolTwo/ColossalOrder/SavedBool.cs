namespace LoadOrderToolTwo.ColossalOrder;

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
			if (this.m_AutoUpdate || !this.m_Synced)
			{
				base.Sync();
			}
			return m_Value;
		}
		set
		{
			m_Value = value;
			if (base.settingsFile != null)
			{
				base.settingsFile.SetValue(this.m_Name, m_Value);
			}
		}
	}

	protected override void SyncImpl()
	{
		if (base.settingsFile != null)
		{
			this.m_Exists = base.settingsFile.GetValue(this.m_Name, ref m_Value);
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
