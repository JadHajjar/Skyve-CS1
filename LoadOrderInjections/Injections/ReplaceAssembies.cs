using ColossalFramework.Plugins;
using KianCommons;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using PluginInfo = ColossalFramework.Plugins.PluginManager.PluginInfo;

namespace LoadOrderInjections.Injections {
    public static class ReplaceAssembies {
        public static Dictionary<string, AssemblyDefinition> asms_ = new();
        public static string[] shared_ = new string[] {
            "UnifiedUILib",
        };

        public static void Init(PluginInfo[] plugins) {
            try {
                Log.Called();

                foreach (var pluginInfo in plugins) {
                    string modPath = pluginInfo.modPath;
                    string[] dllPaths = Directory.GetFiles(modPath, "*.dll", SearchOption.AllDirectories);
                    foreach (string dllPath in dllPaths) {
                        var asm = LoadingApproach.ReadAssemblyDefinition(dllPath);
                        if (asm != null) {
                            asms_[dllPath] = asm;
                        }
                    }
                }
            } catch(Exception ex) { ex.Log(); }
        }

        internal static Version Take(this Version version, int fieldCount) =>
            new Version(version.ToString(fieldCount));

        public static string ReplaceAssemblyPacth(string dllPath) {
            try {
                if (asms_.TryGetValue(dllPath, out var asm)) {
                    string name = asm.Name.Name;
                    if (shared_.Contains(name)) {
                        var varients = asms_.Where(item => item.Value.Name.Name == name).ToArray();
                        var latest = varients.MaxBy(item => item.Value.Name.Version);

                        // if only the revision is different then return the current assembly (good for hot-reload).
                        var version0 = asm.Name.Version.Take(3);
                        var version = latest.Value.Name.Version.Take(3);
                        if (version > version0) {
                            if (name == "UnifiedUILib" && version0 == new Version(1,2,0) && version < new Version(2, 2))
                                return dllPath; // macsurgey's UUI is incompatible with UUI V2.1 or bellow
                            Log.Info($"Replacing {asm.Name.Name} V{asm.Name.Version} with V{latest.Value.Name.Version}", true);


                            // CS will not be able to detect dependency if assembly version changes. therefore we pre-load it here.
                            // if the same DLL is loaded 2 times, nothing happens.
                            LoadDLL(latest.Key); 

                            return latest.Key;
                        }
                    }
                }
            } catch (Exception ex) { ex.Log(); }
            return dllPath;
        }

        public static Assembly LoadDLL(string dllPath) {
            try {
                // use LoadPlugin to ensure FPSBooster will get a chance to patch assembly.
                return ReflectionHelpers.InvokeMethod(
                    PluginManager.instance,
                    "LoadPlugin",
                    dllPath
                    ) as Assembly;
            } catch (Exception ex) {
                ex.Log(false);
                return null;
            }
        }
    }
}
