using UnityEngine;

namespace KianCommons.Math {
    internal struct Vector2D {
        private Vector2 _vector2;
        public const float kEpsilon = 1E-05F;
        public float x => _vector2.x;
        public float z => _vector2.y;
        public float this[int index] { get => _vector2[index]; set => _vector2[index] = value; }

        public static Vector2D down => Vector2.down;
        public static Vector2D up => Vector2.up;
        public static Vector2D one => Vector2.one;
        public static Vector2D zero => Vector2.zero;
        public static Vector2D left => Vector2.left;
        public static Vector2D right => Vector2.right;

        public Vector2D(float x, float z) => _vector2 = new Vector2(x, z);
        public Vector2D(Vector2 v) => _vector2 = v;
        public void Set(float newX, float newY) => _vector2.Set(newX, newY);
        public Vector3D ToVector3D(float h = 0) => new Vector3D(this, h);
        public static implicit operator Vector2D(Vector2 v) => new Vector2D(v);
        public static implicit operator Vector2(Vector2D v) => v._vector2;

        public override bool Equals(object other) {
            if (!(other is Vector2D)) return false;
                Vector2D vector = (Vector2D)other;
            return x == vector.x && z == vector.z;
        }
        public override int GetHashCode() => _vector2.GetHashCode();
        public override string ToString() => $"Vector2D({x},{z})";
        public string ToString(string format) => $"Vector2D({x.ToString(format)},{z.ToString(format)})";

        public float magnitude => _vector2.magnitude;
        public float sqrMagnitude => _vector2.sqrMagnitude;
        public Vector2D normalized => _vector2.normalized;

        public static Vector2D operator +(Vector2D a, Vector2D b) => a._vector2 + b._vector2;
        public static Vector2D operator -(Vector2D a) => -a._vector2;
        public static Vector2D operator -(Vector2D a, Vector2D b) => a._vector2 - b._vector2;
        public static Vector2D operator *(float d, Vector2D a) => d * a._vector2;
        public static Vector2D operator *(Vector2D a, float d) => a._vector2 * d;
        public static Vector2D operator /(Vector2D a, float d) => a._vector2 / d;
        public static bool operator ==(Vector2D lhs, Vector2D rhs) => lhs._vector2 == rhs._vector2;
        public static bool operator !=(Vector2D lhs, Vector2D rhs) => lhs._vector2 != rhs._vector2;


        public Vector2D Scale(Vector2D scale) => Vector2.Scale(this, scale);
        public Vector2D Extend(float magnitude) => this.NewMagnitude(magnitude + this.magnitude);
        public Vector2D NewMagnitude(float magnitude) => magnitude * this.normalized;

        /// <summary>
        /// return value is between 0 to pi. v1 and v2 are interchangeable.
        /// </summary>
        public static float UnsignedAngleRad(Vector2D v1, Vector2D v2) {
            //cos(a) = v1.v2 /(|v1||v2|)         
            float dot = Vector2.Dot(v1, v2);
            float magnitude = Mathf.Sqrt(v1.sqrMagnitude * v2.sqrMagnitude);
            float angle = Mathf.Acos(dot/magnitude);
            return angle;
        }

        public static float Determinent(Vector2D v1, Vector2D v2) =>
            v1.x * v2.z - v1.z * v2.x; // x1*z2 - z1*x2  

        public static Vector2D Vector2ByAgnleRad(float magnitude, float angle) {
            return new Vector2D(
                x: magnitude * Mathf.Cos(angle),
                z: magnitude * Mathf.Sin(angle)
                );
        }

        /// result is between -pi to +pi. angle is CCW with respect to Vector2D.right
        public float SignedAngleRadCCW() => Mathf.Atan2(x, z);

        /// <summary>
        /// return value is between -pi to pi. v1 and v2 are not interchangeable.
        /// the angle goes CCW from v1 to v2.
        /// e.g. v1=0,1 v2=1,0 => angle=pi/2
        /// Note: to convert CCW to CW EITHER swap v1 and v2 OR take the negative of the result.
        /// </summary>
        public static float SignedAngleRadCCW(Vector2D v1, Vector2D v2) {
            float dot = Vector2.Dot(v1, v2);
            float det = Determinent(v1, v2);
            return Mathf.Atan2(det, dot);
        }

        public Vector2D Rotate90CCW() => new Vector2D(-z, +x);
        public Vector2D PerpendicularCCW() => normalized.Rotate90CCW();
        public Vector2D Rotate90CW() => new Vector2D(+z, -x);
        public Vector2D PerpendicularCC() => this.normalized.Rotate90CW();
    }
}
