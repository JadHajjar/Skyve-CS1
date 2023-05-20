using System;
using ColossalFramework;
using UnityEngine;
using ColossalFramework.Math;

namespace KianCommons.StockCode {
    public partial class NetLane2 {
        float m_length;


        NetLane.Flags m_flags;
        Bezier3 m_bezier;

        public void RenderInstance(
            RenderManager.CameraInfo cameraInfo,
            ushort segmentID, uint laneID, NetInfo.Lane laneInfo,
            NetNode.Flags startFlags, NetNode.Flags endFlags,
            Color startColor, Color endColor,
            float startAngle, float endAngle,
            bool invert, int layerMask,
            Vector4 objectIndex1, Vector4 objectIndex2, ref RenderManager.Instance data, ref int propIndex) {
            NetLaneProps laneProps = laneInfo.m_laneProps;

            if(laneProps != null && laneProps.m_props != null) {
                bool backward = (byte)(laneInfo.m_finalDirection & NetInfo.Direction.Both) == 2 || (byte)(laneInfo.m_finalDirection & NetInfo.Direction.AvoidBoth) == 11;
                bool reverse = backward != invert;
                if(backward) { //swap
                    NetNode.Flags flags = startFlags;
                    startFlags = endFlags;
                    endFlags = flags;
                }
                Texture texture = null;
                Vector4 zero = Vector4.zero;
                Vector4 zero2 = Vector4.zero;
                Texture texture2 = null;
                Vector4 zero3 = Vector4.zero;
                Vector4 zero4 = Vector4.zero;
                int nProps = laneProps.m_props.Length;
                for(int i = 0; i < nProps; i++) {
                    NetLaneProps.Prop prop = laneProps.m_props[i];
                    if(this.m_length >= prop.m_minLength) {
                        int repeatCountTimes2 = 2;
                        if(prop.m_repeatDistance > 1f) {
                            repeatCountTimes2 *= Mathf.Max(1, Mathf.RoundToInt(this.m_length / prop.m_repeatDistance));
                        }
                        int currentPropIndex = propIndex;
                        if(propIndex != -1) {
                            propIndex = currentPropIndex + (repeatCountTimes2 + 1) >> 1; // div 2
                        }
                        if(prop.CheckFlags(this.m_flags, startFlags, endFlags)) {
                            float offset = prop.m_segmentOffset * 0.5f;
                            if(this.m_length != 0f) {
                                offset = Mathf.Clamp(offset + prop.m_position.z / this.m_length, -0.5f, 0.5f);
                            }
                            if(reverse) {
                                offset = -offset;
                            }
                            PropInfo propInfo = prop.m_finalProp;
                            if(propInfo != null && (layerMask & 1 << propInfo.m_prefabDataLayer) != 0) {
                                Color color = (prop.m_colorMode != NetLaneProps.ColorMode.EndState) ? startColor : endColor;
                                Randomizer randomizer = new Randomizer((int)(laneID + (uint)i));
                                for(int j = 1; j <= repeatCountTimes2; j += 2) {
                                    if(randomizer.Int32(100u) < prop.m_probability) {
                                        float t = offset + (float)j / (float)repeatCountTimes2;
                                        PropInfo variation = propInfo.GetVariation(ref randomizer);
                                        float scale = variation.m_minScale + (float)randomizer.Int32(10000u) * (variation.m_maxScale - variation.m_minScale) * 0.0001f;
                                        if(prop.m_colorMode == NetLaneProps.ColorMode.Default) {
                                            color = variation.GetColor(ref randomizer);
                                        }
                                        Vector3 pos = this.m_bezier.Position(t);
                                        if(propIndex != -1) {
                                            pos.y = (float)data.m_extraData.GetUShort(currentPropIndex++) * 0.015625f;
                                        }
                                        pos.y += prop.m_position.y;
                                        if(cameraInfo.CheckRenderDistance(pos, variation.m_maxRenderDistance)) {
                                            Vector3 tan = this.m_bezier.Tangent(t);
                                            if(tan != Vector3.zero) {
                                                if(reverse) {
                                                    tan = -tan;
                                                }
                                                Vector3 normalXZ = new Vector3 { x = tan.z, z = -tan.x };
                                                if(prop.m_position.x != 0f) {
                                                    tan.Normalize();
                                                    normalXZ.Normalize();
                                                    pos += normalXZ * prop.m_position.x;
                                                }
                                                float normalAngle = Mathf.Atan2(normalXZ.z, normalXZ.x);
                                                if(prop.m_cornerAngle != 0f || prop.m_position.x != 0f) {
                                                    float angleDiff = endAngle - startAngle;
                                                    if(angleDiff > Mathf.PI) {
                                                        angleDiff -= 2*Mathf.PI;
                                                    }
                                                    if(angleDiff < -Mathf.PI) {
                                                        angleDiff += 2*Mathf.PI;
                                                    }
                                                    float currentAngle = startAngle + angleDiff * t;
                                                    float angle2 = currentAngle - normalAngle;
                                                    if(angle2 > Mathf.PI) {
                                                        angle2 -= 2*Mathf.PI;
                                                    }
                                                    if(angle2 < -Mathf.PI) {
                                                        angle2 += 2*Mathf.PI;
                                                    }
                                                    normalAngle += angle2 * prop.m_cornerAngle;
                                                    if(angle2 != 0f && prop.m_position.x != 0f) {
                                                        float d = Mathf.Tan(angle2);
                                                        pos.x += tan.x * d * prop.m_position.x;
                                                        pos.z += tan.z * d * prop.m_position.x;
                                                    }
                                                }
                                                Vector4 objectIndex3 = (t <= 0.5f) ? objectIndex1 : objectIndex2;
                                                normalAngle += prop.m_angle * 0.0174532924f;
                                                InstanceID id = default(InstanceID);
                                                id.NetSegment = segmentID;
                                                if(variation.m_requireWaterMap) {
                                                    if(texture == null) {
                                                        Singleton<TerrainManager>.instance.GetHeightMapping(Singleton<NetManager>.instance.m_segments.m_buffer[(int)segmentID].m_middlePosition, out texture, out zero, out zero2);
                                                    }
                                                    if(texture2 == null) {
                                                        Singleton<TerrainManager>.instance.GetWaterMapping(Singleton<NetManager>.instance.m_segments.m_buffer[(int)segmentID].m_middlePosition, out texture2, out zero3, out zero4);
                                                    }
                                                    PropInstance.RenderInstance(cameraInfo, variation, id, pos, scale, normalAngle, color, objectIndex3, true, texture, zero, zero2, texture2, zero3, zero4);
                                                } else if(!variation.m_requireHeightMap) {
                                                    PropInstance.RenderInstance(cameraInfo, variation, id, pos, scale, normalAngle, color, objectIndex3, true);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            TreeInfo finalTree = prop.m_finalTree;
                            if(finalTree != null && (layerMask & 1 << finalTree.m_prefabDataLayer) != 0) {
                                Randomizer randomizer2 = new Randomizer((int)(laneID + (uint)i));
                                for(int k = 1; k <= repeatCountTimes2; k += 2) {
                                    if(randomizer2.Int32(100u) < prop.m_probability) {
                                        float t = offset + (float)k / (float)repeatCountTimes2;
                                        TreeInfo variation2 = finalTree.GetVariation(ref randomizer2);
                                        float scale2 = variation2.m_minScale + (float)randomizer2.Int32(10000u) * (variation2.m_maxScale - variation2.m_minScale) * 0.0001f;
                                        float brightness = variation2.m_minBrightness + (float)randomizer2.Int32(10000u) * (variation2.m_maxBrightness - variation2.m_minBrightness) * 0.0001f;
                                        Vector3 position = this.m_bezier.Position(t);
                                        if(propIndex != -1) {
                                            position.y = (float)data.m_extraData.GetUShort(currentPropIndex++) * 0.015625f;
                                        }
                                        position.y += prop.m_position.y;
                                        if(prop.m_position.x != 0f) {
                                            Vector3 vector3 = this.m_bezier.Tangent(t);
                                            if(reverse) {
                                                vector3 = -vector3;
                                            }
                                            vector3.y = 0f;
                                            vector3 = Vector3.Normalize(vector3);
                                            position.x += vector3.z * prop.m_position.x;
                                            position.z -= vector3.x * prop.m_position.x;
                                        }
                                        global::TreeInstance.RenderInstance(cameraInfo, variation2, position, scale2, brightness, RenderManager.DefaultColorLocation);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public void PopulateGroupData(
            ushort segmentID, uint laneID, NetInfo.Lane laneInfo, bool destroyed,
            NetNode.Flags startFlags, NetNode.Flags endFlags,
            float startAngle, float endAngle,
            bool invert, bool terrainHeight, int layer,
            ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance, ref bool hasProps) {
            NetLaneProps laneProps = laneInfo.m_laneProps;
            if(laneProps?.m_props == null) {
                return;
            }
            bool backward = (laneInfo.m_finalDirection & NetInfo.Direction.Both) == NetInfo.Direction.Backward || (laneInfo.m_finalDirection & NetInfo.Direction.AvoidBoth) == NetInfo.Direction.AvoidForward;
            bool reverse = backward != invert;
            if(backward) { //swap
                NetNode.Flags flags = startFlags;
                startFlags = endFlags;
                endFlags = flags;
            }
            int nProps = laneProps.m_props.Length;
            for(int i = 0; i < nProps; i++) {
                NetLaneProps.Prop prop = laneProps.m_props[i];
                if(!prop.CheckFlags(m_flags, startFlags, endFlags) || m_length < prop.m_minLength) {
                    continue;
                }
                int repeatCountTimes2 = 2;
                if(prop.m_repeatDistance > 1f) {
                    repeatCountTimes2 *= Mathf.Max(1, Mathf.RoundToInt(m_length / prop.m_repeatDistance));
                }
                float halfSegmentOffset = prop.m_segmentOffset * 0.5f;
                if(m_length != 0f) {
                    halfSegmentOffset = Mathf.Clamp(halfSegmentOffset + prop.m_position.z / m_length, -0.5f, 0.5f);
                }
                if(reverse) {
                    halfSegmentOffset = 0f - halfSegmentOffset;
                }
                PropInfo finalProp = prop.m_finalProp;
                if((object)finalProp != null) {
                    hasProps = true;
                    if(finalProp.m_prefabDataLayer == layer || finalProp.m_effectLayer == layer) {
                        Color color = Color.white;
                        Randomizer r = new Randomizer((int)laneID + i);
                        for(int j = 1; j <= repeatCountTimes2; j += 2) {
                            if(r.Int32(100u) >= prop.m_probability) {
                                continue;
                            }
                            float t = halfSegmentOffset + (float)j / (float)repeatCountTimes2;
                            PropInfo variation = finalProp.GetVariation(ref r);
                            float scale = variation.m_minScale + (float)r.Int32(10000u) * (variation.m_maxScale - variation.m_minScale) * 0.0001f;
                            if(prop.m_colorMode == NetLaneProps.ColorMode.Default) {
                                color = variation.GetColor(ref r);
                            }
                            if(!variation.m_isDecal && destroyed) {
                                continue;
                            }
                            Vector3 pos = m_bezier.Position(t);
                            Vector3 tan = m_bezier.Tangent(t);
                            if(!(tan != Vector3.zero)) {
                                continue;
                            }
                            if(reverse) {
                                tan = -tan;
                            }
                            tan.y = 0f;
                            if(prop.m_position.x != 0f) {
                                tan = Vector3.Normalize(tan);
                                pos.x += tan.z * prop.m_position.x;
                                pos.z -= tan.x * prop.m_position.x;
                            }
                            float normalAngle = Mathf.Atan2(tan.x, 0f - tan.z);
                            if(prop.m_cornerAngle != 0f || prop.m_position.x != 0f) {
                                float angleDiff = endAngle - startAngle;
                                if(angleDiff > Mathf.PI) {
                                    angleDiff -= Mathf.PI * 2f;
                                }
                                if(angleDiff < -Mathf.PI) {
                                    angleDiff += Mathf.PI * 2f;
                                }
                                var angle2 = startAngle + angleDiff * t - normalAngle;
                                if(angle2 > Mathf.PI) {
                                    angle2 -= Mathf.PI * 2f;
                                }
                                if(angle2 < -Mathf.PI) {
                                    angle2 += Mathf.PI * 2f;
                                }
                                normalAngle += angle2 * prop.m_cornerAngle;
                                if(angle2 != 0f && prop.m_position.x != 0f) {
                                    float d = Mathf.Tan(angle2);
                                    pos.x += tan.x * d * prop.m_position.x;
                                    pos.z += tan.z * d * prop.m_position.x;
                                }
                            }
                            if(terrainHeight) {
                                if(variation.m_requireWaterMap) {
                                    pos.y = Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(pos, timeLerp: false, 0f);
                                } else {
                                    pos.y = Singleton<TerrainManager>.instance.SampleDetailHeight(pos);
                                }
                            }
                            pos.y += prop.m_position.y;
                            InstanceID id = default(InstanceID);
                            id.NetSegment = segmentID;
                            PropInstance.PopulateGroupData(angle: normalAngle + prop.m_angle * (Mathf.PI / 180f), info: variation, layer: layer, id: id, position: pos, scale: scale, color: color, vertexIndex: ref vertexIndex, triangleIndex: ref triangleIndex, groupPosition: groupPosition, data: data, min: ref min, max: ref max, maxRenderDistance: ref maxRenderDistance, maxInstanceDistance: ref maxInstanceDistance);
                        }
                    }
                }
                if(destroyed) {
                    continue;
                }
                TreeInfo finalTree = prop.m_finalTree;
                if((object)finalTree == null) {
                    continue;
                }
                hasProps = true;
                if(finalTree.m_prefabDataLayer != layer) {
                    continue;
                }
                Randomizer r2 = new Randomizer((int)laneID + i);
                for(int k = 1; k <= repeatCountTimes2; k += 2) {
                    if(r2.Int32(100u) >= prop.m_probability) {
                        continue;
                    }
                    float t = halfSegmentOffset + (float)k / (float)repeatCountTimes2;
                    TreeInfo variation2 = finalTree.GetVariation(ref r2);
                    float scale2 = variation2.m_minScale + (float)r2.Int32(10000u) * (variation2.m_maxScale - variation2.m_minScale) * 0.0001f;
                    float brightness = variation2.m_minBrightness + (float)r2.Int32(10000u) * (variation2.m_maxBrightness - variation2.m_minBrightness) * 0.0001f;
                    Vector3 vector3 = m_bezier.Position(t);
                    if(prop.m_position.x != 0f) {
                        Vector3 vector4 = m_bezier.Tangent(t);
                        if(reverse) {
                            vector4 = -vector4;
                        }
                        vector4.y = 0f;
                        vector4 = Vector3.Normalize(vector4);
                        vector3.x += vector4.z * prop.m_position.x;
                        vector3.z -= vector4.x * prop.m_position.x;
                    }
                    if(terrainHeight) {
                        vector3.y = Singleton<TerrainManager>.instance.SampleDetailHeight(vector3);
                    }
                    vector3.y += prop.m_position.y;
                    TreeInstance.PopulateGroupData(variation2, vector3, scale2, brightness, RenderManager.DefaultColorLocation, ref vertexIndex, ref triangleIndex, groupPosition, data, ref min, ref max, ref maxRenderDistance, ref maxInstanceDistance);
                }
            }
        }

        public void RefreshInstance(ushort segmentID, uint laneID, NetInfo.Lane laneInfo, float startAngle, float endAngle, bool invert, ref RenderManager.Instance data, ref int propIndex) {
            NetLaneProps laneProps = laneInfo.m_laneProps;
            if (laneProps != null && laneProps.m_props != null) {
                bool backward = (byte)(laneInfo.m_finalDirection & NetInfo.Direction.Both) == 2 || (byte)(laneInfo.m_finalDirection & NetInfo.Direction.AvoidBoth) == 11;
                bool reverse = backward != invert;
                int nProps = laneProps.m_props.Length;
                for (int i = 0; i < nProps; i++) {
                    NetLaneProps.Prop prop = laneProps.m_props[i];
                    if (this.m_length >= prop.m_minLength) {
                        int repeatCountTimes2 = 2;
                        if (prop.m_repeatDistance > 1f) {
                            repeatCountTimes2 *= Mathf.Max(1, Mathf.RoundToInt(this.m_length / prop.m_repeatDistance));
                        }
                        int currentPropIndex = propIndex;
                        propIndex = currentPropIndex + ((repeatCountTimes2 + 1) >> 1);
                        float halfSegmentOffset = prop.m_segmentOffset * 0.5f;
                        if (this.m_length != 0f) {
                            halfSegmentOffset = Mathf.Clamp(halfSegmentOffset + prop.m_position.z / this.m_length, -0.5f, 0.5f);
                        }
                        if (reverse) {
                            halfSegmentOffset = -halfSegmentOffset;
                        }
                        PropInfo propInfo = prop.m_finalProp;
                        if (propInfo != null) {
                            Randomizer randomizer = new Randomizer((int)(laneID + (uint)i));
                            for (int j = 1; j <= repeatCountTimes2; j += 2) {
                                if (randomizer.Int32(100U) < prop.m_probability) {
                                    float t = halfSegmentOffset + (float)j / (float)repeatCountTimes2;
                                    PropInfo variation = propInfo.GetVariation(ref randomizer);
                                    randomizer.Int32(10000U);
                                    if (prop.m_colorMode == NetLaneProps.ColorMode.Default) {
                                        variation.GetColor(ref randomizer);
                                    }
                                    Vector3 worldPos = this.m_bezier.Position(t);
                                    Vector3 dir = this.m_bezier.Tangent(t);
                                    if (dir != Vector3.zero) {
                                        if (reverse) {
                                            dir = -dir;
                                        }
                                        dir.y = 0f;
                                        if (prop.m_position.x != 0f) {
                                            dir = Vector3.Normalize(dir);
                                            worldPos.x += dir.z * prop.m_position.x;
                                            worldPos.z -= dir.x * prop.m_position.x;
                                        }
                                        float num6 = Mathf.Atan2(dir.x, -dir.z);
                                        if (prop.m_cornerAngle != 0f || prop.m_position.x != 0f) {
                                            float num7 = endAngle - startAngle;
                                            if (num7 > Mathf.PI) {
                                                num7 -= 2 * Mathf.PI;
                                            }
                                            if (num7 < -Mathf.PI) {
                                                num7 += 2 * Mathf.PI;
                                            }
                                            float num8 = startAngle + num7 * t;
                                            num7 = num8 - num6;
                                            if (num7 > Mathf.PI) {
                                                num7 -= 2 * Mathf.PI;
                                            }
                                            if (num7 < -Mathf.PI) {
                                                num7 += 2 * Mathf.PI;
                                            }
                                            num6 += num7 * prop.m_cornerAngle;
                                            if (num7 != 0f && prop.m_position.x != 0f) {
                                                float num9 = Mathf.Tan(num7);
                                                worldPos.x += dir.x * num9 * prop.m_position.x;
                                                worldPos.z += dir.z * num9 * prop.m_position.x;
                                            }
                                        }
                                        if (variation.m_requireWaterMap) {
                                            worldPos.y = Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(worldPos, false, 0f);
                                        } else {
                                            worldPos.y = Singleton<TerrainManager>.instance.SampleDetailHeight(worldPos);
                                        }
                                        data.m_extraData.SetUShort(currentPropIndex++, (ushort)Mathf.Clamp(Mathf.RoundToInt(worldPos.y * 64f), 0, 65535));
                                    }
                                }
                            }
                        }
                        TreeInfo treeInfo = prop.m_finalTree;
                        if (treeInfo != null) {
                            if (prop.m_upgradable) {
                                TreeInfo treeInfo2 = Singleton<NetManager>.instance.m_segments.m_buffer[(int)segmentID].TreeInfo;
                                if (treeInfo2 != null) {
                                    treeInfo = treeInfo2;
                                }
                            }
                            Randomizer randomizer2 = new Randomizer((int)(laneID + (uint)i));
                            for (int k = 1; k <= repeatCountTimes2; k += 2) {
                                if (randomizer2.Int32(100U) < prop.m_probability) {
                                    float t = halfSegmentOffset + (float)k / (float)repeatCountTimes2;
                                    treeInfo.GetVariation(ref randomizer2);
                                    randomizer2.Int32(10000U);
                                    randomizer2.Int32(10000U);
                                    Vector3 worldPos2 = this.m_bezier.Position(t);
                                    worldPos2.y += prop.m_position.y;
                                    if (prop.m_position.x != 0f) {
                                        Vector3 dir = this.m_bezier.Tangent(t);
                                        if (reverse) {
                                            dir = -dir;
                                        }
                                        dir.y = 0f;
                                        dir = Vector3.Normalize(dir);
                                        worldPos2.x += dir.z * prop.m_position.x;
                                        worldPos2.z -= dir.x * prop.m_position.x;
                                    }
                                    worldPos2.y = Singleton<TerrainManager>.instance.SampleDetailHeight(worldPos2);
                                    data.m_extraData.SetUShort(currentPropIndex++, (ushort)Mathf.Clamp(Mathf.RoundToInt(worldPos2.y * 64f), 0, 65535));
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}