namespace KianCommons {
    using System;
    using System.Collections;
    using System.Diagnostics;
    internal static class Assertion {
        [Conditional("DEBUG")]
        internal static void AssertDebug(bool con, string m = "") => Assert(con, m);

        [Conditional("DEBUG")]
        internal static void AssertNotNullDebug(object obj, string m = "") => AssertNotNull(obj, m);

        [Conditional("DEBUG")]
        internal static void AssertEqualDebug<T>(T a, T b, string m = "")
            where T : IComparable
            => AssertEqual(a, b, m);

        [Conditional("DEBUG")]
        internal static void AssertNeqDebug<T>(T a, T b, string m = "")
            where T : IComparable
            => AssertNeq(a, b, m);

        [Conditional("DEBUG")]
        internal static void AssertGTDebug<T>(T a, T b, string m = "") where T : IComparable =>
            Assert(a.CompareTo(b) > 0, $"expected {a} > {b} | " + m);

        [Conditional("DEBUG")]
        internal static void AssertGTEqDebug<T>(T a, T b, string m = "") where T : IComparable =>
            Assert(a.CompareTo(b) >= 0, $"expected {a} >= {b} | " + m);


        internal static void AssertNotNull(object obj, string m = "") =>
            Assert(obj != null, " unexpected null " + m);

        internal static void AssertEqual<T>(T a, T b, string m = "") where T : IComparable =>
            Assert(a.CompareTo(b) == 0, $"expected {a} == {b} | " + m);

        internal static void AssertNeq<T>(T a, T b, string m = "") where T : IComparable =>
            Assert(a.CompareTo(b) != 0, $"expected {a} != {b} | " + m);

        internal static void AssertGT<T>(T a, T b, string m = "") where T : IComparable =>
            Assert(a.CompareTo(b) > 0, $"expected {a} > {b} | " + m);

        internal static void AssertGTEq<T>(T a, T b, string m = "") where T : IComparable =>
            Assert(a.CompareTo(b) >= 0, $"expected {a} >= {b} | " + m);

        internal static void NotNull(object obj, string m = "") =>
            Assert(obj != null, " unexpected null " + m);

        internal static void Equal<T>(T a, T b, string m = "") where T : IComparable =>
            Assert(a.CompareTo(b) == 0, $"expected {a} == {b} | " + m);

        internal static void Neq<T>(T a, T b, string m = "") where T : IComparable =>
            Assert(a.CompareTo(b) != 0, $"expected {a} != {b} | " + m);

        internal static void GT<T>(T a, T b, string m = "") where T : IComparable =>
            Assert(a.CompareTo(b) > 0, $"expected {a} > {b} | " + m);

        internal static void GTEq<T>(T a, T b, string m = "") where T : IComparable =>
            Assert(a.CompareTo(b) >= 0, $"expected {a} >= {b} | " + m);

        internal static void Assert(bool con, string m = "") {
            if (!con) {
                m = "Assertion failed: " + m;
                Log.Error(m);
                throw new System.Exception(m);
            }
        }

        internal static void InRange(IList list, int index) {
            NotNull(list);
            Assert(index >= 0 && index < list.Count, $"index={index} Count={list.Count}");
        }
        

        internal static void AssertStack() {
            var frames = new StackTrace().FrameCount;
            //Log.Debug("stack frames=" + frames);
            if (frames > 200) {
                Exception e = new StackOverflowException("stack frames=" + frames);
                Log.Error(e.ToString());
                throw e;
            }
        }
    }
}