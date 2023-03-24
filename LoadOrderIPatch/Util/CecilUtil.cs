namespace LoadOrderIPatch {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using System.IO;
    using LoadOrderIPatch.Patches;
    using Patch.API;
    using System.Reflection;

    public static class CecilUtil {
        internal static AssemblyDefinition ReadAssemblyDefinition(string dllpath) {
            try {
                Log.Called(dllpath);
                var r = new MyAssemblyResolver();
                r.AddSearchDirectory(Entry.GamePaths.ManagedFolderPath);
                r.AddSearchDirectory(Path.GetDirectoryName(dllpath));
                var readerParameters = new ReaderParameters {
                    ReadWrite = false,
                    InMemory = true,
                    AssemblyResolver = r,
                };
                r.ReaderParameters = readerParameters;
                var asm = AssemblyDefinition.ReadAssembly(dllpath, readerParameters);

                if (asm == null)
                    Log.Info("Assembly Definition at " + dllpath + " failed to load.");

                return asm;
            }
            catch (Exception ex) {
                Log.Info("Assembly Definition at " + dllpath + " failed to load.\n" + ex.Message);
                return null;
            }
        }

        public static Instruction Duplicate(this Instruction instruction)
        {
            if (instruction == null) throw new ArgumentNullException("instruction");
            if (instruction.OpCode == null) throw new ArgumentException("instruction.Opcode is null");
            try {
                if(instruction.Operand == null) {
                    return Instruction.Create(instruction.OpCode);
                } else {
                    var ret = Instruction.Create(OpCodes.Ldarg_0); // hack around.
                    ret.OpCode = instruction.OpCode;
                    ret.Operand = instruction.Operand;
                    return ret;
                }
            } catch {
                Log.Error($"instruction='{instruction}' opcode='{instruction?.OpCode}' operand='{instruction?.Operand}'");
                throw;
            }
        }

        public static Instruction GetLDArg(this MethodDefinition method, string argName, bool throwOnError = true) {
            if (!throwOnError && !HasParameter(method, argName))
                return null;
            byte idx = (byte)GetParameterLoc(method, argName);
            if (!method.IsStatic)
                idx++; // first argument is object instance.
            if (idx == 0) {
                return Instruction.Create(OpCodes.Ldarg_0);
            } else if (idx == 1) {
                return Instruction.Create(OpCodes.Ldarg_1);
            } else if (idx == 2) {
                return Instruction.Create(OpCodes.Ldarg_2);
            } else if (idx == 3) {
                return Instruction.Create(OpCodes.Ldarg_3);
            } else {
                return Instruction.Create(OpCodes.Ldarg_S, idx);
            }
        }

        /// <summary>
        /// Post condition: for instance mCreateInstruction add one to get argument location
        /// </summary>
        public static byte GetParameterLoc(this MethodDefinition method, string name) {
            var parameters = method.Parameters;
            for (byte i = 0; i < parameters.Count; ++i) {
                if (parameters[i].Name == name) {
                    return i;
                }
            }
            throw new Exception($"did not found parameter with name:<{name}>");
        }

        public static bool HasParameter(this MethodDefinition method, string name) {
            foreach(var param in method.Parameters) {
                if (param.Name == name)
                    return true;
            }
            return false;
        }

        public static bool IsSameInstruction(this Instruction a, Instruction b, bool debug = false) {
            if (a.OpCode == b.OpCode) {
                if (a.Operand == b.Operand) {
                    return true;
                }

                // This special code is needed for some reason because the == operator doesn't work on System.Byte
                return (a.Operand is byte aByte && b.Operand is byte bByte && aByte == bByte);
            } else {
                return false;
            }
        }

        public static bool IsLdLoc(this Instruction instruction) {
            return (instruction.OpCode == OpCodes.Ldloc_0 || instruction.OpCode == OpCodes.Ldloc_1 ||
                    instruction.OpCode == OpCodes.Ldloc_2 || instruction.OpCode == OpCodes.Ldloc_3
                    || instruction.OpCode == OpCodes.Ldloc_S || instruction.OpCode == OpCodes.Ldloc
                );
        }

        public static bool IsStLoc(this Instruction instruction) {
            return (instruction.OpCode == OpCodes.Stloc_0 || instruction.OpCode == OpCodes.Stloc_1 ||
                    instruction.OpCode == OpCodes.Stloc_2 || instruction.OpCode == OpCodes.Stloc_3
                    || instruction.OpCode == OpCodes.Stloc_S || instruction.OpCode == OpCodes.Stloc
                );
        }

        /// <summary>
        /// Get the instruction to load the variable which is stored here.
        /// </summary>
        public static Instruction BuildLdLocFromStLoc(this Instruction instruction) {
            if (instruction.OpCode == OpCodes.Stloc_0) {
                return Instruction.Create(OpCodes.Ldloc_0);
            } else if (instruction.OpCode == OpCodes.Stloc_1) {
                return Instruction.Create(OpCodes.Ldloc_1);
            } else if (instruction.OpCode == OpCodes.Stloc_2) {
                return Instruction.Create(OpCodes.Ldloc_2);
            } else if (instruction.OpCode == OpCodes.Stloc_3) {
                return Instruction.Create(OpCodes.Ldloc_3);
            } else if (instruction.OpCode == OpCodes.Stloc_S) {
                return Instruction.Create(OpCodes.Ldloc_S, (sbyte)instruction.Operand);
            } else if (instruction.OpCode == OpCodes.Stloc) {
                return Instruction.Create(OpCodes.Ldloc, (sbyte)instruction.Operand);
            } else {
                throw new Exception("instruction is not stloc! : " + instruction);
            }
        }
        public static Instruction BuildStLocFromLdLoc(this Instruction instruction) {
            if (instruction.OpCode == OpCodes.Ldloc_0) {
                return Instruction.Create(OpCodes.Stloc_0);
            } else if (instruction.OpCode == OpCodes.Ldloc_1) {
                return Instruction.Create(OpCodes.Stloc_1);
            } else if (instruction.OpCode == OpCodes.Ldloc_2) {
                return Instruction.Create(OpCodes.Stloc_2);
            } else if (instruction.OpCode == OpCodes.Ldloc_3) {
                return Instruction.Create(OpCodes.Stloc_3);
            } else if (instruction.OpCode == OpCodes.Ldloc_S) {
                return Instruction.Create(OpCodes.Stloc_S, (sbyte)instruction.Operand);
            } else if (instruction.OpCode == OpCodes.Ldloc) {
                return Instruction.Create(OpCodes.Stloc, (sbyte)instruction.Operand);
            } else {
                throw new Exception("instruction is not ldloc! : " + instruction);
            }
        }

        internal static string IL2STR(this IEnumerable<Instruction> instructions) {
            string ret = "";
            foreach (var code in instructions) {
                ret += code + "\n";
            }
            return ret;
        }

        public class InstructionNotFoundException : Exception {
            public InstructionNotFoundException() : base() { }
            public InstructionNotFoundException(string m) : base(m) { }

        }

        public static int SearchInstruction(List<Instruction> codes, Instruction instruction, int index, int dir = +1, int counter = 1) {
            try {
                return SearchGeneric(codes, idx => IsSameInstruction(codes[idx], instruction), index, dir, counter);
            }
            catch (InstructionNotFoundException) {
                throw new InstructionNotFoundException(" Did not found instruction: " + instruction);
            }
        }

        public static int SearchGeneric(List<Instruction> codes, Func<int,bool> predicate, int index, int dir = +1, int counter = 1, bool throwOnError=true) {
            
            int count = 0;
            for (; 0 <= index && index < codes.Count; index += dir) {
                if (predicate(index)) {
                    if (++count == counter)
                        break;
                }
            }
            if (count != counter) {
                if (throwOnError == true)
                    throw new InstructionNotFoundException(" Did not found instruction[s].");
                else {
                    return -1;
                }
            }
            return index;
        }

        public static bool IsBR32(OpCode opcode) {
            // TODO complete list.
            return opcode == OpCodes.Br || opcode == OpCodes.Brtrue || opcode == OpCodes.Brfalse || opcode == OpCodes.Beq;
        }
    }
}
