using System;

namespace KianCommons {
    internal static class TypeUtil {
        private static bool IsIntegerType(Type type) {
            switch (Type.GetTypeCode(type)) {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return true;
                default:
                    return type.IsArray && IsIntegerType(type.GetElementType());
            }
        }

        public static bool IsNumeric(this Type type) {
            return
                type == typeof(byte) ||
                type == typeof(sbyte) ||
                type == typeof(int) ||
                type == typeof(uint) ||
                type == typeof(short) ||
                type == typeof(ushort) ||
                type == typeof(long) ||
                type == typeof(ulong) ||
                type == typeof(float) ||
                type == typeof(double) ||
                type == typeof(decimal);
        }

        public static bool IsFloatingPoint(this Type type) {
            switch (Type.GetTypeCode(type)) {
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsSigned(this Type type) {
            switch (Type.GetTypeCode(type)) {
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsInteger(this Type type) => type.IsPrimitive && IsIntegerType(type);

        public static bool IsSignedInteger(this Type type) => type.IsSigned() && IsInteger(type);
    }
}