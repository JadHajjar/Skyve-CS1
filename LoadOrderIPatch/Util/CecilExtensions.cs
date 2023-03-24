using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LoadOrderIPatch {
    internal static class TypeDefinitionExtensions {
        internal static MethodDefinition GetMethod(
            this ModuleDefinition module,
            string fullTypeName, string methodName,
            bool throwOnError = true)
        {
            
            
            TypeDefinition type = module.Types.FirstOrDefault(t => t.FullName == fullTypeName);
            if (type is null) {
                if (throwOnError)
                    throw new Exception(fullTypeName + " not found");
                return null;
            }

            var ret = type.Methods.FirstOrDefault(method => method.Name.Equals(methodName));
            if (ret is null && throwOnError)
                throw new Exception(methodName + " not found");
            return ret;
        }

        internal static MethodDefinition GetMethod(
            this ModuleDefinition module,
            string fullName,
            bool throwOnError = true)
        {
            int i = fullName.LastIndexOf(".");
            string methodName = fullName.Substring(i + 1);
            string typeName = fullName.Substring(0,i);
            return module.GetMethod(typeName, methodName, throwOnError);
        }

        internal static MethodDefinition GetMethod(
            this TypeDefinition type,
            string methodName,
            bool throwOnError = true)
        {
            if (type == null) throw new ArgumentNullException("type");
            try {
                return type.Methods.Single(_m => _m.Name == methodName);
            } catch {
                if (throwOnError)
                    throw new Exception($"could not find method:`{methodName}` in type:`{type.FullName}`");
                return null;
            }
        }
        /// <summary>
        /// Test if type is subclass. Only first base type 
        /// </summary>
        /// <param name="childType"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static bool IsSubclass(this TypeDefinition childType, IEnumerable<TypeDefinition> parent)
        {
            return childType.BaseType != null && parent.Any(p => childType.BaseType.FullName.Equals(p.FullName));
        }

        public static bool ExtendsAnyOfBaseClasses(this TypeDefinition childType, IEnumerable<TypeDefinition> parent)
        {
            if (childType.BaseType == null) {
                return false;
            }

            IEnumerable<TypeDefinition> typeDefinitions = parent as TypeDefinition[] ?? parent.ToArray();
            return typeDefinitions.Any(p => childType.BaseType.FullName.Equals(p.FullName)) ||
                childType.BaseType.Resolve().ExtendsAnyOfBaseClasses(typeDefinitions);
        }

        public static bool ExtendsAnyOfBaseClasses(this TypeDefinition childType, IEnumerable<string> parent)
        {
            if (childType.BaseType == null) {
                return false;
            }

            IEnumerable<string> typeDefinitions = parent as string[] ?? parent.ToArray();
            return typeDefinitions.Any(p => childType.BaseType.Name.Equals(p)) ||
                childType.BaseType.Resolve().ExtendsAnyOfBaseClasses(typeDefinitions);
        }

        /// <summary>
        /// Returns base class name.
        /// </summary>
        /// <param name="childType"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static string ExtendedParentTypeName(this TypeDefinition childType, IEnumerable<TypeDefinition> parent)
        {
            if (childType.BaseType == null) {
                return childType.Name;
            }


            IEnumerable<TypeDefinition> typeDefinitions = parent as TypeDefinition[] ?? parent.ToArray();
            if (!typeDefinitions.Any(p => childType.BaseType.FullName.Equals(p.FullName))) {
                return childType.BaseType.Resolve().ExtendedParentTypeName(typeDefinitions);
            }

            return childType.BaseType.Name;
        }

        #region singular versions

        public static bool IsSubclass(this TypeDefinition childType, TypeDefinition parent)
        {
            return childType.BaseType != null
                && childType.BaseType.FullName.Equals(parent.FullName);
        }

        public static bool Extends(this TypeDefinition childType, string parent)
        {
            return childType.BaseType.Name.Equals(parent)
                || childType.BaseType.Resolve().Extends(parent);
        }

        public static bool Extends(this TypeDefinition childType, TypeDefinition parent)
        {
            return childType.BaseType.FullName.Equals(parent.FullName)
                || childType.BaseType.Resolve().Extends(parent);
        }

        #endregion

        public static bool ImplementsInterface(this TypeDefinition type, string fullInterfaceName) {
            return type.GetAllInterfaces().Any(i => i.FullName == fullInterfaceName);
        }

        public static IEnumerable<TypeReference> GetAllInterfaces(this TypeDefinition type) {
            while (type != null) {
                foreach (var i in type.Interfaces)
                    yield return i.InterfaceType;
                try { type = type.BaseType?.Resolve(); } catch { type = null; }
            }
        }
    }

    public static class InstructionExtensions {
        public static bool Calls(this Instruction code, string method)
        {
            if (method is null) throw new ArgumentNullException(nameof(method));
            if (code.OpCode != OpCodes.Call && code.OpCode != OpCodes.Callvirt) return false;
            string name = (code.Operand as MethodReference)?.Name
                ?? (code.Operand as MethodInfo)?.Name;
            return name == method;
        }

        /// <summary>
        /// used to patch opcodes with operands that are IMemberDefinition or MemberInfo
        /// </summary>
        /// <param name="operand">operand name</param>
        /// <returns></returns>
        public static bool Is(this Instruction code, OpCode opcode, string operand) {
            return code.OpCode == opcode && code.GetOperandName() == operand;
        }

        /// <summary>
        /// get operand name if it is IMemberDefinition or MemberInfo
        /// </summary>
        public static string GetOperandName(this Instruction code) =>
            (code.Operand as IMemberDefinition)?.Name ?? (code.Operand as MemberInfo)?.Name;

    }
    public static class ILProcessorExtensions {
        public static void InsertAfter(this ILProcessor ilprocessor, Instruction target, params Instruction[] codes) {
            foreach (var code in codes.Reverse())
                ilprocessor.InsertAfter(target, code);
        }

        public static void InsertBefore(this ILProcessor ilprocessor, Instruction target, params Instruction[] codes) {
            foreach(var code in codes)
                ilprocessor.InsertBefore(target, code);
        }

        public static void Prefix(this ILProcessor ilprocessor, params Instruction[] codes) {
            var target = ilprocessor.Body.Instructions.First();
            for (int i = 0; i < codes.Length; i++)
                ilprocessor.InsertBefore(target, codes[i]);
        }
    }
}