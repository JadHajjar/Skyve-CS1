namespace GameState
{
	public class SavedString : SavedValue
	{
        public readonly string value;

        public SavedString(string name, string fileName) : this(name, fileName, "") { }

        public SavedString(string name, string fileName, string def) : base(name, fileName) {
            value = def;
            m_Exists = settingsFile.GetValue(name, ref value);
        }

        public static implicit operator string(SavedString s) {
            return s.value;
        }

        public override string ToString() {
            return value.ToString();
        }
    }
}
