using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KianCommons.Math {
    // start dir and end dir are pointing inwards.
    internal struct CubicBezier3 {
        public ControlPoint3 Start;
        public ControlPoint3 End;

        public CubicBezier3(ControlPoint3 start, ControlPoint3 end) {
            Start = start;
            End = end;
        }

        // result points on the path toward the end point.
        public ControlPoint3 GetCenterPointAndDir() {
            var point1 = LineUtil.GetClosestPoint(Start.Point, Start.Point + Start.Dir, End.Point);
            var point2 = LineUtil.GetClosestPoint(End.Point, End.Point + End.Dir, Start.Point);

            point1 = point1 * 0.25f + Start.Point * 0.75f; // lerp
            point2 = point2 * 0.25f + End.Point * 0.75f; // lerp

            ControlPoint3 ret = new ControlPoint3 {
                Point = (point1 + point2) * 0.5f,
                Dir = (point2 - point1).normalized,
            };
            return ret;
        }
    }
}
