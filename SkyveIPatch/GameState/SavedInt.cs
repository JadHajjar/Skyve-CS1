namespace GameState {
    public class SavedInt : SavedValue {
        public readonly int value;

        public SavedInt(string name, string fileName) : this(name, fileName, 0) { }

        public SavedInt(string name, string fileName, int def) : base(name, fileName) {
            value = def;
            m_Exists = settingsFile.GetValue(name, ref value);
        }

        public static implicit operator int(SavedInt s) {
            return s.value;
        }

        public override string ToString() {
            return value.ToString();
        }
    }
}
