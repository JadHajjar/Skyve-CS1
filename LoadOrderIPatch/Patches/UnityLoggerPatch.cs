using Mono.Cecil;
using Mono.Cecil.Cil;
using Patch.API;
using System;
using System.Linq;
using System.Diagnostics;
using ILogger = Patch.API.ILogger;

namespace LoadOrderIPatch.Patches {

    /// <summary>
    /// modifies Unity Logger to insert time stamps
    /// </summary>
    public class UnityLoggerPatch : IPatch {
        public int PatchOrderAsc { get; } = 101;
        public AssemblyToPatch PatchTarget { get; } = new AssemblyToPatch("UnityEngine", new Version());
        private ILogger logger_;
        private string workingPath_;

        public AssemblyDefinition Execute(
            AssemblyDefinition assemblyDefinition, 
            ILogger logger, 
            string patcherWorkingPath,
            IPaths gamePaths) {
            logger_ = logger;
            workingPath_ = patcherWorkingPath;
            assemblyDefinition = LogFormatPatch(assemblyDefinition);
            return assemblyDefinition;
        }

        public AssemblyDefinition LogFormatPatch(AssemblyDefinition asmUnity)
        {
            Log.StartPatching();
            var module = asmUnity.Modules.First();
            var tDebugLogHandler = module.Types
                .First(_t => _t.FullName == "UnityEngine.DebugLogHandler");
            MethodDefinition mTarget = tDebugLogHandler.Methods
                .First(_m => _m.Name == "LogFormat");
  
            var mrAddTimeStamp = module.ImportReference(
                GetType().GetMethod(nameof(AddTimeStamp)));

            Instruction callFormat = mTarget.Body.Instructions.
                First(_c => (_c.Operand as MethodReference)?.Name == "Format");
            Instruction callAddTimeStamp = Instruction.Create(OpCodes.Call, mrAddTimeStamp);
            ILProcessor ilProcessor = mTarget.Body.GetILProcessor();
            ilProcessor.InsertAfter(callFormat, callAddTimeStamp);

            Log.Successful();
            return asmUnity;
        }

        public static Stopwatch m_Timer;
        public static string AddTimeStamp(string message)
        {
            if (m_Timer == null)
            {
                m_Timer = Stopwatch.StartNew();
                //Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
                //Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
                //Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
                //Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);
            }
            message = message.Replace("\r\n", "\n"); // work around new line issue.
            return m_Timer.ElapsedMilliseconds.ToString("#,0") + "ms | " + message;
        }
    }
}