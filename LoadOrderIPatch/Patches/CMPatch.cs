using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.PlatformServices;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Patch.API;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using static LoadOrderIPatch.Commons;
using ILogger = Patch.API.ILogger;

namespace LoadOrderIPatch.Patches {
    extern alias Injections;
    using SteamUtilities = Injections.LoadOrderInjections.SteamUtilities;
    using CMPatchHeleprs = Injections.LoadOrderInjections.CMPatchHelpers;

    public class CMPatch : IPatch {
        public int PatchOrderAsc { get; } = 100;
        public AssemblyToPatch PatchTarget { get; } = new AssemblyToPatch("ColossalManaged", new Version(0, 3, 0, 0));
        private ILogger logger_;
        private string workingPath_;

        public AssemblyDefinition Execute(
            AssemblyDefinition assemblyDefinition,
            ILogger logger,
            string patcherWorkingPath,
            IPaths gamePaths) {
            try {
                logger_ = logger;
                workingPath_ = patcherWorkingPath;

                bool noReporters = Environment.GetCommandLineArgs().Any(_arg => _arg == "-cold-reload");
                if (noReporters)
                    NoReportersPatch(assemblyDefinition);

                LoadAssembliesROPatch(assemblyDefinition);

                assemblyDefinition = LoadAssembliesPatch(assemblyDefinition);
                CreateUserModInstancePatch(assemblyDefinition);
                AddPluginsPatch(assemblyDefinition); // changed
#if DEBUG
                //assemblyDefinition = InsertPrintStackTrace(assemblyDefinition);
#endif

                EnsureIncludedExcludedPackagePatch(assemblyDefinition);

                bool noAssets = Environment.GetCommandLineArgs().Any(_arg => _arg == "-noAssets");
                if (noAssets) {
                    assemblyDefinition = NoCustomAssetsPatch(assemblyDefinition);
                } else {
                    ExcludeAssetFilePatch(assemblyDefinition);
                    ExcludeAssetDirPatch(assemblyDefinition);
                }
                LoadCloudPackagesPatch(assemblyDefinition);

                LoadPluginPatch(assemblyDefinition);
            } catch (Exception ex) { ex.Log(); }
            return assemblyDefinition;
        }


        // avoid memory leak by skipping reporters.
        public void NoReportersPatch(AssemblyDefinition CM) {
            Log.StartPatching();
            var module = CM.Modules.First();
            {
                MethodDefinition mTarget1 = module.GetMethod("ColossalFramework.Packaging.PackageManager.CreateReporter");

                var instructions = mTarget1.Body.Instructions;
                ILProcessor ilProcessor = mTarget1.Body.GetILProcessor();

                /**********************************/
                Instruction ret = Instruction.Create(OpCodes.Ret);
                ilProcessor.Prefix(ret);
            }

            {
                MethodDefinition mTarget = module.GetMethod("ColossalFramework.Plugins.PluginManager.CreateReporters");
                var instructions = mTarget.Body.Instructions;
                ILProcessor ilProcessor = mTarget.Body.GetILProcessor();

                /**********************************/
                Instruction ret = Instruction.Create(OpCodes.Ret);
                ilProcessor.Prefix(ret);
            }

            Log.Successful();
        }

        /// <summary>
        /// loads assembly with symbols
        /// </summary>
        public void LoadPluginPatch(AssemblyDefinition CM) {
            Log.StartPatching();
            var module = CM.MainModule;
            var tPluginManager = module.GetType("ColossalFramework.Plugins", "PluginManager");

            MethodDefinition mTarget = tPluginManager.GetMethod("LoadPlugin");
            var ilprocessor = mTarget.Body.GetILProcessor();
            var instructions = mTarget.Body.Instructions;

            const string fpsMethod = "LoadOrScanAndPatch";
            bool touchedByFPS = instructions.Any(code => code.Calls(fpsMethod));
            if (touchedByFPS) {
                Log.Info("ignoring LoadPluginPatch because FPSBooster already loads symbols");
                return;
            }

            var mrInjection = module.ImportReference(
                GetType().GetMethod(nameof(LoadPlugingWithSymbols)));

            ilprocessor.Prefix(
                Instruction.Create(OpCodes.Ldarg_1),
                Instruction.Create(OpCodes.Call, mrInjection),
                Instruction.Create(OpCodes.Ret));

            Log.Successful();
        }

        public static Assembly LoadPlugingWithSymbols(string dllPath) {
            try {
                Assembly assembly;
                string symPath = dllPath + ".mdb";
                if (File.Exists(symPath)) {
                    CODebugBase<InternalLogChannel>.Log(InternalLogChannel.Mods, "Loading " + dllPath + "\nSymbols " + symPath);
                    assembly = Assembly.Load(File.ReadAllBytes(dllPath), File.ReadAllBytes(symPath));
                } else {
                    CODebugBase<InternalLogChannel>.Log(InternalLogChannel.Mods, "Loading " + dllPath);
                    assembly = Assembly.Load(File.ReadAllBytes(dllPath));
                }
                if (assembly != null) {
                    CODebugBase<InternalLogChannel>.Log(InternalLogChannel.Mods, "Assembly " + assembly.FullName + " loaded.");
                } else {
                    CODebugBase<InternalLogChannel>.Error(InternalLogChannel.Mods, "Assembly at " + dllPath + " failed to load.");
                }
                return assembly;
            } catch (Exception ex) {
                CODebugBase<InternalLogChannel>.Error(InternalLogChannel.Mods, "Assembly at " + dllPath + " failed to load.\n" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// if the assembly already exists in the current domain, 
        /// do not double load it.
        /// Note: this patch is never used at the moment. Also if I do use it, it might be incompatible with FPSBooster
        /// Note2: THis does not prevent FPSBooster from double loading assemblies.
        /// </summary>
        /// <param name="CM"></param>
        public void NoDoubleLoadPatch(AssemblyDefinition CM) {
            Log.StartPatching();
            var cm = CM.MainModule;
            var mTarget = cm.GetMethod(
                "ColossalFramework.Plugins.PluginManager.LoadPlugin");
            ILProcessor ilProcessor = mTarget.Body.GetILProcessor();
            var instructions = mTarget.Body.Instructions;
            var first = instructions.First();

            var loi = GetInjectionsAssemblyDefinition(workingPath_);
            var mInjection = loi.MainModule.GetMethod(
                "LoadOrderInjections.Injections.LoadingApproach.ExistingAssembly");
            var mrInjection = cm.ImportReference(mInjection);

            var loadDllPath = mTarget.GetLDArg("dllPath");
            var callExistingAssembly = Instruction.Create(OpCodes.Call, mrInjection);
            var storeResult = Instruction.Create(OpCodes.Stloc_0);
            var returnResult = instructions.Last(
                c => c.OpCode == OpCodes.Ldloc_0 &&
                c.Next?.OpCode == OpCodes.Ret);
            var GotoToReturnResultIfNotNull = Instruction.Create(OpCodes.Brfalse, returnResult);

            /*
            result =  ExistingAssembly(dllPath)
            if(result is not null) goto ReturnResult
            ...
            ReturnResult: 
            return result

             */
            ilProcessor.Prefix(loadDllPath, callExistingAssembly, storeResult, GotoToReturnResultIfNotNull);

            Log.Successful();
        }

        /// <summary>
        /// only load latest version of shared assemblies.
        /// </summary>
        public void LoadAssembliesROPatch(AssemblyDefinition CM) {
            Log.StartPatching();
            var module = CM.MainModule;
            var tPluginManager = module.GetType("ColossalFramework.Plugins.PluginManager");
            var mTarget = tPluginManager.GetMethod("LoadAssembliesRO");

            ILProcessor ilProcessor = mTarget.Body.GetILProcessor();
            var instructions = mTarget.Body.Instructions;

            var loi = GetInjectionsAssemblyDefinition(workingPath_);

            {
                var mReplaceAssemblyPath = loi.MainModule.GetMethod("LoadOrderInjections.Injections.ReplaceAssembies.ReplaceAssemblyPacth");
                var callReplaceAssemblyPath = Instruction.Create(
                    OpCodes.Call, module.ImportReference(mReplaceAssemblyPath));
                var storeFile = instructions.Single(_c => _c.OpCode == OpCodes.Stloc_3); // foreach(file in files)
                ilProcessor.InsertBefore(storeFile, callReplaceAssemblyPath);
            }

            Log.Successful();
        }

        /// <summary>
        /// inject logs in CreateUserModInstance
        ///  - method called
        ///  - method successful
        ///  - before everyGetExportedTypes
        /// </summary>
        public void CreateUserModInstancePatch(AssemblyDefinition CM) {
            try {
                Log.StartPatching();
                var module = CM.MainModule;

                var type1 = module.GetType("ColossalFramework.Plugins.PluginManager");
                var type2 = type1.NestedTypes.Single(_t => _t.Name == "PluginInfo");
                var mTarget = type2.GetMethod("CreateUserModInstance");

                ILProcessor ilProcessor = mTarget.Body.GetILProcessor();
                var instructions = mTarget.Body.Instructions;

                var loi = GetInjectionsAssemblyDefinition(workingPath_);
                var tLogs = loi.MainModule.GetType("LoadOrderInjections.Injections.Logs");

                Instruction CallLogsMethod(string name) {
                    var method = tLogs.GetMethod(name);
                    var methodReference = module.ImportReference(method);
                    return Instruction.Create(OpCodes.Call, methodReference);
                }


                var loadPluginInfo = Instruction.Create(OpCodes.Ldarg_0);

                {
                    var callBeforeCtor = CallLogsMethod("BeforeUserModCtor");
                    ilProcessor.Prefix(loadPluginInfo.Duplicate(), callBeforeCtor);
                }
                {
                    var callAfterCtor = CallLogsMethod("AfterUserModCtor");
                    ilProcessor.InsertBefore(instructions.Last(), loadPluginInfo.Duplicate(), callAfterCtor); // postfix
                }
                {
                    var callGetExportedTypes = instructions.First(_c => _c.Calls("GetExportedTypes"));
                    var loadAsm = Instruction.Create(OpCodes.Dup); // duplicate argument
                    var callBeforeGetExportedTypes = CallLogsMethod("BeforeCreateUserModInstanceGetExportedTypes");
                    ilProcessor.InsertBefore(callGetExportedTypes, loadAsm, callBeforeGetExportedTypes);
                }

                Log.Successful();
            } catch (Exception ex) { ex.Log(); }
        }

        public void LoadCloudPackagesPatch(AssemblyDefinition CM) {
            try {
                Log.Called();
                var module = CM.MainModule;
                var mTarget = module.GetMethod("ColossalFramework.Packaging.PackageManager.LoadCloudPackages");
                ILProcessor ilProcessor = mTarget.Body.GetILProcessor();
                var instructions = mTarget.Body.Instructions;
                var last = instructions.Last();

                var mIsCloudEnabled = typeof(CMPatch).GetMethod(nameof(IsCloudEnabled));
                var callIsCloudEnabled = Instruction.Create(OpCodes.Call, module.ImportReference(mIsCloudEnabled));
                var skipIfFalse = Instruction.Create(OpCodes.Brfalse, last);

                ilProcessor.Prefix(callIsCloudEnabled, skipIfFalse);
                Log.Successful();
            } catch (Exception ex) { ex.Log(); }
        }

        public static bool IsCloudEnabled() => SteamUtilities.IsCloudEnabled();

        public AssemblyDefinition NoCustomAssetsPatch(AssemblyDefinition CM) {
            try {
                Log.StartPatching();
                var module = CM.MainModule;
                var type = module.GetType("ColossalFramework.Packaging.PackageManager");
                var mTargets = type.Methods.Where(_m =>
                    _m.Name.StartsWith("Load") && _m.Name.EndsWith("Packages")); //Load*Packages
                foreach (var mTarget in mTargets) {
                    bool top = mTarget.Name == "LoadPackages" && mTarget.Parameters.Count == 0;
                    if (top) continue;
                    bool loadPath = mTarget.HasParameter("path"); // LoadPackages(string path,bool)
                    ILProcessor ilProcessor = mTarget.Body.GetILProcessor();
                    var instructions = mTarget.Body.Instructions;
                    var first = instructions.First();

                    Log.Info("patching " + mTarget.Name);
                    if (loadPath) {
                        // skip method only if path is asset path
                        var ret = instructions.Last();
                        var LdArgPath = mTarget.GetLDArg("path");
                        var mIsAssetPath = GetType().GetMethod(nameof(IsAssetPath));
                        var callIsAssetPath = Instruction.Create(OpCodes.Call, module.ImportReference(mIsAssetPath));
                        var skipIfAsset = Instruction.Create(OpCodes.Brtrue, ret);
                        ilProcessor.Prefix(LdArgPath, callIsAssetPath, skipIfAsset);
                    } else {
                        // return to skip method.
                        var ret = Instruction.Create(OpCodes.Ret);
                        ilProcessor.Prefix(ret);
                    }
                }

                Log.Successful();
            } catch (Exception ex) { ex.Log(); }
            return CM;
        }

        public static bool IsAssetPath(string path) {
            return path == DataLocation.assetsPath;
        }

        /// <summary>
        /// if excluded asset is updated then we can have both 'file.crp' and '_file.crp'.
        /// this patch moves 'file.crp' to '_file.crp'
        /// </summary>
        public void EnsureIncludedExcludedPackagePatch(AssemblyDefinition CM) {
            try {
                Log.StartPatching();
                var cm = CM.MainModule;
                var type = cm.GetType("ColossalFramework.Packaging.PackageManager");
                var mTargets = type.Methods.Where(_m => _m.Name == "LoadPackages");
                var tCMPatchHelpers = GetInjectionsAssemblyDefinition(workingPath_).MainModule.GetType("LoadOrderInjections.CMPatchHelpers");
                var mCheckFiles = tCMPatchHelpers.GetMethod(nameof(CMPatchHeleprs.CheckFiles));
                var mIsDirectoryExcluded = tCMPatchHelpers.GetMethod(nameof(CMPatchHeleprs.IsDirectoryExcluded));
                var mIsIDExcluded = tCMPatchHelpers.GetMethod(nameof(CMPatchHeleprs.IsIDExcluded));


                // insert checkfiles
                foreach (var mTarget in mTargets) {
                    ILProcessor ilProcessor = mTarget.Body.GetILProcessor();
                    var instructions = mTarget.Body.Instructions;

                    var callGetFiles = instructions.FirstOrDefault(_c => _c.Calls("GetFiles"));
                    if (callGetFiles != null) {
                        Log.Info($"injecting CheckFiles() in {mTarget}");

                        var callCheckFiles = Instruction.Create(OpCodes.Call, cm.ImportReference(mCheckFiles));
                        ilProcessor.InsertBefore(callGetFiles, callCheckFiles);
                    }
                }

                Log.Successful();
            } catch (Exception ex) { ex.Log(); }
        }

        /// <summary>
        /// if asset containing dir is excluded (has _ or .excluded) then asset is skipped.
        /// </summary>
        public void ExcludeAssetDirPatch(AssemblyDefinition CM) {
            try {
                Log.StartPatching();
                var cm = CM.MainModule;
                var type = cm.GetType("ColossalFramework.Packaging.PackageManager");
                var mTargets = type.Methods.Where(_m => _m.Name == "LoadPackages");
                var tCMPatchHelpers = GetInjectionsAssemblyDefinition(workingPath_).MainModule.GetType("LoadOrderInjections.CMPatchHelpers");
                var mCheckFiles = tCMPatchHelpers.GetMethod(nameof(CMPatchHeleprs.CheckFiles));
                var mIsDirectoryExcluded = tCMPatchHelpers.GetMethod(nameof(CMPatchHeleprs.IsDirectoryExcluded));
                var mIsIDExcluded = tCMPatchHelpers.GetMethod(nameof(CMPatchHeleprs.IsIDExcluded));

                // inject Is*Excluded
                foreach (var mTarget in mTargets) {
                    ILProcessor ilProcessor = mTarget.Body.GetILProcessor();
                    var instructions = mTarget.Body.Instructions;
                    var last = instructions.Last();

                    if (mTarget.Parameters.Any(p => p.Name == "id")) {
                        // public void LoadPackages(PublishedFileId id)
                        Log.Info($"injecting IsIDExcluded() in {mTarget}");
                        ilProcessor.Prefix(
                            mTarget.GetLDArg("id"),
                            Instruction.Create(OpCodes.Call, cm.ImportReference(mIsIDExcluded)), // call IsIDExcluded(id)
                            Instruction.Create(OpCodes.Brtrue, last)); // return if excluded
                    } else if (mTarget.Parameters.Any(p => p.Name == "path")) {
                        // public void LoadPackages(string path, bool createReporter)
                        Log.Info($"injecting IsDirectoryExcluded() in {mTarget}");
                        ilProcessor.Prefix(
                            mTarget.GetLDArg("path"),
                            Instruction.Create(OpCodes.Call, cm.ImportReference(mIsDirectoryExcluded)), // call IsDirectoryExcluded(path)
                            Instruction.Create(OpCodes.Brtrue, last)); // return if excluded
                    }
                }

                Log.Successful();
            } catch (Exception ex) { ex.Log(); }
        }

        /// <summary>
        /// if asset is '_file.crp' then asset is skipped.
        /// </summary>
        public void ExcludeAssetFilePatch(AssemblyDefinition CM) {
            try {
                Log.StartPatching();
                var module = CM.MainModule;
                var type = module.GetType("ColossalFramework.Packaging.PackageManager");

                //void Update(string path)
                //void Update(PublishedFileId id, string path)
                var mTargets = type.Methods.Where(_m => _m.Name == "Update").ToList();

                // LoadPackages(string path, bool)
                var mLoadPackages = type.Methods.Single(_m =>
                   _m.Name == "LoadPackages" && _m.HasParameter("path"));

                mTargets.Add(mLoadPackages);


                foreach (var mTarget in mTargets) {
                    ILProcessor ilProcessor = mTarget.Body.GetILProcessor();
                    var instructions = mTarget.Body.Instructions;
                    var first = instructions.First();
                    var last = instructions.Last();

                    // skip method only if path is asset path
                    var LdArgPath = mTarget.GetLDArg("path");
                    var mIsExcluded = typeof(Packages).GetMethod(nameof(Packages.IsFileExcluded));
                    var callIsExcluded = Instruction.Create(OpCodes.Call, module.ImportReference(mIsExcluded));
                    var skipIfExcluded = Instruction.Create(OpCodes.Brtrue, last); // goto to return.
                    ilProcessor.Prefix(LdArgPath, callIsExcluded, skipIfExcluded);
                }

                Log.Successful();
            }catch(Exception ex) {
                Log.Exception(ex);
            }
        }

#if DEBUG
        // get the stack trace for debugging purposes.
        // modify this method to print the desired stacktrace. 
        public AssemblyDefinition InsertPrintStackTrace(AssemblyDefinition CM)
        {
            Log.StartPatching();
            var module = CM.MainModule;
            var type = module.GetType("ColossalFramework.PlatformServices.PlatformServiceBehaviour");
            var mTarget = type.Methods.Single(_m => _m.Name == "Awake");
            ILProcessor ilProcessor = mTarget.Body.GetILProcessor();
            var instructions = mTarget.Body.Instructions;
            var first = instructions.First();

            var mInjection = GetType().GetMethod(nameof(LogStackTrace));
            var mrInjection = module.ImportReference(mInjection);
            var callInjection = Instruction.Create(OpCodes.Call, mrInjection);

            ilProcessor.InsertBefore(first, callInjection);

            Log.Info("PlatformServiceBehaviour_Awake_Patch applied successfully!");
            return CM;
        }
#endif

        public static void LogStackTrace()
        {
            UnityEngine.Debug.Log("[LoadOrderIPatch] stack trace is: " + Environment.StackTrace);
        }


        /// <summary>
        /// Sorts Assembly dictionary (hackish) at the beginning of PluginManager.LoadAssemblies()
        /// </summary>
        public AssemblyDefinition LoadAssembliesPatch(AssemblyDefinition CM)
        {
            Log.StartPatching();
            var tPluginManager = CM.MainModule.Types
                .First(_t => _t.FullName == "ColossalFramework.Plugins.PluginManager");
            MethodDefinition mTarget = tPluginManager.Methods
                .First(_m => _m.Name == "LoadAssemblies");

            AssemblyDefinition asm = GetInjectionsAssemblyDefinition(workingPath_);
            var tSortPlugins = asm.MainModule.Types
                .First(_t => _t.Name == "SortPlugins");
            MethodDefinition mdSort = tSortPlugins.Methods
                .First(_m => _m.Name == "Sort");
            MethodReference mrSort = tPluginManager.Module.ImportReference(mdSort);

            Instruction loadPlugins = Instruction.Create(OpCodes.Ldarg_1);
            Instruction callSort = Instruction.Create(OpCodes.Call, mrSort);
            ILProcessor ilProcessor = mTarget.Body.GetILProcessor();
            ilProcessor.Prefix(loadPlugins, callSort);

            Log.Successful();
            return CM;
        }

        /// <summary>
        /// loads LoadOrderInjections.dll at the beginning of PluginManger.LoadPlugins()
        /// </summary>
        public AssemblyDefinition LoadPluginsPatch(AssemblyDefinition CM)
        {
            Log.StartPatching();
            var tPluginManager = CM.MainModule.Types
                .First(_t => _t.FullName == "ColossalFramework.Plugins.PluginManager");
            MethodDefinition mTarget = tPluginManager.Methods
                .First(_m => _m.Name == "LoadPlugins");

            MethodDefinition mInjection = tPluginManager.Methods
                .First(_m => _m.Name == "LoadPlugin");

            var dllPath = Path.Combine(workingPath_, "LoadOrderInjections.dll");

            Instruction loadThis = Instruction.Create(OpCodes.Ldarg_0);
            Instruction loadDllPath = Instruction.Create(OpCodes.Ldstr, dllPath);
            Instruction callInjection = Instruction.Create(OpCodes.Call, mInjection);

            Instruction first = mTarget.Body.Instructions.First();
            ILProcessor ilProcessor = mTarget.Body.GetILProcessor();
            ilProcessor.InsertBefore(first, loadThis); // load pluggins arg
            ilProcessor.InsertAfter(loadThis, loadDllPath);
            ilProcessor.InsertAfter(loadDllPath, callInjection);
            Log.Successful();
            return CM;
        }

        /// <summary>
        /// inserts time stamps about enabling plugins.
        /// </summary>
        public void AddPluginsPatch(AssemblyDefinition CM) {
            try {
                Log.StartPatching();
                var tPluginManager = CM.MainModule.Types
                 .First(_t => _t.FullName == "ColossalFramework.Plugins.PluginManager");
                MethodDefinition mTarget = tPluginManager.Methods
                    .First(_m => _m.Name == "AddPlugins");
                ILProcessor ilProcessor = mTarget.Body.GetILProcessor();
                var codes = mTarget.Body.Instructions.ToList();

                Instruction LoadPlugins = mTarget.GetLDArg("plugins");
                Instruction InvokeOnEnabled = codes.First(
                    _c => (_c.Operand as MethodReference)?.Name == "Invoke");

                AssemblyDefinition asm = GetInjectionsAssemblyDefinition(workingPath_);
                var tLogs = asm.MainModule.Types
                    .First(_t => _t.Name == "Logs");

                MethodDefinition mdBeforeEnable = tLogs.Methods
                    .First(_m => _m.Name == "BeforeEnable");
                MethodReference mrBeforeEnable = tPluginManager.Module.ImportReference(mdBeforeEnable);

                MethodDefinition mdAfterEnable = tLogs.Methods
                    .First(_m => _m.Name == "AfterEnable");
                MethodReference mrAfterEnable = tPluginManager.Module.ImportReference(mdAfterEnable);

                Instruction LoadPluginInfo = InvokeOnEnabled.Previous.Previous.Previous; // get pluginInfo in mOnEnabled.Invoke(pluginInfo.userModInstance, null)
                Instruction callBeforeEnable = Instruction.Create(OpCodes.Call, mrBeforeEnable);
                Instruction callAfterEnable = Instruction.Create(OpCodes.Call, mrAfterEnable);

                // inject calls
                ilProcessor.InsertBefore(InvokeOnEnabled, LoadPluginInfo.Duplicate(), callBeforeEnable);
                ilProcessor.InsertAfter(InvokeOnEnabled, LoadPluginInfo.Duplicate(), callAfterEnable);

                Log.Successful();
            } catch (Exception ex) { ex.Log(); }
        }
    }
}