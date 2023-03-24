namespace KianCommons.Serialization {
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using KianCommons;

    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    struct FloatConverter {
        [FieldOffset(0)] private int m_int;

        [FieldOffset(0)] private float m_float;

        ///<summary>recast float to int without changing bits</summary>
        public static int AsInt(float f) => new FloatConverter { m_float = f }.m_int;

        ///<summary>recast int to float without changing bits</summary>
        public static float AsFloat(int i) => new FloatConverter { m_int = i }.m_float;
    }

    public class SimpleDataSerializer {
        public readonly Version Version;
        public readonly MemoryStream Stream;

        // make deserailizer
        SimpleDataSerializer(byte[] data) {
            Stream = new MemoryStream(data, false);
            Version = ReadVersion();
        }

        // make serailizer
        SimpleDataSerializer(Version version, int capacity) {
            Stream = new MemoryStream(capacity);
            Assertion.AssertNotNull(version);
            Version = version;
            WriteVersion(version);
        }

        public static SimpleDataSerializer Reader(byte[] data) => new SimpleDataSerializer(data);
        public static SimpleDataSerializer Writer(Version version, int capacity) =>
            new SimpleDataSerializer(version, capacity);

        public byte[] GetBytes() => Stream.ToArray();


        // from ColossalFramework.IO.DataSerializer
        public int ReadInt32() {
            int num = (Stream.ReadByte() & 255) << 24;
            num |= (Stream.ReadByte() & 255) << 16;
            num |= (Stream.ReadByte() & 255) << 8;
            return num | (Stream.ReadByte() & 255);
        }
        public void WriteInt32(int value) {
            Stream.WriteByte((byte)(value >> 24 & 255));
            Stream.WriteByte((byte)(value >> 16 & 255));
            Stream.WriteByte((byte)(value >> 8 & 255));
            Stream.WriteByte((byte)(value & 255));
        }

        public uint ReadUInt32() {
            uint num = ((uint)(Stream.ReadByte() & 255)) << 24;
            num |= ((uint)(Stream.ReadByte() & 255)) << 16;
            num |= ((uint)(Stream.ReadByte() & 255)) << 8;
            return num | (uint)(Stream.ReadByte() & 255);
        }
        public void WriteUInt32(uint value) {
            Stream.WriteByte((byte)(value >> 24 & 255u));
            Stream.WriteByte((byte)(value >> 16 & 255u));
            Stream.WriteByte((byte)(value >> 8 & 255u));
            Stream.WriteByte((byte)(value & 255u));
        }

        public void WriteFloat(float value) {
            int i = FloatConverter.AsInt(value);
            this.WriteInt32(i);
        }

        public float ReadFloat() {
            int i = this.ReadInt32();
            return FloatConverter.AsFloat(i);
        }

        public Version ReadVersion() {
            return new Version(
                Math.Max(0, ReadInt32()),
                Math.Max(0, ReadInt32()),
                Math.Max(0, ReadInt32()),
                Math.Max(0, ReadInt32()));
        }
        public void WriteVersion(Version version) {
            Assertion.AssertNotNull(version);
            WriteInt32(version.Major);
            WriteInt32(version.Minor);
            WriteInt32(version.Build);
            WriteInt32(version.Revision);
        }

        public void WriteInt32Array(int [] array) {
            if (array == null) {
                WriteInt32(-1);
                return;
            }

            WriteInt32(array.Length);
            for(int i = 0; i < array.Length; ++i) {
                WriteInt32(array[i]);
            }
        }
        public int[] ReadInt32Array() {
            int n = ReadInt32();
            if(n < 0) {
                return null;
            }

            int[] array = new int[n];
            for (int i = 0; i < n; ++i) {
                array[i] = ReadInt32();
            }
            return array;
        }

    }
}
