namespace KianCommons.StockCode {
    using ColossalFramework;
    using ColossalFramework.Math;
    using KianCommons;
    using UnityEngine;

    public class NetTool2 : NetTool {

        public static ToolErrors CreateNode(NetInfo info, ControlPoint startPoint, ControlPoint middlePoint, ControlPoint endPoint, FastList<NodePosition> nodeBuffer, int maxSegments, bool test, bool testEnds, bool visualize, bool autoFix, bool needMoney, bool invert, bool switchDir, ushort relocateBuildingID, out ushort firstNode, out ushort lastNode, out ushort segmentID2, out int cost, out int productionRate) {
            ushort segmentID = middlePoint.m_segment;
            NetInfo oldInfo = null;
            if (startPoint.m_segment == segmentID || endPoint.m_segment == segmentID) {
                segmentID = 0;
            }
            uint buildIndex = Singleton<SimulationManager>.instance.m_currentBuildIndex;
            bool reverse = invert;
            bool smoothStart = true;
            bool smoothEnd = true;
            bool enableDouble = false;
            NetSegment.Flags flags = NetSegment.Flags.None;
            ushort nameSeed = 0;
            string text = null;
            bool adjusted = false;
            bool autoFix0 = autoFix;
            if (segmentID != 0) {
                enableDouble = DefaultTool.FindSecondarySegment(segmentID) != 0;
                maxSegments = Mathf.Min(1, maxSegments);
                cost = -segmentID.ToSegment().Info.m_netAI.GetConstructionCost(startPoint.m_position, endPoint.m_position, startPoint.m_elevation, endPoint.m_elevation);
                if ((segmentID.ToSegment().m_flags & NetSegment.Flags.Collapsed) != 0) {
                    cost /= 2;
                }
                buildIndex = segmentID.ToSegment().m_buildIndex;
                smoothStart = (Singleton<NetManager>.instance.m_nodes.m_buffer[startPoint.m_node].m_flags & NetNode.Flags.Middle) != 0;
                smoothEnd = (Singleton<NetManager>.instance.m_nodes.m_buffer[endPoint.m_node].m_flags & NetNode.Flags.Middle) != 0;
                autoFix = false;
                if ((segmentID.ToSegment().m_flags & NetSegment.Flags.Collapsed) != 0) {
                    info = segmentID.ToSegment().Info;
                    switchDir = false;
                } else if (switchDir) {
                    reverse = !reverse;
                    info = segmentID.ToSegment().Info;
                }
                if (!test && !visualize) {
                    if ((segmentID.ToSegment().m_flags & NetSegment.Flags.Invert) != 0) {
                        reverse = !reverse;
                    }
                    flags = segmentID.ToSegment().m_flags & (NetSegment.Flags.YieldEnd | NetSegment.Flags.YieldStart);
                    if ((segmentID.ToSegment().m_flags & NetSegment.Flags.CustomName) != 0) {
                        InstanceID empty = InstanceID.Empty;
                        empty.NetSegment = segmentID;
                        text = Singleton<InstanceManager>.instance.GetName(empty);
                    }
                    if ((Singleton<NetManager>.instance.m_adjustedSegments[segmentID >> 6] & (ulong)(1L << (int)segmentID)) != 0) {
                        adjusted = true;
                    }
                    nameSeed = segmentID.ToSegment().m_nameSeed;
                    if ((segmentID.ToSegment().m_flags & NetSegment.Flags.Collapsed) != 0) {
                        Singleton<NetManager>.instance.m_roadDestroyed.Deactivate();
                        Singleton<NetManager>.instance.m_roadDestroyed2.Deactivate();
                    }
                    oldInfo = segmentID.ToSegment().Info;
                    Singleton<NetManager>.instance.ReleaseSegment(segmentID, keepNodes: true);
                    segmentID = 0;
                }
            } else {
                if (autoFix && NetUtil.LHT) {
                    reverse = !reverse;
                }
                cost = 0;
            }
            ToolController properties = Singleton<ToolManager>.instance.m_properties;
            ulong[] collidingSegments = null;
            ulong[] collidingBuildings = null;
            ToolErrors errors = ToolErrors.None;
            if (test || !visualize) {
                properties.BeginColliding(out collidingSegments, out collidingBuildings);
            }
            try {
                ushort buildingID = 0;
                BuildingInfo ownerBuilding;
                Vector3 ownerPosition;
                Vector3 ownerDirection;
                if (segmentID != 0 && switchDir) {
                    ownerBuilding = null;
                    ownerPosition = Vector3.zero;
                    ownerDirection = Vector3.forward;
                    productionRate = 0;
                    if (info.m_forwardVehicleLaneCount == info.m_backwardVehicleLaneCount) {
                        errors |= ToolErrors.CannotUpgrade;
                    }
                } else {
                    errors |= info.m_netAI.CheckBuildPosition(test, visualize, overlay: false, autoFix, ref startPoint, ref middlePoint, ref endPoint, out ownerBuilding, out ownerPosition, out ownerDirection, out productionRate);
                }
                if (test) {
                    Vector3 direction = middlePoint.m_direction;
                    Vector3 direction2 = -endPoint.m_direction;
                    if (maxSegments != 0 && segmentID == 0 && direction.x * direction2.x + direction.z * direction2.z >= 0.8f) {
                        errors |= ToolErrors.InvalidShape;
                    }
                    if (maxSegments != 0 && !CheckStartAndEnd(segmentID, startPoint.m_segment, startPoint.m_node, endPoint.m_segment, endPoint.m_node, collidingSegments)) {
                        errors |= ToolErrors.ObjectCollision;
                    }
                    if (startPoint.m_node != 0) {
                        if (maxSegments != 0 && !CanAddSegment(startPoint.m_node, direction, collidingSegments, segmentID)) {
                            errors |= ToolErrors.ObjectCollision;
                        }
                    } else if (startPoint.m_segment != 0 && !CanAddNode(startPoint.m_segment, startPoint.m_position, direction, maxSegments != 0, collidingSegments)) {
                        errors |= ToolErrors.ObjectCollision;
                    }
                    if (endPoint.m_node != 0) {
                        if (maxSegments != 0 && !CanAddSegment(endPoint.m_node, direction2, collidingSegments, segmentID)) {
                            errors |= ToolErrors.ObjectCollision;
                        }
                    } else if (endPoint.m_segment != 0 && !CanAddNode(endPoint.m_segment, endPoint.m_position, direction2, maxSegments != 0, collidingSegments)) {
                        errors |= ToolErrors.ObjectCollision;
                    }
                    if (!Singleton<NetManager>.instance.CheckLimits()) {
                        errors |= ToolErrors.TooManyObjects;
                    }
                }
                if ((object)ownerBuilding != null) {
                    if (visualize) {
                        RenderNodeBuilding(ownerBuilding, ownerPosition, ownerDirection);
                    } else if (test) {
                        errors |= TestNodeBuilding(ownerBuilding, ownerPosition, ownerDirection, 0, 0, 0, test, collidingSegments, collidingBuildings);
                    } else {
                        float angle = Mathf.Atan2(0f - ownerDirection.x, ownerDirection.z);
                        if (Singleton<BuildingManager>.instance.CreateBuilding(out buildingID, ref Singleton<SimulationManager>.instance.m_randomizer, ownerBuilding, ownerPosition, angle, 0, Singleton<SimulationManager>.instance.m_currentBuildIndex)) {
                            Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].m_flags |= Building.Flags.FixedHeight;
                            Singleton<SimulationManager>.instance.m_currentBuildIndex++;
                        }
                    }
                }
                bool curved = middlePoint.m_direction.x * endPoint.m_direction.x + middlePoint.m_direction.z * endPoint.m_direction.z <= 0.999f;
                float lenStart = new Vector2(startPoint.m_position.x - middlePoint.m_position.x, startPoint.m_position.z - middlePoint.m_position.z).magnitude;
                float lenEnd = new Vector2(middlePoint.m_position.x - endPoint.m_position.x, middlePoint.m_position.z - endPoint.m_position.z).magnitude;
                float length = lenStart + lenEnd;
                if (test && maxSegments != 0) {
                    float minSegmentLength = info.m_netAI.GetMinSegmentLength();
                    if (curved && segmentID == 0) {
                        if (lenStart < minSegmentLength) {
                            errors |= ToolErrors.TooShort;
                        }
                        if (lenEnd < minSegmentLength) {
                            errors |= ToolErrors.TooShort;
                        }
                    } else if (length < minSegmentLength) {
                        errors |= ToolErrors.TooShort;
                    }
                }
                segmentID2 = 0;
                int segmentCount = Mathf.Min(maxSegments, Mathf.FloorToInt(length / 100f) + 1);
                if (segmentCount >= 2) {
                    enableDouble = true;
                }
                ushort nodeID = startPoint.m_node;
                Vector3 startPos = startPoint.m_position;
                Vector3 middleDir = middlePoint.m_direction;
                NetSegment.CalculateMiddlePoints(startPoint.m_position, middlePoint.m_direction, endPoint.m_position, -endPoint.m_direction, smoothStart, smoothEnd, out var bPos, out var cPos);
                nodeBuffer.Clear();
                NodePosition nodePositionItem = default(NodePosition);
                nodePositionItem.m_position = startPos;
                nodePositionItem.m_direction = middleDir;
                nodePositionItem.m_minY = startPos.y;
                nodePositionItem.m_maxY = startPos.y;
                nodePositionItem.m_terrainHeight = Singleton<TerrainManager>.instance.SampleRawHeightSmooth(nodePositionItem.m_position);
                if (!info.m_blockWater) {
                    float blockHieght = Singleton<TerrainManager>.instance.SampleBlockHeightSmooth(nodePositionItem.m_position);
                    if (blockHieght < nodePositionItem.m_terrainHeight - 15f) {
                        nodePositionItem.m_terrainHeight = blockHieght;
                    }
                }
                nodePositionItem.m_elevation = startPoint.m_elevation;
                nodePositionItem.m_double = false;
                if (startPoint.m_node != 0) {
                    nodePositionItem.m_double = (Singleton<NetManager>.instance.m_nodes.m_buffer[startPoint.m_node].m_flags & NetNode.Flags.Double) != 0;
                } else if (info.m_netAI.RequireDoubleSegments() && maxSegments <= 1) {
                    nodePositionItem.m_double = true;
                }
                nodePositionItem.m_nodeInfo = null;
                nodeBuffer.Add(nodePositionItem);
                for (int iSegment = 1; iSegment <= segmentCount; iSegment++) {
                    nodePositionItem.m_elevation = Mathf.Lerp(startPoint.m_elevation, endPoint.m_elevation, (float)iSegment / (float)segmentCount);
                    if (iSegment != segmentCount && nodePositionItem.m_elevation > 0.1f) {
                        if (startPoint.m_elevation > -0.1f && startPoint.m_elevation < 0.1f) {
                            int deltaH = Mathf.FloorToInt((endPoint.m_elevation - startPoint.m_elevation) / 8f);
                            float iElevation = Mathf.Clamp(segmentCount - deltaH, 0, segmentCount - 1);
                            float t = Mathf.Clamp01((iSegment - iElevation) / (segmentCount - iElevation));
                            nodePositionItem.m_elevation = Mathf.Lerp(startPoint.m_elevation, endPoint.m_elevation, t);
                        } else if (endPoint.m_elevation > -0.1f && endPoint.m_elevation < 0.1f) {
                            int deltaH = Mathf.FloorToInt((startPoint.m_elevation - endPoint.m_elevation) / 8f);
                            float iElevation = Mathf.Clamp(deltaH, 1, segmentCount);
                            float t = Mathf.Clamp01(iSegment / iElevation);
                            nodePositionItem.m_elevation = Mathf.Lerp(startPoint.m_elevation, endPoint.m_elevation, t);
                        }
                    } else if (iSegment != segmentCount && nodePositionItem.m_elevation < -0.1f) {
                        if (startPoint.m_elevation > -0.1f && startPoint.m_elevation < 0.1f) {
                            int deltaH = Mathf.FloorToInt((startPoint.m_elevation - endPoint.m_elevation) / 8f);
                            float iElevation = Mathf.Clamp(segmentCount - deltaH, 0, segmentCount - 1);
                            float t = Mathf.Clamp01((iSegment - iElevation) / (segmentCount - iElevation));
                            nodePositionItem.m_elevation = Mathf.Lerp(startPoint.m_elevation, endPoint.m_elevation, t);
                        } else if (endPoint.m_elevation > -0.1f && endPoint.m_elevation < 0.1f) {
                            int deltaH = Mathf.FloorToInt((endPoint.m_elevation - startPoint.m_elevation) / 8f);
                            float iElevation = Mathf.Clamp(deltaH, 1, segmentCount);
                            float t = Mathf.Clamp01(iSegment / iElevation);
                            nodePositionItem.m_elevation = Mathf.Lerp(startPoint.m_elevation, endPoint.m_elevation, t);
                        }
                    }
                    nodePositionItem.m_double = false;
                    if (iSegment == segmentCount) {
                        nodePositionItem.m_position = endPoint.m_position;
                        nodePositionItem.m_direction = endPoint.m_direction;
                        nodePositionItem.m_minY = endPoint.m_position.y;
                        nodePositionItem.m_maxY = endPoint.m_position.y;
                        if (endPoint.m_node != 0) {
                            nodePositionItem.m_double = endPoint.m_node.ToNode().m_flags.IsFlagSet(NetNode.Flags.Double);
                        }
                    } else if (curved) {
                        nodePositionItem.m_position = Bezier3.Position(startPoint.m_position, bPos, cPos, endPoint.m_position, (float)iSegment / (float)segmentCount);
                        nodePositionItem.m_direction = Bezier3.Tangent(startPoint.m_position, bPos, cPos, endPoint.m_position, (float)iSegment / (float)segmentCount);
                        nodePositionItem.m_position.y = NetSegment.SampleTerrainHeight(info, nodePositionItem.m_position, visualize, nodePositionItem.m_elevation);
                        nodePositionItem.m_direction = VectorUtils.NormalizeXZ(nodePositionItem.m_direction);
                        nodePositionItem.m_minY = 0f;
                        nodePositionItem.m_maxY = 1280f;
                    } else {
                        nodePositionItem.m_position = LerpPosition(snap: info.m_netAI.GetLengthSnap(), refPos1: startPoint.m_position, refPos2: endPoint.m_position, t: (float)iSegment / (float)segmentCount);
                        nodePositionItem.m_direction = endPoint.m_direction;
                        nodePositionItem.m_position.y = NetSegment.SampleTerrainHeight(info, nodePositionItem.m_position, visualize, nodePositionItem.m_elevation);
                        nodePositionItem.m_minY = 0f;
                        nodePositionItem.m_maxY = 1280f;
                    }
                    nodePositionItem.m_terrainHeight = Singleton<TerrainManager>.instance.SampleRawHeightSmooth(nodePositionItem.m_position);
                    if (!info.m_blockWater) {
                        float iBlockHeight = Singleton<TerrainManager>.instance.SampleBlockHeightSmooth(nodePositionItem.m_position);
                        if (iBlockHeight < nodePositionItem.m_terrainHeight - 15f) {
                            nodePositionItem.m_terrainHeight = iBlockHeight;
                        }
                    }
                    nodeBuffer.Add(nodePositionItem);
                }
                ToolErrors toolErrors = CheckNodeHeights(info, nodeBuffer);
                if (toolErrors != ToolErrors.None && test) {
                    errors |= toolErrors;
                }
                float nodeHeight0 = nodeBuffer.m_buffer[0].m_position.y - nodeBuffer.m_buffer[0].m_terrainHeight;
                if (nodeHeight0 > 0f && ((segmentCount >= 1 && nodeBuffer.m_buffer[1].m_position.y - nodeBuffer.m_buffer[1].m_terrainHeight < -8f) || (nodeHeight0 < 11f && nodeBuffer.m_buffer[0].m_elevation < 1f))) {
                    nodeHeight0 = 0f;
                    nodeBuffer.m_buffer[0].m_terrainHeight = nodeBuffer.m_buffer[0].m_position.y;
                }
                if (autoFix0 || !info.m_useFixedHeight) {
                    nodeBuffer.m_buffer[0].m_nodeInfo = info.m_netAI.GetInfo(nodeHeight0, nodeHeight0, length, startPoint.m_outside, outgoing: false, curved, enableDouble, ref errors);
                } else {
                    nodeBuffer.m_buffer[0].m_nodeInfo = info;
                }
                int iSegment2;
                for (iSegment2 = 1; iSegment2 <= segmentCount; iSegment2++) {
                    nodeHeight0 = nodeBuffer.m_buffer[iSegment2].m_position.y - nodeBuffer.m_buffer[iSegment2].m_terrainHeight;
                    if (nodeHeight0 > 0f && (nodeBuffer.m_buffer[iSegment2 - 1].m_position.y - nodeBuffer.m_buffer[iSegment2 - 1].m_terrainHeight < -8f || (segmentCount > iSegment2 && nodeBuffer.m_buffer[iSegment2 + 1].m_position.y - nodeBuffer.m_buffer[iSegment2 + 1].m_terrainHeight < -8f) || (nodeHeight0 < 11f && nodeBuffer.m_buffer[iSegment2].m_elevation < 1f))) {
                        nodeHeight0 = 0f;
                        nodeBuffer.m_buffer[iSegment2].m_terrainHeight = nodeBuffer.m_buffer[iSegment2].m_position.y;
                    }
                    if (autoFix0 || !info.m_useFixedHeight) {
                        nodeBuffer.m_buffer[iSegment2].m_nodeInfo = info.m_netAI.GetInfo(nodeHeight0, nodeHeight0, length, incoming: false, iSegment2 == segmentCount && endPoint.m_outside, curved, enableDouble, ref errors);
                    } else {
                        nodeBuffer.m_buffer[iSegment2].m_nodeInfo = info;
                    }
                }
                iSegment2 = 1;
                int segmentCounter = 0;
                NetInfo prevNodeInfo = null;
                while (iSegment2 <= segmentCount) {
                    NetInfo nodeInfo = nodeBuffer.m_buffer[iSegment2].m_nodeInfo;
                    if (iSegment2 != segmentCount && (object)nodeInfo == prevNodeInfo) {
                        segmentCounter++;
                        iSegment2++;
                    } else {
                        if (segmentCounter != 0 && prevNodeInfo.m_netAI.RequireDoubleSegments()) {
                            int index0 = iSegment2 - segmentCounter - 1;
                            int index1 = iSegment2;
                            if ((segmentCounter & 1) == 0) {
                                nodeBuffer.RemoveAt(iSegment2 - 1);
                                segmentCount--;
                                index1--;
                                for (int i = index0 + 1; i < index1; i++) {
                                    int index2 = index0;
                                    int index3 = index1;
                                    if (prevNodeInfo.m_netAI.FlattenOnlyOneDoubleSegment()) {
                                        index2 = i - ((i - index0) & 1);
                                        index3 = index2 + 2;
                                    }
                                    float t = (float)(i - index2) / (float)(index3 - index2);
                                    nodeBuffer.m_buffer[i].m_position = Vector3.Lerp(nodeBuffer.m_buffer[index2].m_position, nodeBuffer.m_buffer[index3].m_position, t);
                                    nodeBuffer.m_buffer[i].m_direction = VectorUtils.NormalizeXZ(Vector3.Lerp(nodeBuffer.m_buffer[index2].m_direction, nodeBuffer.m_buffer[index3].m_direction, t));
                                    nodeBuffer.m_buffer[i].m_elevation = Mathf.Lerp(nodeBuffer.m_buffer[index2].m_elevation, nodeBuffer.m_buffer[index3].m_elevation, t);
                                    nodeBuffer.m_buffer[i].m_terrainHeight = Singleton<TerrainManager>.instance.SampleRawHeightSmooth(nodeBuffer.m_buffer[i].m_position);
                                    if (!info.m_blockWater) {
                                        float iBlockHeight = Singleton<TerrainManager>.instance.SampleBlockHeightSmooth(nodeBuffer.m_buffer[i].m_position);
                                        if (iBlockHeight < nodeBuffer.m_buffer[i].m_terrainHeight - 15f) {
                                            nodeBuffer.m_buffer[i].m_terrainHeight = iBlockHeight;
                                        }
                                    }
                                }
                            } else {
                                for (int i = index0 + 1; i < index1; i++) {
                                    int index2 = index0;
                                    int index3 = index1;
                                    if (prevNodeInfo.m_netAI.FlattenOnlyOneDoubleSegment()) {
                                        index2 = i - ((i - index0) & 1);
                                        index3 = index2 + 2;
                                    }
                                    float t = (float)(i - index2) / (float)(index3 - index2);
                                    nodeBuffer.m_buffer[i].m_position = Vector3.Lerp(nodeBuffer.m_buffer[index2].m_position, nodeBuffer.m_buffer[index3].m_position, t);
                                    nodeBuffer.m_buffer[i].m_direction = VectorUtils.NormalizeXZ(Vector3.Lerp(nodeBuffer.m_buffer[index2].m_direction, nodeBuffer.m_buffer[index3].m_direction, t));
                                    nodeBuffer.m_buffer[i].m_elevation = Mathf.Lerp(nodeBuffer.m_buffer[index2].m_elevation, nodeBuffer.m_buffer[index3].m_elevation, t);
                                    nodeBuffer.m_buffer[i].m_terrainHeight = Singleton<TerrainManager>.instance.SampleRawHeightSmooth(nodeBuffer.m_buffer[i].m_position);
                                    if (!info.m_blockWater) {
                                        float iBlockHeight = Singleton<TerrainManager>.instance.SampleBlockHeightSmooth(nodeBuffer.m_buffer[i].m_position);
                                        if (iBlockHeight < nodeBuffer.m_buffer[i].m_terrainHeight - 15f) {
                                            nodeBuffer.m_buffer[i].m_terrainHeight = iBlockHeight;
                                        }
                                    }
                                }
                                iSegment2++;
                            }
                            for (int m = index0 + 1; m < index1; m++) {
                                nodeBuffer.m_buffer[m].m_double |= ((m - index0) & 1) == 1;
                            }
                        } else {
                            iSegment2++;
                        }
                        segmentCounter = 1;
                    }
                    prevNodeInfo = nodeInfo;
                }
                bool undergroundStart = true;
                bool undergroundEnd = true;
                if (startPoint.m_node != 0) {
                    undergroundStart = startPoint.m_node.ToNode().m_flags.IsFlagSet(NetNode.Flags.Underground);
                }
                if (endPoint.m_node != 0) {
                    undergroundStart = endPoint.m_node.ToNode().m_flags.IsFlagSet(NetNode.Flags.Underground);
                }
                NetInfo iNodeInfo = nodeBuffer[0].m_nodeInfo;
                bool segmentSplit = false;
                if (nodeID == 0 && !test && !visualize) {
                    if (startPoint.m_segment != 0) {
                        if (SplitSegment(startPoint.m_segment, out nodeID, startPos)) {
                            segmentSplit = true;
                        }
                        startPoint.m_segment = 0;
                    } else if (Singleton<NetManager>.instance.CreateNode(out nodeID, ref Singleton<SimulationManager>.instance.m_randomizer, iNodeInfo, startPos, Singleton<SimulationManager>.instance.m_currentBuildIndex)) {
                        if (startPoint.m_outside) {
                            nodeID.ToNode().m_flags |= NetNode.Flags.Outside;
                        }
                        if (startPos.y - nodeBuffer.m_buffer[0].m_terrainHeight < -8f && (iNodeInfo.m_netAI.SupportUnderground() || iNodeInfo.m_netAI.IsUnderground())) {
                            nodeID.ToNode().m_flags |= NetNode.Flags.Underground;
                        }
                        if (nodeBuffer.m_buffer[0].m_double) {
                            nodeID.ToNode().m_flags |= NetNode.Flags.Double;
                        }
                        if (iNodeInfo.m_netAI.IsUnderground()) {
                            nodeID.ToNode().m_elevation = (byte)Mathf.Clamp(Mathf.RoundToInt(0f - nodeBuffer[0].m_elevation), 0, 255);
                        } else if (iNodeInfo.m_netAI.IsOverground() && (nodeBuffer[0].m_elevation > 0.1f || nodeBuffer[0].m_position.y - nodeBuffer[0].m_terrainHeight > 0.1f)) {
                            nodeID.ToNode().m_elevation = (byte)Mathf.Clamp(Mathf.RoundToInt(nodeBuffer[0].m_elevation), 1, 255);
                        } else {
                            nodeID.ToNode().m_flags |= NetNode.Flags.OnGround;
                        }
                        Singleton<SimulationManager>.instance.m_currentBuildIndex++;
                        segmentSplit = true;
                    }
                    startPoint.m_node = nodeID;
                }
                NetNode nodeData0 = default(NetNode);
                nodeData0.m_position = startPos;
                if (nodeBuffer.m_buffer[0].m_double) {
                    nodeData0.m_flags |= NetNode.Flags.Double;
                }
                if (maxSegments == 0) {
                    nodeData0.m_flags |= NetNode.Flags.End;
                }
                if (startPos.y - nodeBuffer.m_buffer[0].m_terrainHeight < -8f && undergroundStart && (iNodeInfo.m_netAI.SupportUnderground() || iNodeInfo.m_netAI.IsUnderground())) {
                    nodeData0.m_flags |= NetNode.Flags.Underground;
                } else if (!iNodeInfo.m_netAI.IsOverground() || nodeBuffer[0].m_position.y - nodeBuffer[0].m_terrainHeight <= 0.1f) {
                    nodeData0.m_flags |= NetNode.Flags.OnGround;
                } else {
                    nodeData0.m_elevation = (byte)Mathf.Clamp(Mathf.RoundToInt(nodeBuffer[0].m_elevation), 1, 255);
                }
                if (startPoint.m_outside) {
                    nodeData0.m_flags |= NetNode.Flags.Outside;
                }
                iNodeInfo.m_netAI.GetNodeBuilding(0, ref nodeData0, out var buildingInfo2, out var heightOffset);
                if (visualize) {
                    if (buildingInfo2 is not null && (nodeID == 0 || segmentID != 0)) {
                        Vector3 building2Position = startPos;
                        building2Position.y += heightOffset;
                        RenderNodeBuilding(buildingInfo2, building2Position, middleDir);
                    }
                    if (iNodeInfo.m_netAI.DisplayTempSegment()) {
                        RenderNode(iNodeInfo, startPos, middleDir);
                    }
                } else if ((object)buildingInfo2 != null && (nodeData0.m_flags & NetNode.Flags.Outside) == 0) {
                    toolErrors = TestNodeBuilding(
                        ignoreNode: startPoint.m_node,
                        ignoreSegment: startPoint.m_segment,
                        ignoreBuilding: GetIgnoredBuilding(startPoint),
                        info: buildingInfo2,
                        position: startPos,
                        direction: middleDir,
                        test: test,
                        collidingSegmentBuffer: collidingSegments,
                        collidingBuildingBuffer: collidingBuildings);
                    if (test && toolErrors != ToolErrors.None) {
                        errors |= toolErrors;
                    }
                }
                if (buildingID != 0 && nodeID != 0 && (nodeID.ToNode().m_flags & NetNode.Flags.Untouchable) == 0) {
                    nodeID.ToNode().m_flags |= NetNode.Flags.Untouchable;
                    nodeID.ToNode().m_nextBuildingNode = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].m_netNode;
                    Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].m_netNode = nodeID;
                }
                for (int iSegment = 1; iSegment <= segmentCount; iSegment++) {
                    Vector3 iPos = nodeBuffer[iSegment].m_position;
                    Vector3 iDir = nodeBuffer[iSegment].m_direction;
                    NetSegment.CalculateMiddlePoints(startPos, middleDir, iPos, -iDir, smoothStart, smoothEnd, out var iBPos, out var iCPos);
                    iNodeInfo = nodeBuffer.m_buffer[iSegment].m_nodeInfo;
                    NetInfo netInfo2 = null;
                    float prevNodeHeight = nodeBuffer[iSegment - 1].m_position.y - nodeBuffer[iSegment - 1].m_terrainHeight;
                    float iNodeHeight = nodeBuffer[iSegment].m_position.y - nodeBuffer[iSegment].m_terrainHeight;
                    if (iSegment == 1 && !undergroundStart && prevNodeHeight < 0f) {
                        prevNodeHeight = 0f;
                    }
                    if (iSegment == segmentCount && !undergroundEnd && iNodeHeight < 0f) {
                        iNodeHeight = 0f;
                    }
                    NetNode endNode = nodeData0;
                    if (nodeBuffer.m_buffer[iSegment].m_double) {
                        netInfo2 = nodeBuffer.m_buffer[iSegment].m_nodeInfo;
                        nodeData0.m_flags |= NetNode.Flags.Double;
                    } else if (nodeBuffer.m_buffer[iSegment - 1].m_double) {
                        netInfo2 = nodeBuffer.m_buffer[iSegment - 1].m_nodeInfo;
                        nodeData0.m_flags &= ~NetNode.Flags.Double;
                    } else {
                        float iMinNodeHeight = Mathf.Min(iNodeHeight, prevNodeHeight);
                        float iMaxNodeHeight = Mathf.Max(iNodeHeight, prevNodeHeight);
                        if (iMaxNodeHeight >= -8f) {
                            for (int t = 1; t < 8; t++) {
                                Vector3 worldPos = Bezier3.Position(startPos, iBPos, iCPos, iPos, t / 8f);
                                float rawH = Singleton<TerrainManager>.instance.SampleRawHeightSmooth(worldPos);
                                if (!info.m_blockWater) {
                                    float blockH = Singleton<TerrainManager>.instance.SampleBlockHeightSmooth(worldPos);
                                    if (blockH < rawH - 15f) {
                                        rawH = blockH;
                                    }
                                }
                                iMaxNodeHeight = Mathf.Max(iMaxNodeHeight, worldPos.y - rawH);
                            }
                        }
                        if (iNodeHeight > -0.1f && iNodeHeight < 0.1f && prevNodeHeight > -0.1f && prevNodeHeight < 0.1f && iMaxNodeHeight < 11f) {
                            iMaxNodeHeight = 0f;
                        } else if (iNodeHeight < 0.1f && prevNodeHeight < 0.1f && iMaxNodeHeight > 0f && iMaxNodeHeight < 11f) {
                            iMaxNodeHeight = 0f;
                        }
                        netInfo2 = ((!autoFix0 && info.m_useFixedHeight) ? info : info.m_netAI.GetInfo(iMinNodeHeight, iMaxNodeHeight, length, iSegment == 1 && startPoint.m_outside, iSegment == segmentCount && endPoint.m_outside, curved, enableDouble: false, ref errors));
                        nodeData0.m_flags &= ~NetNode.Flags.Double;
                    }
                    bool cantMakeUnderground = nodeData0.m_flags.IsFlagSet(NetNode.Flags.Underground);
                    bool cantMakeOverground = !cantMakeUnderground;
                    nodeData0.m_position = iPos;
                    if (iNodeHeight < -8f && (iSegment != segmentCount || undergroundEnd) && (iNodeInfo.m_netAI.SupportUnderground() || iNodeInfo.m_netAI.IsUnderground())) {
                        nodeData0.m_elevation = 0;
                        nodeData0.m_flags |= NetNode.Flags.Underground;
                        nodeData0.m_flags &= ~NetNode.Flags.OnGround;
                        cantMakeUnderground = false;
                    } else if (!iNodeInfo.m_netAI.IsOverground() || iNodeHeight <= 0.1f) {
                        nodeData0.m_elevation = 0;
                        nodeData0.m_flags |= NetNode.Flags.OnGround;
                        nodeData0.m_flags &= ~NetNode.Flags.Underground;
                        cantMakeOverground = false;
                    } else {
                        nodeData0.m_elevation = (byte)Mathf.Clamp(Mathf.RoundToInt(nodeBuffer[iSegment].m_elevation), 1, 255);
                        nodeData0.m_flags &= ~NetNode.Flags.OnGround;
                        nodeData0.m_flags &= ~NetNode.Flags.Underground;
                        cantMakeOverground = false;
                    }
                    if (iSegment == segmentCount && endPoint.m_outside) {
                        nodeData0.m_flags |= NetNode.Flags.Outside;
                    } else {
                        nodeData0.m_flags &= ~NetNode.Flags.Outside;
                    }
                    iNodeInfo.m_netAI.GetNodeBuilding(0, ref nodeData0, out buildingInfo2, out heightOffset);
                    if (visualize) {
                        if ((object)buildingInfo2 != null && (iSegment != segmentCount || endPoint.m_node == 0 || segmentID != 0)) {
                            Vector3 position3 = iPos;
                            position3.y += heightOffset;
                            RenderNodeBuilding(buildingInfo2, position3, iDir);
                        }
                        if (netInfo2.m_netAI.DisplayTempSegment()) {
                            if (nodeBuffer.m_buffer[iSegment].m_double || cantMakeOverground) {
                                RenderSegment(netInfo2, netInfo2.m_netAI.UpdateSegmentFlags(ref nodeData0, ref endNode), iPos, startPos, -iDir, -middleDir, smoothStart, smoothEnd);
                            } else {
                                RenderSegment(netInfo2, netInfo2.m_netAI.UpdateSegmentFlags(ref endNode, ref nodeData0), startPos, iPos, middleDir, iDir, smoothStart, smoothEnd);
                            }
                        }
                    } else {
                        if (netInfo2.m_canCollide) {
                            ItemClass.CollisionType collisionType = netInfo2.m_netAI.GetCollisionType();
                            ItemClass.Layer collisionLayers = netInfo2.m_netAI.GetCollisionLayers();
                            ItemClass.Layer forbiddenCollisionLayers = netInfo2.m_netAI.GetForbiddenCollisionLayers();
                            bool canBeOutside0 = netInfo2.m_placementStyle == ItemClass.Placement.Procedural && !netInfo2.m_overlayVisible;
                            float collisionHalfWidth = netInfo2.m_netAI.GetCollisionHalfWidth();
                            int num26 = Mathf.Max(2, 16 / segmentCount);
                            Vector3 normal = new Vector3(middleDir.z, 0f, 0f - middleDir.x) * collisionHalfWidth;
                            Quad3 quad = default(Quad3);
                            quad.a = startPos - normal;
                            quad.d = startPos + normal;
                            float endRadius = netInfo2.m_netAI.GetEndRadius();
                            if (iSegment == 1 && segmentID == 0 && netInfo2.m_clipSegmentEnds && endRadius != 0f && startPoint.m_node == 0 && startPoint.m_segment == 0) {
                                Vector3 vector4 = startPos;
                                vector4.x -= normal.x * 0.8f - normal.z * 0.6f * endRadius / collisionHalfWidth;
                                vector4.z -= normal.z * 0.8f + normal.x * 0.6f * endRadius / collisionHalfWidth;
                                Vector3 vector5 = startPos;
                                vector5.x += normal.x * 0.8f + normal.z * 0.6f * endRadius / collisionHalfWidth;
                                vector5.z += normal.z * 0.8f - normal.x * 0.6f * endRadius / collisionHalfWidth;
                                Vector3 d = startPos;
                                d.x -= normal.x * 0.3f - normal.z * endRadius / collisionHalfWidth;
                                d.z -= normal.z * 0.3f + normal.x * endRadius / collisionHalfWidth;
                                Vector3 c = startPos;
                                c.x += normal.x * 0.3f + normal.z * endRadius / collisionHalfWidth;
                                c.z += normal.z * 0.3f - normal.x * endRadius / collisionHalfWidth;
                                float minY = startPos.y + netInfo2.m_minHeight;
                                float maxY = startPos.y + netInfo2.m_maxHeight;
                                Quad2 quad2 = Quad2.XZ(quad.a, quad.d, vector5, vector4);
                                Quad2 quad3 = Quad2.XZ(vector4, vector5, c, d);
                                Singleton<NetManager>.instance.OverlapQuad(quad2, minY, maxY, collisionType, collisionLayers, forbiddenCollisionLayers, startPoint.m_node, 0, startPoint.m_segment, collidingSegments);
                                Singleton<NetManager>.instance.OverlapQuad(quad3, minY, maxY, collisionType, collisionLayers, forbiddenCollisionLayers, startPoint.m_node, 0, startPoint.m_segment, collidingSegments);
                                Singleton<BuildingManager>.instance.OverlapQuad(quad2, minY, maxY, collisionType, collisionLayers, GetIgnoredBuilding(startPoint), startPoint.m_node, 0, collidingBuildings);
                                Singleton<BuildingManager>.instance.OverlapQuad(quad3, minY, maxY, collisionType, collisionLayers, GetIgnoredBuilding(startPoint), startPoint.m_node, 0, collidingBuildings);
                                if (test) {
                                    bool canBeOutside = canBeOutside0 || startPoint.m_outside;
                                    if ((properties.m_mode & ItemClass.Availability.AssetEditor) != 0) {
                                        float num27 = 256f;
                                        if (quad2.a.x < 0f - num27 || quad2.a.x > num27 || quad2.a.y < 0f - num27 || quad2.a.y > num27) {
                                            errors |= ToolErrors.OutOfArea;
                                        }
                                        if (quad2.b.x < 0f - num27 || quad2.b.x > num27 || quad2.b.y < 0f - num27 || quad2.b.y > num27) {
                                            errors |= ToolErrors.OutOfArea;
                                        }
                                        if (quad2.c.x < 0f - num27 || quad2.c.x > num27 || quad2.c.y < 0f - num27 || quad2.c.y > num27) {
                                            errors |= ToolErrors.OutOfArea;
                                        }
                                        if (quad2.d.x < 0f - num27 || quad2.d.x > num27 || quad2.d.y < 0f - num27 || quad2.d.y > num27) {
                                            errors |= ToolErrors.OutOfArea;
                                        }
                                    } else if (!canBeOutside && Singleton<GameAreaManager>.instance.QuadOutOfArea(quad2)) {
                                        errors |= ToolErrors.OutOfArea;
                                    }
                                    if ((properties.m_mode & ItemClass.Availability.AssetEditor) != 0) {
                                        float num28 = 256f;
                                        if (quad3.a.x < 0f - num28 || quad3.a.x > num28 || quad3.a.y < 0f - num28 || quad3.a.y > num28) {
                                            errors |= ToolErrors.OutOfArea;
                                        }
                                        if (quad3.b.x < 0f - num28 || quad3.b.x > num28 || quad3.b.y < 0f - num28 || quad3.b.y > num28) {
                                            errors |= ToolErrors.OutOfArea;
                                        }
                                        if (quad3.c.x < 0f - num28 || quad3.c.x > num28 || quad3.c.y < 0f - num28 || quad3.c.y > num28) {
                                            errors |= ToolErrors.OutOfArea;
                                        }
                                        if (quad3.d.x < 0f - num28 || quad3.d.x > num28 || quad3.d.y < 0f - num28 || quad3.d.y > num28) {
                                            errors |= ToolErrors.OutOfArea;
                                        }
                                    } else if (!canBeOutside && Singleton<GameAreaManager>.instance.QuadOutOfArea(quad3)) {
                                        errors |= ToolErrors.OutOfArea;
                                    }
                                }
                            }
                            for (int i = 1; i <= num26; i++) {
                                ushort ignoreNode = 0;
                                ushort ignoreNode2 = 0;
                                ushort ignoreSegment = 0;
                                ushort ignoreBuilding = 0;
                                bool canBeOutside = canBeOutside0;
                                if (segmentID != 0 && segmentCount == 1) {
                                    ignoreNode = startPoint.m_node;
                                    ignoreNode2 = endPoint.m_node;
                                    ignoreBuilding = ((i << 1 <= num26) ? GetIgnoredBuilding(startPoint) : GetIgnoredBuilding(endPoint));
                                } else if (iSegment == 1 && i - 1 << 1 < num26) {
                                    ignoreNode = startPoint.m_node;
                                    if (iSegment == segmentCount && i << 1 >= num26) {
                                        ignoreNode2 = endPoint.m_node;
                                    }
                                    ignoreSegment = startPoint.m_segment;
                                    ignoreBuilding = GetIgnoredBuilding(startPoint);
                                    canBeOutside |= startPoint.m_outside;
                                } else if (iSegment == segmentCount && i << 1 > num26) {
                                    ignoreNode = endPoint.m_node;
                                    if (iSegment == 1 && i - 1 << 1 <= num26) {
                                        ignoreNode2 = startPoint.m_node;
                                    }
                                    ignoreSegment = endPoint.m_segment;
                                    ignoreBuilding = GetIgnoredBuilding(endPoint);
                                    canBeOutside |= endPoint.m_outside;
                                } else if (i - 1 << 1 < num26) {
                                    ignoreNode = nodeID;
                                }
                                float t = (float)i / (float)num26;
                                Vector3 pos = Bezier3.Position(startPos, iBPos, iCPos, iPos, t);
                                Vector3 tan = Bezier3.Tangent(startPos, iBPos, iCPos, iPos, t);
                                normal = new Vector3(tan.z, 0f, 0f - tan.x).normalized * collisionHalfWidth;
                                quad.b = pos - normal;
                                quad.c = pos + normal;
                                float minY2 = Mathf.Min(Mathf.Min(quad.a.y, quad.b.y), Mathf.Min(quad.c.y, quad.d.y)) + netInfo2.m_minHeight;
                                float maxY2 = Mathf.Max(Mathf.Max(quad.a.y, quad.b.y), Mathf.Max(quad.c.y, quad.d.y)) + netInfo2.m_maxHeight;
                                Quad2 quad4 = Quad2.XZ(quad);
                                Singleton<NetManager>.instance.OverlapQuad(quad4, minY2, maxY2, collisionType, collisionLayers, forbiddenCollisionLayers, ignoreNode, ignoreNode2, ignoreSegment, collidingSegments);
                                Singleton<BuildingManager>.instance.OverlapQuad(quad4, minY2, maxY2, collisionType, collisionLayers, ignoreBuilding, ignoreNode, ignoreNode2, collidingBuildings);
                                if (test) {
                                    if ((properties.m_mode & ItemClass.Availability.AssetEditor) != 0) {
                                        float num30 = 256f;
                                        if (quad4.a.x < 0f - num30 || quad4.a.x > num30 || quad4.a.y < 0f - num30 || quad4.a.y > num30) {
                                            errors |= ToolErrors.OutOfArea;
                                        }
                                        if (quad4.b.x < 0f - num30 || quad4.b.x > num30 || quad4.b.y < 0f - num30 || quad4.b.y > num30) {
                                            errors |= ToolErrors.OutOfArea;
                                        }
                                        if (quad4.c.x < 0f - num30 || quad4.c.x > num30 || quad4.c.y < 0f - num30 || quad4.c.y > num30) {
                                            errors |= ToolErrors.OutOfArea;
                                        }
                                        if (quad4.d.x < 0f - num30 || quad4.d.x > num30 || quad4.d.y < 0f - num30 || quad4.d.y > num30) {
                                            errors |= ToolErrors.OutOfArea;
                                        }
                                    } else if (!canBeOutside && Singleton<GameAreaManager>.instance.QuadOutOfArea(quad4)) {
                                        errors |= ToolErrors.OutOfArea;
                                    }
                                }
                                quad.a = quad.b;
                                quad.d = quad.c;
                            }
                            if (iSegment == segmentCount && segmentID == 0 && netInfo2.m_clipSegmentEnds && endRadius != 0f && endPoint.m_node == 0 && endPoint.m_segment == 0) {
                                Vector3 vector7 = iPos;
                                vector7.x -= normal.x * 0.8f + normal.z * 0.6f * endRadius / collisionHalfWidth;
                                vector7.z -= normal.z * 0.8f - normal.x * 0.6f * endRadius / collisionHalfWidth;
                                Vector3 vector8 = iPos;
                                vector8.x += normal.x * 0.8f - normal.z * 0.6f * endRadius / collisionHalfWidth;
                                vector8.z += normal.z * 0.8f + normal.x * 0.6f * endRadius / collisionHalfWidth;
                                Vector3 b = iPos;
                                b.x -= normal.x * 0.3f + normal.z * endRadius / collisionHalfWidth;
                                b.z -= normal.z * 0.3f - normal.x * endRadius / collisionHalfWidth;
                                Vector3 c2 = iPos;
                                c2.x += normal.x * 0.3f - normal.z * endRadius / collisionHalfWidth;
                                c2.z += normal.z * 0.3f + normal.x * endRadius / collisionHalfWidth;
                                float minY3 = iPos.y + netInfo2.m_minHeight;
                                float maxY3 = iPos.y + netInfo2.m_maxHeight;
                                Quad2 quad5 = Quad2.XZ(quad.a, vector7, vector8, quad.d);
                                Quad2 quad6 = Quad2.XZ(vector7, b, c2, vector8);
                                Singleton<NetManager>.instance.OverlapQuad(quad5, minY3, maxY3, collisionType, collisionLayers, forbiddenCollisionLayers, endPoint.m_node, 0, endPoint.m_segment, collidingSegments);
                                Singleton<NetManager>.instance.OverlapQuad(quad6, minY3, maxY3, collisionType, collisionLayers, forbiddenCollisionLayers, endPoint.m_node, 0, endPoint.m_segment, collidingSegments);
                                Singleton<BuildingManager>.instance.OverlapQuad(quad5, minY3, maxY3, collisionType, collisionLayers, GetIgnoredBuilding(endPoint), endPoint.m_node, 0, collidingBuildings);
                                Singleton<BuildingManager>.instance.OverlapQuad(quad6, minY3, maxY3, collisionType, collisionLayers, GetIgnoredBuilding(endPoint), endPoint.m_node, 0, collidingBuildings);
                                if (test) {
                                    bool canBeOutside = canBeOutside0 || endPoint.m_outside;
                                    if ((properties.m_mode & ItemClass.Availability.AssetEditor) != 0) {
                                        float num31 = 256f;
                                        if (quad5.a.x < 0f - num31 || quad5.a.x > num31 || quad5.a.y < 0f - num31 || quad5.a.y > num31) {
                                            errors |= ToolErrors.OutOfArea;
                                        }
                                        if (quad5.b.x < 0f - num31 || quad5.b.x > num31 || quad5.b.y < 0f - num31 || quad5.b.y > num31) {
                                            errors |= ToolErrors.OutOfArea;
                                        }
                                        if (quad5.c.x < 0f - num31 || quad5.c.x > num31 || quad5.c.y < 0f - num31 || quad5.c.y > num31) {
                                            errors |= ToolErrors.OutOfArea;
                                        }
                                        if (quad5.d.x < 0f - num31 || quad5.d.x > num31 || quad5.d.y < 0f - num31 || quad5.d.y > num31) {
                                            errors |= ToolErrors.OutOfArea;
                                        }
                                    } else if (!canBeOutside && Singleton<GameAreaManager>.instance.QuadOutOfArea(quad5)) {
                                        errors |= ToolErrors.OutOfArea;
                                    }
                                    if ((properties.m_mode & ItemClass.Availability.AssetEditor) != 0) {
                                        float num32 = 256f;
                                        if (quad6.a.x < 0f - num32 || quad6.a.x > num32 || quad6.a.y < 0f - num32 || quad6.a.y > num32) {
                                            errors |= ToolErrors.OutOfArea;
                                        }
                                        if (quad6.b.x < 0f - num32 || quad6.b.x > num32 || quad6.b.y < 0f - num32 || quad6.b.y > num32) {
                                            errors |= ToolErrors.OutOfArea;
                                        }
                                        if (quad6.c.x < 0f - num32 || quad6.c.x > num32 || quad6.c.y < 0f - num32 || quad6.c.y > num32) {
                                            errors |= ToolErrors.OutOfArea;
                                        }
                                        if (quad6.d.x < 0f - num32 || quad6.d.x > num32 || quad6.d.y < 0f - num32 || quad6.d.y > num32) {
                                            errors |= ToolErrors.OutOfArea;
                                        }
                                    } else if (!canBeOutside && Singleton<GameAreaManager>.instance.QuadOutOfArea(quad6)) {
                                        errors |= ToolErrors.OutOfArea;
                                    }
                                }
                            }
                        }
                        if ((object)buildingInfo2 != null && (nodeData0.m_flags & NetNode.Flags.Outside) == 0) {
                            ushort ignoreNode3 = (ushort)((iSegment == segmentCount) ? endPoint.m_node : 0);
                            ushort ignoreSegment2 = (ushort)((iSegment == segmentCount) ? endPoint.m_segment : 0);
                            ushort ignoreBuilding2 = (ushort)((iSegment == segmentCount) ? GetIgnoredBuilding(endPoint) : 0);
                            Vector3 position4 = iPos;
                            position4.y += heightOffset;
                            toolErrors = TestNodeBuilding(buildingInfo2, position4, iDir, ignoreNode3, ignoreSegment2, ignoreBuilding2, test, collidingSegments, collidingBuildings);
                            if (test && toolErrors != ToolErrors.None) {
                                errors |= toolErrors;
                            }
                        }
                        if (test) {
                            cost += netInfo2.m_netAI.GetConstructionCost(startPos, iPos, prevNodeHeight, iNodeHeight);
                            if (needMoney && cost > 0 && Singleton<EconomyManager>.instance.PeekResource(EconomyManager.Resource.Construction, cost) != cost) {
                                errors |= ToolErrors.NotEnoughMoney;
                            }
                            if (!netInfo2.m_netAI.BuildUnderground() && !netInfo2.m_netAI.BuildOnWater() && !netInfo2.m_netAI.IgnoreWater()) {
                                float num33 = Singleton<TerrainManager>.instance.WaterLevel(VectorUtils.XZ(startPos));
                                float num34 = Singleton<TerrainManager>.instance.WaterLevel(VectorUtils.XZ(iPos));
                                if ((num33 > startPos.y && num33 > 0f) || (num34 > iPos.y && num34 > 0f)) {
                                    errors |= ToolErrors.CannotBuildOnWater;
                                }
                            }
                            ushort iStartNodeID = 0;
                            ushort iStartSegmentID = 0;
                            ushort iEndNodeID = 0;
                            ushort iEndSegmentID = 0;
                            if (iSegment == 1) {
                                iStartNodeID = startPoint.m_node;
                                iStartSegmentID = startPoint.m_segment;
                            }
                            if (iSegment == segmentCount) {
                                iEndNodeID = endPoint.m_node;
                                iEndSegmentID = endPoint.m_segment;
                            }
                            errors = errors | CanCreateSegment(netInfo2, iStartNodeID, iStartSegmentID, iEndNodeID, iEndSegmentID, segmentID, startPos, iPos, middleDir, -iDir, collidingSegments, testEnds) | netInfo2.m_netAI.CanConnectTo(iStartNodeID, iStartSegmentID, collidingSegments) | netInfo2.m_netAI.CanConnectTo(iEndNodeID, iEndSegmentID, collidingSegments);
                        } else {
                            cost += netInfo2.m_netAI.GetConstructionCost(startPos, iPos, prevNodeHeight, iNodeHeight);
                            if (needMoney && cost > 0) {
                                cost -= Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Construction, cost, netInfo2.m_class);
                                if (cost > 0) {
                                    errors |= ToolErrors.NotEnoughMoney;
                                }
                            }
                            bool canMoveNode = nodeID == 0;
                            bool iSegmentSplit = false;
                            ushort endNodeID = endPoint.m_node;
                            if (iSegment != segmentCount || endNodeID == 0) {
                                if (iSegment == segmentCount && endPoint.m_segment != 0) {
                                    if (SplitSegment(endPoint.m_segment, out endNodeID, iPos)) {
                                        iSegmentSplit = true;
                                    } else {
                                        canMoveNode = true;
                                    }
                                    endPoint.m_segment = 0;
                                } else if (Singleton<NetManager>.instance.CreateNode(out endNodeID, ref Singleton<SimulationManager>.instance.m_randomizer, iNodeInfo, iPos, Singleton<SimulationManager>.instance.m_currentBuildIndex)) {
                                    if (iSegment == segmentCount && endPoint.m_outside) {
                                        Singleton<NetManager>.instance.m_nodes.m_buffer[endNodeID].m_flags |= NetNode.Flags.Outside;
                                    }
                                    if (iNodeHeight < -8f && (iNodeInfo.m_netAI.SupportUnderground() || iNodeInfo.m_netAI.IsUnderground())) {
                                        Singleton<NetManager>.instance.m_nodes.m_buffer[endNodeID].m_flags |= NetNode.Flags.Underground;
                                    }
                                    if (nodeBuffer.m_buffer[iSegment].m_double) {
                                        Singleton<NetManager>.instance.m_nodes.m_buffer[endNodeID].m_flags |= NetNode.Flags.Double;
                                    }
                                    if (iNodeInfo.m_netAI.IsUnderground()) {
                                        Singleton<NetManager>.instance.m_nodes.m_buffer[endNodeID].m_elevation = (byte)Mathf.Clamp(Mathf.RoundToInt(0f - nodeBuffer[iSegment].m_elevation), 0, 255);
                                    } else if (iNodeInfo.m_netAI.IsOverground() && (nodeBuffer[iSegment].m_elevation > 0.1f || nodeBuffer[iSegment].m_position.y - nodeBuffer[iSegment].m_terrainHeight > 0.1f)) {
                                        Singleton<NetManager>.instance.m_nodes.m_buffer[endNodeID].m_elevation = (byte)Mathf.Clamp(Mathf.RoundToInt(nodeBuffer[iSegment].m_elevation), 1, 255);
                                    } else {
                                        Singleton<NetManager>.instance.m_nodes.m_buffer[endNodeID].m_flags |= NetNode.Flags.OnGround;
                                    }
                                    Singleton<SimulationManager>.instance.m_currentBuildIndex++;
                                    iSegmentSplit = true;
                                } else {
                                    canMoveNode = true;
                                }
                                if (iSegment == segmentCount) {
                                    endPoint.m_node = endNodeID;
                                }
                            }
                            if (!canMoveNode && !curved && nodeID.ToNode().m_elevation == Singleton<NetManager>.instance.m_nodes.m_buffer[endNodeID].m_elevation) {
                                Vector3 endPos = startPos;
                                if (iSegment == 1) {
                                    TryMoveNode(ref nodeID, ref middleDir, netInfo2, iPos);
                                    endPos = nodeID.ToNode().m_position;
                                }
                                if (iSegment == segmentCount) {
                                    Vector3 direction4 = -iDir;
                                    TryMoveNode(ref endNodeID, ref direction4, netInfo2, endPos);
                                    iDir = -direction4;
                                }
                            }
                            if (!canMoveNode) {
                                canMoveNode = ((!nodeBuffer.m_buffer[iSegment].m_double && !cantMakeOverground) ? ((nodeBuffer.m_buffer[iSegment - 1].m_double || cantMakeUnderground) ? (!Singleton<NetManager>.instance.CreateSegment(out segmentID2, ref Singleton<SimulationManager>.instance.m_randomizer, netInfo2, null, nodeID, endNodeID, middleDir, -iDir, buildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, reverse)) : ((((uint)(segmentCount - iSegment) & (true ? 1u : 0u)) != 0 || iSegment == 1 || !curved) ? (!Singleton<NetManager>.instance.CreateSegment(out segmentID2, ref Singleton<SimulationManager>.instance.m_randomizer, netInfo2, null, nodeID, endNodeID, middleDir, -iDir, buildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, reverse)) : (!Singleton<NetManager>.instance.CreateSegment(out segmentID2, ref Singleton<SimulationManager>.instance.m_randomizer, netInfo2, null, endNodeID, nodeID, -iDir, middleDir, buildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, !reverse)))) : (!Singleton<NetManager>.instance.CreateSegment(out segmentID2, ref Singleton<SimulationManager>.instance.m_randomizer, netInfo2, null, endNodeID, nodeID, -iDir, middleDir, buildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, !reverse)));
                                if (!canMoveNode) {
                                    segmentID2.ToSegment().m_flags |= flags;
                                    if (nameSeed != 0) {
                                        segmentID2.ToSegment().m_nameSeed = nameSeed;
                                    }
                                    if (text != null) {
                                        segmentID2.ToSegment().m_flags |= NetSegment.Flags.CustomName;
                                        InstanceID segmnetInstance2 = InstanceID.Empty;
                                        segmnetInstance2.NetSegment = segmentID2;
                                        Singleton<InstanceManager>.instance.SetName(segmnetInstance2, text);
                                    }
                                    if (adjusted) {
                                        Singleton<NetManager>.instance.m_adjustedSegments[segmentID2 >> 6] |= 1UL << segmentID2;
                                    }
                                    Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                                    buildIndex = Singleton<SimulationManager>.instance.m_currentBuildIndex;
                                    DispatchPlacementEffect(startPos, iBPos, iCPos, iPos, info.m_halfWidth, bulldozing: false);
                                    netInfo2.m_netAI.ManualActivation(segmentID2, ref segmentID2.ToSegment(), oldInfo);
                                }
                            }
                            if (canMoveNode) {
                                if (segmentSplit && nodeID != 0) {
                                    Singleton<NetManager>.instance.ReleaseNode(nodeID);
                                    nodeID = 0;
                                }
                                if (iSegmentSplit && endNodeID != 0) {
                                    Singleton<NetManager>.instance.ReleaseNode(endNodeID);
                                    endNodeID = 0;
                                }
                            }
                            if (buildingID != 0 && endNodeID != 0 && (Singleton<NetManager>.instance.m_nodes.m_buffer[endNodeID].m_flags & NetNode.Flags.Untouchable) == 0) {
                                Singleton<NetManager>.instance.m_nodes.m_buffer[endNodeID].m_flags |= NetNode.Flags.Untouchable;
                                Singleton<NetManager>.instance.m_nodes.m_buffer[endNodeID].m_nextBuildingNode = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].m_netNode;
                                Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].m_netNode = endNodeID;
                            }
                            if (buildingID != 0 && segmentID2 != 0 && (segmentID2.ToSegment().m_flags & NetSegment.Flags.Untouchable) == 0) {
                                segmentID2.ToSegment().m_flags |= NetSegment.Flags.Untouchable;
                            }
                            nodeID = endNodeID;
                        }
                    }
                    startPos = iPos;
                    middleDir = iDir;
                    segmentSplit = false;
                }
                if (visualize) {
                    if (iNodeInfo.m_netAI.DisplayTempSegment()) {
                        RenderNode(iNodeInfo, startPos, -middleDir);
                    }
                } else {
                    BuildingTool.IgnoreRelocateSegments(relocateBuildingID, collidingSegments, collidingBuildings);
                    if (CheckCollidingSegments(collidingSegments, collidingBuildings, segmentID) && test && (errors & (ToolErrors.InvalidShape | ToolErrors.TooShort | ToolErrors.SlopeTooSteep | ToolErrors.HeightTooHigh | ToolErrors.TooManyConnections)) == 0) {
                        errors |= ToolErrors.ObjectCollision;
                    }
                    if (BuildingTool.CheckCollidingBuildings(null, collidingBuildings, collidingSegments) && test) {
                        errors |= ToolErrors.ObjectCollision;
                    }
                    if (!test) {
                        ReleaseNonImportantSegments(collidingSegments);
                        BuildingTool.ReleaseNonImportantBuildings(collidingBuildings);
                    }
                }
                for (int i = 0; i <= segmentCount; i++) {
                    nodeBuffer.m_buffer[i].m_nodeInfo = null;
                }
                firstNode = startPoint.m_node;
                lastNode = endPoint.m_node;
                return errors;
            } finally {
                if (cost < 0) {
                    cost = 0;
                }
                if (test || !visualize) {
                    properties.EndColliding();
                }
            }
        }
    }
}
