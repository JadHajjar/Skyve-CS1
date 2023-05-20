namespace KianCommons.Math {
    using UnityEngine;

    internal struct ControlPoint3 {
        public Vector3 Point;
        public Vector3 Dir;
        public ControlPoint3(Vector3 point, Vector3 dir) {
            Point = point;
            Dir = dir;
        }
        public ControlPoint3 Reverse => new ControlPoint3(Point, -Dir);
    }
}
