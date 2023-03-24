using ColossalFramework;
using ColossalFramework.Math;
using KianCommons;
using System;
using UnityEngine;
using KianCommons;

namespace KianCommons.StockCode {
    class NetAI2: NetAI {
        // NetAI
        public NetInfo m_info;

        public virtual void UpdateLanes(ushort segmentID, ref NetSegment segment, bool loading) {
            bool flag = Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic == SimulationMetaData.MetaBool.True;
            uint prevLaneID = 0u;
            uint laneID = segment.m_lanes;
            segment.CalculateCorner(segmentID, heightOffset: true, start: true, true,
                out var cornerPosStartLeft, out var cornerDirStartLeft, out _);
            segment.CalculateCorner(segmentID, heightOffset: true, start: false, leftSide: true,
                out var cornerPosEndLeft, out var cornerDirEndLeft, out bool smoothStart);
            segment.CalculateCorner(segmentID, heightOffset: true, start: true, leftSide: false,
                out var cornerPosStartRight, out var cornerDirStartRight, out _);
            segment.CalculateCorner(segmentID, heightOffset: true, start: false, leftSide: false,
                out var cornerPosEndRight, out var cornerDirEndRight, out bool smoothEnd);

            bool segmentInverted = segment.m_flags.IsFlagSet(NetSegment.Flags.Invert);

            float cc = 128 / Mathf.PI;//40.7436638f
            Vector3 deltaPosStart = cornerPosStartRight - cornerPosStartLeft;
            Vector3 deltaPosEnd = cornerPosEndRight - cornerPosEndLeft;
            if (segmentInverted) {
                segment.m_cornerAngleStart = (byte)(Mathf.RoundToInt(Mathf.Atan2(deltaPosStart.z, deltaPosStart.x) * cc) & 255);
                segment.m_cornerAngleEnd = (byte)(Mathf.RoundToInt(Mathf.Atan2(-deltaPosEnd.z, -deltaPosEnd.x) * cc) & 255);
            } else {
                segment.m_cornerAngleStart = (byte)(Mathf.RoundToInt(Mathf.Atan2(-deltaPosStart.z, -deltaPosStart.x) * cc) & 255);
                segment.m_cornerAngleEnd = (byte)(Mathf.RoundToInt(Mathf.Atan2(deltaPosEnd.z, deltaPosEnd.x) * cc) & 255);
            }
            NetLane.Flags flags = NetLane.Flags.None;
            if (segment.m_flags.IsFlagSet(NetSegment.Flags.YieldStart)) {
                flags |= segmentInverted ? NetLane.Flags.YieldStart : NetLane.Flags.YieldEnd;
            }
            if (segment.m_flags.IsFlagSet(NetSegment.Flags.YieldEnd)) {
                flags |= segmentInverted ? NetLane.Flags.YieldEnd : NetLane.Flags.YieldStart;
            }
            float lengthAcc = 0f;
            float laneCount = 0f;
            for (int i = 0; i < this.m_info.m_lanes.Length; i++) {
                if (laneID == 0u) {
                    if (!Singleton<NetManager>.instance.CreateLanes(
                        out laneID, ref Singleton<SimulationManager>.instance.m_randomizer, segmentID, 1)) {
                        break;
                    }
                    if (prevLaneID != 0u) {
                        prevLaneID.ToLane().m_nextLane = laneID;
                    } else {
                        segment.m_lanes = laneID;
                    }
                }
                NetInfo.Lane laneInfo = this.m_info.m_lanes[i];
                float lanePos01 = laneInfo.m_position / (this.m_info.m_halfWidth * 2f) + 0.5f; // lane pos rescaled between 0~1
                if (segmentInverted) {
                    lanePos01 = 1f - lanePos01;
                }
                Vector3 startPos = cornerPosStartLeft + (cornerPosStartRight - cornerPosStartLeft) * lanePos01;
                Vector3 startDir = Vector3.Lerp(cornerDirStartLeft, cornerDirStartRight, lanePos01);
                Vector3 endPos = cornerPosEndRight + (cornerPosEndLeft - cornerPosEndRight) * lanePos01;
                Vector3 endDir = Vector3.Lerp(cornerDirEndRight, cornerDirEndLeft, lanePos01);
                startPos.y += laneInfo.m_verticalOffset;
                endPos.y += laneInfo.m_verticalOffset;
                Vector3 b;
                Vector3 c;
                NetSegment.CalculateMiddlePoints(startPos, startDir, endPos, endDir, smoothStart, smoothEnd, out b, out c);
                NetLane.Flags flags2 = laneID.ToLane().Flags();
                NetLane.Flags flags3 = flags;
                flags2 &= ~(NetLane.Flags.YieldStart | NetLane.Flags.YieldEnd);
                if ((byte)(laneInfo.m_finalDirection & NetInfo.Direction.Both) == 2) {
                    flags3 &= ~NetLane.Flags.YieldEnd;
                }
                if ((byte)(laneInfo.m_finalDirection & NetInfo.Direction.Both) == 1) {
                    flags3 &= ~NetLane.Flags.YieldStart;
                }
                flags2 |= flags3;
                if (flag) {
                    flags2 |= NetLane.Flags.Inverted;
                } else {
                    flags2 &= ~NetLane.Flags.Inverted;
                }
                laneID.ToLane().m_bezier = new Bezier3(startPos, b, c, endPos);
                laneID.ToLane().m_segment = segmentID;
                laneID.ToLane().m_flags = (ushort)flags2;
                laneID.ToLane().m_firstTarget = 0;
                laneID.ToLane().m_lastTarget = byte.MaxValue;
                lengthAcc += laneID.ToLane().UpdateLength();
                laneCount += 1f;
                prevLaneID = laneID;
                laneID = laneID.ToLane().m_nextLane;
            }
            if (laneCount != 0f) {
                segment.m_averageLength = lengthAcc / laneCount;
            } else {
                segment.m_averageLength = 0f;
            }
        }

    }

    public class RoadBaseAI2 : RoadBaseAI {
        // RoadBaseAI
        public override void UpdateLanes(ushort segmentID, ref NetSegment segment, bool loading) {
            bool LHT = Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic == SimulationMetaData.MetaBool.True;
            segment.CalculateCorner(segmentID, heightOffset: true, start: true, true,
                out var cornerPosStartLeft, out var cornerDirStartLeft, out _);
            segment.CalculateCorner(segmentID, heightOffset: true, start: false, leftSide: true,
                out var cornerPosEndLeft, out var cornerDirEndLeft, out bool smoothStart);
            segment.CalculateCorner(segmentID, heightOffset: true, start: true, leftSide: false,
                out var cornerPosStartRight, out var cornerDirStartRight, out _);
            segment.CalculateCorner(segmentID, heightOffset: true, start: false, leftSide: false,
                out var cornerPosEndRight, out var cornerDirEndRight, out bool smoothEnd);

            if (segment.m_flags.IsFlagSet(NetSegment.Flags.Invert)) {
                segment.m_cornerAngleStart = (byte)(Mathf.RoundToInt(Mathf.Atan2(cornerPosStartRight.z - cornerPosStartLeft.z, cornerPosStartRight.x - cornerPosStartLeft.x) * 40.7436638f) & 255);
                segment.m_cornerAngleEnd = (byte)(Mathf.RoundToInt(Mathf.Atan2(cornerPosEndLeft.z - cornerPosEndRight.z, cornerPosEndLeft.x - cornerPosEndRight.x) * 40.7436638f) & 255);
            } else {
                segment.m_cornerAngleStart = (byte)(Mathf.RoundToInt(Mathf.Atan2(cornerPosStartLeft.z - cornerPosStartRight.z, cornerPosStartLeft.x - cornerPosStartRight.x) * 40.7436638f) & 255);
                segment.m_cornerAngleEnd = (byte)(Mathf.RoundToInt(Mathf.Atan2(cornerPosEndRight.z - cornerPosEndLeft.z, cornerPosEndRight.x - cornerPosEndLeft.x) * 40.7436638f) & 255);
            }
            int endLeft = 0;
            int endForward = 0;
            int endRight = 0;
            int endLeft2 = 0;
            int endForward2 = 0;
            int endRight2 = 0;
            bool flag2 = false;
            bool flag3 = false;
            segment.m_endNode.ToNode().CountLanes(segment.m_endNode, segmentID,
                NetInfo.Direction.Forward, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, VehicleInfo.VehicleType.Car,
                -segment.m_endDirection, ref endLeft, ref endForward, ref endRight, ref endLeft2, ref endForward2, ref endRight2);
            if (segment.m_endNode.ToNode().m_flags.IsFlagSet(
                NetNode.Flags.End | NetNode.Flags.Middle | NetNode.Flags.Bend | NetNode.Flags.Outside)) {
                if (endLeft + endForward + endRight == 0) {
                    flag3 = true;
                } else {
                    flag2 = true;
                }
            }
            int startLeft = 0;
            int startForward = 0;
            int startRight = 0;
            int startLeft2 = 0;
            int startForward2 = 0;
            int startRight2 = 0;
            bool hasStartLanes = false;
            bool noStartLanes = false;
            segment.m_startNode.ToNode().CountLanes(
                segment.m_startNode, segmentID,
                NetInfo.Direction.Forward, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, VehicleInfo.VehicleType.Car,
                -segment.m_startDirection, ref startLeft, ref startForward, ref startRight, ref startLeft2, ref startForward2, ref startRight2);
            if (segment.m_startNode.ToNode().m_flags.IsFlagSet(NetNode.Flags.End | NetNode.Flags.Middle | NetNode.Flags.Bend | NetNode.Flags.Outside)) {
                if (startLeft + startForward + startRight == 0) {
                    noStartLanes = true;
                } else {
                    hasStartLanes = true;
                }
            }
            NetLane.Flags flags = NetLane.Flags.None;
            if (endLeft2 != 0 && endLeft == 0) {
                flags |= segment.m_flags.IsFlagSet(NetSegment.Flags.Invert) ? NetLane.Flags.EndOneWayLeft : NetLane.Flags.StartOneWayLeft;
            }
            if (endRight2 != 0 && endRight == 0) {
                flags |= segment.m_flags.IsFlagSet(NetSegment.Flags.Invert) ? NetLane.Flags.EndOneWayRight : NetLane.Flags.StartOneWayRight;
            }
            if (startLeft2 != 0 && startLeft == 0) {
                flags |= segment.m_flags.IsFlagSet(NetSegment.Flags.Invert)? NetLane.Flags.StartOneWayLeft : NetLane.Flags.EndOneWayLeft;
            }
            if (startRight2 != 0 && startRight == 0) {
                flags |= segment.m_flags.IsFlagSet(NetSegment.Flags.Invert)  ? NetLane.Flags.StartOneWayRight : NetLane.Flags.EndOneWayRight;
            }
            if ((segment.m_flags.IsFlagSet(NetSegment.Flags.YieldStart))) {
                flags |= segment.m_flags.IsFlagSet(NetSegment.Flags.Invert)  ? NetLane.Flags.YieldStart : NetLane.Flags.YieldEnd;
            }
            if ((segment.m_flags.IsFlagSet(NetSegment.Flags.YieldEnd))) {
                flags |= segment.m_flags.IsFlagSet(NetSegment.Flags.Invert)? NetLane.Flags.YieldEnd : NetLane.Flags.YieldStart;
            }
            float lengthAcc = 0f;
            float lengthCount = 0f;
            uint prevLaneID = 0u;
            uint laneID = segment.m_lanes;
            for (int i = 0; i < this.m_info.m_lanes.Length; i++) {
                if (laneID == 0u) {
                    if (!NetManager.instance.CreateLanes(out laneID, ref Singleton<SimulationManager>.instance.m_randomizer, segmentID, 1)) {
                        break;
                    }
                    if (prevLaneID != 0u) {
                        NetManager.instance.m_lanes.m_buffer[(int)((UIntPtr)prevLaneID)].m_nextLane = laneID;
                    } else {
                        segment.m_lanes = laneID;
                    }
                }
                NetInfo.Lane lane = this.m_info.m_lanes[i];
                float lanePos01 = lane.m_position / (this.m_info.m_halfWidth * 2f) + 0.5f;
                if ((segment.m_flags.IsFlagSet(NetSegment.Flags.Invert))) {
                    lanePos01 = 1f - lanePos01;
                }
                Vector3 startPos = cornerPosStartLeft + (cornerPosStartRight - cornerPosStartLeft) * lanePos01;
                Vector3 startDir = Vector3.Lerp(cornerDirStartLeft, cornerDirStartRight, lanePos01);
                Vector3 endPos = cornerPosEndRight + (cornerPosEndLeft - cornerPosEndRight) * lanePos01;
                Vector3 endDir = Vector3.Lerp(cornerDirEndRight, cornerDirEndLeft, lanePos01);
                startPos.y += lane.m_verticalOffset;
                endPos.y += lane.m_verticalOffset;
                NetSegment.CalculateMiddlePoints(startPos, startDir, endPos, endDir, smoothStart, smoothEnd, out var b, out var c);
                NetLane.Flags flags2 = laneID.ToLane().Flags();
                NetLane.Flags flags3 = flags;
                flags2 &= ~(NetLane.Flags.Forward | NetLane.Flags.Left | NetLane.Flags.Right | NetLane.Flags.Merge | NetLane.Flags.YieldStart | NetLane.Flags.YieldEnd | NetLane.Flags.StartOneWayLeft | NetLane.Flags.StartOneWayRight | NetLane.Flags.EndOneWayLeft | NetLane.Flags.EndOneWayRight);
                if ((byte)(lane.m_finalDirection & NetInfo.Direction.Both) == 2) {
                    flags3 &= ~NetLane.Flags.YieldEnd;
                }
                if ((byte)(lane.m_finalDirection & NetInfo.Direction.Both) == 1) {
                    flags3 &= ~NetLane.Flags.YieldStart;
                }
                if ((lane.m_vehicleType & VehicleInfo.VehicleType.Monorail) != VehicleInfo.VehicleType.None) {
                    flags3 &= ~(NetLane.Flags.YieldStart | NetLane.Flags.YieldEnd);
                }
                flags2 |= flags3;
                if (LHT) {
                    flags2 |= NetLane.Flags.Inverted;
                } else {
                    flags2 &= ~NetLane.Flags.Inverted;
                }
                int tailInnerPortion = 0;
                int tailOuterPortion = 255;
                if ((byte)(lane.m_laneType & (NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle)) != 0) {
                    bool backward = lane.m_finalDirection.IsFlagSet(NetInfo.Direction.Forward) == segment.m_flags.IsFlagSet(NetSegment.Flags.Invert);
                    int tailLeft;
                    int tailForward;
                    int tailRight;
                    if (backward) {
                        tailLeft = endLeft;
                        tailForward = endForward;
                        tailRight = endRight;
                    } else {
                        tailLeft = startLeft;
                        tailForward = startForward;
                        tailRight = startRight;
                    }
                    int innterSimilarLaneIndex;
                    int outerSimilarLaneIndex;
                    if ((byte)(lane.m_finalDirection & NetInfo.Direction.Forward) != 0) {
                        innterSimilarLaneIndex = lane.m_similarLaneIndex;
                        outerSimilarLaneIndex = lane.m_similarLaneCount - lane.m_similarLaneIndex - 1;
                    } else {
                        innterSimilarLaneIndex = lane.m_similarLaneCount - lane.m_similarLaneIndex - 1;
                        outerSimilarLaneIndex = lane.m_similarLaneIndex;
                    }
                    int totalTail = tailLeft + tailForward + tailRight;
                    tailInnerPortion = 255;
                    tailOuterPortion = 0;
                    if (totalTail != 0) {
                        if (lane.m_similarLaneCount > totalTail && totalTail > 0) {
                            tailInnerPortion = totalTail * innterSimilarLaneIndex / lane.m_similarLaneCount;
                            tailOuterPortion = totalTail - totalTail * outerSimilarLaneIndex / lane.m_similarLaneCount;
                            flags2 |= NetLane.Flags.Merge;
                            if (tailInnerPortion < tailLeft) {
                                flags2 |= NetLane.Flags.Left;
                            }
                            if (totalTail - tailOuterPortion < tailRight) {
                                flags2 |= NetLane.Flags.Right;
                            }
                            if (tailForward != 0 && tailInnerPortion < tailLeft + tailForward && tailOuterPortion > tailLeft) {
                                flags2 |= NetLane.Flags.Forward;
                            }
                        } else {
                            int num26;
                            int num27;
                            if (lane.m_similarLaneCount >= totalTail) {
                                num26 = tailLeft;
                                num27 = tailRight;
                            } else {
                                num26 = tailLeft * lane.m_similarLaneCount / (totalTail + (tailForward >> 1));
                                num27 = tailRight * lane.m_similarLaneCount / (totalTail + (tailForward >> 1));
                            }
                            int num28 = num26;
                            int num29 = lane.m_similarLaneCount - num26 - num27;
                            int num30 = num27;
                            if (num29 > 0) {
                                if (tailLeft > num26) {
                                    num28++;
                                }
                                if (tailRight > num27) {
                                    num30++;
                                }
                            }
                            if (innterSimilarLaneIndex < num28) {
                                int num31 = (innterSimilarLaneIndex * tailLeft + num28 - 1) / num28;
                                int num32 = ((innterSimilarLaneIndex + 1) * tailLeft + num28 - 1) / num28;
                                if (num32 > num31) {
                                    flags2 |= NetLane.Flags.Left;
                                    tailInnerPortion = Mathf.Min(tailInnerPortion, num31);
                                    tailOuterPortion = Mathf.Max(tailOuterPortion, num32);
                                }
                            }
                            if (innterSimilarLaneIndex >= num26 && outerSimilarLaneIndex >= num27 && tailForward != 0) {
                                if (lane.m_similarLaneCount > totalTail) {
                                    num26++;
                                }
                                int num33 = tailLeft + ((innterSimilarLaneIndex - num26) * tailForward + num29 - 1) / num29;
                                int num34 = tailLeft + ((innterSimilarLaneIndex + 1 - num26) * tailForward + num29 - 1) / num29;
                                if (num34 > num33) {
                                    flags2 |= NetLane.Flags.Forward;
                                    tailInnerPortion = Mathf.Min(tailInnerPortion, num33);
                                    tailOuterPortion = Mathf.Max(tailOuterPortion, num34);
                                }
                            }
                            if (outerSimilarLaneIndex < num30) {
                                int num35 = totalTail - ((outerSimilarLaneIndex + 1) * tailRight + num30 - 1) / num30;
                                int num36 = totalTail - (outerSimilarLaneIndex * tailRight + num30 - 1) / num30;
                                if (num36 > num35) {
                                    flags2 |= NetLane.Flags.Right;
                                    tailInnerPortion = Mathf.Min(tailInnerPortion, num35);
                                    tailOuterPortion = Mathf.Max(tailOuterPortion, num36);
                                }
                            }
                            if (this.m_highwayRules) {
                                if ((flags2 & NetLane.Flags.LeftRight) == NetLane.Flags.Left) {
                                    if ((flags2 & NetLane.Flags.Forward) == NetLane.Flags.None || (tailForward >= 2 && tailLeft == 1)) {
                                        tailOuterPortion = Mathf.Min(tailOuterPortion, tailInnerPortion + 1);
                                    }
                                } else if ((flags2 & NetLane.Flags.LeftRight) == NetLane.Flags.Right && ((flags2 & NetLane.Flags.Forward) == NetLane.Flags.None || (tailForward >= 2 && tailRight == 1))) {
                                    tailInnerPortion = Mathf.Max(tailInnerPortion, tailOuterPortion - 1);
                                }
                            }
                        }
                    }
                    if (backward) {
                        if (flag2) {
                            flags2 &= ~(NetLane.Flags.Forward | NetLane.Flags.Left | NetLane.Flags.Right);
                        } else if (flag3) {
                            flags2 |= NetLane.Flags.Forward;
                        }
                    } else if (hasStartLanes) {
                        flags2 &= ~(NetLane.Flags.Forward | NetLane.Flags.Left | NetLane.Flags.Right);
                    } else if (noStartLanes) {
                        flags2 |= NetLane.Flags.Forward;
                    }
                }
                laneID.ToLane().m_bezier = new Bezier3(startPos, b, c, endPos);
                laneID.ToLane().m_segment = segmentID;
                laneID.ToLane().m_flags = (ushort)flags2;
                laneID.ToLane().m_firstTarget = (byte)tailInnerPortion;
                laneID.ToLane().m_lastTarget = (byte)tailOuterPortion;
                lengthAcc += laneID.ToLane().UpdateLength();
                lengthCount += 1f;
                prevLaneID = laneID;
                laneID = laneID.ToLane().m_nextLane;
            }
            if (lengthCount != 0f) {
                segment.m_averageLength = lengthAcc / lengthCount;
            } else {
                segment.m_averageLength = 0f;
            }
            bool joinedJunction = segment.m_averageLength < 11f &&
                segment.m_startNode.ToNode().m_flags.IsFlagSet(NetNode.Flags.Junction) &&
                segment.m_endNode.ToNode().m_flags.IsFlagSet(NetNode.Flags.Junction);

            laneID = segment.m_lanes;
            int laneIndex = 0;
            while (laneIndex < m_info.m_lanes.Length && laneID != 0u) {
                NetLane.Flags flags4 = laneID.ToLane().Flags() & ~NetLane.Flags.JoinedJunction;
                if (joinedJunction) {
                    flags4 |= NetLane.Flags.JoinedJunction;
                }
                laneID.ToLane().m_flags = (ushort)flags4;
                laneID = laneID.ToLane().m_nextLane;
                laneIndex++;
            }
            if (!loading) {
                int xBountStart = Mathf.Max((int)((segment.m_bounds.min.x - 16f) / 64f + 135f), 0);
                int zBountStart = Mathf.Max((int)((segment.m_bounds.min.z - 16f) / 64f + 135f), 0);
                int xBountEnd = Mathf.Min((int)((segment.m_bounds.max.x + 16f) / 64f + 135f), 269);
                int zBoundEnd = Mathf.Min((int)((segment.m_bounds.max.z + 16f) / 64f + 135f), 269);
                for (int zBound = zBountStart; zBound <= zBoundEnd; zBound++) {
                    for (int xBound = xBountStart; xBound <= xBountEnd; xBound++) {
                        ushort gridIndex = NetManager.instance.m_nodeGrid[zBound * 270 + xBound];
                        int watchDog = 0;
                        while (gridIndex != 0) {
                            NetInfo info = NetManager.instance.m_nodes.m_buffer[(int)gridIndex].Info;
                            Vector3 position = NetManager.instance.m_nodes.m_buffer[(int)gridIndex].m_position;
                            float num44 = Mathf.Max(Mathf.Max(segment.m_bounds.min.x - 16f - position.x, segment.m_bounds.min.z - 16f - position.z), Mathf.Max(position.x - segment.m_bounds.max.x - 16f, position.z - segment.m_bounds.max.z - 16f));
                            if (num44 < 0f) {
                                info.m_netAI.NearbyLanesUpdated(gridIndex, ref NetManager.instance.m_nodes.m_buffer[(int)gridIndex]);
                            }
                            gridIndex = NetManager.instance.m_nodes.m_buffer[(int)gridIndex].m_nextGridNode;
                            if (++watchDog >= 32768) {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                break;
                            }
                        }
                    }
                }
                if (this.m_info.m_hasPedestrianLanes && (this.m_info.m_hasForwardVehicleLanes || this.m_info.m_hasBackwardVehicleLanes)) {
                    RoadBaseAI.CheckBuildings(segmentID, ref segment);
                }
            }
        }

        public virtual float GetNodeBuildingAngle(ushort nodeID, ref NetNode data) {
            NetManager instance = Singleton<NetManager>.instance;
            float[] angleBuffer = instance.m_angleBuffer;
            int angleCount = 0;
            for (int segmentIndex = 0; segmentIndex < 8; segmentIndex++) {
                ushort segmentId = data.GetSegment(segmentIndex);
                if (segmentId != 0) {
                    var dir = segmentId.ToSegment().GetDirection(nodeID);
                    float angle = Mathf.Atan2(dir.x, -dir.z) * 0.159154937f + 1.25f;
                    angleBuffer[angleCount++] = angle - Mathf.Floor(angle);
                    angle += 0.5f;
                    angleBuffer[angleCount++] = angle - Mathf.Floor(angle);
                }
            }
            float result = 0f;
            if (angleCount != 0) {
                if (angleCount == 2) {
                    result = angleBuffer[0] + 0.75f;
                } else {
                    Array.Sort(angleBuffer, 0, angleCount);
                    var angle1 = angleBuffer[angleCount - 1];
                    var angle2 = angleBuffer[0];
                    result = (angle1 + angle2) * 0.5f;
                    float maxAngleDiff = angle2 - angle1 + 1; // why +1 ?
                    for (int angleIndex = 1; angleIndex < angleCount; angleIndex++) {
                        angle1 = angleBuffer[angleIndex - 1];
                        angle2 = angleBuffer[angleIndex];
                        float angleDiff = angle2 - angle1;
                        if (angleDiff > maxAngleDiff) {
                            maxAngleDiff = angleDiff;
                            result = (angle1 + angle2) * 0.5f;
                        }
                    }
                }
            }
            return result;
        }


    }
}
