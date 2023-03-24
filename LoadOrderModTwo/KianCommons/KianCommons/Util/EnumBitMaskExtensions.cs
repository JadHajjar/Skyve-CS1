namespace KianCommons {
    using ColossalFramework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static KianCommons.Math.MathUtil;
    using System.Reflection;
    using static KianCommons.ReflectionHelpers;
    using KianCommons.Math;
    using System.Globalization;
    using System.Diagnostics;

    internal static class EnumBitMaskExtensions {
        [Obsolete("this is buggy as it assumes enum is 0,1,2,3,4 ...\n" +
            "use String2EnumValue instead")]
        internal static int String2Enum<T>(string str) where T : Enum =>
            Array.IndexOf(Enum.GetNames(typeof(T)), str);
        
        internal static object String2EnumValue<T>(string str) where T : Enum =>
            Enum.Parse(typeof(T), str);
            
        internal static T Max<T>()
            where T : Enum =>
            Enum.GetValues(typeof(T)).Cast<T>().Max();

        internal static void SetBit(this ref byte b, int idx) => b |= (byte)(1 << idx);
        internal static void ClearBit(this ref byte b, int idx) => b &= ((byte)~(1 << idx));
        internal static bool GetBit(this byte b, int idx) => (b & (byte)(1 << idx)) != 0;
        internal static void SetBit(this ref byte b, int idx, bool value) {
            if (value)
                b.SetBit(idx);
            else
                b.ClearBit(idx);
        }

        public static int CountOnes(int value) {
            int count = 0;
            while(value != 0) {
                value >>= 1;
                count++;
            }
            return count;
        }
        public static int CountOnes(long value) {
            int count = 0;
            while(value != 0) {
                value >>= 1;
                count++;
            }
            return count;
        }
        public static int CountOnes(uint value) {
            int count = 0;
            while(value != 0) {
                value >>= 1;
                count++;
            }
            return count;
        }
        public static int CountOnes(ulong value) {
            int count = 0;
            while(value != 0) {
                value >>= 1;
                count++;
            }
            return count;
        }

        internal static T GetMaxEnumValue<T>() =>
            System.Enum.GetValues(typeof(T)).Cast<T>().Max();

        internal static int GetEnumCount<T>() =>
            System.Enum.GetValues(typeof(T)).Length;

        [Conditional("DEBUG")]
        private static void CheckEnumWithFlags<T>() where T : struct, Enum, IConvertible =>
            CheckEnumWithFlags(typeof(T));

        [Conditional("DEBUG")]
        private static void CheckEnumWithFlags(Type type) {
            // code from: private static void ColossalFramework.EnumExtensions.CheckEnumWithFlags<T>()
            if (!type.IsEnum)
                throw new ArgumentException($"Type '{type.FullName}' is not an enum");

            if (!Attribute.IsDefined(type, typeof(FlagsAttribute)))
                throw new ArgumentException($"Type '{type.FullName}' doesn't have the 'Flags' attribute");

            if (!Enum.GetUnderlyingType(type).IsInteger())
                throw new Exception($"Type '{type.FullName}' is not integer based enum.");
        }
        internal static bool IsFlagSet(this NetInfo.Direction value, NetInfo.Direction flag) => (value & flag) != 0;
        internal static bool IsFlagSet(this NetInfo.LaneType value, NetInfo.LaneType flag) => (value & flag) != 0;
        internal static bool IsFlagSet(this VehicleInfo.VehicleType value, VehicleInfo.VehicleType flag) => (value & flag) != 0;
        internal static bool IsFlagSet(this NetNode.Flags value, NetNode.Flags flag) => (value & flag) != 0;
        internal static bool IsFlagSet(this NetSegment.Flags value, NetSegment.Flags flag) => (value & flag) != 0;
        internal static bool IsFlagSet(this NetLane.Flags value, NetLane.Flags flag) => (value & flag) != 0;

        internal static bool CheckFlags(this NetInfo.Direction value, NetInfo.Direction required, NetInfo.Direction forbidden = 0) =>
            (value & (required | forbidden)) == required;
        internal static bool CheckFlags(this NetInfo.LaneType value, NetInfo.LaneType required, NetInfo.LaneType forbidden = 0) =>
            (value & (required | forbidden)) == required;
        internal static bool CheckFlags(this VehicleInfo.VehicleType value, VehicleInfo.VehicleType required, VehicleInfo.VehicleType forbidden = 0) =>
            (value & (required | forbidden)) == required;
        internal static bool CheckFlags(this NetNode.Flags value, NetNode.Flags required, NetNode.Flags forbidden =0) =>
            (value & (required | forbidden)) == required;
        internal static bool CheckFlags(this NetSegment.Flags value, NetSegment.Flags required, NetSegment.Flags forbidden=0) =>
            (value & (required | forbidden)) == required;
        internal static bool CheckFlags(this NetLane.Flags value, NetLane.Flags required, NetLane.Flags forbidden = 0) =>
            (value & (required | forbidden)) == required;


        public static bool CheckFlags<T>(this T value, T required, T forbidden)
            where T : struct, Enum, IConvertible {
            CheckEnumWithFlags<T>();
            long value2 = value.ToInt64();
            long required2 = required.ToInt64();
            long forbidden2 = forbidden.ToInt64();
            return (value2 & (required2 | forbidden2)) == required2;
        }

        // this was commented out to make DCR mod faster. we want compiler to choose one of the faster overloads.
        //public static bool CheckFlags<T>(this T value, T required)
        //    where T : struct, Enum, IConvertible {
        //    CheckEnumWithFlags<T>();
        //    long value2 = value.ToInt64();
        //    long required2 = required.ToInt64();
        //    return (value2 & required2) == required2;
        //}

        /// <summary>
        /// can convert any enum based on signed/unsigned integer to long
        /// </summary>
        public static ulong ToUInt64(this IConvertible value) {
            Type type = value.GetType();
            if (type.IsEnum)
                type = Enum.GetUnderlyingType(type);

            if (type.IsSigned()) {
                return (ulong)(value.ToInt64(CultureInfo.InvariantCulture));
            } else {
                return value.ToUInt64(CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// can convert any enum based on signed/unsigned integer to long
        /// </summary>
        public static long ToInt64(this IConvertible value) {
            Type type = value.GetType();
            if (type.IsEnum)
                type = Enum.GetUnderlyingType(type);

            if (type.IsSigned()) {
                return value.ToInt64(CultureInfo.InvariantCulture);
            } else {
                return (long)value.ToUInt64(CultureInfo.InvariantCulture);
            }
        }


        public static IEnumerable<IConvertible> GetPow2Values(Type enumType) {
            CheckEnumWithFlags(enumType);
            var values = Enum.GetValues(enumType).Cast<IConvertible>();
            return values.Where(val => IsPow2(val));
        }

        public static IEnumerable<T> GetPow2Values<T>() where T : struct, Enum, IConvertible {
            CheckEnumWithFlags(typeof(T));
            var values = Enum.GetValues(typeof(T)).Cast<T>();
            return values.Where(val => IsPow2(val));
        }
        public static IEnumerable<T> ExtractPow2Flags<T>(this T flags)
            where T : struct, Enum, IConvertible {
            return GetPow2Values<T>().Where(flag => flags.IsFlagSet(flag));
        }

        public static IEnumerable<IConvertible> ExtractPow2Flags(this IConvertible flags){
            return GetPow2Values(flags.GetType()).Where(flag => flags.IsFlagSet(flag));
        }
        public static bool IsFlagSet(this IConvertible flags, IConvertible flag) {
            CheckEnumWithFlags(flags.GetType());
            long a = flags.ToInt64();
            long b = flag.ToInt64();
            return (a & b) != 0;
        }

        public static IEnumerable<int> GetPow2ValuesI32(Type enumType) {
            CheckEnumWithFlags(enumType);
            var values = Enum.GetValues(enumType).Cast<int>();
            return values.Where(v => IsPow2(v));
        }

        public static MemberInfo GetEnumMemberInfo(this Type enumType, object value) {
            if (enumType is null) throw new ArgumentNullException("enumType");
            string name = Enum.GetName(enumType, value);
            if (name == null)
                throw new Exception($"{enumType.GetType().Name}:{value} not found");
            return enumType.GetMember(name, ALL).FirstOrDefault() ??
                throw new Exception($"{enumType.GetType().Name}.{name} not found");
        }

        public static MemberInfo GetEnumMemberInfo(this Enum value) =>
            value.GetType().GetEnumMemberInfo(value);
        

        public static T[] GetEnumMemberAttributes<T>(Type enumType, object value) where T : Attribute =>
            enumType.GetEnumMemberInfo(value).GetAttributes<T>();
        
        public static T[] GetEnumMemberAttributes<T>(this Enum value) where T : Attribute =>
            GetEnumMemberAttributes<T>(value.GetType(), value);
        

        public static T[] GetEnumValues<T>() where T: struct, Enum, IConvertible =>
            Enum.GetValues(typeof(T)) as T[];
    }
}