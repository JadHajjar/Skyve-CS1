namespace KianCommons.Math {
    using UnityEngine;
    using ColossalFramework.Math;
    using System;
    internal static class MathUtil {
        public const float Epsilon = 0.001f;
        public static bool EqualAprox(float a, float b, float error = Epsilon) {
            float diff = a - b;
            return (diff > -error) & (diff < error);
        }

        public static bool IsPow2(IConvertible x) {
            switch (Type.GetTypeCode(x.GetType())) {
                case TypeCode.Byte:
                    return IsPow2((ulong)(byte)x);
                case TypeCode.UInt16:
                    return IsPow2((ulong)(UInt16)x);
                case TypeCode.UInt32:
                    return IsPow2((ulong)(UInt32)x);
                case TypeCode.UInt64:
                    return IsPow2((ulong)(UInt64)x);
                case TypeCode.SByte:
                    return IsPow2((SByte)x);
                case TypeCode.Int16:
                    return IsPow2((short)x);
                case TypeCode.Int32:
                    return IsPow2((int)x);
                case TypeCode.Int64:
                    return IsPow2((long)x);
                default:
                    throw new ArgumentException("expected x to be integer. got " + x);
            }
        }

        public static bool IsPow2(ulong x) => x != 0 && (x & (x - 1)) == 0;
        public static bool IsPow2(long x) => x != 0 && (x & (x - 1)) == 0;

        // these are required to support negative numbers.
        public static bool IsPow2(int x) => x != 0 && (x & (x - 1)) == 0;
        public static bool IsPow2(short x) => x != 0 && (x & (x - 1)) == 0;
        public static bool IsPow2(SByte x) => x != 0 && (x & (x - 1)) == 0;

        internal static ushort Clamp2U16(int value) => (ushort)Mathf.Clamp(value, 0, ushort.MaxValue);
    }

    internal static class Extensions {
        public static Quad3 ToCS3D(this Quad2 q) {
            return new Quad3(
                q.a.ToCS3D(),
                q.b.ToCS3D(),
                q.c.ToCS3D(),
                q.d.ToCS3D());
        }
    }
}
