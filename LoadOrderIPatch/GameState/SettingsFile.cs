using System;
using System.Collections.Generic;
using System.IO;

namespace GameState {
    internal class SettingsFile {
        private const string settingsIdentifier = "CGSF";
        private const string extension = ".cgs";

        private static Dictionary<string, SettingsFile> m_SettingsFiles = new Dictionary<string, SettingsFile>();

        internal static void Add(SettingsFile settingsFile) {
            try {
                settingsFile.Load();
                m_SettingsFiles.Add(settingsFile.fileName, settingsFile);
            } catch (Exception ex) {
                GameStateUtil.logger.Error(ex.ToString());
            }
        }

        internal static SettingsFile GetOrLoad(string name) {
            SettingsFile result;
            if (!m_SettingsFiles.TryGetValue(name, out result)) {
                result = new SettingsFile { fileName = name };
                Add(result);
            }
            return result;
        }

        internal ushort version { get; private set; }

        private Dictionary<string, int> m_SettingsIntValues = new Dictionary<string, int>();

        private Dictionary<string, bool> m_SettingsBoolValues = new Dictionary<string, bool>();

        private Dictionary<string, float> m_SettingsFloatValues = new Dictionary<string, float>();

        private Dictionary<string, string> m_SettingsStringValues = new Dictionary<string, string>();

        internal string pathName { get; private set; }

        internal string fileName {
            get {
                return Path.GetFileNameWithoutExtension(pathName);
            }
            set {
                string path = GameStateUtil.localAppDataPath;
                path = Path.Combine(path, value);
                if (!path.EndsWith(extension))
                    path += extension;
                pathName = path;
            }
        }

        internal Stream CreateReadStream() {
            return new FileStream(pathName, FileMode.Open, FileAccess.Read);
        }

        internal bool IsValid() => !string.IsNullOrEmpty(pathName) && File.Exists(pathName);

        private bool ValidateID(char[] id) {
            for (int i = 0; i < 4; i++)  {
                if (id[i] != settingsIdentifier[i])
                    return false;
            }
            return true;
        }

        private void Deserialize(Stream stream) {
            using (BinaryReader binaryReader = new BinaryReader(stream)) {
                if (ValidateID(binaryReader.ReadChars(4))) {
                    version = binaryReader.ReadUInt16();
                    
                    lock (m_SettingsIntValues) {
                        m_SettingsIntValues.Clear();
                        int num = binaryReader.ReadInt32();
                        for (int i = 0; i < num; i++) {
                            string key = binaryReader.ReadString();
                            int value = binaryReader.ReadInt32();
                            m_SettingsIntValues.Add(key, value);
                        }
                    }
                    lock (m_SettingsBoolValues) {
                        m_SettingsBoolValues.Clear();
                        int num2 = binaryReader.ReadInt32();
                        for (int j = 0; j < num2; j++) {
                            string key2 = binaryReader.ReadString();
                            bool value2 = binaryReader.ReadBoolean();
                            m_SettingsBoolValues.Add(key2, value2);
                        }
                    }
                    lock (m_SettingsFloatValues) {
                        m_SettingsFloatValues.Clear();
                        int num3 = binaryReader.ReadInt32();
                        for (int k = 0; k < num3; k++) {
                            string key3 = binaryReader.ReadString();
                            float value3 = binaryReader.ReadSingle();
                            m_SettingsFloatValues.Add(key3, value3);
                        }
                    }
                    lock (m_SettingsStringValues) {
                        m_SettingsStringValues.Clear();
                        int num4 = binaryReader.ReadInt32();
                        for (int l = 0; l < num4; l++) {
                            string key4 = binaryReader.ReadString();
                            string value4 = binaryReader.ReadString();
                            m_SettingsStringValues.Add(key4, value4);
                        }
                    }
                    return;
                }
                throw new Exception("Setting file '" + fileName + "' header mismatch.");
            }
        }

        internal void Load() {
            if (IsValid()) {
                GameStateUtil.logger.Info("Loading " + pathName);
                using (Stream stream = CreateReadStream()) {
                    Deserialize(stream);
                }
            } else {
                GameStateUtil.logger.Info(pathName + " does not exists");
            }
        }


        internal bool GetValue(string name, out object v) {
            int num;
            if (m_SettingsIntValues.TryGetValue(name, out num)) {
                v = num;
                return true;
            }
            bool flag;
            if (m_SettingsBoolValues.TryGetValue(name, out flag)) {
                v = flag;
                return true;
            }
            string text;
            if (m_SettingsStringValues.TryGetValue(name, out text)) {
                v = text;
                return true;
            }
            float num2;
            if (m_SettingsFloatValues.TryGetValue(name, out num2)) {
                v = num2;
                return true;
            }
            v = null;
            return false;
        }

        internal bool GetValue(string name, ref string val) {
            string text;
            if (m_SettingsStringValues.TryGetValue(name, out text)) {
                val = text;
                return true;
            }
            return false;
        }
    
        internal bool GetValue(string name, ref bool val) {
            bool flag;
            if (m_SettingsBoolValues.TryGetValue(name, out flag)) {
                val = flag;
                return true;
            }
            return false;
        }


        internal bool GetValue(string name, ref int val) {
            int num;
            if (m_SettingsIntValues.TryGetValue(name, out num)) {
                val = num;
                return true;
            }
            return false;
        }

        internal bool GetValue(string name, ref float val) {
            float num;
            if (m_SettingsFloatValues.TryGetValue(name, out num)) {
                val = num;
                return true;
            }
            return false;
        }
    }
}
