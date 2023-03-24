using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;
using KianCommons;
using System;

namespace KianCommons.StockCode {
    public partial struct NetNode2 {
        public NetInfo Info => null;
        public ushort GetSegment(int i) => 0;
        public NetNode.Flags m_flags;
        public Notification.Problem m_problems;
        public float m_heightOffset;
        public Bounds m_bounds;
        Vector3 m_position;
        const int INVALID_RENDER_INDEX = ushort.MaxValue;
        const uint NODE_HOLDER = BuildingManager.MAX_BUILDING_COUNT + NetManager.MAX_SEGMENT_COUNT;

        public static void RenderLod(RenderManager.CameraInfo cameraInfo, NetInfo.LodValue lod) {
            NetManager instance = Singleton<NetManager>.instance;
            MaterialPropertyBlock materialBlock = instance.m_materialBlock;
            materialBlock.Clear();
            Mesh mesh;
            int upperLoadCount;
            if(lod.m_lodCount <= 1) {
                mesh = lod.m_key.m_mesh.m_mesh1;
                upperLoadCount = 1;
            } else if(lod.m_lodCount <= 4) {
                mesh = lod.m_key.m_mesh.m_mesh4;
                upperLoadCount = 4;
            } else {
                mesh = lod.m_key.m_mesh.m_mesh8;
                upperLoadCount = 8;
            }
            for(int i = lod.m_lodCount; i < upperLoadCount; i++) {
                lod.m_leftMatrices[i] = default(Matrix4x4);
                lod.m_leftMatricesB[i] = default(Matrix4x4);
                lod.m_rightMatrices[i] = default(Matrix4x4);
                lod.m_rightMatricesB[i] = default(Matrix4x4);
                lod.m_meshScales[i] = default;
                lod.m_centerPositions[i] = default;
                lod.m_sideScales[i] = default;
                lod.m_objectIndices[i] = default;
                lod.m_meshLocations[i] = cameraInfo.m_forward * -100000f;
            }
            materialBlock.SetMatrixArray(instance.ID_LeftMatrices, lod.m_leftMatrices);
            materialBlock.SetMatrixArray(instance.ID_LeftMatricesB, lod.m_leftMatricesB);
            materialBlock.SetMatrixArray(instance.ID_RightMatrices, lod.m_rightMatrices);
            materialBlock.SetMatrixArray(instance.ID_RightMatricesB, lod.m_rightMatricesB);
            materialBlock.SetVectorArray(instance.ID_MeshScales, lod.m_meshScales);
            materialBlock.SetVectorArray(instance.ID_CenterPositions, lod.m_centerPositions);
            materialBlock.SetVectorArray(instance.ID_SideScales, lod.m_sideScales);
            materialBlock.SetVectorArray(instance.ID_ObjectIndices, lod.m_objectIndices);
            materialBlock.SetVectorArray(instance.ID_MeshLocations, lod.m_meshLocations);
            if(lod.m_surfaceTexA != null) {
                materialBlock.SetTexture(instance.ID_SurfaceTexA, lod.m_surfaceTexA);
                materialBlock.SetTexture(instance.ID_SurfaceTexB, lod.m_surfaceTexB);
                materialBlock.SetVector(instance.ID_SurfaceMapping, lod.m_surfaceMapping);
                lod.m_surfaceTexA = null;
                lod.m_surfaceTexB = null;
            }
            if(mesh != null) {
                Bounds bounds = default(Bounds);
                bounds.SetMinMax(lod.m_lodMin - new Vector3(100f, 100f, 100f), lod.m_lodMax + new Vector3(100f, 100f, 100f));
                mesh.bounds = bounds;
                lod.m_lodMin = new Vector3(100000f, 100000f, 100000f);
                lod.m_lodMax = new Vector3(-100000f, -100000f, -100000f);
                instance.m_drawCallData.m_lodCalls++;
                instance.m_drawCallData.m_batchedCalls += lod.m_lodCount - 1;
                Graphics.DrawMesh(mesh, Matrix4x4.identity, lod.m_material, lod.m_key.m_layer, null, 0, materialBlock);
            }
            lod.m_lodCount = 0;
        }


        public void PopulateGroupData(ushort nodeID, int groupX, int groupZ, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance, ref bool requireSurfaceMaps) {
            NetInfo info = this.Info;
            if (this.m_problems != Notification.Problem.None && layer == Singleton<NotificationManager>.instance.m_notificationLayer && (this.m_flags & NetNode.Flags.Temporary) == NetNode.Flags.None) {
                Vector3 position = this.m_position;
                position.y += info.m_maxHeight;
                Notification.PopulateGroupData(this.m_problems, position, 1f, groupX, groupZ, ref vertexIndex, ref triangleIndex, groupPosition, data, ref min, ref max, ref maxRenderDistance, ref maxInstanceDistance);
            }
            bool flag = false;
            if ((this.m_flags & NetNode.Flags.Junction) != NetNode.Flags.None) {
                NetManager netMan = Singleton<NetManager>.instance;
                Vector3 pos = this.m_position;
                for (int i1 = 0; i1 < 8; i1++) {
                    ushort segmentID1 = this.GetSegment(i1);
                    if (segmentID1 != 0) {
                        NetInfo info1 = segmentID1.ToSegment().Info;
                        ItemClass connectionClass = info1.GetConnectionClass();
                        Vector3 dir1 = segmentID1.ToSegment().GetDirection(nodeID);
                        float maxDot = -1f;
                        for (int i2 = 0; i2 < 8; i2++) {
                            ushort segmentID2 = this.GetSegment(i2);
                            if (segmentID2 != 0 && segmentID2 != segmentID1) {
                                NetInfo info2 = segmentID2.ToSegment().Info;
                                ItemClass connectionClass2 = info2.GetConnectionClass();
                                if (((info.m_netLayers | info1.m_netLayers | info2.m_netLayers) & 1 << layer) != 0 && (connectionClass.m_service == connectionClass2.m_service || (info1.m_nodeConnectGroups & info2.m_connectGroup) != NetInfo.ConnectGroup.None || (info2.m_nodeConnectGroups & info1.m_connectGroup) != NetInfo.ConnectGroup.None)) {
                                    Vector3 dir2 = segmentID2.ToSegment().GetDirection(nodeID);
                                    float dot = dir1.x * dir2.x + dir1.z * dir2.z;
                                    maxDot = Mathf.Max(maxDot, dot);
                                    bool connects1 = info1.m_requireDirectRenderers && (info1.m_nodeConnectGroups == NetInfo.ConnectGroup.None || (info1.m_nodeConnectGroups & info2.m_connectGroup) != NetInfo.ConnectGroup.None);
                                    bool connects2 = info2.m_requireDirectRenderers && (info2.m_nodeConnectGroups == NetInfo.ConnectGroup.None || (info2.m_nodeConnectGroups & info1.m_connectGroup) != NetInfo.ConnectGroup.None);
                                    if (i2 > i1 && (connects1 || connects2)) {
                                        float maxTurnAngleCos = Mathf.Min(info1.m_maxTurnAngleCos, info2.m_maxTurnAngleCos);
                                        if (dot < 0.01f - maxTurnAngleCos) {
                                            float prio1;
                                            if (connects1) {
                                                prio1 = info1.m_netAI.GetNodeInfoPriority(segmentID1, ref segmentID1.ToSegment());
                                            } else {
                                                prio1 = -1E+08f;
                                            }
                                            float prio2;
                                            if (connects2) {
                                                prio2 = info2.m_netAI.GetNodeInfoPriority(segmentID2, ref segmentID2.ToSegment());
                                            } else {
                                                prio2 = -1E+08f;
                                            }
                                            if (prio1 >= prio2) {
                                                if (info1.m_nodes != null && info1.m_nodes.Length != 0) {
                                                    flag = true;
                                                    float vscale = info1.m_netAI.GetVScale();
                                                    Vector3 zero = Vector3.zero;
                                                    Vector3 zero2 = Vector3.zero;
                                                    Vector3 vector2 = Vector3.zero;
                                                    Vector3 vector3 = Vector3.zero;
                                                    Vector3 zero3 = Vector3.zero;
                                                    Vector3 zero4 = Vector3.zero;
                                                    Vector3 zero5 = Vector3.zero;
                                                    Vector3 zero6 = Vector3.zero;
                                                    bool start = segmentID1.ToSegment().m_startNode == nodeID;
                                                    bool flag4;
                                                    segmentID1.ToSegment().CalculateCorner(segmentID1, true, start, false, out zero, out zero3, out flag4);
                                                    segmentID1.ToSegment().CalculateCorner(segmentID1, true, start, true, out zero2, out zero4, out flag4);
                                                    start = (segmentID2.ToSegment().m_startNode == nodeID);
                                                    segmentID2.ToSegment().CalculateCorner(segmentID2, true, start, true, out vector2, out zero5, out flag4);
                                                    segmentID2.ToSegment().CalculateCorner(segmentID2, true, start, false, out vector3, out zero6, out flag4);
                                                    Vector3 b = (vector3 - vector2) * (info1.m_halfWidth / info2.m_halfWidth * 0.5f - 0.5f);
                                                    vector2 -= b;
                                                    vector3 += b;
                                                    Vector3 vector4;
                                                    Vector3 vector5;
                                                    NetSegment.CalculateMiddlePoints(zero, -zero3, vector2, -zero5, true, true, out vector4, out vector5);
                                                    Vector3 vector6;
                                                    Vector3 vector7;
                                                    NetSegment.CalculateMiddlePoints(zero2, -zero4, vector3, -zero6, true, true, out vector6, out vector7);
                                                    Matrix4x4 leftMatrix = NetSegment.CalculateControlMatrix(zero, vector4, vector5, vector2, zero2, vector6, vector7, vector3, groupPosition, vscale);
                                                    Matrix4x4 rightMatrix = NetSegment.CalculateControlMatrix(zero2, vector6, vector7, vector3, zero, vector4, vector5, vector2, groupPosition, vscale);
                                                    Vector4 vector8 = new Vector4(0.5f / info1.m_halfWidth, 1f / info1.m_segmentLength, 1f, 1f);
                                                    Vector4 colorLocation;
                                                    Vector4 vector9;
                                                    if (NetNode.BlendJunction(nodeID)) {
                                                        colorLocation = RenderManager.GetColorLocation(86016u + (uint)nodeID);
                                                        vector9 = colorLocation;
                                                    } else {
                                                        colorLocation = RenderManager.GetColorLocation((uint)(49152 + segmentID1));
                                                        vector9 = RenderManager.GetColorLocation((uint)(49152 + segmentID2));
                                                    }
                                                    Vector4 vector10 = new Vector4(colorLocation.x, colorLocation.y, vector9.x, vector9.y);
                                                    for (int k = 0; k < info1.m_nodes.Length; k++) {
                                                        NetInfo.Node node1 = info1.m_nodes[k];
                                                        if ((node1.m_connectGroup == NetInfo.ConnectGroup.None || (node1.m_connectGroup & info2.m_connectGroup & NetInfo.ConnectGroup.AllGroups) != NetInfo.ConnectGroup.None) &&
                                                            node1.m_layer == layer && node1.CheckFlags(this.m_flags) && node1.m_combinedLod != null && node1.m_directConnect) {
                                                            Vector4 objectIndex = vector10;
                                                            Vector4 meshScale = vector8;
                                                            if (node1.m_requireWindSpeed) {
                                                                objectIndex.w = Singleton<WeatherManager>.instance.GetWindSpeed(this.m_position);
                                                            }
                                                            if ((node1.m_connectGroup & NetInfo.ConnectGroup.Oneway) != NetInfo.ConnectGroup.None) {
                                                                bool flag5 = segmentID1.ToSegment().m_startNode == nodeID == ((segmentID1.ToSegment().m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                                                if (info2.m_hasBackwardVehicleLanes != info2.m_hasForwardVehicleLanes || (node1.m_connectGroup & NetInfo.ConnectGroup.Directional) != NetInfo.ConnectGroup.None) {
                                                                    bool flag6 = segmentID2.ToSegment().m_startNode == nodeID == ((segmentID2.ToSegment().m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                                                    if (flag5 == flag6) {
                                                                        goto IL_7A7;
                                                                    }
                                                                }
                                                                if (flag5) {
                                                                    if ((node1.m_connectGroup & NetInfo.ConnectGroup.OnewayStart) == NetInfo.ConnectGroup.None) {
                                                                        goto IL_7A7;
                                                                    }
                                                                } else {
                                                                    if ((node1.m_connectGroup & NetInfo.ConnectGroup.OnewayEnd) == NetInfo.ConnectGroup.None) {
                                                                        goto IL_7A7;
                                                                    }
                                                                    meshScale.x = -meshScale.x;
                                                                    meshScale.y = -meshScale.y;
                                                                }
                                                            }
                                                            NetNode.PopulateGroupData(info1, node1, leftMatrix, rightMatrix, meshScale, objectIndex, ref vertexIndex, ref triangleIndex, data, ref requireSurfaceMaps);
                                                        }
                                                        IL_7A7:;
                                                    }
                                                }
                                            } else if (info2.m_nodes != null && info2.m_nodes.Length != 0) {
                                                flag = true;
                                                float vscale2 = info2.m_netAI.GetVScale();
                                                Vector3 vector11 = Vector3.zero;
                                                Vector3 vector12 = Vector3.zero;
                                                Vector3 zero7 = Vector3.zero;
                                                Vector3 zero8 = Vector3.zero;
                                                Vector3 zero9 = Vector3.zero;
                                                Vector3 zero10 = Vector3.zero;
                                                Vector3 zero11 = Vector3.zero;
                                                Vector3 zero12 = Vector3.zero;
                                                bool start2 = segmentID1.ToSegment().m_startNode == nodeID;
                                                bool flag7;
                                                segmentID1.ToSegment().CalculateCorner(segmentID1, true, start2, false, out vector11, out zero9, out flag7);
                                                segmentID1.ToSegment().CalculateCorner(segmentID1, true, start2, true, out vector12, out zero10, out flag7);
                                                start2 = (segmentID2.ToSegment().m_startNode == nodeID);
                                                segmentID2.ToSegment().CalculateCorner(segmentID2, true, start2, true, out zero7, out zero11, out flag7);
                                                segmentID2.ToSegment().CalculateCorner(segmentID2, true, start2, false, out zero8, out zero12, out flag7);
                                                Vector3 b2 = (vector12 - vector11) * (info2.m_halfWidth / info1.m_halfWidth * 0.5f - 0.5f);
                                                vector11 -= b2;
                                                vector12 += b2;
                                                Vector3 vector13;
                                                Vector3 vector14;
                                                NetSegment.CalculateMiddlePoints(vector11, -zero9, zero7, -zero11, true, true, out vector13, out vector14);
                                                Vector3 vector15;
                                                Vector3 vector16;
                                                NetSegment.CalculateMiddlePoints(vector12, -zero10, zero8, -zero12, true, true, out vector15, out vector16);
                                                Matrix4x4 leftMatrix2 = NetSegment.CalculateControlMatrix(vector11, vector13, vector14, zero7, vector12, vector15, vector16, zero8, groupPosition, vscale2);
                                                Matrix4x4 rightMatrix2 = NetSegment.CalculateControlMatrix(vector12, vector15, vector16, zero8, vector11, vector13, vector14, zero7, groupPosition, vscale2);
                                                Vector4 vector17 = new Vector4(0.5f / info2.m_halfWidth, 1f / info2.m_segmentLength, 1f, 1f);
                                                Vector4 colorLocation2;
                                                Vector4 vector18;
                                                if (NetNode.BlendJunction(nodeID)) {
                                                    colorLocation2 = RenderManager.GetColorLocation(86016u + (uint)nodeID);
                                                    vector18 = colorLocation2;
                                                } else {
                                                    colorLocation2 = RenderManager.GetColorLocation((uint)(49152 + segmentID1));
                                                    vector18 = RenderManager.GetColorLocation((uint)(49152 + segmentID2));
                                                }
                                                Vector4 vector19 = new Vector4(colorLocation2.x, colorLocation2.y, vector18.x, vector18.y);
                                                for (int l = 0; l < info2.m_nodes.Length; l++) {
                                                    NetInfo.Node node2 = info2.m_nodes[l];
                                                    if ((node2.m_connectGroup == NetInfo.ConnectGroup.None || (node2.m_connectGroup & info1.m_connectGroup & NetInfo.ConnectGroup.AllGroups) != NetInfo.ConnectGroup.None) &&
                                                        node2.m_layer == layer && node2.CheckFlags(this.m_flags) && node2.m_combinedLod != null && node2.m_directConnect) {
                                                        Vector4 objectIndex2 = vector19;
                                                        Vector4 meshScale2 = vector17;
                                                        if (node2.m_requireWindSpeed) {
                                                            objectIndex2.w = Singleton<WeatherManager>.instance.GetWindSpeed(this.m_position);
                                                        }
                                                        if ((node2.m_connectGroup & NetInfo.ConnectGroup.Oneway) != NetInfo.ConnectGroup.None) {
                                                            bool flag8 = segmentID2.ToSegment().m_startNode == nodeID == ((segmentID2.ToSegment().m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                                            if (info1.m_hasBackwardVehicleLanes != info1.m_hasForwardVehicleLanes || (node2.m_connectGroup & NetInfo.ConnectGroup.Directional) != NetInfo.ConnectGroup.None) {
                                                                bool flag9 = segmentID1.ToSegment().m_startNode == nodeID == ((segmentID1.ToSegment().m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                                                if (flag9 == flag8) {
                                                                    goto IL_C08;
                                                                }
                                                            }
                                                            if (flag8) {
                                                                if ((node2.m_connectGroup & NetInfo.ConnectGroup.OnewayStart) == NetInfo.ConnectGroup.None) {
                                                                    goto IL_C08;
                                                                }
                                                                meshScale2.x = -meshScale2.x;
                                                                meshScale2.y = -meshScale2.y;
                                                            } else if ((node2.m_connectGroup & NetInfo.ConnectGroup.OnewayEnd) == NetInfo.ConnectGroup.None) {
                                                                goto IL_C08;
                                                            }
                                                        }
                                                        NetNode.PopulateGroupData(info2, node2, leftMatrix2, rightMatrix2, meshScale2, objectIndex2, ref vertexIndex, ref triangleIndex, data, ref requireSurfaceMaps);
                                                    }
                                                    IL_C08:;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        pos += dir1 * (2f + maxDot * 2f);
                    }
                }
                pos.y = this.m_position.y + (float)this.m_heightOffset * 0.015625f;
                if ((info.m_netLayers & 1 << layer) != 0 && info.m_requireSegmentRenderers) {
                    for (int m = 0; m < 8; m++) {
                        ushort segment3 = this.GetSegment(m);
                        if (segment3 != 0) {
                            NetInfo info3 = segment3.ToSegment().Info;
                            if (info3.m_nodes != null && info3.m_nodes.Length != 0) {
                                flag = true;
                                float vscale3 = info3.m_netAI.GetVScale();
                                Vector3 zero13 = Vector3.zero;
                                Vector3 zero14 = Vector3.zero;
                                Vector3 zero15 = Vector3.zero;
                                Vector3 zero16 = Vector3.zero;
                                Vector3 vector20 = Vector3.zero;
                                Vector3 vector21 = Vector3.zero;
                                Vector3 a3 = Vector3.zero;
                                Vector3 a4 = Vector3.zero;
                                Vector3 zero17 = Vector3.zero;
                                Vector3 zero18 = Vector3.zero;
                                Vector3 zero19 = Vector3.zero;
                                Vector3 zero20 = Vector3.zero;
                                NetSegment netSegment = segment3.ToSegment();
                                ItemClass connectionClass3 = info3.GetConnectionClass();
                                Vector3 dir = netSegment.GetDirection(nodeID);
                                float num6 = -4f;
                                float num7 = -4f;
                                ushort num8 = 0;
                                ushort num9 = 0;
                                for (int n = 0; n < 8; n++) {
                                    ushort segment4 = this.GetSegment(n);
                                    if (segment4 != 0 && segment4 != segment3) {
                                        NetInfo info4 = segment4.ToSegment().Info;
                                        ItemClass connectionClass4 = info4.GetConnectionClass();
                                        if (connectionClass3.m_service == connectionClass4.m_service) {
                                            NetSegment netSegment2 = segment4.ToSegment();
                                            Vector3 dir2 = netSegment2.GetDirection(nodeID);
                                            float num10 = dir.x * dir2.x + dir.z * dir2.z;
                                            if (dir2.z * dir.x - dir2.x * dir.z < 0f) {
                                                if (num10 > num6) {
                                                    num6 = num10;
                                                    num8 = segment4;
                                                }
                                                num10 = -2f - num10;
                                                if (num10 > num7) {
                                                    num7 = num10;
                                                    num9 = segment4;
                                                }
                                            } else {
                                                if (num10 > num7) {
                                                    num7 = num10;
                                                    num9 = segment4;
                                                }
                                                num10 = -2f - num10;
                                                if (num10 > num6) {
                                                    num6 = num10;
                                                    num8 = segment4;
                                                }
                                            }
                                        }
                                    }
                                }
                                bool start3 = netSegment.m_startNode == nodeID;
                                bool flag10;
                                netSegment.CalculateCorner(segment3, true, start3, false, out zero13, out zero15, out flag10);
                                netSegment.CalculateCorner(segment3, true, start3, true, out zero14, out zero16, out flag10);
                                Matrix4x4 leftMatrix3;
                                Matrix4x4 rightMatrix3;
                                Matrix4x4 leftMatrixB;
                                Matrix4x4 rightMatrixB;
                                Vector4 meshScale3;
                                Vector4 centerPos;
                                Vector4 sideScale;
                                if (num8 != 0 && num9 != 0) {
                                    float num11 = info3.m_pavementWidth / info3.m_halfWidth * 0.5f;
                                    float y = 1f;
                                    if (num8 != 0) {
                                        NetSegment netSegment3 = num8.ToSegment();
                                        NetInfo info6 = netSegment3.Info;
                                        start3 = (netSegment3.m_startNode == nodeID);
                                        netSegment3.CalculateCorner(num8, true, start3, true, out vector20, out a3, out flag10);
                                        netSegment3.CalculateCorner(num8, true, start3, false, out vector21, out a4, out flag10);
                                        float num12 = info6.m_pavementWidth / info6.m_halfWidth * 0.5f;
                                        num11 = (num11 + num12) * 0.5f;
                                        y = 2f * info3.m_halfWidth / (info3.m_halfWidth + info6.m_halfWidth);
                                    }
                                    float num13 = info3.m_pavementWidth / info3.m_halfWidth * 0.5f;
                                    float w = 1f;
                                    if (num9 != 0) {
                                        NetSegment netSegment4 = num9.ToSegment();
                                        NetInfo info7 = netSegment4.Info;
                                        start3 = (netSegment4.m_startNode == nodeID);
                                        netSegment4.CalculateCorner(num9, true, start3, true, out zero17, out zero19, out flag10);
                                        netSegment4.CalculateCorner(num9, true, start3, false, out zero18, out zero20, out flag10);
                                        float num14 = info7.m_pavementWidth / info7.m_halfWidth * 0.5f;
                                        num13 = (num13 + num14) * 0.5f;
                                        w = 2f * info3.m_halfWidth / (info3.m_halfWidth + info7.m_halfWidth);
                                    }
                                    Vector3 vector24;
                                    Vector3 vector25;
                                    NetSegment.CalculateMiddlePoints(zero13, -zero15, vector20, -a3, true, true, out vector24, out vector25);
                                    Vector3 vector26;
                                    Vector3 vector27;
                                    NetSegment.CalculateMiddlePoints(zero14, -zero16, vector21, -a4, true, true, out vector26, out vector27);
                                    Vector3 vector28;
                                    Vector3 vector29;
                                    NetSegment.CalculateMiddlePoints(zero13, -zero15, zero17, -zero19, true, true, out vector28, out vector29);
                                    Vector3 vector30;
                                    Vector3 vector31;
                                    NetSegment.CalculateMiddlePoints(zero14, -zero16, zero18, -zero20, true, true, out vector30, out vector31);
                                    leftMatrix3 = NetSegment.CalculateControlMatrix(zero13, vector24, vector25, vector20, zero13, vector24, vector25, vector20, groupPosition, vscale3);
                                    rightMatrix3 = NetSegment.CalculateControlMatrix(zero14, vector26, vector27, vector21, zero14, vector26, vector27, vector21, groupPosition, vscale3);
                                    leftMatrixB = NetSegment.CalculateControlMatrix(zero13, vector28, vector29, zero17, zero13, vector28, vector29, zero17, groupPosition, vscale3);
                                    rightMatrixB = NetSegment.CalculateControlMatrix(zero14, vector30, vector31, zero18, zero14, vector30, vector31, zero18, groupPosition, vscale3);
                                    meshScale3 = new Vector4(0.5f / info3.m_halfWidth, 1f / info3.m_segmentLength, 0.5f - info3.m_pavementWidth / info3.m_halfWidth * 0.5f, info3.m_pavementWidth / info3.m_halfWidth * 0.5f);
                                    centerPos = pos - groupPosition;
                                    centerPos.w = (leftMatrix3.m33 + rightMatrix3.m33 + leftMatrixB.m33 + rightMatrixB.m33) * 0.25f;
                                    sideScale = new Vector4(num11, y, num13, w);
                                } else {
                                    pos.x = (zero13.x + zero14.x) * 0.5f;
                                    pos.z = (zero13.z + zero14.z) * 0.5f;
                                    vector20 = zero14;
                                    vector21 = zero13;
                                    a3 = zero16;
                                    a4 = zero15;
                                    float d = info.m_netAI.GetEndRadius() * 1.33333337f;
                                    Vector3 vector32 = zero13 - zero15 * d;
                                    Vector3 vector33 = vector20 - a3 * d;
                                    Vector3 vector34 = zero14 - zero16 * d;
                                    Vector3 vector35 = vector21 - a4 * d;
                                    Vector3 vector36 = zero13 + zero15 * d;
                                    Vector3 vector37 = vector20 + a3 * d;
                                    Vector3 vector38 = zero14 + zero16 * d;
                                    Vector3 vector39 = vector21 + a4 * d;
                                    leftMatrix3 = NetSegment.CalculateControlMatrix(zero13, vector32, vector33, vector20, zero13, vector32, vector33, vector20, groupPosition, vscale3);
                                    rightMatrix3 = NetSegment.CalculateControlMatrix(zero14, vector38, vector39, vector21, zero14, vector38, vector39, vector21, groupPosition, vscale3);
                                    leftMatrixB = NetSegment.CalculateControlMatrix(zero13, vector36, vector37, vector20, zero13, vector36, vector37, vector20, groupPosition, vscale3);
                                    rightMatrixB = NetSegment.CalculateControlMatrix(zero14, vector34, vector35, vector21, zero14, vector34, vector35, vector21, groupPosition, vscale3);
                                    leftMatrix3.SetRow(3, leftMatrix3.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
                                    rightMatrix3.SetRow(3, rightMatrix3.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
                                    leftMatrixB.SetRow(3, leftMatrixB.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
                                    rightMatrixB.SetRow(3, rightMatrixB.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
                                    meshScale3 = new Vector4(0.5f / info3.m_halfWidth, 1f / info3.m_segmentLength, 0.5f - info3.m_pavementWidth / info3.m_halfWidth * 0.5f, info3.m_pavementWidth / info3.m_halfWidth * 0.5f);
                                    centerPos = pos - groupPosition;
                                    centerPos.w = (leftMatrix3.m33 + rightMatrix3.m33 + leftMatrixB.m33 + rightMatrixB.m33) * 0.25f;
                                    sideScale = new Vector4(info3.m_pavementWidth / info3.m_halfWidth * 0.5f, 1f, info3.m_pavementWidth / info3.m_halfWidth * 0.5f, 1f);
                                }
                                Vector4 colorLocation3;
                                Vector4 vector40;
                                if (NetNode.BlendJunction(nodeID)) {
                                    colorLocation3 = RenderManager.GetColorLocation(86016u + (uint)nodeID);
                                    vector40 = colorLocation3;
                                } else {
                                    colorLocation3 = RenderManager.GetColorLocation((uint)(49152 + segment3));
                                    vector40 = RenderManager.GetColorLocation(86016u + (uint)nodeID);
                                }
                                Vector4 vector41 = new Vector4(colorLocation3.x, colorLocation3.y, vector40.x, vector40.y);
                                for (int num15 = 0; num15 < info3.m_nodes.Length; num15++) {
                                    NetInfo.Node node3 = info3.m_nodes[num15];
                                    if (node3.m_layer == layer && node3.CheckFlags(this.m_flags) && node3.m_combinedLod != null && !node3.m_directConnect) {
                                        Vector4 objectIndex3 = vector41;
                                        if (node3.m_requireWindSpeed) {
                                            objectIndex3.w = Singleton<WeatherManager>.instance.GetWindSpeed(this.m_position);
                                        }
                                        NetNode.PopulateGroupData(info3, node3, leftMatrix3, rightMatrix3, leftMatrixB, rightMatrixB, meshScale3, centerPos, sideScale, objectIndex3, ref vertexIndex, ref triangleIndex, data, ref requireSurfaceMaps);
                                    }
                                }
                            }
                        }
                    }
                }
            } else if ((info.m_netLayers & 1 << layer) != 0) {
                if ((this.m_flags & NetNode.Flags.End) != NetNode.Flags.None) {
                    if (info.m_nodes != null && info.m_nodes.Length != 0) {
                        flag = true;
                        float vScale = info.m_netAI.GetVScale() / 1.5f;
                        Vector3 zero21 = Vector3.zero;
                        Vector3 zero22 = Vector3.zero;
                        Vector3 vector42 = Vector3.zero;
                        Vector3 vector43 = Vector3.zero;
                        Vector3 zero23 = Vector3.zero;
                        Vector3 zero24 = Vector3.zero;
                        Vector3 a5 = Vector3.zero;
                        Vector3 a6 = Vector3.zero;
                        bool flag11 = false;
                        ushort num16 = 0;
                        for (int num17 = 0; num17 < 8; num17++) {
                            ushort segment5 = this.GetSegment(num17);
                            if (segment5 != 0) {
                                NetSegment netSegment5 = Singleton<NetManager>.instance.m_segments.m_buffer[segment5];
                                bool start4 = netSegment5.m_startNode == nodeID;
                                bool flag12;
                                netSegment5.CalculateCorner(segment5, true, start4, false, out zero21, out zero23, out flag12);
                                netSegment5.CalculateCorner(segment5, true, start4, true, out zero22, out zero24, out flag12);
                                if (flag11) {
                                    a5 = -zero23;
                                    a6 = -zero24;
                                    zero23.y = 0.25f;
                                    zero24.y = 0.25f;
                                    a5.y = -5f;
                                    a6.y = -5f;
                                    vector42 = zero21 - zero23 * 10f + a5 * 10f;
                                    vector43 = zero22 - zero24 * 10f + a6 * 10f;
                                } else {
                                    vector42 = zero22;
                                    vector43 = zero21;
                                    a5 = zero24;
                                    a6 = zero23;
                                }
                                num16 = segment5;
                            }
                        }
                        if (flag11) {
                            Vector3 vector44;
                            Vector3 vector45;
                            NetSegment.CalculateMiddlePoints(zero21, -zero23, vector42, -a5, true, true, out vector44, out vector45);
                            Vector3 vector46;
                            Vector3 vector47;
                            NetSegment.CalculateMiddlePoints(zero22, -zero24, vector43, -a6, true, true, out vector46, out vector47);
                            Matrix4x4 leftMatrix4 = NetSegment.CalculateControlMatrix(zero21, vector44, vector45, vector42, zero22, vector46, vector47, vector43, groupPosition, vScale);
                            Matrix4x4 rightMatrix4 = NetSegment.CalculateControlMatrix(zero22, vector46, vector47, vector43, zero21, vector44, vector45, vector42, groupPosition, vScale);
                            Vector4 meshScale4 = new Vector4(0.5f / info.m_halfWidth, 1f / info.m_segmentLength, 1f, 1f);
                            Vector4 colorLocation4 = RenderManager.GetColorLocation(86016u + (uint)nodeID);
                            Vector4 vector48 = new Vector4(colorLocation4.x, colorLocation4.y, colorLocation4.x, colorLocation4.y);
                            if (info.m_segments != null && info.m_segments.Length != 0) {
                                for (int num18 = 0; num18 < info.m_segments.Length; num18++) {
                                    NetInfo.Segment segment6 = info.m_segments[num18];
                                    bool flag13;
                                    if (segment6.m_layer == layer && segment6.CheckFlags(NetSegment.Flags.Bend | (Singleton<NetManager>.instance.m_segments.m_buffer[num16].m_flags & NetSegment.Flags.Collapsed), out flag13) && segment6.m_combinedLod != null) {
                                        Vector4 objectIndex4 = vector48;
                                        if (segment6.m_requireWindSpeed) {
                                            objectIndex4.w = Singleton<WeatherManager>.instance.GetWindSpeed(this.m_position);
                                        }
                                        NetSegment.PopulateGroupData(info, segment6, leftMatrix4, rightMatrix4, meshScale4, objectIndex4, ref vertexIndex, ref triangleIndex, groupPosition, data, ref requireSurfaceMaps);
                                    }
                                }
                            }
                        } else {
                            float d2 = info.m_netAI.GetEndRadius() * 1.33333337f;
                            Vector3 vector49 = zero21 - zero23 * d2;
                            Vector3 vector50 = vector42 - a5 * d2;
                            Vector3 vector51 = zero22 - zero24 * d2;
                            Vector3 vector52 = vector43 - a6 * d2;
                            Vector3 vector53 = zero21 + zero23 * d2;
                            Vector3 vector54 = vector42 + a5 * d2;
                            Vector3 vector55 = zero22 + zero24 * d2;
                            Vector3 vector56 = vector43 + a6 * d2;
                            Matrix4x4 leftMatrix5 = NetSegment.CalculateControlMatrix(zero21, vector49, vector50, vector42, zero21, vector49, vector50, vector42, groupPosition, vScale);
                            Matrix4x4 rightMatrix5 = NetSegment.CalculateControlMatrix(zero22, vector55, vector56, vector43, zero22, vector55, vector56, vector43, groupPosition, vScale);
                            Matrix4x4 leftMatrixB2 = NetSegment.CalculateControlMatrix(zero21, vector53, vector54, vector42, zero21, vector53, vector54, vector42, groupPosition, vScale);
                            Matrix4x4 rightMatrixB2 = NetSegment.CalculateControlMatrix(zero22, vector51, vector52, vector43, zero22, vector51, vector52, vector43, groupPosition, vScale);
                            leftMatrix5.SetRow(3, leftMatrix5.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
                            rightMatrix5.SetRow(3, rightMatrix5.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
                            leftMatrixB2.SetRow(3, leftMatrixB2.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
                            rightMatrixB2.SetRow(3, rightMatrixB2.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
                            Vector4 meshScale5 = new Vector4(0.5f / info.m_halfWidth, 1f / info.m_segmentLength, 0.5f - info.m_pavementWidth / info.m_halfWidth * 0.5f, info.m_pavementWidth / info.m_halfWidth * 0.5f);
                            Vector4 centerPos2 = new Vector4(this.m_position.x - groupPosition.x, this.m_position.y - groupPosition.y + (float)this.m_heightOffset * 0.015625f, this.m_position.z - groupPosition.z, 0f);
                            centerPos2.w = (leftMatrix5.m33 + rightMatrix5.m33 + leftMatrixB2.m33 + rightMatrixB2.m33) * 0.25f;
                            Vector4 sideScale2 = new Vector4(info.m_pavementWidth / info.m_halfWidth * 0.5f, 1f, info.m_pavementWidth / info.m_halfWidth * 0.5f, 1f);
                            Vector4 colorLocation5 = RenderManager.GetColorLocation((uint)(49152 + num16));
                            Vector4 vector57 = new Vector4(colorLocation5.x, colorLocation5.y, colorLocation5.x, colorLocation5.y);
                            for (int num19 = 0; num19 < info.m_nodes.Length; num19++) {
                                NetInfo.Node node4 = info.m_nodes[num19];
                                if (node4.m_layer == layer && node4.CheckFlags(this.m_flags) && node4.m_combinedLod != null && !node4.m_directConnect) {
                                    Vector4 objectIndex5 = vector57;
                                    if (node4.m_requireWindSpeed) {
                                        objectIndex5.w = Singleton<WeatherManager>.instance.GetWindSpeed(this.m_position);
                                    }
                                    NetNode.PopulateGroupData(info, node4, leftMatrix5, rightMatrix5, leftMatrixB2, rightMatrixB2, meshScale5, centerPos2, sideScale2, objectIndex5, ref vertexIndex, ref triangleIndex, data, ref requireSurfaceMaps);
                                }
                            }
                        }
                    }
                } else if ((this.m_flags & NetNode.Flags.Bend) != NetNode.Flags.None && ((info.m_segments != null && info.m_segments.Length != 0) || (info.m_nodes != null && info.m_nodes.Length != 0))) {
                    float vscale4 = info.m_netAI.GetVScale();
                    Vector3 zero25 = Vector3.zero;
                    Vector3 zero26 = Vector3.zero;
                    Vector3 zero27 = Vector3.zero;
                    Vector3 zero28 = Vector3.zero;
                    Vector3 zero29 = Vector3.zero;
                    Vector3 zero30 = Vector3.zero;
                    Vector3 zero31 = Vector3.zero;
                    Vector3 zero32 = Vector3.zero;
                    ushort num20 = 0;
                    ushort num21 = 0;
                    bool flag14 = false;
                    int num22 = 0;
                    for (int num23 = 0; num23 < 8; num23++) {
                        ushort segment7 = this.GetSegment(num23);
                        if (segment7 != 0) {
                            NetSegment netSegment6 = Singleton<NetManager>.instance.m_segments.m_buffer[segment7];
                            bool flag15 = ++num22 == 1;
                            bool flag16 = netSegment6.m_startNode == nodeID;
                            if ((!flag15 && !flag14) || (flag15 && !flag16)) {
                                bool flag17;
                                netSegment6.CalculateCorner(segment7, true, flag16, false, out zero25, out zero29, out flag17);
                                netSegment6.CalculateCorner(segment7, true, flag16, true, out zero26, out zero30, out flag17);
                                flag14 = true;
                                num20 = segment7;
                            } else {
                                bool flag17;
                                netSegment6.CalculateCorner(segment7, true, flag16, true, out zero27, out zero31, out flag17);
                                netSegment6.CalculateCorner(segment7, true, flag16, false, out zero28, out zero32, out flag17);
                                num21 = segment7;
                            }
                        }
                    }
                    Vector3 vector58;
                    Vector3 vector59;
                    NetSegment.CalculateMiddlePoints(zero25, -zero29, zero27, -zero31, true, true, out vector58, out vector59);
                    Vector3 vector60;
                    Vector3 vector61;
                    NetSegment.CalculateMiddlePoints(zero26, -zero30, zero28, -zero32, true, true, out vector60, out vector61);
                    Matrix4x4 leftMatrix6 = NetSegment.CalculateControlMatrix(zero25, vector58, vector59, zero27, zero26, vector60, vector61, zero28, groupPosition, vscale4);
                    Matrix4x4 rightMatrix6 = NetSegment.CalculateControlMatrix(zero26, vector60, vector61, zero28, zero25, vector58, vector59, zero27, groupPosition, vscale4);
                    Vector4 vector62 = new Vector4(0.5f / info.m_halfWidth, 1f / info.m_segmentLength, 1f, 1f);
                    Vector4 colorLocation6 = RenderManager.GetColorLocation(86016u + (uint)nodeID);
                    Vector4 vector63 = new Vector4(colorLocation6.x, colorLocation6.y, colorLocation6.x, colorLocation6.y);
                    if (info.m_segments != null && info.m_segments.Length != 0) {
                        for (int num24 = 0; num24 < info.m_segments.Length; num24++) {
                            NetInfo.Segment segment8 = info.m_segments[num24];
                            bool flag18;
                            if (segment8.m_layer == layer && segment8.CheckFlags(info.m_netAI.GetBendFlags(nodeID, ref this), out flag18) && segment8.m_combinedLod != null && !segment8.m_disableBendNodes) {
                                Vector4 objectIndex6 = vector63;
                                Vector4 meshScale6 = vector62;
                                if (segment8.m_requireWindSpeed) {
                                    objectIndex6.w = Singleton<WeatherManager>.instance.GetWindSpeed(this.m_position);
                                }
                                if (flag18) {
                                    meshScale6.x = -meshScale6.x;
                                    meshScale6.y = -meshScale6.y;
                                }
                                flag = true;
                                NetSegment.PopulateGroupData(info, segment8, leftMatrix6, rightMatrix6, meshScale6, objectIndex6, ref vertexIndex, ref triangleIndex, groupPosition, data, ref requireSurfaceMaps);
                            }
                        }
                    }
                    if (info.m_nodes != null && info.m_nodes.Length != 0) {
                        for (int num25 = 0; num25 < info.m_nodes.Length; num25++) {
                            NetInfo.Node node5 = info.m_nodes[num25];
                            if ((node5.m_connectGroup == NetInfo.ConnectGroup.None || (node5.m_connectGroup & info.m_connectGroup & NetInfo.ConnectGroup.AllGroups) != NetInfo.ConnectGroup.None) && node5.m_layer == layer && node5.CheckFlags(this.m_flags) && node5.m_combinedLod != null && node5.m_directConnect) {
                                Vector4 objectIndex7 = vector63;
                                Vector4 meshScale7 = vector62;
                                if (node5.m_requireWindSpeed) {
                                    objectIndex7.w = Singleton<WeatherManager>.instance.GetWindSpeed(this.m_position);
                                }
                                if ((node5.m_connectGroup & NetInfo.ConnectGroup.Oneway) != NetInfo.ConnectGroup.None) {
                                    NetManager instance2 = Singleton<NetManager>.instance;
                                    bool flag19 = instance2.m_segments.m_buffer[num20].m_startNode == nodeID == ((instance2.m_segments.m_buffer[num20].m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                    bool flag20 = instance2.m_segments.m_buffer[num21].m_startNode == nodeID == ((instance2.m_segments.m_buffer[num21].m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                    if (flag19 == flag20) {
                                        goto IL_21EC;
                                    }
                                    if (flag19) {
                                        if ((node5.m_connectGroup & NetInfo.ConnectGroup.OnewayStart) == NetInfo.ConnectGroup.None) {
                                            goto IL_21EC;
                                        }
                                    } else {
                                        if ((node5.m_connectGroup & NetInfo.ConnectGroup.OnewayEnd) == NetInfo.ConnectGroup.None) {
                                            goto IL_21EC;
                                        }
                                        meshScale7.x = -meshScale7.x;
                                        meshScale7.y = -meshScale7.y;
                                    }
                                }
                                flag = true;
                                NetNode.PopulateGroupData(info, node5, leftMatrix6, rightMatrix6, meshScale7, objectIndex7, ref vertexIndex, ref triangleIndex, data, ref requireSurfaceMaps);
                            }
                            IL_21EC:;
                        }
                    }
                }
            }
            if (flag) {
                min = Vector3.Min(min, this.m_bounds.min);
                max = Vector3.Max(max, this.m_bounds.max);
                maxRenderDistance = Mathf.Max(maxRenderDistance, 30000f);
                maxInstanceDistance = Mathf.Max(maxInstanceDistance, 1000f);
            }
        }

        public bool CalculateGroupData(ushort nodeID, int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays) {
            bool result = false;
            NetInfo info = this.Info;
            if (this.m_problems != Notification.Problem.None && layer == Singleton<NotificationManager>.instance.m_notificationLayer && (this.m_flags & NetNode.Flags.Temporary) == NetNode.Flags.None && Notification.CalculateGroupData(ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays)) {
                result = true;
            }
            if ((this.m_flags & NetNode.Flags.Junction) != NetNode.Flags.None) {
                NetManager instance = Singleton<NetManager>.instance;
                Vector3 a = this.m_position;
                for (int i1 = 0; i1 < 8; i1++) {
                    ushort segmentID1 = this.GetSegment(i1);
                    if (segmentID1 != 0) {
                        NetInfo info1 = instance.m_segments.m_buffer[segmentID1].Info;
                        ItemClass connectionClass = info1.GetConnectionClass();
                        Vector3 dir1 = segmentID1.ToSegment().GetDirection(nodeID);
                        float maxDot = -1f;
                        for (int i2 = 0; i2 < 8; i2++) {
                            ushort segmentID2 = this.GetSegment(i2);
                            if (segmentID2 != 0 && segmentID2 != segmentID1) {
                                NetInfo info2 = instance.m_segments.m_buffer[segmentID2].Info;
                                ItemClass connectionClass2 = info2.GetConnectionClass();
                                if (((info.m_netLayers | info1.m_netLayers | info2.m_netLayers) & 1 << layer) != 0 && (connectionClass.m_service == connectionClass2.m_service || (info1.m_nodeConnectGroups & info2.m_connectGroup) != NetInfo.ConnectGroup.None || (info2.m_nodeConnectGroups & info1.m_connectGroup) != NetInfo.ConnectGroup.None)) {
                                    Vector3 dir2 = segmentID2.ToSegment().GetDirection(nodeID);
                                    float dot = dir1.x * dir2.x + dir1.z * dir2.z;
                                    maxDot = Mathf.Max(maxDot, dot);
                                    bool connects1 = info1.m_requireDirectRenderers && (info1.m_nodeConnectGroups == NetInfo.ConnectGroup.None || (info1.m_nodeConnectGroups & info2.m_connectGroup) != NetInfo.ConnectGroup.None);
                                    bool connects2 = info2.m_requireDirectRenderers && (info2.m_nodeConnectGroups == NetInfo.ConnectGroup.None || (info2.m_nodeConnectGroups & info1.m_connectGroup) != NetInfo.ConnectGroup.None);
                                    if (i2 > i1 && (connects1 || connects2)) {
                                        float maxTurnAngleCos = Mathf.Min(info1.m_maxTurnAngleCos, info2.m_maxTurnAngleCos);
                                        if (dot < 0.01f - maxTurnAngleCos) {
                                            float prio1;
                                            if (connects1) {
                                                prio1 = info1.m_netAI.GetNodeInfoPriority(segmentID1, ref instance.m_segments.m_buffer[segmentID1]);
                                            } else {
                                                prio1 = -1E+08f;
                                            }
                                            float prio2;
                                            if (connects2) {
                                                prio2 = info2.m_netAI.GetNodeInfoPriority(segmentID2, ref instance.m_segments.m_buffer[segmentID2]);
                                            } else {
                                                prio2 = -1E+08f;
                                            }
                                            if (prio1 >= prio2) {
                                                if (info1.m_nodes != null && info1.m_nodes.Length != 0) {
                                                    result = true;
                                                    for (int k = 0; k < info1.m_nodes.Length; k++) {
                                                        NetInfo.Node node1 = info1.m_nodes[k];
                                                        if ((node1.m_connectGroup == NetInfo.ConnectGroup.None || (node1.m_connectGroup & info2.m_connectGroup & NetInfo.ConnectGroup.AllGroups) != NetInfo.ConnectGroup.None) &&
                                                            node1.m_layer == layer && node1.CheckFlags(this.m_flags) && node1.m_combinedLod != null && node1.m_directConnect) {
                                                            if ((node1.m_connectGroup & NetInfo.ConnectGroup.Oneway) != NetInfo.ConnectGroup.None) {
                                                                bool flag3 = instance.m_segments.m_buffer[segmentID1].m_startNode == nodeID == ((instance.m_segments.m_buffer[segmentID1].m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                                                if (info2.m_hasBackwardVehicleLanes != info2.m_hasForwardVehicleLanes || (node1.m_connectGroup & NetInfo.ConnectGroup.Directional) != NetInfo.ConnectGroup.None) {
                                                                    bool flag4 = instance.m_segments.m_buffer[segmentID2].m_startNode == nodeID == ((instance.m_segments.m_buffer[segmentID2].m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                                                    if (flag3 == flag4) {
                                                                        goto IL_4C3;
                                                                    }
                                                                }
                                                                if (flag3) {
                                                                    if ((node1.m_connectGroup & NetInfo.ConnectGroup.OnewayStart) == NetInfo.ConnectGroup.None) {
                                                                        goto IL_4C3;
                                                                    }
                                                                } else if ((node1.m_connectGroup & NetInfo.ConnectGroup.OnewayEnd) == NetInfo.ConnectGroup.None) {
                                                                    goto IL_4C3;
                                                                }
                                                            }
                                                            NetNode.CalculateGroupData(node1, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
                                                        }
                                                        IL_4C3:;
                                                    }
                                                }
                                            } else if (info2.m_nodes != null && info2.m_nodes.Length != 0) {
                                                result = true;
                                                for (int l = 0; l < info2.m_nodes.Length; l++) {
                                                    NetInfo.Node node2 = info2.m_nodes[l];
                                                    if ((node2.m_connectGroup == NetInfo.ConnectGroup.None || (node2.m_connectGroup & info1.m_connectGroup & NetInfo.ConnectGroup.AllGroups) != NetInfo.ConnectGroup.None) &&
                                                        node2.m_layer == layer && node2.CheckFlags(this.m_flags) && node2.m_combinedLod != null && node2.m_directConnect) {
                                                        if ((node2.m_connectGroup & NetInfo.ConnectGroup.Oneway) != NetInfo.ConnectGroup.None) {
                                                            bool flag5 = instance.m_segments.m_buffer[segmentID2].m_startNode == nodeID == ((instance.m_segments.m_buffer[segmentID2].m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                                            if (info1.m_hasBackwardVehicleLanes != info1.m_hasForwardVehicleLanes || (node2.m_connectGroup & NetInfo.ConnectGroup.Directional) != NetInfo.ConnectGroup.None) {
                                                                bool flag6 = instance.m_segments.m_buffer[segmentID1].m_startNode == nodeID == ((instance.m_segments.m_buffer[segmentID1].m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                                                if (flag6 == flag5) {
                                                                    goto IL_66E;
                                                                }
                                                            }
                                                            if (flag5) {
                                                                if ((node2.m_connectGroup & NetInfo.ConnectGroup.OnewayStart) == NetInfo.ConnectGroup.None) {
                                                                    goto IL_66E;
                                                                }
                                                            } else if ((node2.m_connectGroup & NetInfo.ConnectGroup.OnewayEnd) == NetInfo.ConnectGroup.None) {
                                                                goto IL_66E;
                                                            }
                                                        }
                                                        NetNode.CalculateGroupData(node2, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
                                                    }
                                                    IL_66E:;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        a += dir1 * (2f + maxDot * 2f);
                    }
                }
                a.y = this.m_position.y + (float)this.m_heightOffset * 0.015625f;
                if ((info.m_netLayers & 1 << layer) != 0 && info.m_requireSegmentRenderers) {
                    for (int i3 = 0; i3 < 8; i3++) {
                        ushort segmentID3 = this.GetSegment(i3);
                        if (segmentID3 != 0) {
                            NetInfo info3 = instance.m_segments.m_buffer[segmentID3].Info;
                            if (info3.m_nodes != null && info3.m_nodes.Length != 0) {
                                result = true;
                                for (int n = 0; n < info3.m_nodes.Length; n++) {
                                    NetInfo.Node node3 = info3.m_nodes[n];
                                    if (node3.m_layer == layer && node3.CheckFlags(this.m_flags) && node3.m_combinedLod != null && !node3.m_directConnect) {
                                        NetNode.CalculateGroupData(node3, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
                                    }
                                }
                            }
                        }
                    }
                }
            } else if ((info.m_netLayers & 1 << layer) != 0) {
                if ((this.m_flags & NetNode.Flags.End) != NetNode.Flags.None) {
                    if (info.m_nodes != null && info.m_nodes.Length != 0) {
                        result = true;
                        for (int num6 = 0; num6 < info.m_nodes.Length; num6++) {
                            NetInfo.Node node = info.m_nodes[num6];
                            if (node.m_layer == layer && node.CheckFlags(this.m_flags) && node.m_combinedLod != null && !node.m_directConnect) {
                                NetNode.CalculateGroupData(node, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
                            }
                        }
                    }
                } else if ((this.m_flags & NetNode.Flags.Bend) != NetNode.Flags.None) {
                    if (info.m_segments != null && info.m_segments.Length != 0) {
                        result = true;
                        for (int num7 = 0; num7 < info.m_segments.Length; num7++) {
                            NetInfo.Segment segment4 = info.m_segments[num7];
                            bool flag7;
                            if (segment4.m_layer == layer && segment4.CheckFlags(info.m_netAI.GetBendFlags(nodeID, ref this), out flag7) && segment4.m_combinedLod != null && !segment4.m_disableBendNodes) {
                                NetSegment.CalculateGroupData(segment4, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
                            }
                        }
                    }
                    if (info.m_nodes != null && info.m_nodes.Length != 0) {
                        result = true;
                        for (int num8 = 0; num8 < info.m_nodes.Length; num8++) {
                            NetInfo.Node node5 = info.m_nodes[num8];
                            if ((node5.m_connectGroup == NetInfo.ConnectGroup.None || (node5.m_connectGroup & info.m_connectGroup & NetInfo.ConnectGroup.AllGroups) != NetInfo.ConnectGroup.None) && node5.m_layer == layer
                                && node5.CheckFlags(this.m_flags) && node5.m_combinedLod != null && node5.m_directConnect) {
                                if ((node5.m_connectGroup & NetInfo.ConnectGroup.Oneway) != NetInfo.ConnectGroup.None) {
                                    NetManager instance2 = Singleton<NetManager>.instance;
                                    ushort segmentID1 = 0;
                                    ushort segmentID2 = 0;
                                    bool primarySegmentDetected = false;
                                    int counter = 0;
                                    for (int segmentIndex = 0; segmentIndex < 8; segmentIndex++) {
                                        ushort segmentID = this.GetSegment(segmentIndex);
                                        if (segmentID != 0) {
                                            bool firstLoop = ++counter == 1;
                                            bool startNode = segmentID.ToSegment().IsStartNode(nodeID);
                                            if ((!firstLoop && !primarySegmentDetected) || (firstLoop && !startNode)) {
                                                primarySegmentDetected = true;
                                                segmentID1 = segmentID;
                                            } else {
                                                segmentID2 = segmentID;
                                            }
                                        }
                                    }
                                    bool reverse1 = instance2.m_segments.m_buffer[segmentID1].m_startNode == nodeID == ((instance2.m_segments.m_buffer[segmentID1].m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                    bool reverse2 = instance2.m_segments.m_buffer[segmentID2].m_startNode == nodeID == ((instance2.m_segments.m_buffer[segmentID2].m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                    if (reverse1 == reverse2)
                                        goto continue;
                                    if (reverse1) {
                                        if ((node5.m_connectGroup & NetInfo.ConnectGroup.OnewayStart) == NetInfo.ConnectGroup.None) {
                                            goto continue;
                                        }
                                    } else if ((node5.m_connectGroup & NetInfo.ConnectGroup.OnewayEnd) == NetInfo.ConnectGroup.None) {
                                        goto continue;
                                    }
                                }
                                NetNode.CalculateGroupData(node5, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
                            }
                            IL_B19:;
                        }
                    }
                }
            }
            return result;
        }

        private int CalculateRendererCount(NetInfo info) {
            if((this.m_flags & NetNode.Flags.Junction) != NetNode.Flags.None) {
                int ret = (int)this.m_connectCount;
                if(info.m_requireSegmentRenderers) {
                    ret += this.CountSegments();
                }
                return ret;
            }
            return 1;
        }

        public void RenderInstance(RenderManager.CameraInfo cameraInfo, ushort nodeID, int layerMask) {
            if(this.m_flags == NetNode.Flags.None) {
                return;
            }
            NetInfo info = this.Info;
            if(!cameraInfo.Intersect(this.m_bounds)) {
                return;
            }
            if(this.m_problems != Notification.Problem.None && (layerMask & 1 << Singleton<NotificationManager>.instance.m_notificationLayer) != 0 && (this.m_flags & NetNode.Flags.Temporary) == NetNode.Flags.None) {
                Vector3 position = this.m_position;
                position.y += Mathf.Max(5f, info.m_maxHeight);
                Notification.RenderInstance(cameraInfo, this.m_problems, position, 1f);
            }
            if((layerMask & info.m_netLayers) == 0) {
                return;
            }
            if((this.m_flags & (NetNode.Flags.End | NetNode.Flags.Bend | NetNode.Flags.Junction)) == NetNode.Flags.None) {
                return;
            }
            if((this.m_flags & NetNode.Flags.Bend) != NetNode.Flags.None) {
                if(info.m_segments == null || info.m_segments.Length == 0) {
                    return;
                }
            } else if(info.m_nodes == null || info.m_nodes.Length == 0) {
                return;
            }
            uint count = (uint)this.CalculateRendererCount(info);
            RenderManager renderManger = Singleton<RenderManager>.instance;
            if(renderManger.RequireInstance(NODE_HOLDER + nodeID, count, out uint renderIndex)) {
                int iter = 0;
                while(renderIndex != INVALID_RENDER_INDEX) {
                    this.RenderInstance(cameraInfo, nodeID, info, iter, this.m_flags, ref renderIndex, ref renderManger.m_instances[renderIndex]);
                    if(++iter > 36) {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                        break;
                    }
                }
            }
            info.m_netAI.RenderNode(nodeID, ref this, cameraInfo);
        }

        private void RenderInstance(RenderManager.CameraInfo cameraInfo, ushort nodeID, NetInfo info, int iter, NetNode.Flags flags, ref uint instanceIndex, ref RenderManager.Instance data) {
            if (data.m_dirty) {
                data.m_dirty = false;
                if (iter == 0) {
                    if ((flags & NetNode.Flags.Junction) != NetNode.Flags.None) {
                        this.RefreshJunctionData(nodeID, info, instanceIndex);
                    } else if ((flags & NetNode.Flags.Bend) != NetNode.Flags.None) {
                        this.RefreshBendData(nodeID, info, instanceIndex, ref data);
                    } else if ((flags & NetNode.Flags.End) != NetNode.Flags.None) {
                        this.RefreshEndData(nodeID, info, instanceIndex, ref data);
                    }
                }
            }
            if (data.m_initialized) {
                if ((flags & NetNode.Flags.Junction) != NetNode.Flags.None) {
                    if ((data.m_dataInt0 & 8) != 0) {
                        ushort segmentID1 = this.GetSegment(data.m_dataInt0 & 7);
                        ushort segmentID2 = this.GetSegment(data.m_dataInt0 >> 4);
                        if (segmentID1 != 0 && segmentID2 != 0) {
                            NetManager netMan = Singleton<NetManager>.instance;
                            info = segmentID1.ToSegment().Info;
                            NetInfo info2 = segmentID2.ToSegment().Info;
                            NetNode.Flags flags2 = flags;
                            if (((segmentID1.ToSegment().m_flags | segmentID2.ToSegment().m_flags) & NetSegment.Flags.Collapsed) != NetSegment.Flags.None) {
                                flags2 |= NetNode.Flags.Collapsed;
                            }
                            for (int i = 0; i < info.m_nodes.Length; i++) {
                                NetInfo.Node node = info.m_nodes[i];
                                if (node.CheckFlags(flags2) && node.m_directConnect && (node.m_connectGroup == NetInfo.ConnectGroup.None || (node.m_connectGroup & info2.m_connectGroup & NetInfo.ConnectGroup.AllGroups) != NetInfo.ConnectGroup.None)) {
                                    Vector4 dataVector = data.m_dataVector3;
                                    Vector4 dataVector2 = data.m_dataVector0;
                                    if (node.m_requireWindSpeed) {
                                        dataVector.w = data.m_dataFloat0;
                                    }
                                    if ((node.m_connectGroup & NetInfo.ConnectGroup.Oneway) != NetInfo.ConnectGroup.None) {
                                        bool flag = segmentID1.ToSegment().m_startNode == nodeID == ((segmentID1.ToSegment().m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                        if (info2.m_hasBackwardVehicleLanes != info2.m_hasForwardVehicleLanes || (node.m_connectGroup & NetInfo.ConnectGroup.Directional) != NetInfo.ConnectGroup.None) {
                                            bool flag2 = segmentID2.ToSegment().m_startNode == nodeID == ((segmentID2.ToSegment().m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                            if (flag == flag2) {
                                                goto IL_570;
                                            }
                                        }
                                        if (flag) {
                                            if ((node.m_connectGroup & NetInfo.ConnectGroup.OnewayStart) == NetInfo.ConnectGroup.None) {
                                                goto IL_570;
                                            }
                                        } else {
                                            if ((node.m_connectGroup & NetInfo.ConnectGroup.OnewayEnd) == NetInfo.ConnectGroup.None) {
                                                goto IL_570;
                                            }
                                            dataVector2.x = -dataVector2.x;
                                            dataVector2.y = -dataVector2.y;
                                        }
                                    }
                                    if (cameraInfo.CheckRenderDistance(data.m_position, node.m_lodRenderDistance)) {
                                        netMan.m_materialBlock.Clear();
                                        netMan.m_materialBlock.SetMatrix(netMan.ID_LeftMatrix, data.m_dataMatrix0);
                                        netMan.m_materialBlock.SetMatrix(netMan.ID_RightMatrix, data.m_extraData.m_dataMatrix2);
                                        netMan.m_materialBlock.SetVector(netMan.ID_MeshScale, dataVector2);
                                        netMan.m_materialBlock.SetVector(netMan.ID_ObjectIndex, dataVector);
                                        netMan.m_materialBlock.SetColor(netMan.ID_Color, data.m_dataColor0);
                                        if (node.m_requireSurfaceMaps && data.m_dataTexture1 != null) {
                                            netMan.m_materialBlock.SetTexture(netMan.ID_SurfaceTexA, data.m_dataTexture0);
                                            netMan.m_materialBlock.SetTexture(netMan.ID_SurfaceTexB, data.m_dataTexture1);
                                            netMan.m_materialBlock.SetVector(netMan.ID_SurfaceMapping, data.m_dataVector1);
                                        }
                                        NetManager netManager = netMan;
                                        netManager.m_drawCallData.m_defaultCalls = netManager.m_drawCallData.m_defaultCalls + 1;
                                        Graphics.DrawMesh(node.m_nodeMesh, data.m_position, data.m_rotation, node.m_nodeMaterial, node.m_layer, null, 0, netMan.m_materialBlock);
                                    } else {
                                        NetInfo.LodValue combinedLod = node.m_combinedLod;
                                        if (combinedLod != null) {
                                            if (node.m_requireSurfaceMaps && data.m_dataTexture0 != combinedLod.m_surfaceTexA) {
                                                if (combinedLod.m_lodCount != 0) {
                                                    NetSegment.RenderLod(cameraInfo, combinedLod);
                                                }
                                                combinedLod.m_surfaceTexA = data.m_dataTexture0;
                                                combinedLod.m_surfaceTexB = data.m_dataTexture1;
                                                combinedLod.m_surfaceMapping = data.m_dataVector1;
                                            }
                                            combinedLod.m_leftMatrices[combinedLod.m_lodCount] = data.m_dataMatrix0;
                                            combinedLod.m_rightMatrices[combinedLod.m_lodCount] = data.m_extraData.m_dataMatrix2;
                                            combinedLod.m_meshScales[combinedLod.m_lodCount] = dataVector2;
                                            combinedLod.m_objectIndices[combinedLod.m_lodCount] = dataVector;
                                            combinedLod.m_meshLocations[combinedLod.m_lodCount] = data.m_position;
                                            combinedLod.m_lodMin = Vector3.Min(combinedLod.m_lodMin, data.m_position);
                                            combinedLod.m_lodMax = Vector3.Max(combinedLod.m_lodMax, data.m_position);
                                            if (++combinedLod.m_lodCount == combinedLod.m_leftMatrices.Length) {
                                                NetSegment.RenderLod(cameraInfo, combinedLod);
                                            }
                                        }
                                    }
                                }
                                IL_570:;
                            }
                        }
                    } else {
                        ushort segmentID = this.GetSegment(data.m_dataInt0 & 7);
                        if (segmentID != 0) {
                            NetManager netMan = Singleton<NetManager>.instance;
                            info = segmentID.ToSegment().Info;
                            for (int j = 0; j < info.m_nodes.Length; j++) {
                                NetInfo.Node node = info.m_nodes[j];
                                if (node.CheckFlags(flags) && !node.m_directConnect) {
                                    Vector4 extraDataVector4 = data.m_extraData.m_dataVector4;
                                    if (node.m_requireWindSpeed) {
                                        extraDataVector4.w = data.m_dataFloat0;
                                    }
                                    if (cameraInfo.CheckRenderDistance(data.m_position, node.m_lodRenderDistance)) {
                                        netMan.m_materialBlock.Clear();
                                        netMan.m_materialBlock.SetMatrix(netMan.ID_LeftMatrix, data.m_dataMatrix0);
                                        netMan.m_materialBlock.SetMatrix(netMan.ID_RightMatrix, data.m_extraData.m_dataMatrix2);
                                        netMan.m_materialBlock.SetMatrix(netMan.ID_LeftMatrixB, data.m_extraData.m_dataMatrix3);
                                        netMan.m_materialBlock.SetMatrix(netMan.ID_RightMatrixB, data.m_dataMatrix1);
                                        netMan.m_materialBlock.SetVector(netMan.ID_MeshScale, data.m_dataVector0);
                                        netMan.m_materialBlock.SetVector(netMan.ID_CenterPos, data.m_dataVector1);
                                        netMan.m_materialBlock.SetVector(netMan.ID_SideScale, data.m_dataVector2);
                                        netMan.m_materialBlock.SetVector(netMan.ID_ObjectIndex, extraDataVector4);
                                        netMan.m_materialBlock.SetColor(netMan.ID_Color, data.m_dataColor0);
                                        if (node.m_requireSurfaceMaps && data.m_dataTexture1 != null) {
                                            netMan.m_materialBlock.SetTexture(netMan.ID_SurfaceTexA, data.m_dataTexture0);
                                            netMan.m_materialBlock.SetTexture(netMan.ID_SurfaceTexB, data.m_dataTexture1);
                                            netMan.m_materialBlock.SetVector(netMan.ID_SurfaceMapping, data.m_dataVector3);
                                        }
                                        NetManager netManager2 = netMan;
                                        netManager2.m_drawCallData.m_defaultCalls = netManager2.m_drawCallData.m_defaultCalls + 1;
                                        Graphics.DrawMesh(node.m_nodeMesh, data.m_position, data.m_rotation, node.m_nodeMaterial, node.m_layer, null, 0, netMan.m_materialBlock);
                                    } else {
                                        NetInfo.LodValue combinedLod2 = node.m_combinedLod;
                                        if (combinedLod2 != null) {
                                            if (node.m_requireSurfaceMaps && data.m_dataTexture0 != combinedLod2.m_surfaceTexA) {
                                                if (combinedLod2.m_lodCount != 0) {
                                                    NetNode.RenderLod(cameraInfo, combinedLod2);
                                                }
                                                combinedLod2.m_surfaceTexA = data.m_dataTexture0;
                                                combinedLod2.m_surfaceTexB = data.m_dataTexture1;
                                                combinedLod2.m_surfaceMapping = data.m_dataVector3;
                                            }
                                            combinedLod2.m_leftMatrices[combinedLod2.m_lodCount] = data.m_dataMatrix0;
                                            combinedLod2.m_leftMatricesB[combinedLod2.m_lodCount] = data.m_extraData.m_dataMatrix3;
                                            combinedLod2.m_rightMatrices[combinedLod2.m_lodCount] = data.m_extraData.m_dataMatrix2;
                                            combinedLod2.m_rightMatricesB[combinedLod2.m_lodCount] = data.m_dataMatrix1;
                                            combinedLod2.m_meshScales[combinedLod2.m_lodCount] = data.m_dataVector0;
                                            combinedLod2.m_centerPositions[combinedLod2.m_lodCount] = data.m_dataVector1;
                                            combinedLod2.m_sideScales[combinedLod2.m_lodCount] = data.m_dataVector2;
                                            combinedLod2.m_objectIndices[combinedLod2.m_lodCount] = extraDataVector4;
                                            combinedLod2.m_meshLocations[combinedLod2.m_lodCount] = data.m_position;
                                            combinedLod2.m_lodMin = Vector3.Min(combinedLod2.m_lodMin, data.m_position);
                                            combinedLod2.m_lodMax = Vector3.Max(combinedLod2.m_lodMax, data.m_position);
                                            if (++combinedLod2.m_lodCount == combinedLod2.m_leftMatrices.Length) {
                                                NetNode.RenderLod(cameraInfo, combinedLod2);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                } else if ((flags & NetNode.Flags.End) != NetNode.Flags.None) {
                    NetManager netMan = Singleton<NetManager>.instance;
                    for (int k = 0; k < info.m_nodes.Length; k++) {
                        NetInfo.Node node3 = info.m_nodes[k];
                        if (node3.CheckFlags(flags) && !node3.m_directConnect) {
                            Vector4 dataVector4 = data.m_extraData.m_dataVector4;
                            if (node3.m_requireWindSpeed) {
                                dataVector4.w = data.m_dataFloat0;
                            }
                            if (cameraInfo.CheckRenderDistance(data.m_position, node3.m_lodRenderDistance)) {
                                netMan.m_materialBlock.Clear();
                                netMan.m_materialBlock.SetMatrix(netMan.ID_LeftMatrix, data.m_dataMatrix0);
                                netMan.m_materialBlock.SetMatrix(netMan.ID_RightMatrix, data.m_extraData.m_dataMatrix2);
                                netMan.m_materialBlock.SetMatrix(netMan.ID_LeftMatrixB, data.m_extraData.m_dataMatrix3);
                                netMan.m_materialBlock.SetMatrix(netMan.ID_RightMatrixB, data.m_dataMatrix1);
                                netMan.m_materialBlock.SetVector(netMan.ID_MeshScale, data.m_dataVector0);
                                netMan.m_materialBlock.SetVector(netMan.ID_CenterPos, data.m_dataVector1);
                                netMan.m_materialBlock.SetVector(netMan.ID_SideScale, data.m_dataVector2);
                                netMan.m_materialBlock.SetVector(netMan.ID_ObjectIndex, dataVector4);
                                netMan.m_materialBlock.SetColor(netMan.ID_Color, data.m_dataColor0);
                                if (node3.m_requireSurfaceMaps && data.m_dataTexture1 != null) {
                                    netMan.m_materialBlock.SetTexture(netMan.ID_SurfaceTexA, data.m_dataTexture0);
                                    netMan.m_materialBlock.SetTexture(netMan.ID_SurfaceTexB, data.m_dataTexture1);
                                    netMan.m_materialBlock.SetVector(netMan.ID_SurfaceMapping, data.m_dataVector3);
                                }
                                NetManager netManager3 = netMan;
                                netManager3.m_drawCallData.m_defaultCalls = netManager3.m_drawCallData.m_defaultCalls + 1;
                                Graphics.DrawMesh(node3.m_nodeMesh, data.m_position, data.m_rotation, node3.m_nodeMaterial, node3.m_layer, null, 0, netMan.m_materialBlock);
                            } else {
                                NetInfo.LodValue combinedLod3 = node3.m_combinedLod;
                                if (combinedLod3 != null) {
                                    if (node3.m_requireSurfaceMaps && data.m_dataTexture0 != combinedLod3.m_surfaceTexA) {
                                        if (combinedLod3.m_lodCount != 0) {
                                            NetNode.RenderLod(cameraInfo, combinedLod3);
                                        }
                                        combinedLod3.m_surfaceTexA = data.m_dataTexture0;
                                        combinedLod3.m_surfaceTexB = data.m_dataTexture1;
                                        combinedLod3.m_surfaceMapping = data.m_dataVector3;
                                    }
                                    combinedLod3.m_leftMatrices[combinedLod3.m_lodCount] = data.m_dataMatrix0;
                                    combinedLod3.m_leftMatricesB[combinedLod3.m_lodCount] = data.m_extraData.m_dataMatrix3;
                                    combinedLod3.m_rightMatrices[combinedLod3.m_lodCount] = data.m_extraData.m_dataMatrix2;
                                    combinedLod3.m_rightMatricesB[combinedLod3.m_lodCount] = data.m_dataMatrix1;
                                    combinedLod3.m_meshScales[combinedLod3.m_lodCount] = data.m_dataVector0;
                                    combinedLod3.m_centerPositions[combinedLod3.m_lodCount] = data.m_dataVector1;
                                    combinedLod3.m_sideScales[combinedLod3.m_lodCount] = data.m_dataVector2;
                                    combinedLod3.m_objectIndices[combinedLod3.m_lodCount] = dataVector4;
                                    combinedLod3.m_meshLocations[combinedLod3.m_lodCount] = data.m_position;
                                    combinedLod3.m_lodMin = Vector3.Min(combinedLod3.m_lodMin, data.m_position);
                                    combinedLod3.m_lodMax = Vector3.Max(combinedLod3.m_lodMax, data.m_position);
                                    if (++combinedLod3.m_lodCount == combinedLod3.m_leftMatrices.Length) {
                                        NetNode.RenderLod(cameraInfo, combinedLod3);
                                    }
                                }
                            }
                        }
                    }
                } else if ((flags & NetNode.Flags.Bend) != NetNode.Flags.None) {
                    NetManager instance4 = Singleton<NetManager>.instance;
                    for (int l = 0; l < info.m_segments.Length; l++) {
                        NetInfo.Segment segment4 = info.m_segments[l];
                        bool flag3;
                        if (segment4.CheckFlags(info.m_netAI.GetBendFlags(nodeID, ref this), out flag3) && !segment4.m_disableBendNodes) {
                            Vector4 dataVector5 = data.m_dataVector3;
                            Vector4 dataVector6 = data.m_dataVector0;
                            if (segment4.m_requireWindSpeed) {
                                dataVector5.w = data.m_dataFloat0;
                            }
                            if (flag3) {
                                dataVector6.x = -dataVector6.x;
                                dataVector6.y = -dataVector6.y;
                            }
                            if (cameraInfo.CheckRenderDistance(data.m_position, segment4.m_lodRenderDistance)) {
                                instance4.m_materialBlock.Clear();
                                instance4.m_materialBlock.SetMatrix(instance4.ID_LeftMatrix, data.m_dataMatrix0);
                                instance4.m_materialBlock.SetMatrix(instance4.ID_RightMatrix, data.m_extraData.m_dataMatrix2);
                                instance4.m_materialBlock.SetVector(instance4.ID_MeshScale, dataVector6);
                                instance4.m_materialBlock.SetVector(instance4.ID_ObjectIndex, dataVector5);
                                instance4.m_materialBlock.SetColor(instance4.ID_Color, data.m_dataColor0);
                                if (segment4.m_requireSurfaceMaps && data.m_dataTexture1 != null) {
                                    instance4.m_materialBlock.SetTexture(instance4.ID_SurfaceTexA, data.m_dataTexture0);
                                    instance4.m_materialBlock.SetTexture(instance4.ID_SurfaceTexB, data.m_dataTexture1);
                                    instance4.m_materialBlock.SetVector(instance4.ID_SurfaceMapping, data.m_dataVector1);
                                }
                                NetManager netManager4 = instance4;
                                netManager4.m_drawCallData.m_defaultCalls = netManager4.m_drawCallData.m_defaultCalls + 1;
                                Graphics.DrawMesh(segment4.m_segmentMesh, data.m_position, data.m_rotation, segment4.m_segmentMaterial, segment4.m_layer, null, 0, instance4.m_materialBlock);
                            } else {
                                NetInfo.LodValue combinedLod4 = segment4.m_combinedLod;
                                if (combinedLod4 != null) {
                                    if (segment4.m_requireSurfaceMaps && data.m_dataTexture0 != combinedLod4.m_surfaceTexA) {
                                        if (combinedLod4.m_lodCount != 0) {
                                            NetSegment.RenderLod(cameraInfo, combinedLod4);
                                        }
                                        combinedLod4.m_surfaceTexA = data.m_dataTexture0;
                                        combinedLod4.m_surfaceTexB = data.m_dataTexture1;
                                        combinedLod4.m_surfaceMapping = data.m_dataVector1;
                                    }
                                    combinedLod4.m_leftMatrices[combinedLod4.m_lodCount] = data.m_dataMatrix0;
                                    combinedLod4.m_rightMatrices[combinedLod4.m_lodCount] = data.m_extraData.m_dataMatrix2;
                                    combinedLod4.m_meshScales[combinedLod4.m_lodCount] = dataVector6;
                                    combinedLod4.m_objectIndices[combinedLod4.m_lodCount] = dataVector5;
                                    combinedLod4.m_meshLocations[combinedLod4.m_lodCount] = data.m_position;
                                    combinedLod4.m_lodMin = Vector3.Min(combinedLod4.m_lodMin, data.m_position);
                                    combinedLod4.m_lodMax = Vector3.Max(combinedLod4.m_lodMax, data.m_position);
                                    if (++combinedLod4.m_lodCount == combinedLod4.m_leftMatrices.Length) {
                                        NetSegment.RenderLod(cameraInfo, combinedLod4);
                                    }
                                }
                            }
                        }
                    }
                    for (int m = 0; m < info.m_nodes.Length; m++) {
                        ushort segment5 = this.GetSegment(data.m_dataInt0 & 7);
                        ushort segment6 = this.GetSegment(data.m_dataInt0 >> 4);
                        if (((instance4.m_segments.m_buffer[segment5].m_flags | instance4.m_segments.m_buffer[segment6].m_flags) & NetSegment.Flags.Collapsed) != NetSegment.Flags.None) {
                            NetNode.Flags flags3 = flags | NetNode.Flags.Collapsed;
                        }
                        NetInfo.Node node4 = info.m_nodes[m];
                        if (node4.CheckFlags(flags) && node4.m_directConnect && (node4.m_connectGroup == NetInfo.ConnectGroup.None || (node4.m_connectGroup & info.m_connectGroup & NetInfo.ConnectGroup.AllGroups) != NetInfo.ConnectGroup.None)) {
                            Vector4 dataVector7 = data.m_dataVector3;
                            Vector4 dataVector8 = data.m_dataVector0;
                            if (node4.m_requireWindSpeed) {
                                dataVector7.w = data.m_dataFloat0;
                            }
                            if ((node4.m_connectGroup & NetInfo.ConnectGroup.Oneway) != NetInfo.ConnectGroup.None) {
                                bool flag4 = instance4.m_segments.m_buffer[segment5].m_startNode == nodeID == ((instance4.m_segments.m_buffer[segment5].m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                bool flag5 = instance4.m_segments.m_buffer[segment6].m_startNode == nodeID == ((instance4.m_segments.m_buffer[segment6].m_flags & NetSegment.Flags.Invert) == NetSegment.Flags.None);
                                if (flag4 == flag5) {
                                    goto IL_1637;
                                }
                                if (flag4) {
                                    if ((node4.m_connectGroup & NetInfo.ConnectGroup.OnewayStart) == NetInfo.ConnectGroup.None) {
                                        goto IL_1637;
                                    }
                                } else {
                                    if ((node4.m_connectGroup & NetInfo.ConnectGroup.OnewayEnd) == NetInfo.ConnectGroup.None) {
                                        goto IL_1637;
                                    }
                                    dataVector8.x = -dataVector8.x;
                                    dataVector8.y = -dataVector8.y;
                                }
                            }
                            if (cameraInfo.CheckRenderDistance(data.m_position, node4.m_lodRenderDistance)) {
                                instance4.m_materialBlock.Clear();
                                instance4.m_materialBlock.SetMatrix(instance4.ID_LeftMatrix, data.m_dataMatrix0);
                                instance4.m_materialBlock.SetMatrix(instance4.ID_RightMatrix, data.m_extraData.m_dataMatrix2);
                                instance4.m_materialBlock.SetVector(instance4.ID_MeshScale, dataVector8);
                                instance4.m_materialBlock.SetVector(instance4.ID_ObjectIndex, dataVector7);
                                instance4.m_materialBlock.SetColor(instance4.ID_Color, data.m_dataColor0);
                                if (node4.m_requireSurfaceMaps && data.m_dataTexture1 != null) {
                                    instance4.m_materialBlock.SetTexture(instance4.ID_SurfaceTexA, data.m_dataTexture0);
                                    instance4.m_materialBlock.SetTexture(instance4.ID_SurfaceTexB, data.m_dataTexture1);
                                    instance4.m_materialBlock.SetVector(instance4.ID_SurfaceMapping, data.m_dataVector1);
                                }
                                NetManager netManager5 = instance4;
                                netManager5.m_drawCallData.m_defaultCalls = netManager5.m_drawCallData.m_defaultCalls + 1;
                                Graphics.DrawMesh(node4.m_nodeMesh, data.m_position, data.m_rotation, node4.m_nodeMaterial, node4.m_layer, null, 0, instance4.m_materialBlock);
                            } else {
                                NetInfo.LodValue combinedLod5 = node4.m_combinedLod;
                                if (combinedLod5 != null) {
                                    if (node4.m_requireSurfaceMaps && data.m_dataTexture0 != combinedLod5.m_surfaceTexA) {
                                        if (combinedLod5.m_lodCount != 0) {
                                            NetSegment.RenderLod(cameraInfo, combinedLod5);
                                        }
                                        combinedLod5.m_surfaceTexA = data.m_dataTexture0;
                                        combinedLod5.m_surfaceTexB = data.m_dataTexture1;
                                        combinedLod5.m_surfaceMapping = data.m_dataVector1;
                                    }
                                    combinedLod5.m_leftMatrices[combinedLod5.m_lodCount] = data.m_dataMatrix0;
                                    combinedLod5.m_rightMatrices[combinedLod5.m_lodCount] = data.m_extraData.m_dataMatrix2;
                                    combinedLod5.m_meshScales[combinedLod5.m_lodCount] = dataVector8;
                                    combinedLod5.m_objectIndices[combinedLod5.m_lodCount] = dataVector7;
                                    combinedLod5.m_meshLocations[combinedLod5.m_lodCount] = data.m_position;
                                    combinedLod5.m_lodMin = Vector3.Min(combinedLod5.m_lodMin, data.m_position);
                                    combinedLod5.m_lodMax = Vector3.Max(combinedLod5.m_lodMax, data.m_position);
                                    if (++combinedLod5.m_lodCount == combinedLod5.m_leftMatrices.Length) {
                                        NetSegment.RenderLod(cameraInfo, combinedLod5);
                                    }
                                }
                            }
                        }
                        IL_1637:;
                    }
                }
            }
            instanceIndex = (uint)data.m_nextInstance;
        }

        public static void CalculateNode(ref NetNode This, ushort nodeID) {
            if (This.m_flags == NetNode.Flags.None) {
                return;
            }
            NetManager netMan = Singleton<NetManager>.instance;
            Vector3 DirFirst = Vector3.zero;
            int iSegment = 0;
            int ConnectCount = 0;
            bool hasSegments = false;
            bool canBeMiddle = false;
            bool bCompatibleButNodeMiddle = false;
            bool isAsymForward = false;
            bool isAsymBackward = false;
            bool needsJunctionFlag = false;
            bool hasCurvedSegment = false;
            bool hasStraightSegment = false;
            bool bCompatibleAndStart2End = false;
            bool allConnectedSegmentsAreFlat = true;
            bool CanModify = true;
            bool bHasDetailMapping = Singleton<TerrainManager>.instance.HasDetailMapping(This.m_position);
            NetInfo prevInfo = null;
            int prev_backwardVehicleLaneCount = 0;
            int prev_m_forwardVehicleLaneCount = 0;
            NetInfo infoNode = null;
            float num5 = -1E+07f;
            for (int i = 0; i < 8; i++) {
                ushort segmentID = This.GetSegment(i);
                if (segmentID != 0) {
                    NetInfo infoSegment = segmentID.ToSegment().Info;
                    float nodeInfoPriority = infoSegment.m_netAI.GetNodeInfoPriority(segmentID, ref segmentID.ToSegment());
                    if (nodeInfoPriority > num5) {
                        infoSegment = infoSegment;
                        num5 = nodeInfoPriority;
                    }
                }
            }
            if (infoNode == null) {
                infoNode = This.Info;
            }
            if (infoNode != This.Info) {
                This.Info = infoNode;
                Singleton<NetManager>.instance.UpdateNodeColors(nodeID);
                if (!infoNode.m_canDisable) {
                    This.m_flags &= ~NetNode.Flags.Disabled;
                }
            }
            bool bStartNodeFirst = false;
            for (int j = 0; j < 8; j++) {
                ushort segmentID = This.GetSegment(j);
                if (segmentID != 0) {
                    iSegment++;
                    ushort startNodeID = segmentID.ToSegment().m_startNode;
                    ushort endNodeID = segmentID.ToSegment().m_endNode;
                    Vector3 startDirection = segmentID.ToSegment().m_startDirection;
                    Vector3 endDirection = segmentID.ToSegment().m_endDirection;
                    bool bStartNode = nodeID == startNodeID;
                    Vector3 currentDir = (!bStartNode) ? endDirection : startDirection;
                    NetInfo infoSegment = segmentID.ToSegment().Info;
                    ItemClass connectionClass = infoSegment.GetConnectionClass();
                    if (!infoSegment.m_netAI.CanModify()) {
                        CanModify = false;
                    }
                    int backwardVehicleLaneCount; // lane count away from node
                    int forwardVehicleLaneCount; // lane count toward node
                    bool segmentInvert = segmentID.ToSegment().m_flags.IsFlagSet(NetSegment.Flags.Invert);
                    if (bStartNode == segmentInvert) // normal
                    {
                        backwardVehicleLaneCount = infoSegment.m_backwardVehicleLaneCount;
                        forwardVehicleLaneCount = infoSegment.m_forwardVehicleLaneCount;
                    } else // reverse
                      {
                        backwardVehicleLaneCount = infoSegment.m_forwardVehicleLaneCount;
                        forwardVehicleLaneCount = infoSegment.m_backwardVehicleLaneCount;
                    }
                    for (int k = j + 1; k < 8; k++) {
                        ushort segmentID2 = This.GetSegment(k);
                        if (segmentID2 != 0) {
                            NetInfo infoSegment2 = segmentID2.ToSegment().Info;
                            ItemClass connectionClass2 = infoSegment2.GetConnectionClass();
                            if (connectionClass2.m_service == connectionClass.m_service || (infoSegment2.m_nodeConnectGroups & infoSegment.m_connectGroup) != NetInfo.ConnectGroup.None || (infoSegment.m_nodeConnectGroups & infoSegment2.m_connectGroup) != NetInfo.ConnectGroup.None) {
                                bool bStartNode2 = nodeID == segmentID2.ToSegment().m_startNode;
                                Vector3 dir2 = segmentID2.ToSegment().GetDirection(nodeID);
                                float dot2 = currentDir.x * dir2.x + currentDir.z * dir2.z;
                                float turnThreshold = 0.01f - Mathf.Min(infoSegment.m_maxTurnAngleCos, infoSegment2.m_maxTurnAngleCos);
                                if (dot2 < turnThreshold) {
                                    if ((infoSegment.m_requireDirectRenderers && (infoSegment.m_nodeConnectGroups == NetInfo.ConnectGroup.None || (infoSegment.m_nodeConnectGroups & infoSegment2.m_connectGroup) != NetInfo.ConnectGroup.None)) || (infoSegment2.m_requireDirectRenderers && (infoSegment2.m_nodeConnectGroups == NetInfo.ConnectGroup.None || (infoSegment2.m_nodeConnectGroups & infoSegment.m_connectGroup) != NetInfo.ConnectGroup.None))) {
                                        ConnectCount++;
                                    }
                                } else {
                                    needsJunctionFlag = true;
                                }
                            } else {
                                needsJunctionFlag = true;
                            }
                        }
                    }

                    if (netMan.m_nodes.m_buffer[startNodeID].m_elevation != netMan.m_nodes.m_buffer[endNodeID].m_elevation)
                        allConnectedSegmentsAreFlat = false;

                    Vector3 startPos = netMan.m_nodes.m_buffer[startNodeID].m_position;
                    Vector3 endPos = netMan.m_nodes.m_buffer[endNodeID].m_position;
                    if (bStartNode)
                        bHasDetailMapping = (bHasDetailMapping && Singleton<TerrainManager>.instance.HasDetailMapping(endPos));
                    else
                        bHasDetailMapping = (bHasDetailMapping && Singleton<TerrainManager>.instance.HasDetailMapping(startPos));

                    if (NetSegment.IsStraight(startPos, startDirection, endPos, endDirection)) {
                        hasStraightSegment = true;
                    } else {
                        hasCurvedSegment = true;
                    }

                    if (iSegment == 1) {
                        bStartNodeFirst = bStartNode;
                        DirFirst = currentDir;
                        hasSegments = true;
                    } else if (iSegment == 2 && infoSegment.IsCombatible(prevInfo) && infoSegment.IsCombatible(infoNode) && (backwardVehicleLaneCount != 0) == (prev_m_forwardVehicleLaneCount != 0) && (forwardVehicleLaneCount != 0) == (prev_backwardVehicleLaneCount != 0)) {
                        float dot = DirFirst.x * currentDir.x + DirFirst.z * currentDir.z;
                        if (backwardVehicleLaneCount != prev_m_forwardVehicleLaneCount || forwardVehicleLaneCount != prev_backwardVehicleLaneCount) {
                            if (backwardVehicleLaneCount > forwardVehicleLaneCount) // second segment is backward
                            {
                                // if second segment is backward then first segment must be forward
                                isAsymForward = true;
                            } else {
                                isAsymBackward = true;
                            }
                            bCompatibleButNodeMiddle = true;
                        } else if (dot < -0.999f) // straight.
                          {
                            canBeMiddle = true;
                        } else {
                            bCompatibleButNodeMiddle = true;
                        }
                        bCompatibleAndStart2End = (bStartNode != bStartNodeFirst);
                    } else {
                        needsJunctionFlag = true;
                    }
                    prevInfo = infoSegment;
                    prev_backwardVehicleLaneCount = backwardVehicleLaneCount;
                    prev_m_forwardVehicleLaneCount = forwardVehicleLaneCount;
                }
            }
            if (!infoNode.m_enableMiddleNodes && canBeMiddle) {
                bCompatibleButNodeMiddle = true;
            }
            if (!infoNode.m_enableBendingNodes && bCompatibleButNodeMiddle) {
                needsJunctionFlag = true;
            }
            if (infoNode.m_requireContinuous && (This.m_flags & NetNode.Flags.Untouchable) != NetNode.Flags.None) {
                needsJunctionFlag = true;
            }
            if (infoNode.m_requireContinuous && !bCompatibleAndStart2End && (canBeMiddle || bCompatibleButNodeMiddle)) {
                needsJunctionFlag = true;
            }
            NetNode.Flags flags = This.m_flags & ~(NetNode.Flags.End | NetNode.Flags.Middle | NetNode.Flags.Bend | NetNode.Flags.Junction | NetNode.Flags.Moveable | NetNode.Flags.AsymForward | NetNode.Flags.AsymBackward);
            if ((flags & NetNode.Flags.Outside) != NetNode.Flags.None) {
                This.m_flags = flags;
            } else if (needsJunctionFlag) {
                This.m_flags = (flags | NetNode.Flags.Junction);
            } else if (bCompatibleButNodeMiddle) {
                if (isAsymForward) {
                    flags |= NetNode.Flags.AsymForward;
                }
                if (isAsymBackward) {
                    flags |= NetNode.Flags.AsymBackward;
                }
                This.m_flags = (flags | NetNode.Flags.Bend);
            } else if (canBeMiddle) {
                if ((!hasCurvedSegment || !hasStraightSegment) && (This.m_flags & (NetNode.Flags.Untouchable | NetNode.Flags.Double)) == NetNode.Flags.None && allConnectedSegmentsAreFlat && CanModify) {
                    flags |= NetNode.Flags.Moveable;
                }
                This.m_flags = (flags | NetNode.Flags.Middle);
            } else if (hasSegments) {
                if ((This.m_flags & NetNode.Flags.Untouchable) == NetNode.Flags.None && allConnectedSegmentsAreFlat && CanModify && infoNode.m_enableMiddleNodes) {
                    flags |= NetNode.Flags.Moveable;
                }
                This.m_flags = (flags | NetNode.Flags.End);
            } else {
                This.m_flags = flags;
            }
            This.m_heightOffset = (byte)((!bHasDetailMapping && infoNode.m_requireSurfaceMaps) ? 64 : 0);
            This.m_connectCount = (byte)ConnectCount;
            BuildingInfo newBuilding;
            float heightOffset;
            infoNode.m_netAI.GetNodeBuilding(nodeID, ref This, out newBuilding, out heightOffset);
            This.UpdateBuilding(nodeID, newBuilding, heightOffset);
        }

        public static bool BlendJunction(ushort nodeID) {
            NetManager netManager = Singleton<NetManager>.instance;
            if ((netManager.m_nodes.m_buffer[nodeID].m_flags & (NetNode.Flags.Middle | NetNode.Flags.Bend)) != NetNode.Flags.None) {
                return true;
            }
            if ((netManager.m_nodes.m_buffer[nodeID].m_flags & NetNode.Flags.Junction) != NetNode.Flags.None) {
                bool bHasForward_Prev = false;
                bool bHasBackward_Prev = false;
                int segmentCount = 0;
                for (int i = 0; i < 8; i++) {
                    ushort segmentID = nodeID.ToNode().GetSegment(i);
                    if (segmentID != 0) {
                        if (++segmentCount >= 3) {
                            return false;
                        }
                        NetInfo info_segment = segmentID.ToSegment().Info;
                        if (!info_segment.m_enableMiddleNodes || info_segment.m_requireContinuous) {
                            return false;
                        }
                        bool bHasForward;
                        bool bHasBackward;
                        bool bStartNode = segmentID.ToSegment().m_startNode == nodeID;
                        bool bInvert = !segmentID.ToSegment().m_flags.IsFlagSet(NetSegment.Flags.Invert);
                        if (bStartNode == bInvert) {
                            bHasForward = info_segment.m_hasForwardVehicleLanes;
                            bHasBackward = info_segment.m_hasBackwardVehicleLanes;
                        } else {
                            bHasForward = info_segment.m_hasBackwardVehicleLanes;
                            bHasBackward = info_segment.m_hasForwardVehicleLanes;
                        }
                        if (segmentCount == 2) {
                            if (bHasForward != bHasBackward_Prev || bHasBackward != bHasForward_Prev) {
                                return false;
                            }
                        } else {
                            bHasForward_Prev = bHasForward;
                            bHasBackward_Prev = bHasBackward;
                        }
                    }
                }
                return segmentCount == 2;
            }
            return false;
        }

        private void RefreshJunctionData(ref NetNode This, ushort nodeID, NetInfo info, uint instanceIndex) {
            NetManager instance = Singleton<NetManager>.instance;
            Vector3 position = m_position;
            for(int i = 0; i < 8; i++) {
                ushort segmentID1 = GetSegment(i);
                if(segmentID1 == 0) 
                    continue;
                NetInfo netInfo1 = segmentID1.ToSegment().Info;
                ItemClass connectionClass = netInfo1.GetConnectionClass();
                Vector3 dir1 = segmentID1.ToSegment().GetDirection(nodeID);
                float maxDotProduct = -1f;
                for(int j = 0; j < 8; j++) {
                    ushort segmentID2 = GetSegment(j);
                    if(segmentID2 == 0 || segmentID2 == segmentID1) {
                        continue;
                    }
                    NetInfo netInfo2 = segmentID2.ToSegment().Info;
                    ItemClass connectionClass2 = netInfo2.GetConnectionClass();
                    if(connectionClass.m_service != connectionClass2.m_service && (netInfo1.m_nodeConnectGroups & netInfo2.m_connectGroup) == 0 && (netInfo2.m_nodeConnectGroups & netInfo1.m_connectGroup) == 0) {
                        continue;
                    }
                    Vector3 vector2 = segmentID2.ToSegment().GetDirection(nodeID);
                    float dotProduct = dir1.x * vector2.x + dir1.z * vector2.z;
                    if(connectionClass.m_service == connectionClass2.m_service) {
                        maxDotProduct = Mathf.Max(maxDotProduct, dotProduct);
                    }
                    bool connects1 = netInfo1.m_requireDirectRenderers && (netInfo1.m_nodeConnectGroups == NetInfo.ConnectGroup.None || (netInfo1.m_nodeConnectGroups & netInfo2.m_connectGroup) != 0);
                    bool connects2 = netInfo2.m_requireDirectRenderers && (netInfo2.m_nodeConnectGroups == NetInfo.ConnectGroup.None || (netInfo2.m_nodeConnectGroups & netInfo1.m_connectGroup) != 0);
                    if(j > i && (connects1 || connects2) && dotProduct < 0.01f - Mathf.Min(netInfo1.m_maxTurnAngleCos, netInfo2.m_maxTurnAngleCos) && instanceIndex != INVALID_RENDER_INDEX) {
                        float prio1 = !connects1 ? -1E+08f : netInfo1.m_netAI.GetNodeInfoPriority(segmentID1, ref segmentID1.ToSegment());
                        float prio2 = !connects2 ? -1E+08f : netInfo2.m_netAI.GetNodeInfoPriority(segmentID2, ref segmentID2.ToSegment());
                        if(prio1 >= prio2) {
                            RefreshJunctionData(nodeID, i, j, netInfo1, netInfo2, segmentID1, segmentID2, ref instanceIndex, ref RenderManager.instance.m_instances[instanceIndex]);
                        } else {
                            RefreshJunctionData(nodeID, j, i, netInfo2, netInfo1, segmentID2, segmentID1, ref instanceIndex, ref RenderManager.instance.m_instances[instanceIndex]);
                        }
                    }
                }
                if(netInfo1.m_requireSegmentRenderers) {
                    position += dir1 * Mathf.Max(2f + maxDotProduct * 2f, netInfo1.m_minCornerOffset * 0.4f);
                }
            }
            position.y = m_position.y + (float)(int)m_heightOffset * 0.015625f;
            if(!info.m_requireSegmentRenderers) {
                return;
            }
            for(int k = 0; k < 8; k++) {
                ushort segment3 = GetSegment(k);
                if(segment3 != 0 && instanceIndex != 65535) {
                    RefreshJunctionData(ref This, nodeID, k, segment3, position, ref instanceIndex, ref RenderManager.instance.m_instances[instanceIndex]);
                }
            }
        }


        /// not-DC node
        /// <param name="centerPos">position between left corner and right corner of segmentID (or something like that).</param>
        private static void RefreshJunctionData(this ref NetNode This, ushort nodeID, int segmentIndex, ushort SegmentID, Vector3 centerPos, ref uint instanceIndex, ref RenderManager.Instance data) {
            Vector3 cornerPos_right = Vector3.zero, cornerDir_right = Vector3.zero, cornerPos_left = Vector3.zero, cornerDir_left = Vector3.zero,
                cornerPosA_right = Vector3.zero, cornerDirA_right = Vector3.zero, cornerPosA_left = Vector3.zero, cornerDirA_left = Vector3.zero,
                cornerPosB_right = Vector3.zero, cornerDirB_right = Vector3.zero, cornerPosB_left = Vector3.zero, cornerDirB_left = Vector3.zero;

            NetManager instance = Singleton<NetManager>.instance;
            data.m_position = This.m_position;
            data.m_rotation = Quaternion.identity;
            data.m_initialized = true;
            NetSegment segment = SegmentID.ToSegment();
            NetInfo info = segment.Info;
            float vscale = info.m_netAI.GetVScale();
            ItemClass connectionClass = info.GetConnectionClass();
            bool bStartNode = nodeID == segment.m_startNode;
            Vector3 dir = segment.GetDirection(nodeID);

            float dot_A = -4f;
            float dot_B = -4f;
            ushort segmentID_A = 0;
            ushort segmentID_B = 0;
            for (int i = 0; i < 8; i++) {
                ushort segmentID2 = This.GetSegment(i);
                if (segmentID2 != 0 && segmentID2 != SegmentID) {
                    NetInfo info2 = instance.m_segments.m_buffer[segmentID2].Info;
                    ItemClass connectionClass2 = info2.GetConnectionClass();
                    if (connectionClass.m_service == connectionClass2.m_service) {
                        NetSegment segment2 = segmentID2.ToSegment();
                        bool bStartNode2 = nodeID != segment2.m_startNode;
                        Vector3 dir2 = segment2.GetDirection(nodeID);
                        float dot = dir.x * dir2.x + dir.z * dir2.z;
                        float determinent = dir2.z * dir.x - dir2.x * dir.z;
                        bool bRight = determinent > 0;
                        bool bWide = dot < 0;
                        // 180 -> det=0 dot=-1
                        if (!bRight) {
                            if (dot > dot_A) // most acute
                            {
                                dot_A = dot;
                                segmentID_A = segmentID2;
                            }
                            dot = -2f - dot;
                            if (dot > dot_B) // widest
                            {
                                dot_B = dot;
                                segmentID_B = segmentID2;
                            }
                        } else {
                            if (dot > dot_B) // most acute
                            {
                                dot_B = dot;
                                segmentID_B = segmentID2;
                            }
                            dot = -2f - dot;
                            if (dot > dot_A) // widest
                            {
                                dot_A = dot;
                                segmentID_A = segmentID2;
                            }
                        }
                    }
                }
            }
            segment.CalculateCorner(SegmentID, true, bStartNode, false, out cornerPos_right, out cornerDir_right, out _);
            segment.CalculateCorner(SegmentID, true, bStartNode, true, out cornerPos_left, out cornerDir_left, out _);
            if (segmentID_A != 0 && segmentID_B != 0) {
                float pavementRatio_avgA = info.m_pavementWidth / info.m_halfWidth * 0.5f;
                float widthRatioA = 1f;//redundant
                                       //if (segmentID_A != 0) // redundant
                {
                    NetSegment segment_A = instance.m_segments.m_buffer[segmentID_A];
                    NetInfo infoA = segment_A.Info;
                    bStartNode = (segment_A.m_startNode == nodeID);
                    segment_A.CalculateCorner(segmentID_A, true, bStartNode, true, out cornerPosA_right, out cornerDirA_right, out _);
                    segment_A.CalculateCorner(segmentID_A, true, bStartNode, false, out cornerPosA_left, out cornerDirA_left, out _);
                    float pavementRatioA = infoA.m_pavementWidth / infoA.m_halfWidth * 0.5f;
                    pavementRatio_avgA = (pavementRatio_avgA + pavementRatioA) * 0.5f;
                    widthRatioA = 2f * info.m_halfWidth / (info.m_halfWidth + infoA.m_halfWidth);
                }

                float pavementRatio_avgB = info.m_pavementWidth / info.m_halfWidth * 0.5f;
                float widthRatioB = 1f; //redundant

                //if (segmentID_B != 0) redundant
                {
                    NetSegment segment_B = instance.m_segments.m_buffer[segmentID_B];
                    NetInfo infoB = segment_B.Info;
                    bStartNode = (segment_B.m_startNode == nodeID);
                    segment_B.CalculateCorner(segmentID_B, true, bStartNode, true, out cornerPosB_right, out cornerDirB_right, out _);
                    segment_B.CalculateCorner(segmentID_B, true, bStartNode, false, out cornerPosB_left, out cornerDirB_left, out _);
                    float pavementRatioB = infoB.m_pavementWidth / infoB.m_halfWidth * 0.5f;
                    pavementRatio_avgB = (pavementRatio_avgB + pavementRatioB) * 0.5f;
                    widthRatioB = 2f * info.m_halfWidth / (info.m_halfWidth + infoB.m_halfWidth);
                }

                NetSegment.CalculateMiddlePoints(cornerPos_right, -cornerDir_right, cornerPosA_right, -cornerDirA_right, true, true, out var bpointA_right, out var cpointA_right); // a right
                NetSegment.CalculateMiddlePoints(cornerPos_left, -cornerDir_left, cornerPosA_left, -cornerDirA_left, true, true, out var bpoint_Aleft, out var cpoint_Aleft); // a left
                NetSegment.CalculateMiddlePoints(cornerPos_right, -cornerDir_right, cornerPosB_right, -cornerDirB_right, true, true, out var bpoint_Bright, out var cpoint_Bright); // b right
                NetSegment.CalculateMiddlePoints(cornerPos_left, -cornerDir_left, cornerPosB_left, -cornerDirB_left, true, true, out var bpoint_Bleft, out var cpoint_Bleft); // b left

                data.m_dataMatrix0 = NetSegment.CalculateControlMatrix(cornerPos_right, bpointA_right, cpointA_right, cornerPosA_right, cornerPos_right, bpointA_right, cpointA_right, cornerPosA_right, This.m_position, vscale); // left matrix
                data.m_extraData.m_dataMatrix2 = NetSegment.CalculateControlMatrix(cornerPos_left, bpoint_Aleft, cpoint_Aleft, cornerPosA_left, cornerPos_left, bpoint_Aleft, cpoint_Aleft, cornerPosA_left, This.m_position, vscale);// right matrix
                data.m_extraData.m_dataMatrix3 = NetSegment.CalculateControlMatrix(cornerPos_right, bpoint_Bright, cpoint_Bright, cornerPosB_right, cornerPos_right, bpoint_Bright, cpoint_Bright, cornerPosB_right, This.m_position, vscale); // left matrixB
                data.m_dataMatrix1 = NetSegment.CalculateControlMatrix(cornerPos_left, bpoint_Bleft, cpoint_Bleft, cornerPosB_left, cornerPos_left, bpoint_Bleft, cpoint_Bleft, cornerPosB_left, This.m_position, vscale); // right matrix B

                // Vector4(1/width | 1/length | 0.5 - pavement/width | pavement/width )
                data.m_dataVector0 = new Vector4(0.5f / info.m_halfWidth, 1f / info.m_segmentLength, 0.5f - info.m_pavementWidth / info.m_halfWidth * 0.5f, info.m_pavementWidth / info.m_halfWidth * 0.5f); // mesh scale
                data.m_dataVector1 = centerPos - data.m_position; // center pos
                data.m_dataVector1.w = (data.m_dataMatrix0.m31 + data.m_dataMatrix0.m32 + data.m_extraData.m_dataMatrix2.m31 + data.m_extraData.m_dataMatrix2.m32 + data.m_extraData.m_dataMatrix3.m31 + data.m_extraData.m_dataMatrix3.m32 + data.m_dataMatrix1.m31 + data.m_dataMatrix1.m32) * 0.125f;
                data.m_dataVector2 = new Vector4(pavementRatio_avgA, widthRatioA, pavementRatio_avgB, widthRatioB); //side scale
            } else {
                centerPos.x = (cornerPos_right.x + cornerPos_left.x) * 0.5f;
                centerPos.z = (cornerPos_right.z + cornerPos_left.z) * 0.5f;
                var cornerPos_left_prev = cornerPos_left;
                var cornerPos_right_prev = cornerPos_right;
                cornerDirB_right = cornerDir_left;
                cornerDirB_left = cornerDir_right;
                float d = info.m_netAI.GetEndRadius() * 1.33333337f;
                Vector3 vector13 = cornerPos_right - cornerDir_right * d;
                Vector3 vector14 = cornerPos_left_prev - cornerDirB_right * d;
                Vector3 vector15 = cornerPos_left - cornerDir_left * d;
                Vector3 vector16 = cornerPos_right_prev - cornerDirB_left * d;
                Vector3 vector17 = cornerPos_right + cornerDir_right * d;
                Vector3 vector18 = cornerPos_left_prev + cornerDirB_right * d;
                Vector3 vector19 = cornerPos_left + cornerDir_left * d;
                Vector3 vector20 = cornerPos_right_prev + cornerDirB_left * d;
                data.m_dataMatrix0 = NetSegment.CalculateControlMatrix(cornerPos_right, vector13, vector14, cornerPos_left_prev, cornerPos_right, vector13, vector14, cornerPos_left_prev, This.m_position, vscale);
                data.m_extraData.m_dataMatrix2 = NetSegment.CalculateControlMatrix(cornerPos_left, vector19, vector20, cornerPos_right_prev, cornerPos_left, vector19, vector20, cornerPos_right_prev, This.m_position, vscale);
                data.m_extraData.m_dataMatrix3 = NetSegment.CalculateControlMatrix(cornerPos_right, vector17, vector18, cornerPos_left_prev, cornerPos_right, vector17, vector18, cornerPos_left_prev, This.m_position, vscale);
                data.m_dataMatrix1 = NetSegment.CalculateControlMatrix(cornerPos_left, vector15, vector16, cornerPos_right_prev, cornerPos_left, vector15, vector16, cornerPos_right_prev, This.m_position, vscale);
                data.m_dataMatrix0.SetRow(3, data.m_dataMatrix0.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
                data.m_extraData.m_dataMatrix2.SetRow(3, data.m_extraData.m_dataMatrix2.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
                data.m_extraData.m_dataMatrix3.SetRow(3, data.m_extraData.m_dataMatrix3.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
                data.m_dataMatrix1.SetRow(3, data.m_dataMatrix1.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
                data.m_dataVector0 = new Vector4(0.5f / info.m_halfWidth, 1f / info.m_segmentLength, 0.5f - info.m_pavementWidth / info.m_halfWidth * 0.5f, info.m_pavementWidth / info.m_halfWidth * 0.5f);
                data.m_dataVector1 = centerPos - data.m_position;
                data.m_dataVector1.w = (data.m_dataMatrix0.m31 + data.m_dataMatrix0.m32 + data.m_extraData.m_dataMatrix2.m31 + data.m_extraData.m_dataMatrix2.m32 + data.m_extraData.m_dataMatrix3.m31 + data.m_extraData.m_dataMatrix3.m32 + data.m_dataMatrix1.m31 + data.m_dataMatrix1.m32) * 0.125f;
                data.m_dataVector2 = new Vector4(info.m_pavementWidth / info.m_halfWidth * 0.5f, 1f, info.m_pavementWidth / info.m_halfWidth * 0.5f, 1f);
            }
            Vector4 colorLocation;
            Vector4 vector21;
            if (NetNode.BlendJunction(nodeID)) {
                colorLocation = RenderManager.GetColorLocation(86016u + (uint)nodeID);
                vector21 = colorLocation;
            } else {
                colorLocation = RenderManager.GetColorLocation((uint)(49152 + SegmentID));
                vector21 = RenderManager.GetColorLocation(86016u + (uint)nodeID);
            }
            data.m_extraData.m_dataVector4 = new Vector4(colorLocation.x, colorLocation.y, vector21.x, vector21.y);
            data.m_dataInt0 = segmentIndex;
            data.m_dataColor0 = info.m_color;
            data.m_dataColor0.a = 0f;
            data.m_dataFloat0 = Singleton<WeatherManager>.instance.GetWindSpeed(data.m_position);
            if (info.m_requireSurfaceMaps) {
                Singleton<TerrainManager>.instance.GetSurfaceMapping(data.m_position, out data.m_dataTexture0, out data.m_dataTexture1, out data.m_dataVector3);
            }
            instanceIndex = (uint)data.m_nextInstance;
        }

        // DC
        private void RefreshJunctionData(ushort nodeID, int segmentIndex, int segmentIndex2, NetInfo info, NetInfo info2, ushort segmentID, ushort segmentID2, ref uint instanceIndex, ref RenderManager.Instance data) {
            data.m_position = this.m_position;
            data.m_rotation = Quaternion.identity;
            data.m_initialized = true;
            float vscale = info.m_netAI.GetVScale();
            bool startNode = segmentID.ToSegment().IsStartNode(nodeID);
            segmentID.ToSegment().CalculateCorner(segmentID, true, start: startNode, leftSide: false, out var CornerPosL, out var CornerDirL, out _);
            segmentID.ToSegment().CalculateCorner(segmentID, true, start: startNode, leftSide: true, out var CornerPosR, out var CornerDirR, out _);
            bool startNode2 = segmentID2.ToSegment().IsStartNode(nodeID);
            segmentID2.ToSegment().CalculateCorner(segmentID2, true, start: startNode2, leftSide:true, out var CornerPos2L, out var CornerDir2L, out _);
            segmentID2.ToSegment().CalculateCorner(segmentID2, true, start: startNode2, leftSide: false, out var CornerPos2R, out var CornerDir2R, out _);
            Vector3 b = (CornerPos2R - CornerPos2L) * (info.m_halfWidth / info2.m_halfWidth * 0.5f - 0.5f);
            CornerPos2L -= b;
            CornerPos2R += b;
            NetSegment.CalculateMiddlePoints(CornerPosL, -CornerDirL, CornerPos2L, -CornerDir2L, true, true, out var bpointL, out var cpointL);
            NetSegment.CalculateMiddlePoints(CornerPosR, -CornerDirR, CornerPos2R, -CornerDir2R, true, true, out var bpointR, out var cpointR);
            data.m_dataMatrix0 = NetSegment.CalculateControlMatrix(CornerPosL, bpointL, cpointL, CornerPos2L, CornerPosR, bpointR, cpointR, CornerPos2R, this.m_position, vscale);
            data.m_extraData.m_dataMatrix2 = NetSegment.CalculateControlMatrix(CornerPosR, bpointR, cpointR, CornerPos2R, CornerPosL, bpointL, cpointL, CornerPos2L, this.m_position, vscale);
            data.m_dataVector0 = new Vector4(0.5f / info.m_halfWidth, 1f / info.m_segmentLength, 1f, 1f);
            Vector4 colorLocation;
            Vector4 colorLocation2;
            if (NetNode.BlendJunction(nodeID)) {
                colorLocation2 = colorLocation = RenderManager.GetColorLocation(NODE_HOLDER + nodeID);
            } else {
                colorLocation = RenderManager.GetColorLocation(SEGMENT_HOLDER + segmentID);
                colorLocation2 = RenderManager.GetColorLocation(SEGMENT_HOLDER + segmentID2);
            }
            data.m_dataVector3 = new Vector4(colorLocation.x, colorLocation.y, colorLocation2.x, colorLocation2.y);
            data.m_dataInt0 = (8 | segmentIndex | segmentIndex2 << 4);
            data.m_dataColor0 = info.m_color;
            data.m_dataColor0.a = 0f;
            data.m_dataFloat0 = Singleton<WeatherManager>.instance.GetWindSpeed(data.m_position);
            if (info.m_requireSurfaceMaps) {
                TerrainManager.instance.GetSurfaceMapping(data.m_position, out data.m_dataTexture0, out data.m_dataTexture1, out data.m_dataVector1);
            }
            instanceIndex = data.m_nextInstance;
        }

        private void RefreshEndData(ushort nodeID, NetInfo info, uint instanceIndex, ref RenderManager.Instance data) {
            data.m_position = this.m_position;
            data.m_rotation = Quaternion.identity;
            data.m_initialized = true;
            float vScale = info.m_netAI.GetVScale() / 1.5f;
            Vector3 cornerR = Vector3.zero;
            Vector3 cornerL = Vector3.zero;
            Vector3 posA = Vector3.zero;
            Vector3 posB = Vector3.zero;
            Vector3 dirR = Vector3.zero;
            Vector3 dirL = Vector3.zero;
            Vector3 dirA = Vector3.zero;
            Vector3 dirB = Vector3.zero;
            bool flag = false;
            ushort segmentID = 0;
            int segmentIndex = 0;
            for (int i = 0; i < 8; i++) {
                ushort segmentIDi = this.GetSegment(i);
                if (segmentIDi != 0) {
                    NetSegment netSegment = Singleton<NetManager>.instance.m_segments.m_buffer[segmentIDi];
                    bool startNode = netSegment.m_startNode == nodeID;
                    netSegment.CalculateCorner(segmentIDi, true, startNode, false, out cornerR, out dirR, out _);
                    netSegment.CalculateCorner(segmentIDi, true, startNode, true, out cornerL, out dirL, out _);
                    if (flag) {
                        dirA = -dirR;
                        dirB = -dirL;
                        dirR.y = 0.25f;
                        dirL.y = 0.25f;
                        dirA.y = -5f;
                        dirB.y = -5f;
                        posA = cornerR - dirR * 10f + dirA * 10f;
                        posB = cornerL - dirL * 10f + dirB * 10f;
                    } else {
                        posA = cornerL;
                        posB = cornerR;
                        dirA = dirL;
                        dirB = dirR;
                    }
                    segmentID = segmentIDi;
                    segmentIndex = i;
                }
            }
            if (flag) {
                Vector3 vector3;
                Vector3 vector4;
                NetSegment.CalculateMiddlePoints(cornerR, -dirR, posA, -dirA, true, true, out vector3, out vector4);
                Vector3 vector5;
                Vector3 vector6;
                NetSegment.CalculateMiddlePoints(cornerL, -dirL, posB, -dirB, true, true, out vector5, out vector6);
                data.m_dataMatrix0 = NetSegment.CalculateControlMatrix(cornerR, vector3, vector4, posA, cornerL, vector5, vector6, posB, this.m_position, vScale);
                data.m_extraData.m_dataMatrix2 = NetSegment.CalculateControlMatrix(cornerL, vector5, vector6, posB, cornerR, vector3, vector4, posA, this.m_position, vScale);
                data.m_dataVector0 = new Vector4(0.5f / info.m_halfWidth, 1f / info.m_segmentLength, 1f, 1f);
                Vector4 colorLocation = RenderManager.GetColorLocation(86016u + (uint)nodeID);
                data.m_dataVector3 = new Vector4(colorLocation.x, colorLocation.y, colorLocation.x, colorLocation.y);
                data.m_dataColor0 = info.m_color;
                data.m_dataColor0.a = 0f;
                data.m_dataFloat0 = Singleton<WeatherManager>.instance.GetWindSpeed(data.m_position);
                data.m_dataInt0 = (8 | segmentIndex);
            } else {
                float d = info.m_netAI.GetEndRadius() * 1.33333337f;
                Vector3 vector7 = cornerR - dirR * d;
                Vector3 vector8 = posA - dirA * d;
                Vector3 vector9 = cornerL - dirL * d;
                Vector3 vector10 = posB - dirB * d;
                Vector3 vector11 = cornerR + dirR * d;
                Vector3 vector12 = posA + dirA * d;
                Vector3 vector13 = cornerL + dirL * d;
                Vector3 vector14 = posB + dirB * d;
                data.m_dataMatrix0 = NetSegment.CalculateControlMatrix(cornerR, vector7, vector8, posA, cornerR, vector7, vector8, posA, this.m_position, vScale);
                data.m_extraData.m_dataMatrix2 = NetSegment.CalculateControlMatrix(cornerL, vector13, vector14, posB, cornerL, vector13, vector14, posB, this.m_position, vScale);
                data.m_extraData.m_dataMatrix3 = NetSegment.CalculateControlMatrix(cornerR, vector11, vector12, posA, cornerR, vector11, vector12, posA, this.m_position, vScale);
                data.m_dataMatrix1 = NetSegment.CalculateControlMatrix(cornerL, vector9, vector10, posB, cornerL, vector9, vector10, posB, this.m_position, vScale);
                data.m_dataMatrix0.SetRow(3, data.m_dataMatrix0.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
                data.m_extraData.m_dataMatrix2.SetRow(3, data.m_extraData.m_dataMatrix2.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
                data.m_extraData.m_dataMatrix3.SetRow(3, data.m_extraData.m_dataMatrix3.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
                data.m_dataMatrix1.SetRow(3, data.m_dataMatrix1.GetRow(3) + new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
                data.m_dataVector0 = new Vector4(0.5f / info.m_halfWidth, 1f / info.m_segmentLength, 0.5f - info.m_pavementWidth / info.m_halfWidth * 0.5f, info.m_pavementWidth / info.m_halfWidth * 0.5f);
                data.m_dataVector1 = new Vector4(0f, (float)this.m_heightOffset * 0.015625f, 0f, 0f);
                data.m_dataVector1.w = (data.m_dataMatrix0.m31 + data.m_dataMatrix0.m32 + data.m_extraData.m_dataMatrix2.m31 + data.m_extraData.m_dataMatrix2.m32 + data.m_extraData.m_dataMatrix3.m31 + data.m_extraData.m_dataMatrix3.m32 + data.m_dataMatrix1.m31 + data.m_dataMatrix1.m32) * 0.125f;
                data.m_dataVector2 = new Vector4(info.m_pavementWidth / info.m_halfWidth * 0.5f, 1f, info.m_pavementWidth / info.m_halfWidth * 0.5f, 1f);
                Vector4 colorLocation2 = RenderManager.GetColorLocation((uint)(49152 + segmentID));
                data.m_extraData.m_dataVector4 = new Vector4(colorLocation2.x, colorLocation2.y, colorLocation2.x, colorLocation2.y);
                data.m_dataColor0 = info.m_color;
                data.m_dataColor0.a = 0f;
                data.m_dataFloat0 = Singleton<WeatherManager>.instance.GetWindSpeed(data.m_position);
                data.m_dataInt0 = segmentIndex;
            }
            if (info.m_requireSurfaceMaps) {
                Singleton<TerrainManager>.instance.GetSurfaceMapping(data.m_position, out data.m_dataTexture0, out data.m_dataTexture1, out data.m_dataVector3);
            }
        }
    }
}