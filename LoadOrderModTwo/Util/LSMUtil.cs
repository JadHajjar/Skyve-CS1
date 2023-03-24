namespace LoadOrderMod.Util {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using static ColossalFramework.Plugins.PluginManager;


    public static class LSMUtil {
        public const string LSM = "LoadingScreenModRevisited";
        public const ulong WSID = 2858591409;
        internal static bool IsLSM(this PluginInfo p) =>
            p?.name is string name  && (name == WSID.ToString() || name == LSM);

        internal static Assembly GetLSMAssembly() =>
            AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(_asm => _asm.GetName().Name == LSM);


        /// <param name="type">full type name minus assembly name and root name space</param>
        /// <returns>corresponding types from LSM or LSMTest or both</returns>
        public static IEnumerable<Type> GetTypeFromLSMS(string type) {
            if(GetLSMAssembly() is Assembly lsm) {
                var ret = lsm.GetType($"{LSM}.{type}", false);
                if (ret != null)
                    yield return ret;
            }
        }
    }
}
