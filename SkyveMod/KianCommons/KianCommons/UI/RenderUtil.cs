using ColossalFramework;
using ColossalFramework.Math;
using KianCommons.Math;
using UnityEngine;
using UnityEngine.Experimental.Director;

namespace KianCommons.UI {
    internal static class RenderUtil {
        /// <summary>
        /// Draws a half sausage at segment end.
        /// </summary>
        /// <param name="segmentId"></param>
        /// <param name="cut">The length of the highlight [0~1] </param>
        /// <param name="bStartNode">Determines the direction of the half sausage.</param>
        public static void DrawCutSegmentEnd(RenderManager.CameraInfo cameraInfo,
                       ushort segmentId,
                       float cut,
                       bool bStartNode,
                       Color color,
                       bool alpha = false) {
            if (!NetUtil.IsSegmentValid(segmentId))
                return;

            ref NetSegment segment = ref segmentId.ToSegment();
            float halfWidth = segment.Info.m_halfWidth;

            NetNode[] nodeBuffer = Singleton<NetManager>.instance.m_nodes.m_buffer;
            bool IsMiddle(ushort nodeId) => (nodeBuffer[nodeId].m_flags & NetNode.Flags.Middle) != 0;

            Bezier3 bezier;
            bezier.a = segment.m_startNode.ToNode().m_position;
            bezier.d = segment.m_endNode.ToNode().m_position;

            NetSegment.CalculateMiddlePoints(
                bezier.a,
                segment.m_startDirection,
                bezier.d,
                segment.m_endDirection,
                IsMiddle(segment.m_startNode),
                IsMiddle(segment.m_endNode),
                out bezier.b,
                out bezier.c);

            if (bStartNode) {
                bezier = bezier.Cut(0, cut);
            } else {
                bezier = bezier.Cut(1 - cut, 1);
            }

            Singleton<ToolManager>.instance.m_drawCallData.m_overlayCalls++;
            Singleton<RenderManager>.instance.OverlayEffect.DrawBezier(
                cameraInfo,
                color,
                bezier,
                halfWidth * 2f,
                bStartNode ? 0 : halfWidth,
                bStartNode ? halfWidth : 0,
                -1f,
                1280f,
                false,
                alpha);
        }

        public static void RenderStripOnSegment(RenderManager.CameraInfo cameraInfo,
                   ushort segmentId,
                   Vector3 pos,
                   float width,
                   Color color,
                   bool alpha = false) {
            if (!NetUtil.IsSegmentValid(segmentId))
                return;

            ref NetSegment segment = ref segmentId.ToSegment();
            float halfWidth = segment.Info.m_halfWidth;
            Bezier3 bezier = segment.CalculateSegmentBezier3();
            float len = bezier.ArcLength();
            float delta_t = 0.5f * width / len;

            float t = bezier.GetClosestT(pos);
            float t1 = t - delta_t;
            float t2 = t + delta_t;
            if (t1 < 0) t1 = 0;
            if (t2 > 1) t2 = 1;
            bezier = bezier.Cut(t1, t2);

            Singleton<ToolManager>.instance.m_drawCallData.m_overlayCalls++;
            Singleton<RenderManager>.instance.OverlayEffect.DrawBezier(
                cameraInfo,
                color,
                bezier,
                halfWidth * 2f,
                halfWidth,
                halfWidth,
                -1f,
                1280f,
                false,
                alpha);
        }


        public static void RenderSegmnetOverlay(
            RenderManager.CameraInfo cameraInfo,
            ushort segmentId,
            Color color,
            bool alphaBlend = false) {
            if (!NetUtil.IsSegmentValid(segmentId))
                return;

            ref NetSegment segment = ref segmentId.ToSegment();
            float hw = segment.Info.m_halfWidth;

            NetNode[] nodeBuffer = Singleton<NetManager>.instance.m_nodes.m_buffer;
            bool IsMiddle(ushort nodeId) => (nodeBuffer[nodeId].m_flags & NetNode.Flags.Middle) != 0;

            Bezier3 bezier;
            bezier.a = segment.m_startNode.ToNode().m_position;
            bezier.d = segment.m_endNode.ToNode().m_position;

            NetSegment.CalculateMiddlePoints(
                bezier.a,
                segment.m_startDirection,
                bezier.d,
                segment.m_endDirection,
                IsMiddle(segment.m_startNode),
                IsMiddle(segment.m_endNode),
                out bezier.b,
                out bezier.c);

            bezier.Render(cameraInfo, color, hw, alphaBlend);
        }

        public static void RenderLaneOverlay(
            RenderManager.CameraInfo cameraInfo,
            LaneData laneData,
            Color color,
            bool alphaBlend = false) {
            float hw;
            try { hw = laneData.LaneInfo.m_width * 0.5f; } catch { hw = 0.5f; }
            laneData.Bezier.Render(cameraInfo, color, hw, alphaBlend);
        }

        public static void RenderInstanceOverlay(
            RenderManager.CameraInfo cameraInfo,
            InstanceID instanceID,
            Color color,
            bool alphaBlend = false) {
            if (!instanceID.IsValid())
                return;
            switch (instanceID.Type) {
                case InstanceType.NetLane:
                    LaneData laneData = NetUtil.GetLaneData(instanceID.NetLane);
                    RenderUtil.RenderLaneOverlay(cameraInfo, laneData, color, alphaBlend);
                    break;
                case InstanceType.NetSegment:
                    RenderUtil.RenderSegmnetOverlay(cameraInfo, instanceID.NetSegment, color, alphaBlend);
                    break;
                case InstanceType.NetNode:
                    RenderUtil.DrawNodeCircle(cameraInfo, color, instanceID.NetNode, alphaBlend);
                    break;
                default:
                    Log.Error("Unexpected InstanceID.Type: " + instanceID.Type);
                    return;
            }
        }

        public static void DrawNodeCircle(RenderManager.CameraInfo cameraInfo,
                           Color color,
                           ushort nodeId,
                           bool alphaBlend = false) {
            Vector3 pos = nodeId.ToNode().m_position;
            float r = 0;
            int n = 0;
            foreach (ushort segmentId in NetUtil.IterateNodeSegments(nodeId)) {
                r += segmentId.ToSegment().Info.m_halfWidth;
                n++;
            }
            r = r / n;
            DrawOverlayCircle(cameraInfo, color, pos, r, alphaBlend);
        }

        public static void DrawOverlayCircle(RenderManager.CameraInfo cameraInfo,
                               Color color,
                               Vector3 position,
                               float raduis,
                               bool alphaBlend = false) {
            Singleton<ToolManager>.instance.m_drawCallData.m_overlayCalls++;
            Singleton<RenderManager>.instance.OverlayEffect.DrawCircle(
                cameraInfo,
                color,
                position,
                raduis * 2,
                position.y - 100f,
                position.y + 100f,
                false,
                alphaBlend);
        }

        public static void Render(this Bezier3 bezier, RenderManager.CameraInfo cameraInfo,
            Color color, float hw, bool alphaBlend = false, bool cutEnds = true) {
            Singleton<ToolManager>.instance.m_drawCallData.m_overlayCalls++;
            float cut = cutEnds ? hw : 0;
            RenderManager.instance.OverlayEffect.DrawBezier(
                cameraInfo, color,
                bezier, hw * 2,
                cut, cut, -1, 1024, false, alphaBlend);

        }

        public static void RenderArrow(this Bezier3 bezier, RenderManager.CameraInfo cameraInfo,
            Color color, float hw, bool alphaBlend = false) {
            bezier.Render(cameraInfo, color, hw, alphaBlend, cutEnds: true);

            Vector2 center = bezier.d.ToCS2D();
            Vector2 dira = bezier.Tangent(1).ToCS2D().normalized * hw * 2;

            var dirb = dira.Rotate90CW();
            var dirc = dira.Rotate90CCW();

            Quad2 quad = new Quad2 {
                a = center + dira,
                b = center + dirb,
                c = center + dirc,
                d = center + dirc,
            };

            Singleton<ToolManager>.instance.m_drawCallData.m_overlayCalls++;
            RenderManager.instance.OverlayEffect.DrawQuad(
                cameraInfo, color, quad.ToCS3D(), 
                -1, 1024, false, alphaBlend);

        }

        public static void RenderLine(Segment3 line, RenderManager.CameraInfo cameraInfo, Color color) {
            Singleton<ToolManager>.instance.m_drawCallData.m_overlayCalls++;
            RenderManager.instance.OverlayEffect.DrawSegment(cameraInfo, color, line, 0,
                0,
                -1, 1024, false, true);
        }

        public static void RenderLine(Vector2 a, Vector2 b, RenderManager.CameraInfo cameraInfo, Color color) {
            var line = new Segment3 {
                a = NetUtil.Get3DPos(a),
                b = NetUtil.Get3DPos(a),
            };
            RenderLine(line, cameraInfo, color);
        }

        public static void RenderGrid(RenderManager.CameraInfo cameraInfo, GridVector grid, Color color) {
            var corner0 = grid.GetGirdStartCorner();
            var corner3 = new GridVector(grid.x + 1, grid.y + 1).GetGirdStartCorner();
            var corner1 = new Vector2(corner0.x, corner3.y);
            var corner2 = new Vector2(corner3.x, corner0.y);
            RenderLine(corner0, corner1, cameraInfo, color);
            RenderLine(corner0, corner2, cameraInfo, color);
            RenderLine(corner3, corner1, cameraInfo, color);
            RenderLine(corner3, corner2, cameraInfo, color);
        }

        public static void RenderGrids(RenderManager.CameraInfo cameraInfo, Vector3 pos, Color color) {
            var grid = new GridVector(pos);
            for (int dx = -2; dx <= +2; ++dx) {
                for (int dy = -2; dy <= +2; ++dy) {
                    var grid2 = grid;
                    grid2.x += dx;
                    grid2.y += dy;
                    RenderGrid(cameraInfo, grid2, color);
                }
            }
        }
    }
}
