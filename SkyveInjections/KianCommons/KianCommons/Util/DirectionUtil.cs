namespace KianCommons {
    internal static class DirectionUtil {
        internal static bool LHT => NetUtil.LHT;
        internal static bool RHT => NetUtil.RHT;

        internal static NetLane.Flags ArrowNear => RHT ? NetLane.Flags.Right : NetLane.Flags.Left;
        internal static NetLane.Flags ArrowFar => RHT ? NetLane.Flags.Left : NetLane.Flags.Right;
        internal static NetLane.Flags ArrowNearForward => ArrowNear | NetLane.Flags.Forward;
        internal static NetLane.Flags ArrowFarForward => ArrowFar | NetLane.Flags.Forward;

        internal static ushort GetNearSegment(this ref NetSegment segment, ushort nodeId) =>
            RHT ? segment.GetRightSegment(nodeId) : segment.GetLeftSegment(nodeId);

        internal static ushort GetFarSegment(this ref NetSegment segment, ushort nodeId) =>
            LHT ? segment.GetRightSegment(nodeId) : segment.GetLeftSegment(nodeId);
    }
}
