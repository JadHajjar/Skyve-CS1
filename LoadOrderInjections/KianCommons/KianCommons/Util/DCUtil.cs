namespace KianCommons {
    using KianCommons.Plugins;

    internal static class DCUtil {
        //public const VehicleInfo.VehicleType TRACK_VEHICLE_TYPES =
        //    VehicleInfo.VehicleType.Tram |
        //    VehicleInfo.VehicleType.Metro |
        //    VehicleInfo.VehicleType.Train |
        //    VehicleInfo.VehicleType.Monorail;

        /// <summary>
        /// returns true if asset has vehicle type
        /// returns false if asset does not have vehicle type or the given vehicle type is 0.
        /// </summary>
        public static bool HasLane(ushort segmentID, VehicleInfo.VehicleType vehicleType) =>
             (segmentID.ToSegment().Info.m_vehicleTypes & vehicleType) != 0;

        public static bool IsMedian(NetInfo.Node nodeInfo, NetInfo netInfo) {
            VehicleInfo.VehicleType vehicleType = GetVehicleType(nodeInfo);
            return !netInfo.m_vehicleTypes.IsFlagSet(vehicleType); // vehicleType == 0 => median
        }

        public static VehicleInfo.VehicleType GetVehicleType(this NetInfo.Node nodeInfo) {
            VehicleInfo.VehicleType vehicleType = AdaptiveRoadsUtil.ARVehicleTypes(nodeInfo);
            if(vehicleType == 0)
                vehicleType = GetVehicleType(nodeInfo.m_connectGroup);
            return vehicleType;
        }

        public static bool IsTrack(NetInfo.Node nodeInfo, NetInfo info) {
            VehicleInfo.VehicleType vehicleType = GetVehicleType(nodeInfo);
            return info.m_vehicleTypes.IsFlagSet(vehicleType); // vehicleType == 0 is checked here.
        }

        // TODO add wide narrow center single for tram and trolley
        public const NetInfo.ConnectGroup DOUBLE =
            NetInfo.ConnectGroup.DoubleMetro | NetInfo.ConnectGroup.DoubleMonorail | NetInfo.ConnectGroup.DoubleTrain |
            NetInfo.ConnectGroup.NarrowTram | NetInfo.ConnectGroup.WideTram |
            NetInfo.ConnectGroup.NarrowTrolleybus | NetInfo.ConnectGroup.WideTrolleybus;

        public const NetInfo.ConnectGroup SINGLE =
            NetInfo.ConnectGroup.SingleMetro | NetInfo.ConnectGroup.SingleMonorail | NetInfo.ConnectGroup.SingleTrain |
            NetInfo.ConnectGroup.CenterTram | NetInfo.ConnectGroup.SingleTram |
            NetInfo.ConnectGroup.CenterTrolleybus | NetInfo.ConnectGroup.SingleTrolleybus;

        public const NetInfo.ConnectGroup STATION =
            NetInfo.ConnectGroup.MetroStation | NetInfo.ConnectGroup.MonorailStation | NetInfo.ConnectGroup.TrainStation;

        public const NetInfo.ConnectGroup TRAM =
            NetInfo.ConnectGroup.CenterTram |
            NetInfo.ConnectGroup.SingleTram |
            NetInfo.ConnectGroup.NarrowTram |
            NetInfo.ConnectGroup.WideTram;
        public const NetInfo.ConnectGroup TRAIN =
            NetInfo.ConnectGroup.DoubleTrain |
            NetInfo.ConnectGroup.SingleTrain |
            NetInfo.ConnectGroup.TrainStation;
        public const NetInfo.ConnectGroup MONORAIL =
            NetInfo.ConnectGroup.DoubleMonorail |
            NetInfo.ConnectGroup.SingleMonorail |
            NetInfo.ConnectGroup.MonorailStation;
        public const NetInfo.ConnectGroup METRO =
            NetInfo.ConnectGroup.DoubleMetro |
            NetInfo.ConnectGroup.SingleMetro |
            NetInfo.ConnectGroup.MetroStation;
        public const NetInfo.ConnectGroup TROLLY =
            NetInfo.ConnectGroup.CenterTrolleybus |
            NetInfo.ConnectGroup.SingleTrolleybus |
            NetInfo.ConnectGroup.NarrowTrolleybus |
            NetInfo.ConnectGroup.WideTrolleybus;


        internal static VehicleInfo.VehicleType GetVehicleType(NetInfo.ConnectGroup flags) {
            VehicleInfo.VehicleType ret = 0;

            if ((flags & TRAM) != 0) {
                ret |= VehicleInfo.VehicleType.Tram;
            }
            if ((flags & METRO) != 0) {
                ret |= VehicleInfo.VehicleType.Metro;
            }
            if ((flags & TRAIN) != 0) {
                ret |= VehicleInfo.VehicleType.Train;
            }
            if ((flags & MONORAIL) != 0) {
                ret |= VehicleInfo.VehicleType.Monorail;
            }
            if ((flags & TROLLY) != 0) {
                ret |= VehicleInfo.VehicleType.Trolleybus;
            }
            return ret;
        }
    }
}
