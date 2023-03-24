namespace LoadOrderIPatch {
    extern alias Injections;
    using LoadOrderIPatch.Patches;
    using Mono.Cecil;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;

    internal static class Commons {
        internal const string InjectionsDLL = InjectionsAssemblyName + ".dll";
        internal const string InjectionsAssemblyName = "LoadOrderInjections";

        internal static AssemblyDefinition GetInjectionsAssemblyDefinition(string dir)
            => CecilUtil.ReadAssemblyDefinition(Path.Combine(dir, InjectionsDLL));

        public static Assembly LoadDLL(string dllPath) {
            try {
                Assembly assembly;
                string symPath = dllPath + ".mdb";
                if (File.Exists(symPath)) {
                    Log.Info("\nLoading " + dllPath + "\nSymbols " + symPath);
                    assembly = Assembly.Load(File.ReadAllBytes(dllPath), File.ReadAllBytes(symPath));
                } else {
                    Log.Info("Loading " + dllPath);
                    assembly = Assembly.Load(File.ReadAllBytes(dllPath));
                }
                if (assembly != null) {
                    Log.Info("Assembly " + assembly.FullName + " loaded.\n");
                } else {
                    Log.Info("Assembly at " + dllPath + " failed to load.\n");
                }
                return assembly;
            } catch (Exception ex) {
                Log.Error("Assembly at " + dllPath + " failed to load.\n" + ex.ToString());
                return null;
            }
        }

 

        public static IEnumerable<TValue []> Chunk<TValue>(
                 this IEnumerable<TValue> values,
                 int chunkSize) {
            using (var enumerator = values.GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    yield return GetChunk(enumerator, chunkSize).ToArray();
                }
            }
        }

        private static IEnumerable<T> GetChunk<T>(
                         IEnumerator<T> enumerator,
                         int chunkSize) {
            do {
                yield return enumerator.Current;
            } while (--chunkSize > 0 && enumerator.MoveNext());
        }
    }
}
