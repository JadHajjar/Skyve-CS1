namespace SkyveIPatch {
    using SkyveShared;
    using System;
    using System.Linq;


    public static class ConfigUtil {
        internal static SkyveConfig config_;
        public static SkyveConfig Config =>
            config_ ??=
                SkyveConfig.Deserialize()
                ?? new SkyveConfig();
        
        public static bool HasArg(string arg) =>
            Environment.GetCommandLineArgs().Any(_arg => _arg == arg);
    }
}
