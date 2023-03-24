namespace KianCommons {
    using ColossalFramework;
    using ColossalFramework.UI;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using static KianCommons.Assertion;

    internal static class ReflectionHelpers {
        internal static Version Version(this Assembly asm) =>
          asm.GetName().Version;

        internal static Version VersionOf(this Type t) =>
            t.Assembly.GetName().Version;

        internal static Version VersionOf(this object obj) =>
            VersionOf(obj.GetType());

        internal static string Name(this Assembly assembly) => assembly.GetName().Name;

        public static string FullName(this MethodBase m) =>
            m.DeclaringType.FullName + "." + m.Name;


        internal static T ShalowClone<T>(this T source) where T : class, new() {
            T target = new T();
            CopyProperties<T>(target, source);
            return target;
        }

        public const BindingFlags COPYABLE = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        internal static void CopyProperties(object target, object origin) {
            var t1 = target.GetType();
            var t2 = origin.GetType();
            Assert(t1 == t2 || t1.IsSubclassOf(t2));
            FieldInfo[] fields = origin.GetType().GetFields(COPYABLE);
            foreach(FieldInfo fieldInfo in fields) {
                //Log.Debug($"Copying field:<{fieldInfo.Name}> ...>");
                object value = fieldInfo.GetValue(origin);
                string strValue = value?.ToString() ?? "null";
                //Log.Debug($"Got field value:<{strValue}> ...>");
                fieldInfo.SetValue(target, value);
                //Log.Debug($"Copied field:<{fieldInfo.Name}> value:<{strValue}>");
            }
        }

        internal static void CopyProperties<T>(object target, object origin) {
            Assert(target is T, "target is T");
            Assert(origin is T, "origin is T");
            FieldInfo[] fields = typeof(T).GetFields(COPYABLE);
            foreach(FieldInfo fieldInfo in fields) {
                //Log.Debug($"Copying field:<{fieldInfo.Name}> ...>");
                object value = fieldInfo.GetValue(origin);
                //string strValue = value?.ToString() ?? "null";
                //Log.Debug($"Got field value:<{strValue}> ...>");
                fieldInfo.SetValue(target, value);
                //Log.Debug($"Copied field:<{fieldInfo.Name}> value:<{strValue}>");
            }
        }

        /// <summary>
        /// copies fields with identical name from origin to target.
        /// even if the declaring types don't match.
        /// only copies existing fields and their types match.
        /// </summary>
        internal static void CopyPropertiesForced<T>(object target, object origin) {
            FieldInfo[] fields = typeof(T).GetFields(COPYABLE);
            foreach(FieldInfo fieldInfo in fields) {
                string fieldName = fieldInfo.Name;
                var originFieldInfo = origin.GetType().GetField(fieldName, ALL);
                var targetFieldInfo = target.GetType().GetField(fieldName, ALL);
                if(originFieldInfo != null && targetFieldInfo != null) {
                    object value = null;
                    try {
                        value = originFieldInfo.GetValue(origin);
                        targetFieldInfo.SetValue(target, value);
                    } catch { }
                }
            }
        }

        internal static void SetAllDeclaredFieldsToNull(object instance) {
            var type = instance.GetType();
            var fields = type.GetAllFields(declaredOnly: true);
            foreach(var f in fields) {
                if(f.FieldType.IsClass) {
                    if(Log.VERBOSE)
                        Log.Debug($"SetAllDeclaredFieldsToNull: setting {instance}.{f} = null");
                    f.SetValue(instance, null);
                }
            }
        }

        /// <summary>
        /// call this in OnDestroy() to clear all references.
        /// </summary>
        internal static void SetAllDeclaredFieldsToNull(this UIComponent c) =>
            SetAllDeclaredFieldsToNull(c as object);

        internal static string GetPrettyFunctionName(MethodInfo m) {
            string s = m.Name;
            string[] ss = s.Split(new[] { "g__", "|" }, System.StringSplitOptions.RemoveEmptyEntries);
            if(ss.Length == 3)
                return ss[1];
            return s;
        }

        internal static T GetAttribute<T>(this ICustomAttributeProvider memberInfo, bool inherit = true) where T : Attribute {
            return memberInfo.GetAttributes<T>().FirstOrDefault();
        }

        internal static T[] GetAttributes<T>(this ICustomAttributeProvider memberInfo, bool inherit = true) where T : Attribute {
            return memberInfo.GetCustomAttributes(typeof(T), inherit) as T[];
        }

        internal static bool HasAttribute<T>(this MemberInfo member, bool inherit = true) where T : Attribute
            => HasAttribute(member, typeof(T), inherit);

        internal static bool HasAttribute(this MemberInfo member, Type t, bool inherit = true) {
            var att = member.GetCustomAttributes(t, inherit);
            return att != null && att.Length != 0;
        }

        internal static IEnumerable<FieldInfo> GetFieldsWithAttribute<T>(
            this object obj, bool inherit = true) where T : Attribute {
            return obj.GetType().GetFields()
                .Where(_field => _field.HasAttribute<T>(inherit));
        }

        internal static IEnumerable<FieldInfo> GetFieldsWithAttribute<T>(
            this Type type, bool inherit = true) where T : Attribute {
            return type.GetFields()
                .Where(_field => _field.HasAttribute<T>(inherit));
        }

        internal static IEnumerable<MemberInfo> GetMembersWithAttribute<T>(
            this object obj, bool inherit = true) where T : Attribute {
            return obj.GetType().GetMembersWithAttribute<T>(inherit);
        }

        internal static IEnumerable<MemberInfo> GetMembersWithAttribute<T>(
            this Type type, bool inherit = true) where T : Attribute {
            return type.GetMembers().Where(_member => _member.HasAttribute<T>(inherit));
        }

        public const BindingFlags ALL = BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.GetField
            | BindingFlags.SetField
            | BindingFlags.GetProperty
            | BindingFlags.SetProperty;

        public const BindingFlags ALL_Declared = ALL | BindingFlags.DeclaredOnly;

        /// <summary>
        /// get value of the instant field target.Field.
        /// </summary>
        internal static object GetFieldValue(object target, string fieldName) {
            if (target == null)
                throw new ArgumentNullException("target");
            if (fieldName == null)
                throw new ArgumentNullException("fieldName");
            var type = target.GetType();
            var field = type.GetField(fieldName, ALL)
                ?? throw new Exception($"{type}.{fieldName} not found");
            return field.GetValue(target);
        }

        /// <summary>
        /// Get value of a static field from T.fieldName
        /// </summary>
        internal static object GetFieldValue<T>(string fieldName)
            => GetFieldValue(typeof(T), fieldName);

        /// <summary>
        /// Get value of a static field from type.fieldName
        /// </summary>
        internal static object GetFieldValue(Type type, string fieldName) {
            if (type == null)
                throw new ArgumentNullException("target");
            if (fieldName == null)
                throw new ArgumentNullException("fieldName");

            var field = type.GetField(fieldName, ALL)
                ?? throw new Exception($"{type}.{fieldName} not found");
            return field.GetValue(null);
        }

        /// <summary>
        /// sets static T.fieldName to value.
        /// </summary>
        internal static void SetFieldValue<T>(string fieldName, object value) =>
            SetFieldValue(typeof(T), fieldName, value);

        /// <summary>
        /// sets static targetType.fieldName to value.
        /// </summary>
        internal static void SetFieldValue(Type targetType, string fieldName, object value) {
            var field = GetField(targetType, fieldName);
            field.SetValue(null, value);
        }

        /// <summary>
        /// sets target.fieldName to value.
        /// </summary>
        internal static void SetFieldValue(object target, string fieldName, object value) {
            var field = GetField(target.GetType(), fieldName);
            field.SetValue(target, value);
        }

        internal static FieldInfo GetField(object target, string fieldName, bool throwOnError = true) =>
            GetField(target.GetType(), fieldName, throwOnError);
        internal static FieldInfo GetField<T>(string fieldName, bool throwOnError = true) =>
            GetField(typeof(T), fieldName, throwOnError);
        internal static FieldInfo GetField(Type declaringType, string fieldName, bool throwOnError = true) {
            var ret = declaringType.GetField(fieldName, ALL);
            if(ret == null && throwOnError)
                throw new Exception($"{declaringType}.{fieldName} not found");
            return ret;
        }


        /// <summary>
        /// gets method of any access type.
        /// </summary>
        internal static MethodInfo GetMethod(Type type, string method, bool throwOnError = true) {
            if(type == null) throw new ArgumentNullException("type");
            var ret = type.GetMethod(method, ALL);
            if(throwOnError && ret == null)
                throw new Exception($"Method not found: {type.Name}.{method}");
            return ret;
        }

        /// <summary>
        /// gets method of any access type.
        /// </summary>
        internal static MethodInfo GetMethod(
            Type type,
            string name,
            BindingFlags bindingFlags,
            Type[] types,
            bool throwOnError = false) {
            if(type == null) throw new ArgumentNullException("type");
            var ret = type.GetMethod(name, bindingFlags, null, types, null);
            if(throwOnError && ret == null)
                throw new Exception($"failed to retrieve method {type}.{name}({types.ToSTR()} bindingFlags:{bindingFlags.ToSTR()})");
            return ret;
        }

        internal static MethodInfo GetMethod(
            Type type,
            string name,
            BindingFlags bindingFlags,
            bool throwOnError = true) {
            if(type == null) throw new ArgumentNullException("type");
            var ret = type.GetMethod(name, bindingFlags);
            if(throwOnError && ret == null)
                throw new Exception($"failed to retrieve method {type}.{name} bindingFlags:{bindingFlags.ToSTR()}");
            return ret;
        }

        internal static string ToSTR(this BindingFlags flags) => flags == ALL ? "ALL" : flags.ToString();



        /// <summary>
        /// Invokes static method of any access type.
        /// like: type.method()
        /// </summary>
        /// <param name="method">static method without parameters</param>
        /// <returns>return value of the function if any. null otherwise</returns>
        internal static object InvokeMethod<T>(string method) {
            return GetMethod(typeof(T), method, true)?.Invoke(null, null);
        }

        internal static object InvokeMethod<T>(string method, params object[] args) {
            return GetMethod(typeof(T), method, true)?.Invoke(null, args);
        }

        /// <summary>
        /// Invokes static method of any access type.
        /// like: type.method()
        /// </summary>
        /// <param name="method">static method without parameters</param>
        /// <returns>return value of the function if any. null otherwise</returns>
        internal static object InvokeMethod(Type type, string method) {
            return GetMethod(type, method, true)?.Invoke(null, null);
        }

        /// <summary>
        /// Invokes static method of any access type.
        /// like: qualifiedType.method()
        /// </summary>
        /// <param name="method">static method without parameters</param>
        /// <returns>return value of the function if any. null otherwise</returns>
        internal static object InvokeMethod(string qualifiedType, string method) {
            var type = Type.GetType(qualifiedType, true);
            return InvokeMethod(type, method);
        }

        /// <summary>
        /// Invokes instance method of any access type.
        /// like: qualifiedType.method()
        /// </summary>
        /// <param name="method">instance method without parameters</param>
        /// <returns>return value of the function if any. null otherwise</returns>
        internal static object InvokeMethod(object instance, string method) {
            var type = instance.GetType();
            return GetMethod(type, method, true)?.Invoke(instance, null);
        }

        internal static object InvokeMethod(object instance, string method, params object[] args) {
            var type = instance.GetType();
            return GetMethod(type, method, true)?.Invoke(instance, args);
        }

        internal static object InvokeSetter(object instance, string propertyName, object value) {
            var type = instance.GetType();
            var property = type.GetProperty(propertyName) ?? throw new Exception($"{type}.{propertyName} not found");
            return property.GetSetMethod().Invoke(instance, new object[] { value });
        }

        internal static EventInfo GetEvent(Type type, string eventName, bool throwOnError = true) {
            var e = type.GetEvent(eventName, ALL);
            if(e == null && throwOnError)
                throw new Exception($"could not find {eventName} in {type}");
            return e;
        }


        //instance
        internal static T EventToDelegate<T>(object instance, string eventName)
            where T : Delegate {
            return (T)instance
                .GetType()
                .GetField(eventName, ALL)
                .GetValue(instance);
        }

        //static
        internal static T EventToDelegate<T>(Type type, string eventName)
            where T : Delegate {
            return (T)type
                .GetField(eventName, ALL)
                .GetValue(null);
        }

        //instance
        internal static void InvokeEvent(object instance, string eventName, bool verbose = false) {
            var d = GetEventDelegates(instance, eventName);
            if(verbose) Log.Info($"Executing event `{instance.GetType().FullName}.{eventName}` ...");
            ExecuteDelegates(d, verbose);
        }



        //static
        internal static void InvokeEvent(Type type, string eventName, bool verbose = false) {
            var d = GetEventDelegates(type, eventName);
            if(verbose) Log.Info($"Executing event `{type.FullName}.{eventName}` ...");
            ExecuteDelegates(d, verbose);
        }

        //static
        internal static Delegate[] GetEventDelegates(Type type, string eventName) {
            MulticastDelegate eventDelagate =
                (MulticastDelegate)type
                .GetField(eventName, ALL)
                .GetValue(null);
            return eventDelagate.GetInvocationList();
        }

        //instance
        internal static Delegate[] GetEventDelegates(object instance, string eventName) {
            MulticastDelegate eventDelagate =
                (MulticastDelegate)instance.GetType()
                .GetField(eventName, ALL)
                .GetValue(instance);
            return eventDelagate.GetInvocationList();
        }

        internal static void ExecuteDelegates(Delegate[] delegates, bool verbose = false) {
            if(delegates is null) throw new ArgumentNullException("delegates");
            var timer = new Stopwatch();
            foreach(Delegate dlg in delegates) {
                if(dlg == null) continue;
                if(verbose) {
                    Log.Info($"Executing {dlg.Target}:{dlg.Method.Name} ...");
                    timer.Reset(); timer.Start();
                }
                dlg.Method.Invoke(dlg.Target, null);
                if(verbose) {
                    var ms = timer.ElapsedMilliseconds;
                    Log.Info($"Done executing {dlg.Target}:{dlg.Method.Name}! duration={ms:#,0}ms");
                }
            }
        }

        internal static string ThisMethod {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get => CurrentMethod(2);
        }

        private static string JoinArgs(object[] args) {
            if(args.IsNullorEmpty())
                return "";
            else
                return args.Select(a => a.ToSTR()).Join(", ");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static string CurrentMethod(int i = 1,  params object[] args) {
            var method = new StackFrame(i).GetMethod();
                return $"{method.DeclaringType.Name}.{method.Name}({JoinArgs(args)})";
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static string CurrentMethodFull(int i = 1, params object[] args) {
            var method = new StackFrame(i).GetMethod();
            if(args.IsNullorEmpty()) {
                var parameters = method
                    .GetParameters()
                    .Select(p => $"{p.ParameterType.Name} {p.Name}")
                    .Join(" ,");
                return $"{method.FullName()}({parameters})";
            } else {
                return $"{method.FullName()}({JoinArgs(args)})";
            }
        }

        internal static void LogCalled(params object [] args) => Log.Info(CurrentMethod(2, args) + " called.", false);
        internal static void LogSucceeded() => Log.Info(CurrentMethod(2) + " succeeded!", false);
    }

    internal static class ReflectionExtension {
        const BindingFlags ALL = ReflectionHelpers.ALL;

        internal static MethodInfo GetMethod(this Type type, string name, bool throwOnError = true) {
            return ReflectionHelpers.GetMethod(type, name, ALL, throwOnError);
        }

        internal static MethodInfo GetMethod(this Type type, string name, BindingFlags binding = ALL, bool throwOnError = true) {
            return ReflectionHelpers.GetMethod(type, name, binding, throwOnError);
        }

        internal static MethodInfo GetMethod(
            this Type type,
            string name,
            Type[] types,
            BindingFlags binding = ALL,
            bool throwOnError = true) {
            return ReflectionHelpers.GetMethod(type, name, binding, types, throwOnError);
        }

        // boxing and unboxing on SetValue so that it can be called with ref on struct types.
        internal static void SetValue<TStruct>(this FieldInfo fieldInfo, ref TStruct obj, object val) where TStruct:struct {
            var tmp = (object)obj;
            fieldInfo.SetValue(tmp, val);
            obj = (TStruct)tmp;
        }

        internal static void SetValue<TStruct>(this PropertyInfo propertyInfo, ref TStruct obj, object value, object[] index) where TStruct:struct{
            var tmp = (object)obj;
            propertyInfo.SetValue(tmp, value, index);
            obj = (TStruct)tmp;
        }
    }
}
