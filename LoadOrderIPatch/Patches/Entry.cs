namespace LoadOrderIPatch.Patches {


    using System;
    using Mono.Cecil;
    using Patch.API;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.IO;
    using System.Linq;
    using static Commons;
    public class Entry : IPatch {
        public static ILogger Logger { get; private set; }
        public static IPaths GamePaths { get; private set; }
        public static string PatcherWorkingPath { get; private set; }
        public static string LocalLOMData => Path.Combine(GamePaths.AppDataPath, "LoadOrderTwo");

        public int PatchOrderAsc => 0;
        public AssemblyToPatch PatchTarget => null;

        public AssemblyDefinition Execute(
            AssemblyDefinition assemblyDefinition,
            ILogger logger,
            string patcherWorkingPath,
            IPaths gamePaths) {
            try {
                Logger = logger;
                GamePaths = gamePaths;
                PatcherWorkingPath = patcherWorkingPath;
                Log.Init();

                var args = Environment.GetCommandLineArgs();
                Log.Info("command line args are: " + string.Join(" ", args));

                if (IsDebugMono())
                    Log.Warning("Debug mono is slow! use Load order tool to change it.");

                LoadDLL(Path.Combine(patcherWorkingPath, InjectionsDLL));
                LoadDLL(Path.Combine(patcherWorkingPath, "System.Threading.dll"));

                    FileUtil.CacheWSFiles();
            } catch(Exception ex) {
                Log.Exception(ex);
            }
            return assemblyDefinition;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool IsDebugMono() {
            try {
                string file = new StackFrame(true).GetFileName();
                return file?.EndsWith(".cs") ?? false;
            } catch (Exception ex) {
                Logger.Error(ex.ToString());
                return false;
            }
        }



    }
}
