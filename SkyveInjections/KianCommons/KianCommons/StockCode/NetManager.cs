namespace KianCommons.StockCode.x {
    using ColossalFramework;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class NetManager : SimulationManagerBase<NetManager, NetProperties>, ISimulationManager, IRenderableManager, ITerrainManager {
        public override bool CalculateGroupData(int groupX, int groupZ, int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays) {
            bool result = false;
            const int resolutionRatio = NODEGRID_RESOLUTION / RenderManager.GROUP_RESOLUTION; // = 270/45 = 6
            int net_x0 = groupX * resolutionRatio;
            int net_z0 = groupZ * resolutionRatio;
            int net_x1 = (groupX + 1) * resolutionRatio - 1;
            int net_z1 = (groupZ + 1) * resolutionRatio - 1;
            for(int net_z = net_z0; net_z <= net_z1; net_z++) {
                for(int net_x = net_x0; net_x <= net_x1; net_x++) {
                    ushort nodeID = m_nodeGrid[net_z * NODEGRID_RESOLUTION + net_x];
                    int watchdog = 0;
                    while(nodeID != 0) {
                        if(nodeID.ToNode().CalculateGroupData(nodeID, layer, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays)) {
                            result = true;
                        }
                        nodeID = nodeID.ToNode().m_nextGridNode;
                        if(++watchdog >= 32768) {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            for(int net_z = net_z0; net_z <= net_z1; net_z++) {
                for(int net_x = net_x0; net_x <= net_x1; net_x++) {
                    ushort segmentID = m_segmentGrid[net_z * 270 + net_x];
                    int watchdog = 0;
                    while(segmentID != 0) {
                        if(segmentID.ToSegment().CalculateGroupData(segmentID, layer, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays)) {
                            result = true;
                        }
                        segmentID = segmentID.ToSegment().m_nextGridSegment;
                        if(++watchdog >= 36864) {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return result;
        }

        public override void PopulateGroupData(int groupX, int groupZ, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance, ref bool requireSurfaceMaps) {
            const int resolutionRatio = NODEGRID_RESOLUTION / RenderManager.GROUP_RESOLUTION; // = 270/45 = 6
            int net_x0 = groupX * resolutionRatio;
            int net_z0 = groupZ * resolutionRatio;
            int net_x1 = (groupX + 1) * resolutionRatio - 1;
            int net_z1 = (groupZ + 1) * resolutionRatio - 1;
            for(int net_z = net_z0; net_z <= net_z1; net_z++) {
                for(int net_x = net_x0; net_x <= net_x1; net_x++) {
                    ushort nodeID = m_nodeGrid[net_z * NODEGRID_RESOLUTION + net_x];
                    int watchdog = 0;
                    while(nodeID != 0) {
                        nodeID.ToNode().PopulateGroupData(nodeID, groupX, groupZ, layer, ref vertexIndex, ref triangleIndex, groupPosition, data, ref min, ref max, ref maxRenderDistance, ref maxInstanceDistance, ref requireSurfaceMaps);
                        nodeID = nodeID.ToNode().m_nextGridNode;
                        if(++watchdog >= 32768) {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            for(int net_z = net_z0; net_z <= net_z1; net_z++) {
                for(int net_x = net_x0; net_x <= net_x1; net_x++) {
                    ushort segmentID = m_segmentGrid[net_z * 270 + net_x];
                    int watchdog = 0;
                    while(segmentID != 0) {
                        segmentID.ToSegment().PopulateGroupData(segmentID, groupX, groupZ, layer, ref vertexIndex, ref triangleIndex, groupPosition, data, ref min, ref max, ref maxRenderDistance, ref maxInstanceDistance, ref requireSurfaceMaps);
                        segmentID = segmentID.ToSegment().m_nextGridSegment;
                        if(++watchdog >= 36864) {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
        }


        protected override void EndRenderingImpl(RenderManager.CameraInfo cameraInfo) {
            FastList<RenderGroup> renderedGroups = Singleton<RenderManager>.instance.m_renderedGroups;
            this.m_nameInstanceBuffer.Clear();
            this.m_visibleRoadNameSegment = 0;
            this.m_visibleTrafficLightNode = 0;
            for(int groupIndex = 0; groupIndex < renderedGroups.m_size; groupIndex++) {
                RenderGroup renderGroup = renderedGroups.m_buffer[groupIndex];
                if(renderGroup.m_instanceMask != 0) {
                    const int resolutionRatio = NODEGRID_RESOLUTION / RenderManager.GROUP_RESOLUTION; // = 270/45 = 6
                    int net_x0 = renderGroup.m_x * resolutionRatio;
                    int net_z0 = renderGroup.m_z * resolutionRatio;
                    int net_x1 = (renderGroup.m_x + 1) * resolutionRatio - 1; // = net_x + 5
                    int net_z1 = (renderGroup.m_z + 1) * resolutionRatio - 1; // = net_z + 5
                    for(int net_z = net_z0; net_z <= net_z1; net_z++) {
                        for(int net_x = net_x0; net_x <= net_x1; net_x++) {
                            int gridIndex = net_z * NODEGRID_RESOLUTION + net_x;
                            ushort nodeID = this.m_nodeGrid[gridIndex];
                            int watchdog = 0;
                            while(nodeID != 0) {
                                nodeID.ToNode().RenderInstance(cameraInfo, nodeID, renderGroup.m_instanceMask);
                                nodeID = nodeID.ToNode().m_nextGridNode;
                                if(++watchdog >= 32768) {
                                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                    break;
                                }
                            }
                        }
                    }
                    for(int net_z = net_z0; net_z <= net_z1; net_z++) {
                        for(int net_x = net_x0; net_x <= net_x1; net_x++) {
                            int gridIndex = net_z * 270 + net_x;
                            ushort segmentID = this.m_segmentGrid[gridIndex];
                            int watchdog = 0;
                            while(segmentID != 0) {
                                this.m_segments.m_buffer[(int)segmentID].RenderInstance(cameraInfo, segmentID, renderGroup.m_instanceMask);
                                segmentID = this.m_segments.m_buffer[(int)segmentID].m_nextGridSegment;
                                if(++watchdog >= 36864) {
                                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            this.m_lastVisibleRoadNameSegment = this.m_visibleRoadNameSegment;
            this.m_lastVisibleTrafficLightNode = this.m_visibleTrafficLightNode;
            int num11 = PrefabCollection<NetInfo>.PrefabCount();
            for(int n = 0; n < num11; n++) {
                NetInfo prefab = PrefabCollection<NetInfo>.GetPrefab((uint)n);
                if(prefab != null) {
                    if(prefab.m_segments != null) {
                        for(int num12 = 0; num12 < prefab.m_segments.Length; num12++) {
                            NetInfo.Segment segment = prefab.m_segments[num12];
                            NetInfo.LodValue combinedLod = segment.m_combinedLod;
                            if(combinedLod != null && combinedLod.m_lodCount != 0) {
                                NetSegment.RenderLod(cameraInfo, combinedLod);
                            }
                        }
                    }
                    if(prefab.m_nodes != null) {
                        for(int num13 = 0; num13 < prefab.m_nodes.Length; num13++) {
                            NetInfo.Node node = prefab.m_nodes[num13];
                            NetInfo.LodValue combinedLod2 = node.m_combinedLod;
                            if(combinedLod2 != null && combinedLod2.m_lodCount != 0) {
                                if(node.m_directConnect) {
                                    NetSegment.RenderLod(cameraInfo, combinedLod2);
                                } else {
                                    NetNode.RenderLod(cameraInfo, combinedLod2);
                                }
                            }
                        }
                    }
                }
            }
        }
        public const int MAX_NODE_COUNT = 32768;

        // Token: 0x040045BE RID: 17854
        public const int MAX_SEGMENT_COUNT = 36864;

        // Token: 0x040045BF RID: 17855
        public const int MAX_LANE_COUNT = 262144;

        // Token: 0x040045C0 RID: 17856
        public const int MAX_MAP_NODES = 16384;

        // Token: 0x040045C1 RID: 17857
        public const int MAX_MAP_SEGMENTS = 16384;

        // Token: 0x040045C2 RID: 17858
        public const int MAX_MAP_LANES = 131072;

        // Token: 0x040045C3 RID: 17859
        public const int MAX_ASSET_NODES = 1024;

        // Token: 0x040045C4 RID: 17860
        public const int MAX_ASSET_SEGMENTS = 1024;

        // Token: 0x040045C5 RID: 17861
        public const int MAX_ASSET_LANES = 8192;

        // Token: 0x040045C6 RID: 17862
        public const float NODEGRID_CELL_SIZE = 64f;

        // Token: 0x040045C7 RID: 17863
        public const int NODEGRID_RESOLUTION = 270;

        // Token: 0x040045C8 RID: 17864
        public int m_nodeCount;

        // Token: 0x040045C9 RID: 17865
        public int m_segmentCount;

        // Token: 0x040045CA RID: 17866
        public int m_laneCount;

        // Token: 0x040045CB RID: 17867
        public int m_infoCount;

        // Token: 0x040045CC RID: 17868
        [NonSerialized]
        public Array16<NetNode> m_nodes;

        // Token: 0x040045CD RID: 17869
        [NonSerialized]
        public Array16<NetSegment> m_segments;

        // Token: 0x040045CE RID: 17870
        [NonSerialized]
        public Array32<NetLane> m_lanes;

        // Token: 0x040045CF RID: 17871
        [NonSerialized]
        public ulong[] m_updatedNodes;

        // Token: 0x040045D0 RID: 17872
        [NonSerialized]
        public ulong[] m_updatedSegments;

        // Token: 0x040045D1 RID: 17873
        [NonSerialized]
        public bool m_nodesUpdated;

        // Token: 0x040045D2 RID: 17874
        [NonSerialized]
        public bool m_segmentsUpdated;

        // Token: 0x040045D3 RID: 17875
        [NonSerialized]
        public ushort[] m_nodeGrid;

        // Token: 0x040045D4 RID: 17876
        [NonSerialized]
        public ushort[] m_segmentGrid;

        // Token: 0x040045D5 RID: 17877
        [NonSerialized]
        public float[] m_angleBuffer;

        // Token: 0x040045D6 RID: 17878
        [NonSerialized]
        public MaterialPropertyBlock m_materialBlock;

        // Token: 0x040045D7 RID: 17879
        [NonSerialized]
        public int ID_LeftMatrix;

        // Token: 0x040045D8 RID: 17880
        [NonSerialized]
        public int ID_RightMatrix;

        // Token: 0x040045D9 RID: 17881
        [NonSerialized]
        public int ID_LeftMatrixB;

        // Token: 0x040045DA RID: 17882
        [NonSerialized]
        public int ID_RightMatrixB;

        // Token: 0x040045DB RID: 17883
        [NonSerialized]
        public int ID_MeshScale;

        // Token: 0x040045DC RID: 17884
        [NonSerialized]
        public int ID_ObjectIndex;

        // Token: 0x040045DD RID: 17885
        [NonSerialized]
        public int ID_CenterPos;

        // Token: 0x040045DE RID: 17886
        [NonSerialized]
        public int ID_SideScale;

        // Token: 0x040045DF RID: 17887
        [NonSerialized]
        public int ID_Color;

        // Token: 0x040045E0 RID: 17888
        [NonSerialized]
        public int ID_SurfaceTexA;

        // Token: 0x040045E1 RID: 17889
        [NonSerialized]
        public int ID_SurfaceTexB;

        // Token: 0x040045E2 RID: 17890
        [NonSerialized]
        public int ID_SurfaceMapping;

        // Token: 0x040045E3 RID: 17891
        [NonSerialized]
        public int ID_HeightMap;

        // Token: 0x040045E4 RID: 17892
        [NonSerialized]
        public int ID_HeightMapping;

        // Token: 0x040045E5 RID: 17893
        [NonSerialized]
        public int ID_MainTex;

        // Token: 0x040045E6 RID: 17894
        [NonSerialized]
        public int ID_XYSMap;

        // Token: 0x040045E7 RID: 17895
        [NonSerialized]
        public int ID_APRMap;

        // Token: 0x040045E8 RID: 17896
        [NonSerialized]
        public int ID_LeftMatrices;

        // Token: 0x040045E9 RID: 17897
        [NonSerialized]
        public int ID_RightMatrices;

        // Token: 0x040045EA RID: 17898
        [NonSerialized]
        public int ID_LeftMatricesB;

        // Token: 0x040045EB RID: 17899
        [NonSerialized]
        public int ID_RightMatricesB;

        // Token: 0x040045EC RID: 17900
        [NonSerialized]
        public int ID_MeshScales;

        // Token: 0x040045ED RID: 17901
        [NonSerialized]
        public int ID_ObjectIndices;

        // Token: 0x040045EE RID: 17902
        [NonSerialized]
        public int ID_CenterPositions;

        // Token: 0x040045EF RID: 17903
        [NonSerialized]
        public int ID_SideScales;

        // Token: 0x040045F0 RID: 17904
        [NonSerialized]
        public int ID_MeshLocations;

        // Token: 0x040045F1 RID: 17905
        [GuideMetaData(19)]
        [NonSerialized]
        public NetNodeInstanceGuide m_outsideNodeNotConnected;

        // Token: 0x040045F2 RID: 17906
        [GuideMetaData(33)]
        [NonSerialized]
        public NetNodeInstanceGuide m_transportNodeNotConnected;

        // Token: 0x040045F3 RID: 17907
        [GuideMetaData(34)]
        [NonSerialized]
        public NetSegmentInstanceGuide m_shortRoadTraffic;

        // Token: 0x040045F4 RID: 17908
        [GuideMetaData(36)]
        [NonSerialized]
        public ServiceTypeGuide m_optionsNotUsed;

        // Token: 0x040045F5 RID: 17909
        [GuideMetaData(37)]
        [NonSerialized]
        public ServiceTypeGuide m_elevationNotUsed;

        // Token: 0x040045F6 RID: 17910
        [GuideMetaData(39)]
        [NonSerialized]
        public ServiceTypeGuide m_manualUpgrade;

        // Token: 0x040045F7 RID: 17911
        [GuideMetaData(30)]
        [NonSerialized]
        public GenericGuide m_upgradeExistingRoad;

        // Token: 0x040045F8 RID: 17912
        [GuideMetaData(42)]
        [NonSerialized]
        public GenericGuide m_roadsNotUsed;

        // Token: 0x040045F9 RID: 17913
        [GuideMetaData(51)]
        [NonSerialized]
        public GenericGuide m_onewayRoadPlacement;

        // Token: 0x040045FA RID: 17914
        [GuideMetaData(55)]
        [NonSerialized]
        public GenericGuide m_canalsNotUsed;

        // Token: 0x040045FB RID: 17915
        [GuideMetaData(56)]
        [NonSerialized]
        public GenericGuide m_quaysNotUsed;

        // Token: 0x040045FC RID: 17916
        [GuideMetaData(57)]
        [NonSerialized]
        public GenericGuide m_canalDemolished;

        // Token: 0x040045FD RID: 17917
        [GuideMetaData(63)]
        [NonSerialized]
        public GenericGuide m_upgradeWaterPipes;

        // Token: 0x040045FE RID: 17918
        [GuideMetaData(77)]
        [NonSerialized]
        public NetSegmentInstanceGuide m_roadDestroyed;

        // Token: 0x040045FF RID: 17919
        [GuideMetaData(78)]
        [NonSerialized]
        public ServiceTypeGuide m_roadDestroyed2;

        // Token: 0x04004600 RID: 17920
        [GuideMetaData(85)]
        [NonSerialized]
        public NetSegmentInstanceGuide m_roadNames;

        // Token: 0x04004601 RID: 17921
        [GuideMetaData(92)]
        [NonSerialized]
        public NetNodeInstanceGuide m_yieldLights;

        // Token: 0x04004602 RID: 17922
        [GuideMetaData(93)]
        [NonSerialized]
        public GenericGuide m_visualAids;

        // Token: 0x04004603 RID: 17923
        [NonSerialized]
        public bool m_treatWetAsSnow;

        // Token: 0x04004604 RID: 17924
        [NonSerialized]
        public int m_wetnessChanged;

        // Token: 0x04004605 RID: 17925
        [NonSerialized]
        public int m_lastMaxWetness;

        // Token: 0x04004606 RID: 17926
        [NonSerialized]
        public int m_currentMaxWetness;

        // Token: 0x04004607 RID: 17927
        [NonSerialized]
        public FastList<ushort> m_tempNodeBuffer;

        // Token: 0x04004608 RID: 17928
        [NonSerialized]
        public FastList<ushort> m_tempSegmentBuffer;

        // Token: 0x04004609 RID: 17929
        [NonSerialized]
        public FastList<uint> m_nameInstanceBuffer;

        // Token: 0x0400460A RID: 17930
        [NonSerialized]
        public HashSet<ushort> m_updateNameVisibility;

        // Token: 0x0400460B RID: 17931
        [NonSerialized]
        public ulong[] m_adjustedSegments;

        // Token: 0x0400460C RID: 17932
        [NonSerialized]
        public int m_arrowLayer;

        // Token: 0x0400460D RID: 17933
        [NonSerialized]
        public bool m_roadNamesVisibleSetting;

        // Token: 0x0400460E RID: 17934
        [NonSerialized]
        public ushort m_visibleRoadNameSegment;

        // Token: 0x0400460F RID: 17935
        [NonSerialized]
        public ushort m_lastVisibleRoadNameSegment;

        // Token: 0x04004610 RID: 17936
        [NonSerialized]
        public ushort m_visibleTrafficLightNode;

        // Token: 0x04004611 RID: 17937
        [NonSerialized]
        public ushort m_lastVisibleTrafficLightNode;

        // Token: 0x04004612 RID: 17938
        public Texture2D m_lodRgbAtlas;

        // Token: 0x04004613 RID: 17939
        public Texture2D m_lodXysAtlas;

        // Token: 0x04004614 RID: 17940
        public Texture2D m_lodAprAtlas;

        // Token: 0x04004615 RID: 17941
        private FastList<ushort>[] m_serviceSegments;

        // Token: 0x04004616 RID: 17942
        private HashSet<InstanceID> m_smoothColors;

        // Token: 0x04004617 RID: 17943
        private HashSet<ushort> m_tempHashSet;

        // Token: 0x04004618 RID: 17944
        private FastList<InstanceID> m_removeSmoothColors;

        // Token: 0x04004619 RID: 17945
        private FastList<InstanceID> m_newSmoothColors;

        // Token: 0x0400461A RID: 17946
        private int m_roadLayer;

        // Token: 0x0400461B RID: 17947
        private int m_netBuildingLayer;

        // Token: 0x0400461C RID: 17948
        private int m_colorUpdateMinSegment;

        // Token: 0x0400461D RID: 17949
        private int m_colorUpdateMaxSegment;

        // Token: 0x0400461E RID: 17950
        private int m_colorUpdateMinNode;

        // Token: 0x0400461F RID: 17951
        private int m_colorUpdateMaxNode;

        // Token: 0x04004620 RID: 17952
        private object m_colorUpdateLock;

        // Token: 0x04004621 RID: 17953
        private int[] m_tileNodesCount;

        // Token: 0x04004622 RID: 17954
        private bool m_renderDirectionArrows;

        // Token: 0x04004623 RID: 17955
        private bool m_renderDirectionArrowsInfo;

        // Token: 0x04004624 RID: 17956
        private bool m_roadNamesVisible;

        // Token: 0x04004625 RID: 17957
        private float m_roadNameAlpha;

        // Token: 0x04004626 RID: 17958
        private int m_nameModifiedCount;

        // Token: 0x04004627 RID: 17959
        private Camera m_mainCamera;

        // Token: 0x04004628 RID: 17960
        private Camera m_undergroundCamera;

        // Token: 0x04004629 RID: 17961
        private PathVisualizer m_pathVisualizer;

        // Token: 0x0400462A RID: 17962
        private NetAdjust m_netAdjust;

        // Token: 0x0400462B RID: 17963
        private Material m_nameMaterial;


    }
}
