namespace KianCommons.Math {
    using UnityEngine;
    using KianCommons.Math;

    internal static class LineUtil {
        public static bool IntersectLine(Vector2 A, Vector2 B, Vector2 C, Vector2 D, out Vector2 center) {
            // Line AB represented as a1x + b1y = c1 
            float a1 = B.y - A.y;
            float b1 = A.x - B.x;
            float c1 = a1 * (A.x) + b1 * (A.y);

            // Line CD represented as a2x + b2y = c2 
            float a2 = D.y - C.y;
            float b2 = C.x - D.x;
            float c2 = a2 * (C.x) + b2 * (C.y);

            float determinant = a1 * b2 - a2 * b1; // TODO VectorUtil.Determinent(A,B);

            if (MathUtil.EqualAprox(determinant, 0)) {
                // The lines are parallel. This is simplified 
                center = Vector2.zero;
                return false;
            } else {
                center.x = (b2 * c1 - b1 * c2) / determinant;
                center.y = (a1 * c2 - a2 * c1) / determinant;
                return true;
            }
        }
        public static bool Intersect(Vector2 point1, Vector2 dir1, Vector2 point2, Vector2 dir2, out Vector2 center) {
            return IntersectLine(
                point1, point1 + dir1,
                point2, point2 + dir2,
                out center);
        }

        public static Vector3 GetClosestPoint(Vector3 A, Vector3 B, Vector3 P) {
            var AP = P - A;
            var AB = B - A;
            var dot = Vector3.Dot(AP, AB);
            var t = dot / AB.sqrMagnitude; // The normalized "distance" from a to  your closest point
            return A + AB * t;
        }
    }
}
