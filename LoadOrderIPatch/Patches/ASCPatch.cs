using Mono.Cecil;
using Mono.Cecil.Cil;
using Patch.API;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static LoadOrderIPatch.Commons;
using ILogger = Patch.API.ILogger;


namespace LoadOrderIPatch.Patches {
    extern alias Injections;
    using Inject = Injections.LoadOrderInjections.Injections;

    public class ASCPatch : IPatch {
        public int PatchOrderAsc { get; } = 99;
        public AssemblyToPatch PatchTarget { get; } = new AssemblyToPatch("Assembly-CSharp", new Version());
        private ILogger logger_;
        private string workingPath_;

        public AssemblyDefinition Execute(
            AssemblyDefinition assemblyDefinition, 
            ILogger logger, 
            string patcherWorkingPath,
            IPaths gamePaths) {
            try
            {
                logger_ = logger;
                workingPath_ = patcherWorkingPath;

                //GameState.GameStateUtil.logger = logger;
                //var enabled = GameState.GameStateUtil.IsModEnabled(workingPath_);
                //logger.Info("is enabled:" + enabled?.ToString() ?? "null");

                assemblyDefinition = ImproveLoggingPatch(assemblyDefinition);
                assemblyDefinition = BindEnableDisableAllPatch(assemblyDefinition);
                //assemblyDefinition = NewsFeedPanelPatch(assemblyDefinition); // handled by harmony patch
                //InstallResolverLog();
                //if (ConfigUtil.Config.AddHarmonyResolver)
                //    InstallHarmonyResolver();

                assemblyDefinition = SubscriptionManagerPatch(assemblyDefinition); // must be called to check if patch loader is effective.

                if (ConfigUtil.Config.TurnOffSteamPanels)
                    NoQueryPatch(assemblyDefinition); // handled by harmony patch
            } catch (Exception ex) { logger.Error(ex.ToString());  }
            return assemblyDefinition;
        }

        /// <summary>
        /// replaces the normal loading process with subscription manger tool
        /// </summary>
        public AssemblyDefinition SubscriptionManagerPatch(AssemblyDefinition ASC)
        {
            Log.StartPatching();
            var module = ASC.Modules.First();
            MethodDefinition mTarget = module.GetMethod("Starter.Awake");
            var instructions = mTarget.Body.Instructions;
            ILProcessor ilProcessor = mTarget.Body.GetILProcessor();
            AssemblyDefinition asm = GetInjectionsAssemblyDefinition(workingPath_);

            /**********************************/
            bool sman2 = Environment.GetCommandLineArgs().Any(_arg => _arg == "-sman2");
            if (sman2) {
                // this is for testing
                var method = asm.MainModule.GetMethod("LoadOrderInjections.SubscriptionManager.DoNothing");
                var callInjection = Instruction.Create(OpCodes.Call, module.ImportReference(method));
                Instruction brLast = Instruction.Create(OpCodes.Br, instructions.Last()); // return
                Instruction CallBoot = instructions.First(_c => _c.Calls("Boot"));
                Instruction ldfldLoadIntro = instructions.First(_c => _c.Is(OpCodes.Ldfld, "m_loadIntro"));

                ilProcessor.InsertBefore(ldfldLoadIntro, callInjection, brLast);
                Log.Info($"inserted {callInjection} before {ldfldLoadIntro}");
            } else {
                var method = asm.MainModule.GetMethod(
                    "LoadOrderInjections.SubscriptionManager.PostBootAction");
                var call1 = Instruction.Create(OpCodes.Call, module.ImportReference(method));
                Instruction CallBoot = instructions.First(_c => _c.Calls("Boot"));
                Instruction BranchTarget = instructions.Last();// return
                Instruction BrTrueEnd = Instruction.Create(OpCodes.Brtrue, BranchTarget);

                ilProcessor.InsertAfter(CallBoot, call1, BrTrueEnd);
            }
            /**********************************/
            {
                var method = asm.MainModule.GetMethod(
                    "LoadOrderInjections.SteamUtilities.RegisterEvents");
                var call2 = Instruction.Create(OpCodes.Call, module.ImportReference(method));
                ilProcessor.Prefix(call2);
            }
            /**********************************/

            Log.Successful();
            return ASC;
        }

        /// <summary>
        /// removes call to query which causes steam related errors and puts CollossalManged in an unstable state.
        /// </summary>
        public void NoQueryPatch(AssemblyDefinition ASC)
        {
            Log.StartPatching();
            var module = ASC.Modules.First();
            var targetMethod = module.GetMethod("WorkshopAdPanel.Awake");
            var instructions = targetMethod.Body.Instructions;
            ILProcessor ilProcessor = targetMethod.Body.GetILProcessor();
            Instruction callQuery = instructions.First(_c => _c.Calls("QueryItems"));
            ilProcessor.Remove(callQuery); // the pop instruction after cancels out the load instruction before.
            Log.Successful();
        }

        public AssemblyDefinition ImproveLoggingPatch(AssemblyDefinition ASC)
        {
            Log.StartPatching();
            ModuleDefinition module = ASC.Modules.First();
            var entryPoint = module.GetMethod("Starter.Awake");
            var mInjection = GetType().GetMethod(nameof(ApplyGameLoggingImprovements));
            var mrInjection = module.ImportReference(mInjection);

            Instruction first = entryPoint.Body.Instructions.First();
            Instruction callInjection = Instruction.Create(OpCodes.Call, mrInjection);

            ILProcessor ilProcessor = entryPoint.Body.GetILProcessor();
            ilProcessor.InsertBefore(first, callInjection);

            Log.Successful();
            return ASC;
        }

        /// <summary>
        /// Reconfigure Unity logger to remove empty lines of call stack.
        /// Stacktrace is disabled by Unity when game runs in Build mode anyways.
        /// </summary>
        static bool logImproved_ = false;
        public static void ApplyGameLoggingImprovements()
        {
            Debug.Log("Starter.Awake() called.");
            if (logImproved_) return;
            logImproved_ = true;
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
            Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);
            //Debug.Log("************************** Removed logging stacktrace bloat **************************");
        }

        public AssemblyDefinition BindEnableDisableAllPatch(AssemblyDefinition ASC)
        {
            Log.StartPatching();
            var module = ASC.MainModule;
            var mTarget = module.GetMethod("ContentManagerPanel.BindEnableDisableAll");

            // set disclaimer ID to null. this avoids OnSettingsUI getting called all the time.
            Instruction first = mTarget.Body.Instructions.First();
            Instruction loadNull = Instruction.Create(OpCodes.Ldnull);
            ParameterDefinition pDisclaimerID = mTarget.Parameters.Single(_p => _p.Name == "disclaimerID");
            Instruction storeDisclaimerID = Instruction.Create(OpCodes.Starg, pDisclaimerID);

            ILProcessor ilProcessor = mTarget.Body.GetILProcessor();
            ilProcessor.InsertBefore(first, loadNull);
            ilProcessor.InsertAfter(loadNull, storeDisclaimerID);
            Log.Successful();

            return ASC;
        }

        public AssemblyDefinition NewsFeedPanelPatch(AssemblyDefinition ASC)
        {
            Log.StartPatching();
            var module = ASC.MainModule;
            Instruction ret = Instruction.Create(OpCodes.Ret);
            {
                var mTarget = module.GetMethod("NewsFeedPanel.RefreshFeed");
                Instruction first = mTarget.Body.Instructions.First();
                ILProcessor ilProcessor = mTarget.Body.GetILProcessor();
                ilProcessor.InsertBefore(first, ret);
            }
            {
                var mTarget = module.GetMethod("NewsFeedPanel.OnFeedNext");
                Instruction first = mTarget.Body.Instructions.First();
                ILProcessor ilProcessor = mTarget.Body.GetILProcessor();
                ilProcessor.InsertBefore(first, ret);
            }

            Log.Successful();
            return ASC;
        }

        //        public AssemblyDefinition HandleResolve(AssemblyDefinition ASC) {
        //#if DEBUG
        //            Log.StartPatching();
        //            var module = ASC.Modules.First();
        //            var targetMethod = module.GetMethod("Starter.Awake");
        //            var instructions = targetMethod.Body.Instructions;
        //            ILProcessor ilProcessor = targetMethod.Body.GetILProcessor();
        //            Log.Successful();
        //#endif
        //            return ASC;
        //        }

        public void InstallResolverLog() {
            //Log.StartPatching();
            //ResolveEventHandler resolver = Inject.Logs.ResolverLog;
            //AppDomain.CurrentDomain.AssemblyResolve += resolver;
            //AppDomain.CurrentDomain.TypeResolve += resolver;
            //AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += resolver;
            //Log.Successful();
        }

        private static readonly Version MinHarmonyVersionToHandle = new Version(2, 0, 0, 8);
        const string HarmonyName = "0Harmony";
        const string HarmonyName2 = "CitiesHarmony.Harmony";

        public void InstallHarmonyResolver() {
            Log.StartPatching();
            ResolveEventHandler resolver = LoadOrderHarmonyResolver;
            AppDomain.CurrentDomain.AssemblyResolve += resolver;
            AppDomain.CurrentDomain.TypeResolve += resolver;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += resolver;

            Log.Successful();
        }

        public static Assembly LoadOrderHarmonyResolver(object sender, ResolveEventArgs args)
        {
            try {
                //Debug.Log($"LoadOrderHarmonyResolver(sender={sender}, args.Name={args.Name}) called."
                //    + Environment.StackTrace);
                if(IsHarmony2(new AssemblyName(args.Name))) {
                    Debug.Log($"LoadOrderHarmonyResolver(): sender='{sender}'" +
                        $"\n\ttrying to resolve '{args.Name}'\n" +Environment.StackTrace );
                    var ret = GetHarmony2();
                    if(ret == null) {
                        Debug.Log(
                            $"LoadOrderHarmonyResolver: Failed to find assembly 0Harmony " +
                            $"with version >= {MinHarmonyVersionToHandle} " +
                            $"while trying to resolve {args.Name}");
                    } else {
                        Debug.Log($"LoadOrderHarmonyResolver: Resolved '{args.Name}' to {ret}");
                    }
                    return ret;
                }

            } catch (Exception e) {
                UnityEngine.Debug.LogException(e);
            }

            return null;
        }

        public static bool IsHarmony2(AssemblyName assemblyName)
        {
            return (assemblyName.Name == HarmonyName2  || assemblyName.Name == HarmonyName) &&
                   assemblyName.Version >= MinHarmonyVersionToHandle;
        }

        public static Assembly GetHarmony2()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => IsHarmony2(a.GetName()));
        }
    }
}