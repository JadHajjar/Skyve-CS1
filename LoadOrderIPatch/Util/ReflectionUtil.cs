namespace LoadOrderIPatch {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Reflection;

    public static class ReflectionUtil {
        public static MethodInfo GetMethodInfo<T>(T method) 
            where T : Delegate {
            return method.Method;
        }
    }
}
