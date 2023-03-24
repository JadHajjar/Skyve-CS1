namespace LoadOrderIPatch {
    using LoadOrderShared;
    using System;
    using System.Linq;


    public static class ConfigUtil {
        internal static LoadOrderConfig config_;
        public static LoadOrderConfig Config =>
            config_ ??=
                LoadOrderConfig.Deserialize()
                ?? new LoadOrderConfig();
        
        public static bool HasArg(string arg) =>
            Environment.GetCommandLineArgs().Any(_arg => _arg == arg);
    }
}
