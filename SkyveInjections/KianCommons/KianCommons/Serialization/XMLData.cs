/* from Keallu's Hide It mod. */

namespace KianCommons.Serialization {
    using ColossalFramework.IO;
    using System;
    using System.IO;
    using System.Linq;

    public abstract class XMLData<C> : IXMLData
        where C : class, IXMLData, new() {
        private static C instance_;
        public static C Instance => instance_ ??= (Load() ?? new C());
        public static void ResetSettings() {
            instance_ = new C();
            instance_.Save();
        }

        public static C Load() {
            try {
                string configPath = GetPath();
                Log.Called($"load path: '{configPath}'");
                if (!File.Exists(configPath)) {
                    Log.Info("path does not exist");
                    return null;
                } else {
                    string data = File.ReadAllText(configPath);
                    return XMLSerializerUtil.Deserialize<C>(data);
                }
            } catch (Exception ex) {
                ex.Log();
                return null;
            }
        }

        public void Save() {
            try {
                string configPath = GetPath();
                Log.Called("path:" + configPath);
                var data = XMLSerializerUtil.Serialize(this as C);
                File.WriteAllText(configPath, data);
            } catch (Exception ex) { ex.Log(); }
        }

        private static string GetPath() => Path.Combine(DataLocation.executableDirectory, GetConfigPath());
        private static string GetConfigPath() {
            if (typeof(C).GetCustomAttributes(typeof(ConfigurationPathAttribute), true)
                .FirstOrDefault() is ConfigurationPathAttribute configPathAttribute) {
                return configPathAttribute.Value;
            } else {
                return typeof(C).Name + ".xml";
            }
        }
    }

    public interface IXMLData { void Save(); }

    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigurationPathAttribute : Attribute {
        public ConfigurationPathAttribute(string value) => Value = value;
        public string Value { get; private set; }
    }

}

