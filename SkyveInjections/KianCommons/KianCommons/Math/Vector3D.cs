using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace KianCommons.Math {
    internal struct Vector3D {
        public float x;
        public float z;
        public float h;
        public float this[int index] {
            get {
                switch (index) {
                    case 0: return x;
                    case 1: return z;
                    case 2: return h;
                    default: throw new IndexOutOfRangeException("Invalid Vector3D index!");
                }
            }
            set {
                switch (index) {
                    case 0:
                        x = value;
                        break;
                    case 1:
                        z = value;
                        break;
                    case 2:
                        h = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3D index!");
                }
            }
        }

        public Vector3D(float x, float z, float h) {
            this.x = x;
            this.z = z;
            this.h = h;
        }

        public Vector3D(Vector2D vector2d , float h) {
            this.x = vector2d.x;
            this.z = vector2d.z;
            this.h = h;
        }

        public Vector3D(Vector3 v) {
            this.x = v.x;
            this.z = v.z;
            this.h = v.y;
        }

        public void Set(float new_x, float new_y, float new_z) {
            this.x = new_x;
            this.h = new_y;
            this.z = new_z;
        }


        public Vector2D ToVector2D() => new Vector2D(x: x, z: z);
        public static implicit operator Vector3D(Vector3 v) => new Vector3D(v);
        public static implicit operator Vector3(Vector3D v) => new Vector3(x:v.x, y:v.h, z:v.z);

        public override int GetHashCode() {
            return this.x.GetHashCode() ^ this.h.GetHashCode() << 2 ^ this.z.GetHashCode() >> 2;
        }

        public override bool Equals(object other) {
            bool result;
            if (!(other is Vector3D)) {
                result = false;
            } else {
                Vector3D vector = (Vector3D)other;
                result = (this.x.Equals(vector.x) && this.h.Equals(vector.h) && this.z.Equals(vector.z));
            }
            return result;
        }

        public static Vector3D Scale(Vector3D a, Vector3D b) {
            return new Vector3D(a.x * b.x, a.h * b.h, a.z * b.z);
        }

        public Vector3D Scale(Vector3D scale) => Vector3D.Scale(this, scale);

        public static Vector3D Cross(Vector3D lhs, Vector3D rhs) {
            return new Vector3D(lhs.h * rhs.z - lhs.z * rhs.h, lhs.z * rhs.x - lhs.x * rhs.z, lhs.x * rhs.h - lhs.h * rhs.x);
        }

        public static Vector3D Reflect(Vector3D inDirection, Vector3D inNormal) {
            return -2f * Vector3D.Dot(inNormal, inDirection) * inNormal + inDirection;
        }

        public static Vector3D Normalize(Vector3D value) {
            float num = value.magnitude;
            Vector3D result;
            if (num > 1E-05f) {
                result = value / num;
            } else {
                result = Vector3D.zero;
            }
            return result;
        }

        public Vector3D normalized => Normalize(this);

        public static float Dot(Vector3D lhs, Vector3D rhs) {
            return lhs.x * rhs.x + lhs.h * rhs.h + lhs.z * rhs.z;
        }

        public static Vector3D Project(Vector3D vector, Vector3D onNormal) {
            float num = Vector3D.Dot(onNormal, onNormal);
            Vector3D result;
            if (num < Mathf.Epsilon) {
                result = Vector3D.zero;
            } else {
                result = onNormal * Vector3D.Dot(vector, onNormal) / num;
            }
            return result;
        }

        public static Vector3D ProjectOnPlane(Vector3D vector, Vector3D planeNormal) {
            return vector - Vector3D.Project(vector, planeNormal);
        }

        public static float Distance(Vector3D a, Vector3D b) {
            Vector3D vector = new Vector3D(a.x - b.x, a.h - b.h, a.z - b.z);
            return Mathf.Sqrt(vector.x * vector.x + vector.h * vector.h + vector.z * vector.z);
        }

        public static Vector3D ClampMagnitude(Vector3D vector, float maxLength) {
            Vector3D result;
            if (vector.sqrMagnitude > maxLength * maxLength) {
                result = vector.normalized * maxLength;
            } else {
                result = vector;
            }
            return result;
        }

        public float magnitude => Mathf.Sqrt(x * x + z * z + h * h);
        public float sqrMagnitude => x * x + z * z + h * h;

        public static Vector3D Min(Vector3D lhs, Vector3D rhs) {
            return new Vector3D(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.h, rhs.h), Mathf.Min(lhs.z, rhs.z));
        }

        public static Vector3D Max(Vector3D lhs, Vector3D rhs) {
            return new Vector3D(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.h, rhs.h), Mathf.Max(lhs.z, rhs.z));
        }

        public static Vector3D zero => Vector3.zero;
        public static Vector3D one => Vector3.one;
        public static Vector3D forward => Vector3.forward; // x:0,z:1,h:0
        public static Vector3D fwd => forward;
        public static Vector3D back => Vector3.back;
        public static Vector3D up => Vector3.up;
        public static Vector3D down => Vector3.down;
        public static Vector3D left => Vector3.left; // x:-1,y:0,h:0
        public static Vector3D right => Vector3.right;

        public static Vector3D operator +(Vector3D a, Vector3D b) => new Vector3D(a.x + b.x, a.h + b.h, a.z + b.z);
        public static Vector3D operator -(Vector3D a, Vector3D b) => new Vector3D(a.x - b.x, a.h - b.h, a.z - b.z);
        public static Vector3D operator -(Vector3D a) => new Vector3D(-a.x, -a.h, -a.z);
        public static Vector3D operator *(Vector3D a, float d) => new Vector3D(a.x * d, a.h * d, a.z * d);
        public static Vector3D operator *(float d, Vector3D a) => new Vector3D(a.x * d, a.h * d, a.z * d);
        public static Vector3D operator /(Vector3D a, float d) => new Vector3D(a.x / d, a.h / d, a.z / d);
        public static bool operator ==(Vector3D lhs, Vector3D rhs) => (lhs - rhs).magnitude < 9.99999944E-11f;
        public static bool operator !=(Vector3D lhs, Vector3D rhs) => !(lhs == rhs);

        public override string ToString() => $"(x:{x}, z:{z}, h:{h})";

        public static float UnsignedAngleRad(Vector3D from, Vector3D to) {
            return Mathf.Acos(Mathf.Clamp(Vector3D.Dot(from.normalized, to.normalized), -1f, 1f));
        }
        public static float UnsighedAngleRad(Vector3D from, Vector3D to) {
            return Mathf.Acos(Mathf.Clamp(Dot(from.normalized, to.normalized), -1f, 1f));
        }

    }
}
