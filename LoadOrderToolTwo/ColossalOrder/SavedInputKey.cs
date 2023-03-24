namespace LoadOrderToolTwo.ColossalOrder;

public enum KeyCode
{
	None = 0,
}

public class SavedInputKey : SavedValue
{
	private const int MASK_KEY = 268435455;

	private const int MASK_CONTROL = 1073741824;

	private const int MASK_SHIFT = 536870912;

	private const int MASK_ALT = 268435456;

	public static readonly InputKey Empty = Encode(KeyCode.None, false, false, false);

	private InputKey m_Value;

	public static InputKey Encode(KeyCode key, bool control, bool shift, bool alt)
	{
		InputKey inputKey = (int)key;
		if (control)
		{
			inputKey |= 1073741824;
		}
		if (shift)
		{
			inputKey |= 536870912;
		}
		if (alt)
		{
			inputKey |= 268435456;
		}
		return inputKey;
	}

	public SavedInputKey(string name, string fileName) : base(name, fileName, false)
	{
		m_Value = 0;
	}

	public SavedInputKey(string name, string fileName, InputKey def) : base(name, fileName, false)
	{
		m_Value = def;
	}

	public SavedInputKey(string name, string fileName, InputKey def, bool autoUpdate) : base(name, fileName, autoUpdate)
	{
		m_Value = def;
	}

	public SavedInputKey(string name, string fileName, KeyCode key, bool control, bool shift, bool alt, bool autoUpdate) : base(name, fileName, autoUpdate)
	{
		m_Value = Encode(key, control, shift, alt);
	}

	public InputKey value
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
			if (settingsFile != null)
			{
				settingsFile.SetValue(m_Name, m_Value);
			}
		}
	}

	protected override void SyncImpl()
	{
		if (settingsFile != null)
		{
			m_Exists = settingsFile.GetValue(m_Name, ref m_Value);
		}
	}

	public static implicit operator InputKey(SavedInputKey s)
	{
		return s.value;
	}

	//public bool IsPressed()
	//{
	//	int num = this.value;
	//	KeyCode keyCode = (KeyCode)(num & 268435455);
	//	return keyCode != KeyCode.None && Input.GetKey(keyCode) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) == ((num & 1073741824) != 0) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) == ((num & 536870912) != 0) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr)) == ((num & 268435456) != 0);
	//}

	//public bool IsKeyUp()
	//{
	//	int num = this.value;
	//	KeyCode keyCode = (KeyCode)(num & 268435455);
	//	return keyCode != KeyCode.None && Input.GetKeyUp(keyCode) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) == ((num & 1073741824) != 0) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) == ((num & 536870912) != 0) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr)) == ((num & 268435456) != 0);
	//}

	//public bool IsPressed(Event e)
	//{
	//	if (e.type != EventType.KeyDown)
	//	{
	//		return false;
	//	}
	//	int num = this.value;
	//	KeyCode keyCode = (KeyCode)(num & 268435455);
	//	return keyCode != KeyCode.None && e.keyCode == keyCode && (e.modifiers & EventModifiers.Control) != EventModifiers.None == ((num & 1073741824) != 0) && (e.modifiers & EventModifiers.Shift) != EventModifiers.None == ((num & 536870912) != 0) && (e.modifiers & EventModifiers.Alt) != EventModifiers.None == ((num & 268435456) != 0);
	//}

	//public bool IsPressed(EventType type, KeyCode keyCode, EventModifiers modifiers)
	//{
	//	if (type != EventType.KeyDown)
	//	{
	//		return false;
	//	}
	//	int num = this.value;
	//	KeyCode keyCode2 = (KeyCode)(num & 268435455);
	//	return keyCode2 != KeyCode.None && keyCode == keyCode2 && (modifiers & EventModifiers.Control) != EventModifiers.None == ((num & 1073741824) != 0) && (modifiers & EventModifiers.Shift) != EventModifiers.None == ((num & 536870912) != 0) && (modifiers & EventModifiers.Alt) != EventModifiers.None == ((num & 268435456) != 0);
	//}

	public KeyCode Key
	{
		get => (KeyCode)(value & 268435455);
		set => this.value = (this.value & -268435456) | (int)value;
	}

	public bool Control
	{
		get => (value & 1073741824) != 0;
		set
		{
			if (value)
			{
				this.value |= 1073741824;
				return;
			}
			this.value &= -1073741825;
		}
	}

	public bool Shift
	{
		get => (value & 536870912) != 0;
		set
		{
			if (value)
			{
				this.value |= 536870912;
				return;
			}
			this.value &= -536870913;
		}
	}

	public bool Alt
	{
		get => (value & 268435456) != 0;
		set
		{
			if (value)
			{
				this.value |= 268435456;
				return;
			}
			this.value &= -268435457;
		}
	}

	public override string ToString()
	{
		var str = string.Empty;
		if (Control)
		{
			str += "Ctrl+";
		}
		if (Alt)
		{
			str += "Alt+";
		}
		if (Shift)
		{
			str += "Shift+";
		}
		return str + Key.ToString();
	}

	//private static bool IsCommandKey(KeyCode code)
	//{
	//	return code == KeyCode.LeftCommand || code == KeyCode.RightCommand || code == KeyCode.LeftCommand || code == KeyCode.RightCommand || code == KeyCode.LeftWindows || code == KeyCode.RightWindows;
	//}

	//private static string UnityReportingWrongKeysHack(KeyCode code)
	//{
	//	if (!SavedInputKey.IsCommandKey(code))
	//	{
	//		return code.ToString();
	//	}
	//	RuntimePlatform platform = Application.platform;
	//	if (platform == RuntimePlatform.OSXEditor || platform == RuntimePlatform.OSXPlayer)
	//	{
	//		return KeyCode.LeftCommand.ToString();
	//	}
	//	return KeyCode.LeftWindows.ToString();
	//}

	//public string ToLocalizedString(string localeID)
	//{
	//	string str = string.Empty;
	//	if (this.Control)
	//	{
	//		str = str + Locale.Get(localeID, KeyCode.LeftControl.ToString()) + " + ";
	//	}
	//	if (this.Alt)
	//	{
	//		str = str + Locale.Get(localeID, KeyCode.LeftAlt.ToString()) + " + ";
	//	}
	//	if (this.Shift)
	//	{
	//		str = str + Locale.Get(localeID, KeyCode.LeftShift.ToString()) + " + ";
	//	}
	//	string text = SavedInputKey.UnityReportingWrongKeysHack(this.Key);
	//	return str + (Locale.Exists(localeID, text) ? Locale.Get(localeID, text) : text);
	//}

	public static string ToString(int value)
	{
		var str = string.Empty;
		if ((value & 1073741824) != 0)
		{
			str += "Ctrl+";
		}
		if ((value & 268435456) != 0)
		{
			str += "Alt+";
		}
		if ((value & 536870912) != 0)
		{
			str += "Shift+";
		}
		return str + ((KeyCode)(value & 268435455)).ToString();
	}

	//public static string ToLocalizedString(string localeID, int value)
	//{
	//	string str = string.Empty;
	//	if ((value & 1073741824) != 0)
	//	{
	//		str = str + Locale.Get(localeID, KeyCode.LeftControl.ToString()) + " + ";
	//	}
	//	if ((value & 268435456) != 0)
	//	{
	//		str = str + Locale.Get(localeID, KeyCode.LeftAlt.ToString()) + " + ";
	//	}
	//	if ((value & 536870912) != 0)
	//	{
	//		str = str + Locale.Get(localeID, KeyCode.LeftShift.ToString()) + " + ";
	//	}
	//	string text = SavedInputKey.UnityReportingWrongKeysHack((KeyCode)(value & 268435455));
	//	return str + (Locale.Exists(localeID, text) ? Locale.Get(localeID, text) : text);
	//}
}
