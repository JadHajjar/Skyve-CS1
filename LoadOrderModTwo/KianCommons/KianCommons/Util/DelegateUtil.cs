namespace KianCommons {
    using System;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;

    internal static class DelegateUtil {
        public static string FullName(MethodBase m) =>
            m.DeclaringType.FullName + "::" + m.Name;

        /// <typeparam name="TDelegate">delegate type</typeparam>
        /// <returns>Type[] representing arguments of the delegate.</returns>
        internal static Type[] GetParameterTypes<TDelegate>(bool instance = false)
            where TDelegate : Delegate {
            var ret = typeof(TDelegate)
                .GetMethod("Invoke")
                .GetParameters()
                .Select(p => p.ParameterType);
            if(instance) ret = ret.Skip(1); // skip instance
            return ret.ToArray();
        }

        /// <summary>
        /// Gets directly declared method based on a delegate that has
        /// the same name as the target method
        /// </summary>
        /// <param name="type">the class/type where the method is declared</param>
        /// <param name="name">the name of the method</param>
        internal static MethodInfo GetMethod<TDelegate>
            (this Type type, string name = null, bool throwOnError = false, bool instance = false)
            where TDelegate : Delegate {
            name ??= typeof(TDelegate).Name;
            return type.GetMethod(
                name,
                types: GetParameterTypes<TDelegate>(instance),
                throwOnError: throwOnError);
        }

        /// <summary>
        /// creates a closed instance delegate.
        /// </summary>
        /// <typeparam name="TDelegate">a delegate with all the parameters and the return type but without the instance argument</typeparam>
        /// <param name="instance">target instance for the delegate to close on</param>
        /// <param name="name">method name or null to use delegate type as name</param>
        /// <returns>a delegate that can be called without the need to provide instance argument.</returns>
        internal static TDelegate CreateClosedDelegate<TDelegate>(object instance, string name = null) where TDelegate : Delegate {
            try {
                var type = instance?.GetType();
                var method = type?.GetMethod<TDelegate>(name);
                if(method == null) return null;
                return (TDelegate)Delegate.CreateDelegate(type: typeof(TDelegate), firstArgument: instance, method: method);
            } catch(Exception ex) {
                throw new Exception($"CreateClosedDelegate<{typeof(TDelegate).Name}>({instance.GetType().Name},{name}) failed!", ex);
            }
        }

        /// <typeparam name="TDelegate">a delegate with all the parameters and the return type but without the instance argument</typeparam>
        /// <param name="name">method name or null to use delegate type as name</param>
        internal static TDelegate CreateDelegate<TDelegate>(Type type, string name = null, bool instance = false, bool throwOnError = false) where TDelegate : Delegate {
            try {
                var method = type?.GetMethod<TDelegate>(name, instance: instance, throwOnError: throwOnError);
                if(method == null) return null;
                return (TDelegate)Delegate.CreateDelegate(type: typeof(TDelegate), method: method);
            } catch(Exception ex) {
                if (!throwOnError) return null;
                throw new Exception($"CreateDelegate<{typeof(TDelegate).Name}>({instance.GetType().Name},{name}) failed!", ex);
            }
        }
    }
}
