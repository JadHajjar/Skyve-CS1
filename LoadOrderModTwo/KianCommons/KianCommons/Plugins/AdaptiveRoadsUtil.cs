namespace KianCommons.Plugins {
    using System;
    using System.Reflection;
    using static ColossalFramework.Plugins.PluginManager;
    using static KianCommons.Plugins.PluginUtil;
    using ColossalFramework.Plugins;



    internal static class AdaptiveRoadsUtil {
        static AdaptiveRoadsUtil() {
            Init();
            PluginManager.instance.eventPluginsStateChanged += Init;
            PluginManager.instance.eventPluginsChanged += Init;
            LoadingManager.instance.m_levelPreLoaded += Init;
        }

        static void Init() {
            Log.Info("AdaptiveRoadsUtil.Init() called");
            Log.Debug(Environment.StackTrace);
            Plugin = GetAdaptiveRoads();
            IsActive = Plugin.IsActive();

            if (IsActive) {
                asm = Plugin.GetMainAssembly();
                API = asm.GetType("AdaptiveRoads.API", throwOnError: true, ignoreCase: true);
                var version = Plugin.userModInstance.VersionOf() ?? new Version(0, 0);
                Log.Info("AR Version=" + version);
                nodeVehicleTypes_ = CreateDelegate<NodeVehicleTypes>();
                nodeLaneTypes_ = CreateDelegate<NodeLaneTypes>();
                hideBrokenMedians_ = CreateDelegate<HideBrokenMedians>();
                getSharpCorners_ = CreateDelegate<GetSharpCorners>();
                isAdaptive_ = CreateDelegate<IsAdaptive>();
            } else {
                Log.Info("AR not found.");
                asm = null;
                API = null;
            }
        }

        public static PluginInfo Plugin { get; private set; }

        public static bool IsActive { get; private set; }

        public static Assembly asm { get; private set; }
        public static Type API { get; private set; }
        static MethodInfo GetMethod(string name) {
            var ret = API.GetMethod(name);
            if( ret == null) {
                Log.Warning($"AdaptiveRoadsUtil: method {name} not found!");
            }
            return ret;
        }
        static object Invoke(string methodName, params object[] args) =>
            GetMethod(methodName)?.Invoke(null, args);

        static TDelegate CreateDelegate<TDelegate>() where TDelegate : Delegate =>
            DelegateUtil.CreateDelegate<TDelegate>(API);

#pragma warning disable HAA0601, HAA0101
        #region flags
        public static object GetARSegmentFlags(ushort id) {
            if (!IsActive) return null;
            return Invoke("GetARSegmentFlags", id);
        }
        public static object GetARNodeFlags(ushort id) {
            if (!IsActive) return null;
            return Invoke("GetARNodeFlags", id);
        }
        public static object GetARSegmentEndFlags(ushort segmentID, ushort nodeID) {
            if (!IsActive) return null;
            return Invoke("GetARSegmentEndFlags", segmentID, nodeID);
        }
        public static object GetARSegmentEndFlags(ushort segmentID, bool startNode) {
            if (!IsActive) return null;
            ushort nodeID = segmentID.ToSegment().GetNode(startNode);
            return Invoke("GetARSegmentEndFlags", segmentID, nodeID);
        }
        public static object GetARLaneFlags(uint laneId) {
            if (!IsActive) return null;
            return Invoke("GetARLaneFlags", laneId);
        }
        #endregion

        public static void OverrideARSharpner(bool value = true) {
            if (IsActive) 
                Invoke("OverrideSharpner", value);
        }
#pragma warning restore HAA0101, HAA0601

        delegate bool IsAdaptive(NetInfo info);
        static IsAdaptive isAdaptive_;
        public static bool GetIsAdaptive(this NetInfo info) {
            if (isAdaptive_ == null) return false;
            return isAdaptive_(info);
        }

        delegate VehicleInfo.VehicleType NodeVehicleTypes(NetInfo.Node node);
        static NodeVehicleTypes nodeVehicleTypes_;
        public static VehicleInfo.VehicleType ARVehicleTypes(this NetInfo.Node node) {
            if (nodeVehicleTypes_ == null)
                return 0;
            return nodeVehicleTypes_(node);
        }

        delegate NetInfo.LaneType NodeLaneTypes(NetInfo.Node node);
        static NodeLaneTypes nodeLaneTypes_;
        public static NetInfo.LaneType LaneTypes(this NetInfo.Node node) {
            if (nodeLaneTypes_ == null)
                return 0;
            return nodeLaneTypes_(node);
        }

        delegate bool HideBrokenMedians(NetInfo.Node node);
        static HideBrokenMedians hideBrokenMedians_;
        public static bool HideBrokenARMedians(this NetInfo.Node node) {
            if (hideBrokenMedians_ == null)
                return true;
            return hideBrokenMedians_(node);
        }

        delegate bool GetSharpCorners(NetInfo info);
        static GetSharpCorners getSharpCorners_;
        public static bool GetARSharpCorners(this NetInfo info) {
            if (getSharpCorners_ == null)
                return false;
            return getSharpCorners_(info);
        }
    }
}