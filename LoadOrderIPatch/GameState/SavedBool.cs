namespace GameState {
    public class SavedBool : SavedValue {
        public readonly bool value;

        public SavedBool(string name, string fileName) : this(name, fileName, false) { }

        public SavedBool(string name, string fileName, bool def) : base(name, fileName) {
            value = def;
            m_Exists = settingsFile.GetValue(name, ref value);
        }

        public static implicit operator bool(SavedBool s) {
            return s.value;
        }

        public override string ToString() {
            return value.ToString();
        }
    }
}
