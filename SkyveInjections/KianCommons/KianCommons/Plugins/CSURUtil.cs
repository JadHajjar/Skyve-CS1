using System;
using System.Runtime.CompilerServices;

namespace KianCommons.Plugins {
    public static class CSURUtil {
        public const string HARMONY_ID = "csur.toolbox";
        public static float GetMinCornerOffset(ushort nodeID) {
            NetInfo info = nodeID.ToNode().Info;
            float ret = info.m_minCornerOffset;
            if (PluginUtil.CSUREnabled) {
                ret = GetMinCornerOffset_(ret,nodeID); 
            }
            return ret;
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        private static float GetMinCornerOffset_(float cornerOffset0, ushort nodeID) {
            return CSURToolBox.Util.CSURUtil.GetMinCornerOffset(cornerOffset0,nodeID);
        }
    }

}
