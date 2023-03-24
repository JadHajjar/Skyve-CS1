namespace GameState {
    public abstract class SavedValue {
        public SavedValue(string name, string fileName) {
            this.name = name;
            this.m_FileName = fileName;
        }

        protected string name { get; private set; }

        protected string m_FileName;

        internal bool m_Exists;
        public bool exists => m_Exists;

        private SettingsFile m_SettingsFile;
        internal SettingsFile settingsFile => m_SettingsFile ??= SettingsFile.GetOrLoad(m_FileName);

        public ushort version => settingsFile.version;

        public static bool operator ==(SavedValue x, SavedValue y) {
            if (object.ReferenceEquals(x, null)) {
                return object.ReferenceEquals(y, null);
            }
            return x.Equals(y);
        }

        public static bool operator !=(SavedValue x, SavedValue y) {
            return !(x == y);
        }

        public override bool Equals(object obj) {
            return obj != null && ((SavedValue)obj).name == name && ((SavedValue)obj).m_FileName == m_FileName;
        }

        public bool Equals(SavedValue obj) {
            return !(obj == null) && obj.name == name && obj.m_FileName == m_FileName;
        }

        public override int GetHashCode() {
            return name.GetHashCode() ^ m_FileName.GetHashCode();
        }
    }
}
