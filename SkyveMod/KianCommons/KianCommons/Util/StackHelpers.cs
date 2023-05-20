namespace KianCommons {
    using System.Diagnostics;
    using System.Linq;

    internal static class StackHelpers {
        public static string ToStringPretty(this StackTrace st, bool fullPath = false, bool nameSpace = false, bool showArgs = false) {
            string ret = "";
            foreach (var frame in st.GetFrames()) {
                var f = frame.GetFileName();
                if (f == null) {
                    ret += "    at " + frame + "\n";
                    continue;
                }

                //MethodBase m = frame.GetMethod();
                //var t = m.DeclaringType;
                //var args = m.GetParameters().Select(a=>a.ToString()).ToArray();
                //var genericArgs = m.GetGenericArguments();
                //if (nameSpace)
                //    ret += t.FullName;
                //else
                //    ret += t.Name;
                //ret += "." + m.Name;
                //if (m.IsGenericMethod)
                //    ret += "<" + m.GetGenericArguments() + ">";
                //if (showArgs)
                //    ret += "(" + string.Join(", ", args) + ")";
                //else
                //    ret += "()";

                if (!fullPath) {
                    f = f.Split('\\').LastOrDefault();
                }
                var l = frame.GetFileLineNumber();
                ret += $"    at {frame.GetMethod()} in {f}:{l}\n";
            }

            return ret;
        }

    }
}
