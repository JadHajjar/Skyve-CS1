namespace LoadOrderToolTwo.ColossalOrder;

public abstract class SavedValue
{
	protected string m_Name;

	protected string m_FileName;

	protected bool m_Synced;

	protected bool m_Exists;

	protected bool m_AutoUpdate;

	private static SettingsFile? m_SettingsFile;

	public bool exists
	{
		get
		{
			Sync();
			return m_Exists;
		}
	}

	public string name => m_Name;

	public ushort version => settingsFile.version;

	public SettingsFile settingsFile
	{
		get
		{
			//if (m_SettingsFile == null && !m_Synced)
			//{
			//	m_SettingsFile = new SettingsFile() { fileName = m_FileName };
			//	m_SettingsFile.Load();
			//	///m_SettingsFile = GameSettings.FindSettingsFileByName(m_FileName);
			//}

			return m_SettingsFile!;
		}
		set { m_SettingsFile = value; }
	}

	protected void Sync()
	{
		SyncImpl();
		m_Synced = true;
	}

	public SavedValue(string name, string fileName, bool autoUpdate)
	{
		m_Name = name;
		m_FileName = fileName;
		m_AutoUpdate = autoUpdate;
	}

	public void Delete()
	{
		settingsFile.DeleteEntry(m_Name);
		m_Synced = false;
		m_Exists = false;
	}

	public static bool operator ==(SavedValue x, SavedValue y)
	{
		if (ReferenceEquals(x, null))
		{
			return ReferenceEquals(y, null);
		}
		return x.Equals(y);
	}

	public static bool operator !=(SavedValue x, SavedValue y)
	{
		return !(x == y);
	}

	public override bool Equals(object obj)
	{
		return obj != null && ((SavedValue)obj).m_Name == m_Name && ((SavedValue)obj).m_FileName == m_FileName;
	}

	public bool Equals(SavedValue obj)
	{
		return obj is not null && obj.m_Name == m_Name && obj.m_FileName == m_FileName;
	}

	public override int GetHashCode()
	{
		return m_Name.GetHashCode() ^ m_FileName.GetHashCode();
	}

	protected abstract void SyncImpl();
}
