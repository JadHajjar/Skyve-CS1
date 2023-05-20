using ColossalFramework.UI;
using UnityEngine;
using System;
using ColossalFramework;

namespace KianCommons.Tool {
    internal abstract class KianToolBase<T> : KianToolBase where T : ToolBase {
        public static T Instance { get; private set; }
        public static T Create() => Instance = ToolsModifierControl.toolController.gameObject.AddComponent<T>();

        public static void Release() {
            try { DestroyImmediate(Instance); } catch (Exception ex) { ex.Log(); }
            Instance = null;
        }
    }

    internal abstract class KianToolBase : ToolBase  {
        bool leftMouseWasDown_;
        public static Vector3 MousePosition => Input.mousePosition;
        public static Ray MouseRay => Camera.main.ScreenPointToRay(MousePosition);
        public static float MouseRayLength => Camera.main.farClipPlane;
        public static bool MouseRayValid => !UIView.IsInsideUI() && Cursor.visible;

        public ushort HoveredNodeID { get; private set; } = 0;
        public ushort HoveredSegmentID { get; private set; } = 0;
        public bool HoveredStartNode => HoveredSegmentID.ToSegment().IsStartNode(HoveredNodeID);

        public Vector3 HitPos { get; private set; }

        protected bool IsMouseRayValid => !UIView.IsInsideUI() && Cursor.visible && MouseRayValid;
        protected bool HoverValid => IsMouseRayValid && (HoveredSegmentID != 0 || HoveredNodeID != 0);

        protected override void OnDisable() {
            base.OnDisable();
            if(ToolsModifierControl.toolController.CurrentTool == this)
                ToolsModifierControl.SetTool<DefaultTool>();
        }

        protected abstract void OnPrimaryMouseClicked();
        protected abstract void OnSecondaryMouseClicked();
        static InfoManager infoMan => Singleton<InfoManager>.instance;
        public static void SetUnderGroundView() => infoMan.SetCurrentMode(
            InfoManager.InfoMode.Underground, InfoManager.SubInfoMode.UndergroundTunnels);
        public static void SetOverGroundView() => infoMan.SetCurrentMode(
            InfoManager.InfoMode.None, InfoManager.SubInfoMode.None);

        protected virtual void OnPageDown() {
            Log.Info("KianToolBase.OnPageDown()");
            if (MouseRayValid)
                SetUnderGroundView();
        }

        protected virtual void OnPageUp() {
            if (MouseRayValid)
                SetOverGroundView();
        }

        protected override void OnToolUpdate() {
            base.OnToolUpdate();

            DetermineHoveredElements();

            if (Input.GetMouseButtonUp(0)) {
                if (leftMouseWasDown_)
                    OnPrimaryMouseClicked();
            } else if (Input.GetMouseButtonDown(0)) {
                leftMouseWasDown_ = HoverValid;
            } else if (Input.GetMouseButtonDown(1)) {
                OnSecondaryMouseClicked();
                leftMouseWasDown_ = false;
            } else if (UIView.library.Get("PauseMenu").isVisible) {
                UIView.library.Hide("PauseMenu");
                enabled = false;
            } else if (Input.GetKeyUp(KeyCode.PageUp)) {
                OnPageUp();
            } else if (Input.GetKeyUp(KeyCode.PageDown)) {
                OnPageDown();
            }

            if (Input.GetMouseButton(1) || !HoverValid) {
                leftMouseWasDown_ = false; // cancel
            }
        }

        private bool DetermineHoveredElements() {
            try {
                HoveredSegmentID = 0;
                HoveredNodeID = 0;
                HitPos = Vector3.zero;
                if (!IsMouseRayValid)
                    return false;

                // find currently hovered node
                RaycastInput nodeInput = new RaycastInput(MouseRay, MouseRayLength) {
                    m_netService = GetService(),
                    m_ignoreTerrain = true,
                    m_ignoreNodeFlags = NetNode.Flags.None
                };

                if (RayCast(nodeInput, out RaycastOutput nodeOutput)) {
                    HoveredNodeID = nodeOutput.m_netNode;
                    HitPos = nodeOutput.m_hitPos;
                }

                HoveredSegmentID = GetSegmentFromNode();

                if (HoveredSegmentID != 0) {
                    Debug.Assert(HoveredNodeID != 0, "unexpected: HoveredNodeId == 0");
                    return true;
                }

                // find currently hovered segment
                var segmentInput = new RaycastInput(MouseRay, MouseRayLength) {
                    m_netService = GetService(),
                    m_ignoreTerrain = true,
                    m_ignoreSegmentFlags = NetSegment.Flags.None
                };

                if (RayCast(segmentInput, out RaycastOutput segmentOutput)) {
                    HoveredSegmentID = segmentOutput.m_netSegment;
                    HitPos = segmentOutput.m_hitPos;
                }

                if (HoveredNodeID <= 0 && HoveredSegmentID > 0) {
                    // alternative way to get a node hit: check distance to start and end nodes
                    // of the segment
                    ushort startNodeId = HoveredSegmentID.ToSegment().m_startNode;
                    ushort endNodeId = HoveredSegmentID.ToSegment().m_endNode;

                    var vStart = segmentOutput.m_hitPos - startNodeId.ToNode().m_position;
                    var vEnd = segmentOutput.m_hitPos - endNodeId.ToNode().m_position;

                    float startDist = vStart.magnitude;
                    float endDist = vEnd.magnitude;

                    if (startDist < endDist && startDist < 75f) {
                        HoveredNodeID = startNodeId;
                    } else if (endDist < startDist && endDist < 75f) {
                        HoveredNodeID = endNodeId;
                    }
                }

                return HoveredNodeID != 0 || HoveredSegmentID != 0;
            } catch (Exception ex) {
                ex.Log(false);
                return false;
            }
        }

        static float GetAgnele(Vector3 v1, Vector3 v2) {
            float ret = Vector3.Angle(v1, v2);
            if (ret > 180) ret -= 180; //future proofing
            ret = System.Math.Abs(ret);
            return ret;
        }

        ushort GetSegmentFromNode() {
            bool considerSegmentLenght = false;
            ushort minSegId = 0;
            if (HoveredNodeID != 0) {
                NetNode node = HoveredNodeID.ToNode();
                Vector3 dir0 = node.m_position - MousePosition;
                float min_angle = float.MaxValue;
                for (int i = 0; i < 8; ++i) {
                    ushort segmentId = node.GetSegment(i);
                    if (segmentId == 0)
                        continue;
                    NetSegment segment = segmentId.ToSegment();
                    Vector3 dir;
                    if (segment.m_startNode == HoveredNodeID) {
                        dir = segment.m_startDirection;

                    } else {
                        dir = segment.m_endDirection;
                    }
                    float angle = GetAgnele(-dir, dir0);
                    if (considerSegmentLenght)
                        angle *= segment.m_averageLength;
                    if (angle < min_angle) {
                        min_angle = angle;
                        minSegId = segmentId;
                    }
                }
            }
            return minSegId;
        }

        //  copy modified from DefaultTool.GetService()
        public virtual RaycastService GetService() {
            var currentMode = Singleton<InfoManager>.instance.CurrentMode;
            var currentSubMode = Singleton<InfoManager>.instance.CurrentSubMode;
            ItemClass.Availability avaliblity = Singleton<ToolManager>.instance.m_properties.m_mode;
            if ((avaliblity & ItemClass.Availability.MapAndAsset) == ItemClass.Availability.None) {
                switch (currentMode) {
                    case InfoManager.InfoMode.TrafficRoutes:
                    case InfoManager.InfoMode.Tours:
                        break;
                    case InfoManager.InfoMode.Underground:
                        if (currentSubMode == InfoManager.SubInfoMode.Default) {
                            return new RaycastService { m_itemLayers = ItemClass.Layer.MetroTunnels };
                        }
                        // ignore water pipes:
                        return new RaycastService { m_itemLayers = ItemClass.Layer.Default };
                    default:
                        if (currentMode != InfoManager.InfoMode.Water) {
                            if (currentMode == InfoManager.InfoMode.Transport) {
                                return new RaycastService(
                                    ItemClass.Service.PublicTransport,
                                    ItemClass.SubService.None,
                                    ItemClass.Layer.Default | ItemClass.Layer.MetroTunnels
                                    /*| ItemClass.Layer.MetroTunnels | ItemClass.Layer.BlimpPaths | ItemClass.Layer.HelicopterPaths | ItemClass.Layer.FerryPaths*/
                                    );
                            }
                            if (currentMode == InfoManager.InfoMode.Traffic) {
                                break;
                            }
                            if (currentMode != InfoManager.InfoMode.Heating) {
                                return new RaycastService { m_itemLayers = ItemClass.Layer.Default };
                            }
                        }
                        // ignore water pipes:
                        //return new RaycastService(ItemClass.Service.Water, ItemClass.SubService.None, ItemClass.Layer.Default | ItemClass.Layer.WaterPipes);
                        return new RaycastService { m_itemLayers = ItemClass.Layer.Default };
                    case InfoManager.InfoMode.Fishing:
                        // ignore fishing
                        //return new RaycastService(ItemClass.Service.Fishing, ItemClass.SubService.None, ItemClass.Layer.Default | ItemClass.Layer.FishingPaths);
                        return new RaycastService { m_itemLayers = ItemClass.Layer.Default };
                }
                return new RaycastService { m_itemLayers = ItemClass.Layer.Default | ItemClass.Layer.MetroTunnels };
            }
            if (currentMode != InfoManager.InfoMode.Underground) {
                if (currentMode != InfoManager.InfoMode.Tours) {
                    if (currentMode == InfoManager.InfoMode.Transport) {
                        return new RaycastService {
                            m_itemLayers = ItemClass.Layer.Default | ItemClass.Layer.MetroTunnels
                            /*| ItemClass.Layer.AirplanePaths | ItemClass.Layer.ShipPaths | ItemClass.Layer.Markers*/
                        };
                    }
                    if (currentMode != InfoManager.InfoMode.Traffic) {
                        return new RaycastService { m_itemLayers = ItemClass.Layer.Default | ItemClass.Layer.Markers };
                    }
                }
                return new RaycastService {
                    m_itemLayers = ItemClass.Layer.Default | ItemClass.Layer.MetroTunnels | ItemClass.Layer.Markers
                };
            }
            return new RaycastService { m_itemLayers = ItemClass.Layer.MetroTunnels };
        }


    }
}