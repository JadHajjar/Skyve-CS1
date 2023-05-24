namespace KianCommons.StockCode._VehicleAI {
    using ColossalFramework;
    using ColossalFramework.Math;
    using System;
    using UnityEngine;

    public class CarAI2 : VehicleAI {

        private static float CalculateMaxSpeed(float targetDistance, float targetSpeed, float maxBraking) {
            float num = 0.5f * maxBraking;
            float num2 = num + targetSpeed;
            return Mathf.Sqrt(Mathf.Max(0f, num2 * num2 + 2f * targetDistance * maxBraking)) - num;
        }
        private static bool DisableCollisionCheck(ushort vehicleID, ref Vehicle vehicleData) {
            if ((vehicleData.m_flags & Vehicle.Flags.Arriving) != 0 && Mathf.Max(Mathf.Abs(vehicleData.m_targetPos3.x), Mathf.Abs(vehicleData.m_targetPos3.z)) > 8640f - 100f) {
                return true;
            }
            return false;
        }


        public override void SimulationStep(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics) {
            if ((leaderData.m_flags2 & Vehicle.Flags2.Blown) != 0) {
                SimulationStepBlown(vehicleID, ref vehicleData, ref frameData, leaderID, ref leaderData, lodPhysics);
                return;
            }
            if ((leaderData.m_flags2 & Vehicle.Flags2.Floating) != 0) {
                SimulationStepFloating(vehicleID, ref vehicleData, ref frameData, leaderID, ref leaderData, lodPhysics);
                return;
            }
            uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
            frameData.m_position += frameData.m_velocity * 0.5f;
            frameData.m_swayPosition += frameData.m_swayVelocity * 0.5f;
            float max_acceleratrion = m_info.m_acceleration;
            float max_breaking = m_info.m_braking;
            if ((vehicleData.m_flags & Vehicle.Flags.Emergency2) != 0) {
                max_acceleratrion *= 2f;
                max_breaking *= 2f;
            }
            float speed = frameData.m_velocity.magnitude;
            Vector3 deltapos = (Vector3)vehicleData.m_targetPos0 - frameData.m_position;
            float distanceSqr = deltapos.sqrMagnitude;
            float num3 = (speed + max_acceleratrion) * (0.5f + 0.5f * (speed + max_acceleratrion) / max_breaking) + m_info.m_generatedInfo.m_size.z * 0.5f;
            float num4 = Mathf.Max(speed + max_acceleratrion, 5f);
            if (lodPhysics >= 2 && ((currentFrameIndex >> 4) & 3) == (vehicleID & 3)) {
                num4 *= 2f;
            }
            float num5 = Mathf.Max((num3 - num4) / 3f, 1f);
            float num6 = num4 * num4;
            float num7 = num5 * num5;
            int index = 0;
            bool flag = false;
            if ((distanceSqr < num6 || vehicleData.m_targetPos3.w < 0.01f) && (leaderData.m_flags & (Vehicle.Flags.WaitingPath | Vehicle.Flags.Stopped)) == 0) {
                if (leaderData.m_path != 0) {
                    UpdatePathTargetPositions(vehicleID, ref vehicleData, frameData.m_position, ref index, 4, num6, num7);
                    if ((leaderData.m_flags & Vehicle.Flags.Spawned) == 0) {
                        frameData = vehicleData.m_frame0;
                        return;
                    }
                }
                if ((leaderData.m_flags & Vehicle.Flags.WaitingPath) == 0) {
                    while (index < 4) {
                        float minSqrDistance;
                        Vector3 refPos;
                        if (index == 0) {
                            minSqrDistance = num6;
                            refPos = frameData.m_position;
                            flag = true;
                        } else {
                            minSqrDistance = num7;
                            refPos = vehicleData.GetTargetPos(index - 1);
                        }
                        int num8 = index;
                        UpdateBuildingTargetPositions(vehicleID, ref vehicleData, refPos, leaderID, ref leaderData, ref index, minSqrDistance);
                        if (index == num8) {
                            break;
                        }
                    }
                    if (index != 0) {
                        Vector4 targetPos = vehicleData.GetTargetPos(index - 1);
                        while (index < 4) {
                            vehicleData.SetTargetPos(index++, targetPos);
                        }
                    }
                }
                deltapos = (Vector3)vehicleData.m_targetPos0 - frameData.m_position;
                distanceSqr = deltapos.sqrMagnitude;
            }
            if (leaderData.m_path != 0 && (leaderData.m_flags & Vehicle.Flags.WaitingPath) == 0) {
                NetManager instance = Singleton<NetManager>.instance;
                byte b = leaderData.m_pathPositionIndex;
                byte lastPathOffset = leaderData.m_lastPathOffset;
                if (b == byte.MaxValue) {
                    b = 0;
                }
                int totalNoise;
                float num9 = 1f + leaderData.CalculateTotalLength(leaderID, out totalNoise);
                PathManager instance2 = Singleton<PathManager>.instance;
                if (instance2.m_pathUnits.m_buffer[leaderData.m_path].GetPosition(b >> 1, out var position)) {
                    if ((instance.m_segments.m_buffer[position.m_segment].m_flags & NetSegment.Flags.Flooded) != 0 && Singleton<TerrainManager>.instance.HasWater(VectorUtils.XZ(frameData.m_position))) {
                        leaderData.m_flags2 |= Vehicle.Flags2.Floating;
                    }
                    instance.m_segments.m_buffer[position.m_segment].AddTraffic(Mathf.RoundToInt(num9 * 2.5f), totalNoise);
                    bool flag2 = false;
                    if ((b & 1) == 0 || lastPathOffset == 0) {
                        uint laneID = PathManager.GetLaneID(position);
                        if (laneID != 0) {
                            Vector3 b2 = instance.m_lanes.m_buffer[laneID].CalculatePosition((float)(int)position.m_offset * 0.003921569f);
                            float num10 = 0.5f * speed * speed / max_breaking;
                            float z = m_info.m_generatedInfo.m_size.z;
                            if (Vector3.Distance(frameData.m_position, b2) >= num10 + z * 0.5f - 1f) {
                                instance.m_lanes.m_buffer[laneID].ReserveSpace(num9);
                                flag2 = true;
                            }
                        }
                    }
                    if (!flag2 && instance2.m_pathUnits.m_buffer[leaderData.m_path].GetNextPosition(b >> 1, out position)) {
                        uint laneID2 = PathManager.GetLaneID(position);
                        if (laneID2 != 0) {
                            instance.m_lanes.m_buffer[laneID2].ReserveSpace(num9);
                        }
                    }
                }
                if (((currentFrameIndex >> 4) & 0xF) == (leaderID & 0xF)) {
                    bool flag3 = false;
                    uint unitID = leaderData.m_path;
                    int index2 = b >> 1;
                    int num11 = 0;
                    while (num11 < 5) {
                        if (PathUnit.GetNextPosition(ref unitID, ref index2, out position, out var invalid)) {
                            uint laneID3 = PathManager.GetLaneID(position);
                            if (laneID3 != 0 && !instance.m_lanes.m_buffer[laneID3].CheckSpace(num9)) {
                                num11++;
                                continue;
                            }
                        }
                        if (invalid) {
                            InvalidPath(vehicleID, ref vehicleData, leaderID, ref leaderData);
                        }
                        flag3 = true;
                        break;
                    }
                    if (!flag3) {
                        leaderData.m_flags |= Vehicle.Flags.Congestion;
                    }
                }
            }
            float maxSpeed;
            if ((leaderData.m_flags & Vehicle.Flags.Stopped) != 0) {
                maxSpeed = 0f;
            } else {
                maxSpeed = vehicleData.m_targetPos0.w;
                if ((leaderData.m_flags & Vehicle.Flags.DummyTraffic) == 0) {
                    VehicleManager instance3 = Singleton<VehicleManager>.instance;
                    instance3.m_totalTrafficFlow += (uint)Mathf.RoundToInt(speed * 100f / Mathf.Max(1f, vehicleData.m_targetPos0.w));
                    instance3.m_maxTrafficFlow += 100u;
                }
            }
            Quaternion quaternion = Quaternion.Inverse(frameData.m_rotation);
            deltapos = quaternion * deltapos;
            Vector3 vector2 = quaternion * frameData.m_velocity;
            Vector3 vector3 = Vector3.forward;
            Vector3 zero = Vector3.zero;
            Vector3 collisionPush = Vector3.zero;
            float num12 = 0f;
            float num13 = 0f;
            bool blocked = false;
            float len = 0f;
            if (distanceSqr > 1f) {
                vector3 = VectorUtils.NormalizeXZ(deltapos, out len);
                if (len > 1f) {
                    Vector3 v = deltapos;
                    num4 = Mathf.Max(speed, 2f);
                    if (distanceSqr > num4 * num4) {
                        v *= num4 / Mathf.Sqrt(distanceSqr);
                    }
                    bool flag4 = false;
                    if (v.z < Mathf.Abs(v.x)) {
                        if (v.z < 0f) {
                            flag4 = true;
                        }
                        float num14 = Mathf.Abs(v.x);
                        if (num14 < 1f) {
                            v.x = Mathf.Sign(v.x);
                            if (v.x == 0f) {
                                v.x = 1f;
                            }
                            num14 = 1f;
                        }
                        v.z = num14;
                    }
                    vector3 = VectorUtils.NormalizeXZ(v, out var len2);
                    len = Mathf.Min(len, len2);
                    float num15 = (float)Math.PI / 2f * (1f - vector3.z);
                    if (len > 1f) {
                        num15 /= len;
                    }
                    float num16 = len;
                    maxSpeed = ((!(vehicleData.m_targetPos0.w < 0.1f)) ? Mathf.Min(Mathf.Min(maxSpeed, CalculateTargetSpeed(vehicleID, ref vehicleData, 1000f, num15)), CalculateMaxSpeed(num16, vehicleData.m_targetPos1.w, max_breaking * 0.9f)) : Mathf.Min(CalculateTargetSpeed(vehicleID, ref vehicleData, 1000f, num15), CalculateMaxSpeed(num16, Mathf.Min(vehicleData.m_targetPos0.w, vehicleData.m_targetPos1.w), max_breaking * 0.9f)));
                    num16 += VectorUtils.LengthXZ(vehicleData.m_targetPos1 - vehicleData.m_targetPos0);
                    maxSpeed = Mathf.Min(maxSpeed, CalculateMaxSpeed(num16, vehicleData.m_targetPos2.w, max_breaking * 0.9f));
                    num16 += VectorUtils.LengthXZ(vehicleData.m_targetPos2 - vehicleData.m_targetPos1);
                    maxSpeed = Mathf.Min(maxSpeed, CalculateMaxSpeed(num16, vehicleData.m_targetPos3.w, max_breaking * 0.9f));
                    num16 += VectorUtils.LengthXZ(vehicleData.m_targetPos3 - vehicleData.m_targetPos2);
                    if (vehicleData.m_targetPos3.w < 0.01f) {
                        num16 = Mathf.Max(0f, num16 - m_info.m_generatedInfo.m_size.z * 0.5f);
                    }
                    maxSpeed = Mathf.Min(maxSpeed, CalculateMaxSpeed(num16, 0f, max_breaking * 0.9f));
                    if (!DisableCollisionCheck(leaderID, ref leaderData)) {
                        CarAI.CheckOtherVehicles(vehicleID, ref vehicleData, ref frameData, ref maxSpeed, ref blocked, ref collisionPush, num3, max_breaking * 0.9f, lodPhysics);
                    }
                    if (flag4) {
                        maxSpeed = 0f - maxSpeed;
                    }
                    num12 = ((!(maxSpeed < speed)) ? Mathf.Min(b: speed + Mathf.Max(max_acceleratrion, Mathf.Min(max_breaking, 0f - speed)), a: maxSpeed) : Mathf.Max(b: speed - Mathf.Max(max_acceleratrion, Mathf.Min(max_breaking, speed)), a: maxSpeed));
                }
            } else if (speed < 0.1f && flag && ArriveAtDestination(leaderID, ref leaderData)) {
                leaderData.Unspawn(leaderID);
                if (leaderID == vehicleID) {
                    frameData = leaderData.m_frame0;
                }
                return;
            }
            if ((leaderData.m_flags & Vehicle.Flags.Stopped) == 0 && maxSpeed < 0.1f) {
                blocked = true;
            }
            if (blocked) {
                vehicleData.m_blockCounter = (byte)Mathf.Min(vehicleData.m_blockCounter + 1, 255);
            } else {
                vehicleData.m_blockCounter = 0;
            }
            if (len > 1f) {
                num13 = Mathf.Asin(vector3.x) * Mathf.Sign(num12);
                zero = vector3 * num12;
            } else {
                num12 = 0f;
                zero = vector2 + Vector3.ClampMagnitude(deltapos * 0.5f - vector2, max_breaking);
            }
            bool flag5 = ((currentFrameIndex + leaderID) & 0x10) != 0;
            Vector3 vector4 = zero - vector2;
            Vector3 vector5 = frameData.m_rotation * zero;
            frameData.m_velocity = vector5 + collisionPush;
            frameData.m_position += frameData.m_velocity * 0.5f;
            frameData.m_swayVelocity = frameData.m_swayVelocity * (1f - m_info.m_dampers) - vector4 * (1f - m_info.m_springs) - frameData.m_swayPosition * m_info.m_springs;
            frameData.m_swayPosition += frameData.m_swayVelocity * 0.5f;
            frameData.m_steerAngle = num13;
            frameData.m_travelDistance += zero.z;
            frameData.m_lightIntensity.x = 5f;
            frameData.m_lightIntensity.y = ((!(vector4.z < -0.1f)) ? 0.5f : 5f);
            frameData.m_lightIntensity.z = ((!(num13 < -0.1f) || !flag5) ? 0f : 5f);
            frameData.m_lightIntensity.w = ((!(num13 > 0.1f) || !flag5) ? 0f : 5f);
            frameData.m_underground = (vehicleData.m_flags & Vehicle.Flags.Underground) != 0;
            frameData.m_transition = (vehicleData.m_flags & Vehicle.Flags.Transition) != 0;
            if ((vehicleData.m_flags & Vehicle.Flags.Parking) != 0 && len <= 1f && flag) {
                Vector3 forward = vehicleData.m_targetPos1 - vehicleData.m_targetPos0;
                if (forward.sqrMagnitude > 0.01f) {
                    frameData.m_rotation = Quaternion.LookRotation(forward);
                }
            } else if (num12 > 0.1f) {
                if (vector5.sqrMagnitude > 0.01f) {
                    frameData.m_rotation = Quaternion.LookRotation(vector5);
                }
            } else if (num12 < -0.1f && vector5.sqrMagnitude > 0.01f) {
                frameData.m_rotation = Quaternion.LookRotation(-vector5);
            }
            base.SimulationStep(vehicleID, ref vehicleData, ref frameData, leaderID, ref leaderData, lodPhysics);
        }
    }
}
