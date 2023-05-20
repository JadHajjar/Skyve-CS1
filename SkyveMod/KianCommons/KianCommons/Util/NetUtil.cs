using ColossalFramework;
using ColossalFramework.Math;
using KianCommons.Math;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
namespace KianCommons {
    internal class NetServiceException : Exception {
        public NetServiceException(string m) : base(m) { }
        public NetServiceException() : base() { }
        public NetServiceException(string m, Exception e) : base(m, e) { }
    }

    public static class NetUtil {
        public const float SAFETY_NET = 0.02f;

        public static NetManager netMan = NetManager.instance;
        public static NetTool netTool => SingletonStrong<NetTool>.Instance;
        public static SimulationManager simMan => SimulationManager.instance;
        public static TerrainManager terrainMan => TerrainManager.instance;

        public const float MPU = 8f; // meter per unit

        private static NetNode[] nodeBuffer_ = netMan.m_nodes.m_buffer;
        private static NetSegment[] segmentBuffer_ = netMan.m_segments.m_buffer;
        private static NetLane[] laneBuffer_ = netMan.m_lanes.m_buffer;
        public static ref NetNode ToNode(this ushort id) => ref nodeBuffer_[id];
        public static ref NetSegment ToSegment(this ushort id) => ref segmentBuffer_[id];
        public static ref NetLane ToLane(this uint id) => ref laneBuffer_[id];
        internal static NetLane.Flags Flags(this ref NetLane lane) => (NetLane.Flags)lane.m_flags;
        public static NetInfo.Lane GetLaneInfo(uint laneId) =>
            laneId.ToLane().m_segment.ToSegment().Info.m_lanes[GetLaneIndex(laneId)];

        /// <summary>
        /// returns lane data of the given lane ID.
        /// throws exception if unsuccessful.
        /// </summary>
        public static LaneData GetLaneData(uint laneId) {
            Assertion.Assert(laneId != 0, "laneId!=0");
            var flags = laneId.ToLane().Flags();
            bool valid = (flags & NetLane.Flags.Created | NetLane.Flags.Deleted) != NetLane.Flags.Created;
            Assertion.Assert(valid, "valid");
            ushort segmentId = laneId.ToLane().m_segment;
            foreach (var laneData in IterateSegmentLanes(segmentId))
                if (laneData.LaneID == laneId)
                    return laneData;
            throw new Exception($"Unreachable code. " +
                $"lane:{laneId} segment:{segmentId} info:{segmentId.ToSegment().Info}");
        }

        public static bool IsCSUR(this NetInfo info) {
            if (info == null ||
                (info.m_netAI.GetType() != typeof(RoadAI) &&
                info.m_netAI.GetType() != typeof(RoadBridgeAI) &&
                info.m_netAI.GetType() != typeof(RoadTunnelAI))) {
                return false;
            }
            return info.name.Contains(".CSUR ");
        }


        public static ToolBase.ToolErrors InsertNode(NetTool.ControlPoint controlPoint, out ushort nodeId, bool test = false) {
            var ret = NetTool.CreateNode(
                controlPoint.m_segment.ToSegment().Info,
                controlPoint, controlPoint, controlPoint,
                NetTool.m_nodePositionsSimulation,
                maxSegments: 0,
                test: test, visualize: false, autoFix: true, needMoney: false,
                invert: false, switchDir: false,
                relocateBuildingID: 0,
                out nodeId, out var newSegment, out var cost, out var productionRate);
            if (!test) {
                nodeId.ToNode().m_flags |= NetNode.Flags.Middle | NetNode.Flags.Moveable;
            }
            //Log.Debug($"[InsertNode] test={test} errors:{ret} nodeId:{nodeId} newSegment:{newSegment} cost:{cost} productionRate{productionRate}");
            return ret;
        }

        public static int CountPedestrianLanes(this NetInfo info) =>
            info.m_lanes.Count(lane => lane.m_laneType == NetInfo.LaneType.Pedestrian);

        static bool CheckID(this ref NetNode node1, ushort nodeId2) {
            ref NetNode node2 = ref nodeId2.ToNode();
            return node1.m_buildIndex == node2.m_buildIndex &&
                   node1.m_position == node2.m_position;
        }

        static bool CheckID(this ref NetSegment segment1, ushort segmentId2) {
            ref NetSegment segment2 = ref segmentId2.ToSegment();
            return (segment1.m_startNode == segment2.m_startNode) &
                   (segment1.m_endNode == segment2.m_endNode);
        }

        public static ushort GetID(this ref NetNode node) {
            ref NetSegment seg = ref node.GetFirstSegment().ToSegment();
            bool startNode = node.CheckID(seg.m_startNode);
            return startNode ? seg.m_startNode : seg.m_endNode;
        }

        public static ushort GetID(this ref NetSegment segment) {
            ref var node = ref segment.m_startNode.ToNode();
            for (int i = 0; i < 8; ++i) {
                ushort segmentId = node.GetSegment(i);
                if (segment.CheckID(segmentId))
                    return segmentId;
            }
            return 0;
        }

        public static ushort GetFirstSegment(ushort nodeID) => nodeID.ToNode().GetFirstSegment();
        public static ushort GetFirstSegment(this ref NetNode node) {
            ushort segmentID = 0;
            int i;
            for (i = 0; i < 8; ++i) {
                segmentID = node.GetSegment(i);
                if (segmentID != 0)
                    break;
            }
            return segmentID;
        }

        public static Vector3 GetSegmentDir(ushort segmentID, ushort nodeID) {
            bool startNode = IsStartNode(segmentID, nodeID);
            ref NetSegment segment = ref segmentID.ToSegment();
            return startNode ? segment.m_startDirection : segment.m_endDirection;
        }

        internal static float MaxNodeHW(ushort nodeId) {
            float ret = 0;
            foreach (var segmentId in IterateNodeSegments(nodeId)) {
                float hw = segmentId.ToSegment().Info.m_halfWidth;
                if (hw > ret)
                    ret = hw;
            }
            return ret;
        }

        #region Math

        /// Note: inverted flag or LHT does not influence the bezier.
        internal static Bezier3 CalculateSegmentBezier3(this ref NetSegment seg, bool bStartNode = true) {
            ref NetNode startNode = ref seg.m_startNode.ToNode();
            ref NetNode endNode = ref seg.m_endNode.ToNode();
            Bezier3 bezier = new Bezier3 {
                a = startNode.m_position,
                d = endNode.m_position,
            };
            NetSegment.CalculateMiddlePoints(
                bezier.a, seg.m_startDirection,
                bezier.d, seg.m_endDirection,
                startNode.m_flags.IsFlagSet(NetNode.Flags.Middle),
                endNode.m_flags.IsFlagSet(NetNode.Flags.Middle),
                out bezier.b,
                out bezier.c);
            if (!bStartNode)
                bezier = bezier.Invert();
            return bezier;
        }

        /// <param name="startNode"> if true the bezier is inverted so that it will be facing start node</param>
        /// Note: inverted flag or LHT does not influence the bezier.
        internal static Bezier2 CalculateSegmentBezier2(ushort segmentId, bool startNode) {
            Bezier3 bezier3 = segmentId.ToSegment().CalculateSegmentBezier3(startNode);
            Bezier2 bezier2 = bezier3.ToCSBezier2();
            return bezier2;
        }

        /// <param name="endNodeID">bezier will be facing endNodeID</param>
        internal static Bezier2 CalculateSegmentBezier2(ushort segmentId, ushort endNodeID) {
            bool startNode = !IsStartNode(segmentId, endNodeID);
            return CalculateSegmentBezier2(segmentId, startNode);
        }

        internal static float GetClosestT(this ref NetSegment segment, Vector3 position) {
            Bezier3 bezier = segment.CalculateSegmentBezier3();
            return bezier.GetClosestT(position);
        }

        /// <param name="bLeft2">if other segment is to the left side of segmentID.</param>
        /// <param name="cornerDir">is normalized</param>
        internal static void CalculateCorner(
            ushort segmentID, ushort nodeID, bool bLeft2,
            out Vector2 cornerPoint, out Vector2 cornerDir) {
            segmentID.ToSegment().CalculateCorner(
                segmentID,
                true,
                IsStartNode(segmentID, nodeID),
                !bLeft2, // leftSide = if this segment is to the left of the other segment = !bLeft2
                out Vector3 cornerPos,
                out Vector3 cornerDirection,
                out bool smooth);
            cornerPoint = cornerPos.ToCS2D();
            cornerDir = cornerDirection.ToCS2D().normalized;
        }

        /// <param name="bLeft2">if other segment is to the left side of segmentID.</param>
        internal static void CalculateOtherCorner(
            ushort segmentID, ushort nodeID, bool bLeft2,
            out Vector2 cornerPoint, out Vector2 cornerDir) {
            ushort otherSegmentID = bLeft2 ?
                segmentID.ToSegment().GetLeftSegment(nodeID) :
                segmentID.ToSegment().GetRightSegment(nodeID);
            CalculateCorner(otherSegmentID, nodeID, !bLeft2,
                            out cornerPoint, out cornerDir);
        }

        /// <param name="bLeft2">if other segment is to the left side of segmentID.</param>
        internal static void CalculateCorner(
            ushort segmentID, ushort nodeID, bool bLeft2,
            out Vector3 cornerPos, out Vector3 cornerDirection) {
            segmentID.ToSegment().CalculateCorner(
                segmentID,
                true,
                IsStartNode(segmentID, nodeID),
                !bLeft2, // leftSide = if this segment is to the left of the other segment = !bLeft2
                out cornerPos,
                out cornerDirection,
                out _);
        }

        internal static void CalculateSegEndCenter(
            ushort segmentID, ushort nodeID,
            out Vector3 pos, out Vector3 dir) {
            CalculateCorner(segmentID, nodeID, false, out Vector3 pos1, out Vector3 dir1);
            CalculateCorner(segmentID, nodeID, true, out Vector3 pos2, out Vector3 dir2);
            pos = (pos1 + pos2) * 0.5f;
            dir = (dir1 + dir2) * 0.5f;
        }

        #endregion math

        public static float SampleHeight(Vector2 point) {
            return terrainMan.SampleDetailHeightSmooth(point.ToCS3D(0));
        }

        public static Vector3 Get3DPos(Vector2 point) {
            return point.ToCS3D(SampleHeight(point));
        }

        #region copied from TMPE
        /// <summary>
        /// left hand traffic (traffic drives on left like UK).
        /// </summary>
        public static bool LHT => TrafficDrivesOnLeft;

        /// <summary>
        /// right hand traffic (traffic drives on right like most places).
        /// </summary>
        public static bool RHT => !LHT;
        public static bool TrafficDrivesOnLeft =>
            Singleton<SimulationManager>.instance?.m_metaData?.m_invertTraffic
                == SimulationMetaData.MetaBool.True;

        public static bool CanConnectPathToSegment(ushort segmentID) =>
            segmentID.ToSegment().CanConnectPath();

        public static bool CanConnectPath(this ref NetSegment segment) =>
            segment.Info.m_netAI is RoadAI & segment.Info.m_hasPedestrianLanes;

        public static bool CanConnectPath(this NetInfo info) =>
            info.m_netAI is RoadAI & info.m_hasPedestrianLanes;

        internal static bool IsInvert(this ref NetSegment segment) =>
            segment.m_flags.IsFlagSet(NetSegment.Flags.Invert);

        internal static bool IsJunction(this ref NetNode node) =>
            node.m_flags.IsFlagSet(NetNode.Flags.Junction);


        internal static NetInfo.Direction Invert(this NetInfo.Direction direction, bool invert = true) {
            if (invert)
                direction = NetInfo.InvertDirection(direction);
            return direction;
        }

        /// <summary>
        /// checks if vehicles move backward or bypass backward (considers LHT)
        /// </summary>
        /// <returns>true if vehicles move backward,
        /// false if vehicles going ward, bi-directional, or non-directional</returns>
        public static bool IsGoingBackward(this NetInfo.Direction direction) =>
            (direction & NetInfo.Direction.Both) == NetInfo.Direction.Backward ||
            (direction & NetInfo.Direction.AvoidBoth) == NetInfo.Direction.AvoidForward;

        public static bool IsGoingForward(this NetInfo.Direction direction) =>
            (direction & NetInfo.Direction.Both) == NetInfo.Direction.Forward ||
            (direction & NetInfo.Direction.AvoidBoth) == NetInfo.Direction.AvoidBackward;

        /// <summary>
        /// checks if vehicles move backward or bypass backward (considers LHT)
        /// </summary>
        /// <returns>true if vehicles move backward,
        /// false if vehicles going ward, bi-directional, or non-directional</returns>
        public static bool IsGoingBackward(this NetInfo.Lane laneInfo, bool invertDirection = false) =>
            laneInfo.m_finalDirection.Invert(invertDirection).IsGoingBackward();

        public static bool IsGoingForward(this NetInfo.Lane laneInfo, bool invertDirection = false) =>
            laneInfo.m_finalDirection.Invert(invertDirection).IsGoingForward();

        public static bool IsStartNode(ushort segmentId, ushort nodeId) =>
            segmentId.ToSegment().m_startNode == nodeId;

        public static bool IsStartNode(this ref NetSegment segment, ushort nodeId) =>
            segment.m_startNode == nodeId;

        public static ushort GetSegmentNode(ushort segmentID, bool startNode) =>
            segmentID.ToSegment().GetNode(startNode);

        public static ushort GetNode(this ref NetSegment segment, bool startNode) =>
            startNode ? segment.m_startNode : segment.m_endNode;

        public static bool HasNode(ushort segmentId, ushort nodeId) =>
            segmentId.ToSegment().m_startNode == nodeId || segmentId.ToSegment().m_endNode == nodeId;

        public static ushort GetSharedNode(ushort segmentID1, ushort segmentID2) =>
            segmentID1.ToSegment().GetSharedNode(segmentID2);

        public static bool IsSegmentValid(ushort segmentId) {
            if (segmentId == 0 || segmentId >= NetManager.MAX_SEGMENT_COUNT)
                return false;
            return segmentId.ToSegment().IsValid();
        }

        public static bool IsValid(this ref NetSegment segment) {
            if (!segment.Info)
                return false;
            return segment.m_flags
                .CheckFlags(required: NetSegment.Flags.Created, forbidden: NetSegment.Flags.Deleted);

        }

        public static bool IsValid(this InstanceID instance) {
            if (instance.IsEmpty)
                return false;
            if (instance.Type == InstanceType.NetNode)
                return IsNodeValid(instance.NetNode);
            if (instance.Type == InstanceType.NetSegment)
                return IsSegmentValid(instance.NetSegment);
            if (instance.Type == InstanceType.NetLane)
                return IsLaneValid(instance.NetLane);
            return true;
        }


        public static void AssertSegmentValid(ushort segmentId) {
            Assertion.AssertNeq(segmentId, 0, "segmentId");
            Assertion.AssertGT(NetManager.MAX_SEGMENT_COUNT, segmentId);
            Assertion.AssertNotNull(segmentId.ToSegment().Info, $"segment:{segmentId} info");
            var flags = segmentId.ToSegment().m_flags;
            var goodFlags = flags.CheckFlags(required: NetSegment.Flags.Created, forbidden: NetSegment.Flags.Deleted);
            Assertion.Assert(goodFlags,
                $"segment {segmentId} {segmentId.ToSegment().Info} has bad flags: {flags}");
        }

        public static bool IsNodeValid(ushort nodeId) {
            if (nodeId == 0 || nodeId >= NetManager.MAX_NODE_COUNT)
                return false;
            return nodeId.ToNode().IsValid();
        }

        public static bool IsValid(this ref NetNode node) {
            if (node.Info == null)
                return false;
            return node.m_flags
                .CheckFlags(required: NetNode.Flags.Created, forbidden: NetNode.Flags.Deleted);
        }

        public static bool IsLaneValid(uint laneId) {
            if (laneId != 0 && laneId < NetManager.MAX_LANE_COUNT) {
                return laneId.ToLane().Flags().
                    CheckFlags(required: NetLane.Flags.Created, forbidden: NetLane.Flags.Deleted);
            }
            return false;
        }

        public static ushort GetHeadNode(this ref NetSegment segment) {
            // tail node>-------->head node
            bool invert = (segment.m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None;
            invert = invert ^ LHT;
            if (invert) {
                return segment.m_startNode;
            } else {
                return segment.m_endNode;
            }
        }

        public static ushort GetHeadNode(ushort segmentId) =>
            GetHeadNode(ref segmentId.ToSegment());

        public static ushort GetTailNode(this ref NetSegment segment) {
            bool invert = (segment.m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None;
            invert = invert ^ LHT;
            if (!invert) {
                return segment.m_startNode;
            } else {
                return segment.m_endNode;
            }//endif
        }

        public static ushort GetTailNode(ushort segmentId) =>
            GetTailNode(ref segmentId.ToSegment());

        public static bool CalculateIsOneWay(ushort segmentId) {
            int forward = 0;
            int backward = 0;
            segmentId.ToSegment().CountLanes(
                segmentId,
                NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle,
                VehicleInfo.VehicleType.Car | VehicleInfo.VehicleType.Train |
                VehicleInfo.VehicleType.Tram | VehicleInfo.VehicleType.Metro |
                VehicleInfo.VehicleType.Monorail,
				VehicleInfo.VehicleCategory.All,
				ref forward,
                ref backward);
            return (forward == 0) ^ (backward == 0);
        }

        #endregion

        public struct NodeSegments {
            public ushort[] segments;
            public int count;
            void Add(ushort segmentID) {
                segments[count++] = segmentID;
            }

            public NodeSegments(ushort nodeID) {
                segments = new ushort[8];
                count = 0;

                ushort segmentID = GetFirstSegment(nodeID);
                Add(segmentID);
                while (true) {
                    segmentID = segmentID.ToSegment().GetLeftSegment(nodeID);
                    if (segmentID == segments[0])
                        break;
                    else
                        Add(segmentID);
                }
            }
        }

        /// <summary>
        /// returns a counter-clockwise list of segments of the given node ID.
        /// </summary>
        public static IEnumerable<ushort> GetCCSegList(ushort nodeID) {
            ushort segmentID0 = GetFirstSegment(nodeID);
            Assertion.Assert(segmentID0 != 0, "GetFirstSegment!=0");
            yield return segmentID0;
            ushort segmentID = segmentID0;

            // add the rest of the segments.
            while (true) {
                segmentID = segmentID.ToSegment().GetRightSegment(nodeID);
                if ((segmentID == 0) | (segmentID == segmentID0))
                    yield break;
                else
                    yield return segmentID;
            }
        }

        /// <summary>
        /// returns a clock-wise list of segments of the given node ID.
        /// </summary>
        public static IEnumerable<ushort> GetCWSegList(ushort nodeID) {
            ushort segmentID0 = GetFirstSegment(nodeID);
            Assertion.Assert(segmentID0 != 0, "GetFirstSegment!=0");
            yield return segmentID0;
            ushort segmentID = segmentID0;

            // add the rest of the segments.
            while (true) {
                segmentID = segmentID.ToSegment().GetLeftSegment(nodeID);
                if ((segmentID == 0) | (segmentID == segmentID0))
                    yield break;
                else
                    yield return segmentID;
            }
        }

        public static IEnumerable<ushort> IterateNodeSegments(ushort nodeID) {
            for (int i = 0; i < 8; ++i) {
                ushort segmentID = nodeID.ToNode().GetSegment(i);
                if (segmentID != 0) {
                    yield return segmentID;
                }
            }
        }

        public static NodeSegmentIterator IterateSegments(this ref NetNode node)
            => new NodeSegmentIterator(node.GetID());

        public static ushort GetAnotherSegment(this ref NetNode node, ushort segmentId0) {
            for(int i = 0; i < 8; ++i) {
                ushort segmentId = node.GetSegment(i);
                if (segmentId != segmentId0 && segmentId != 0)
                    return segmentId;
            }
            return 0;
        }

        [Obsolete("use IterateNodeSegments instead")]
        internal static IEnumerable<ushort> GetSegmentsCoroutine(ushort nodeID)
            => IterateNodeSegments(nodeID);

        public static void LaneTest(ushort segmentId) {
            string message = $"STRANGE LANE ISSUE: lane count mismatch for " +
                $"segment:{segmentId} Info:{segmentId.ToSegment().Info} IsSegmentValid={IsSegmentValid(segmentId)}\n";
            if (segmentId.ToSegment().Info != null) {

                var laneIDs = new List<uint>();
                for (uint laneID = segmentId.ToSegment().m_lanes;
                    laneID != 0;
                    laneID = laneID.ToLane().m_nextLane) {
                    laneIDs.Add(laneID);
                }

                var laneInfos = segmentId.ToSegment().Info.m_lanes;

                if (laneIDs.Count == laneInfos.Length) return;

                string m1 = "laneIDs=\n";
                foreach (uint laneID in laneIDs)
                    m1 += $"\tlaneID:{laneID} flags:{laneID.ToLane().m_flags} segment:{laneID.ToLane().m_segment} bezier.a={laneID.ToLane().m_bezier.a}\n";

                string m2 = "laneInfoss=\n";
                for (int i = 0; i < laneInfos.Length; ++i) {
                    m2 += $"\tlaneID:{laneInfos[i]} dir:{laneInfos[i].m_direction} laneType:{laneInfos[i].m_laneType} vehicleType:{laneInfos[i].m_vehicleType} pos:{laneInfos[i].m_position}\n";
                }

                message += m1 + m2;
            }
            Log.Error(message, true);
            Log.LogToFileSimple(file: "NodeControler.Strange.log", message: message);
        }

        public static IEnumerable<uint> IterateNodeLanes(ushort nodeId) {
            int idx = 0;
            if (nodeId.ToNode().Info == null) {
                Log.Error("null info: potentially caused by missing assets");
                yield break;
            }
            for (uint laneID = nodeId.ToNode().m_lane;
                laneID != 0;
                laneID = laneID.ToLane().m_nextLane, idx++) {
                yield return laneID;
            }
        }

        // requires testing.
        //public static bool IsLaneHeadingTowardsStartNode(uint laneID, int laneIndex) {
        //    ushort segmentID = laneID.ToLane().m_segment;
        //    var laneInfo = segmentID.ToSegment().Info.m_lanes[laneIndex];
        //    bool backward = laneInfo.m_finalDirection == NetInfo.Direction.Backward;
        //    bool inverted = segmentID.ToSegment().m_flags.IsFlagSet(NetSegment.Flags.Invert);
        //    return backward ^ inverted;
        //}


        public static LaneDataIterator IterateSegmentLanes(ushort segmentId) =>
            new LaneDataIterator(segmentId);

        public static LaneDataIterator IterateLanes(
         ushort segmentId,
         bool? startNode = null,
         NetInfo.LaneType ?laneType = null,
         VehicleInfo.VehicleType ?vehicleType = null) =>
            new LaneDataIterator(segmentId, startNode, laneType, vehicleType);

        public static NetInfo.Lane SortedLane(this NetInfo info, int index) {
            int sortedIndex = info.m_sortedLanes[index];
            return info.m_lanes[sortedIndex];
        }

        public static IEnumerable<NetInfo.Lane> SortedLanes(this NetInfo info) {
            for (int i = 0; i < info.m_sortedLanes.Length; ++i) {
                int sortedIndex = info.m_sortedLanes[i];
                yield return info.m_lanes[sortedIndex];
            }
        }

        /// <summary>
        /// sorted from outer lane to inner lane when heading toward <paramref name="startNode"/>
        /// </summary>
        public static LaneData[] GetSortedLanes(
            ushort segmentId,
            bool? startNode = null,
            NetInfo.LaneType? laneType = null,
            VehicleInfo.VehicleType? vehicleType = null) {
            var lanes = new LaneDataIterator(
                segmentID: segmentId,
                startNode: startNode,
                laneType: laneType,
                vehicleType: vehicleType);
            return lanes.OrderBy(lane => lane.LaneInfo.m_position).ToArray();
        }

        public static int GetLaneIndex(uint laneID) {
            ushort segmentId = laneID.ToLane().m_segment;
            var laneID2 = segmentId.ToSegment().m_lanes;
            int nLanes = segmentId.ToSegment().Info.m_lanes.Length;
            for (int laneIndex2 = 0;
                laneIndex2< nLanes && laneID2 != 0;
                laneIndex2++) {
                if (laneID2 == laneID)
                    return laneIndex2;
                laneID2 = laneID2.ToLane().m_nextLane;
            }
            return -1;
        }
        public static uint GetlaneID(ushort segmentID, int laneIndex) {
            uint laneID = segmentID.ToSegment().m_lanes;
            int n = segmentID.ToSegment().Info.m_lanes.Length;
            for (int i = 0; i < n && laneID != 0; i++) {
                if (i == laneIndex)
                    return laneID;
                laneID = laneID.ToLane().m_nextLane;
            }
            return 0;
        }

        public static string PrintSegmentLanes(ushort segmentID) {
            ref var segment = ref segmentID.ToSegment();
            List<string> ret = new List<string>();
            ret.Add($"ushort segment:{segmentID} info:{segment.Info}");

            var lanes = segment.Info.m_lanes;
            int laneIndex = 0;
            for (uint laneID = segment.m_lanes; laneID != 0; laneID = laneID.ToLane().m_nextLane) {
                if (laneIndex < lanes.Length) {
                    var laneInfo = lanes[laneIndex];
                    ret.Add($"lane[{laneIndex}]:{laneID} {laneInfo.m_laneType}:{laneInfo.m_vehicleType}");
                } else {
                    ret.Add($"WARNING: laneID:{laneID} laneIndex:{laneIndex} exceeds laneCount:{lanes.Length} lane.segment:{laneID.ToLane().m_segment}");
                }
                laneIndex++;
            }

            return ret.Join("\n");
        }
    }

    [Serializable]
    public struct LaneData {
        public uint LaneID;
        public int LaneIndex;
        /// <summary>true if the lane is going toward the start node of its segment (undefined for bidirectional lanes)</summary>
        public bool StartNode;


        [NonSerialized] private NetInfo.Lane laneInfo_;
        public NetInfo.Lane LaneInfo {
            get => laneInfo_ ??= Segment.Info.m_lanes[LaneIndex];
            set => laneInfo_ = value;
        }

        public LaneData(uint laneID, int laneIndex = -1) {
            LaneID = laneID;
            if (laneIndex < 0)
                laneIndex = NetUtil.GetLaneIndex(laneID);
            LaneIndex = laneIndex;

            ushort segmentID = LaneID.ToLane().m_segment;
            try {
                laneInfo_ = segmentID.ToSegment().Info.m_lanes[LaneIndex];
            } catch (IndexOutOfRangeException ex) {
                ex.Log($"LaneIndex:{LaneIndex} laneID={laneID} segmentID={segmentID}.\n" +
                    $"Use network detective mod to debug the segment.", showInPannel: false);
                Log.Error(NetUtil.PrintSegmentLanes(segmentID));
                throw ex;
            }
            bool backward = laneInfo_.IsGoingBackward();
            bool invert = segmentID.ToSegment().IsInvert();

            // simple case: forward, not-invert : false != false -> false
            StartNode = backward != invert; //xnor
        }

        public readonly ushort SegmentID => Lane.m_segment;
        public readonly ref NetSegment Segment => ref SegmentID.ToSegment();
        public readonly ref NetLane Lane => ref LaneID.ToLane();
        public readonly ushort NodeID => StartNode ? Segment.m_startNode : Segment.m_endNode;
        public readonly NetLane.Flags Flags {
            get => (NetLane.Flags)Lane.m_flags;
            set => LaneID.ToLane().m_flags = (ushort)value;
        }

        public bool LeftSide => LaneInfo.m_position < 0 != Segment.m_flags.IsFlagSet(NetSegment.Flags.Invert);
        public bool RightSide => !LeftSide;

        public readonly Bezier3 Bezier => Lane.m_bezier;
        public override string ToString() {
            try {
                return $"LaneData:[segment:{SegmentID} segmentInfo:{Segment.Info} node:{NodeID} laneID:{LaneID} Index={LaneIndex} {LaneInfo?.m_laneType} { LaneInfo?.m_vehicleType}]";
            }
            catch (NullReferenceException) {
                return $"LaneData:[segment:{SegmentID} segmentInfo:{Segment.Info} node:{NodeID} lane ID:{LaneID} null";
            }
        }
    }

    public struct LaneIDIterator : IEnumerable<uint>, IEnumerator<uint> {
        ushort segmentID_;

        uint laneID_;
        int laneIndex_;
        int laneCount_;


        public LaneIDIterator(ushort segmentID) {
            segmentID_ = segmentID;
            laneID_ = 0;
            laneIndex_ = 0;
            laneCount_ = segmentID.ToSegment().Info.m_lanes.Length;
        }

        public void Reset() {
            laneID_ = 0;
            laneIndex_ = 0;
        }

        public void Dispose() { }

        public uint Current => laneID_;
        public int CurrentLaneIndex => laneIndex_;

        public bool MoveNext() {
            if (laneID_ == 0) {
                if (laneIndex_ > 0)
                    return false; 
                laneID_ = segmentID_.ToSegment().m_lanes;
            } else {
                laneID_ = laneID_.ToLane().m_nextLane;
            }
            return laneID_ != 0 && laneIndex_++ < laneCount_;
        }

        public LaneIDIterator GetEnumerator() => this; 
        IEnumerator<uint> IEnumerable<uint>.GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
        object IEnumerator.Current => Current;
    }

    public struct LaneDataIterator : IEnumerable<LaneData>, IEnumerator<LaneData> {
        ushort segmentID_;
        bool? startNode_;
        NetInfo.LaneType? laneType_;
        VehicleInfo.VehicleType? vehicleType_;
        int nLanes_;

        LaneData current_;

        public LaneDataIterator(
            ushort segmentID,
            bool? startNode = null,
            NetInfo.LaneType? laneType = null,
            VehicleInfo.VehicleType? vehicleType = null) { 
            segmentID_ = segmentID;
            startNode_ = startNode;
            laneType_ = laneType;
            vehicleType_ = vehicleType;
            nLanes_ = segmentID.ToSegment().Info.m_lanes.Length;

            current_ = default;
        }

        public bool MoveNext() {
            uint nextLaneId;
            int nextLaneIndex;
            if (current_.LaneID != 0) {
                nextLaneId = current_.Lane.m_nextLane;
                nextLaneIndex = current_.LaneIndex+1;
            } else {
                nextLaneId = segmentID_.ToSegment().m_lanes;
                nextLaneIndex = 0;
            }
            if (nextLaneId == 0) return false;
            if (nextLaneIndex >= nLanes_) {
                if (Log.VERBOSE) {
                    Log.Warning($"lane count mismatch! segment:{segmentID_} laneID:{nextLaneId} laneIndex:{nextLaneIndex}", false);
                    Log.Warning(NetUtil.PrintSegmentLanes(segmentID_), false);
                }
                return false; 
            }
            if (nextLaneId.ToLane().m_segment != segmentID_) {
                if (Log.VERBOSE) {
                    Log.Warning($"lane has different segment:{nextLaneId.ToLane().m_segment}! segment:{segmentID_} laneID:{nextLaneId} laneIndex:{nextLaneIndex}", false);
                    Log.Warning(NetUtil.PrintSegmentLanes(segmentID_), false);
                }
                return false;
            }
            try {
                current_ = new LaneData(nextLaneId, nextLaneIndex);
            } catch (Exception ex) {
                ex.Log($"bad lane! segment:{segmentID_} laneID:{nextLaneId} laneIndex:{nextLaneIndex}", false);
            }

            if (startNode_.HasValue && startNode_.Value != current_.StartNode)
                return MoveNext(); //continue
            if (laneType_.HasValue && !current_.LaneInfo.m_laneType.IsFlagSet(laneType_.Value))
                return MoveNext(); //continue
            if (vehicleType_.HasValue && !current_.LaneInfo.m_vehicleType.IsFlagSet(vehicleType_.Value))
                return MoveNext(); //continue

            return true;
        }

        public int Count {
            get {
                int ret = 0;
                try {
                    while (MoveNext())
                        ret++;
                } finally {
                    Reset();
                }
                return ret;
            }
        }

        public LaneData Current => current_;
        public void Reset() => current_ = default;
        public LaneDataIterator GetEnumerator() => this;
        public void Dispose() { }
        IEnumerator<LaneData> IEnumerable<LaneData>.GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
        object IEnumerator.Current => Current;
    }

    public struct NodeSegmentIterator : IEnumerable<ushort>, IEnumerator<ushort> {
        int i_;
        ushort segmentId_;
        ushort nodeId_;

        public NodeSegmentIterator(ushort nodeId) {
            nodeId_ = nodeId;
            segmentId_ = 0;
            i_ = 0;
        }

        public bool MoveNext() {
            while(i_ < 8) {
                segmentId_ = nodeId_.ToNode().GetSegment(i_++);
                if(segmentId_ != 0)
                    return true;
            }
            segmentId_ = 0;
            return false;
        }

        public ushort Current => segmentId_;
        public NodeSegmentIterator GetEnumerator() => this;
        public void Reset() => i_ = segmentId_ = 0;
        public void Dispose() => Reset();
        object IEnumerator.Current => Current;
        IEnumerator<ushort> IEnumerable<ushort>.GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
    }
}

