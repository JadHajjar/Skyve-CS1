namespace GameState {
    public class SavedFloat : SavedValue {
        public readonly float value;

        public SavedFloat(string name, string fileName) : this(name, fileName, 0) { }

        public SavedFloat(string name, string fileName, float def) : base(name, fileName) {
            value = def;
            m_Exists = settingsFile.GetValue(name, ref value);
        }

        public static implicit operator float(SavedFloat s) {
            return s.value;
        }

        public override string ToString() {
            return value.ToString();
        }
    }
}
