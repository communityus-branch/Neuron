using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Static_Interface.Internal;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Static_Interface.ExtensionSandbox
{
    public static class ApiUtils
    {
        //Todo: auto add loaded plugins namespaces (with caution for names like System.Runtime etc...)
        private static readonly List<string> AllowedNamespaces = new List<string>
        {
            "Static_Interface.*",
            "UnityEngine.*",
            "System.Text.*",
            "System.Xml.Serialization.*",
            "System.Linq.*",
            "System.Globalization.*",
            "System.Collections.*"
        };

        private static readonly List<string> DisallowedNamespaces = new List<string>
        {
            "Static_Interface.Internal.*",
            "Static_Interface.ExtensionSandbox.*",
        };

        private static readonly List<Type> AllowedTypes = new List<Type>
        {
            typeof(string),
            typeof(Math),
            typeof(Enum),
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(uint),
            typeof(ulong),
            typeof(double),
            typeof(float),
            typeof(bool),
            typeof(char),
            typeof(byte),
            typeof(sbyte),
            typeof(decimal),
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(Array),

            typeof(MemberInfo),
            typeof(RuntimeHelpers),

            typeof(TextWriter),
            typeof(TextReader),
            typeof(BinaryWriter),
            typeof(BinaryReader),

            typeof(NullReferenceException),
            typeof(ArgumentException),
            typeof(ArgumentNullException),
            typeof(FormatException),
            typeof(Exception),
            typeof(DivideByZeroException),
            typeof(InvalidCastException),
            typeof(FileNotFoundException),
            typeof(System.Random),
            typeof(Convert),
            typeof(Path),
            typeof(Convert),
            typeof(Nullable<>),
            typeof(StringComparer),
            typeof(StringComparison),
            typeof(StringBuilder),
            typeof(IComparable<>)
        };

        private static readonly List<Type> DisallowedTypes = new List<Type>
        {
            typeof(Time),
            typeof(Physics),
            typeof(Physics2D),
            typeof(Network),
            typeof(Process),
            typeof(ProcessStartInfo),
            typeof(Type),
            typeof(DllImportAttribute),
            typeof(Activator)
        };

        private static readonly Dictionary<Type, string> DisallowedMethods = new Dictionary<Type, string>
        {
            {typeof(Object), "Destroy" },
            {typeof(Object), "DestroyImmediate" },
            {typeof(Object), "DestroyObject" },
            {typeof(Object), "DontDestroyOnLoad" },
            {typeof(GameObject), "AddComponent" }
        };

        private static readonly Dictionary<Type, string> AllowedMethods = new Dictionary<Type, string>
        {

        };

        public static bool IsAllowedMethod(Type type, string method)
        {
            if (AllowedMethods.ContainsKey(type))
            {
                return AllowedMethods[type].Contains(method);
            }
            if (!IsAllowedType(type)) return false;
            if (DisallowedMethods.ContainsKey(type))
            {
                return !DisallowedMethods[type].Contains(method);
            }
            return true;
        }

        public static bool IsAllowedType(Type type)
        {
            if (AllowedTypes.Contains(type) && 
                    !DisallowedTypes.Contains(type))
            {
                return true;
            }

            if (IsInNamespaceList(type, DisallowedNamespaces))
            {
                return false;
            }

            if (IsInNamespaceList(type, AllowedNamespaces))
            {
                return !DisallowedTypes.Contains(type);
            }

            return false;
        }

        private static bool IsInNamespaceList(Type type, List<string> namespaces)
        {
            string typeNamespace = type.Namespace ?? type.Name;

            bool isInList = false;
            foreach (string @namespace in namespaces)
            {
                var tmp = @namespace;
                if (@namespace.EndsWith(".*"))
                {
                    tmp = @namespace.Substring(0, @namespace.Length - 2);
                    isInList = typeNamespace.StartsWith(tmp);
                    if (isInList) break;
                }
                isInList = typeNamespace.Equals(tmp);
                if (isInList) break;
            }
            return isInList;
        }

        public static void AddWhitelist(Assembly asm)
        {
            foreach (Type type in asm.GetTypes()
                .Where(type => !IsInNamespaceList(type, DisallowedNamespaces) 
                            && !IsInNamespaceList(type, AllowedNamespaces)))
            {
                AllowedTypes.Add(type);
            }
        }

        public static bool IsSafe(Assembly baseAssembly, out Type illegalType, out MethodDefinition failedMethod)
        {
            illegalType = null;
            failedMethod = null;
            DefaultAssemblyResolver assemblyResolver = new DefaultAssemblyResolver();
            var readerParameters = new ReaderParameters { AssemblyResolver = assemblyResolver };
            AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(baseAssembly.Location, readerParameters);
            foreach (ModuleDefinition module in asm.Modules)
            {
                foreach (TypeDefinition t in module.Types)
                {
                    Type type = baseAssembly.GetType(t.FullName);
                    if (!CheckType(baseAssembly, type, out illegalType, out failedMethod)) return false;
                }    
            }

            return true;
        }

        private static bool CheckType(Assembly asm, Type type, out Type illegalType, out MethodDefinition failedMethod)
        {
            illegalType = null;
            failedMethod = null;
            if (type == null) return true;
            if (IsDelegate(type)) return true;
            if (!IsAllowedType(type))
            {
                illegalType = type;
                return false;
            }

            DefaultAssemblyResolver assemblyResolver = new DefaultAssemblyResolver();
            var readerParameters = new ReaderParameters { AssemblyResolver = assemblyResolver };

            AssemblyDefinition asmDef = AssemblyDefinition.ReadAssembly(asm.Location, readerParameters);

            TypeDefinition t = null;
            foreach (ModuleDefinition m in asmDef.Modules)
            {
                foreach (TypeDefinition typeDef in m.Types.Where(typeDef => typeDef.FullName.Equals(type.FullName)))
                {
                    t = typeDef;
                    break;
                }
            }

            if (t == null)
            {
                LogUtils.Error("Assert error: TypeDefinition of type " + type.FullName + " not found");
                illegalType = type;
                return false;
            }

            foreach (MethodDefinition def in t.Methods)
            {
                if (!CheckMethod(asm, type, def, out illegalType, out failedMethod))
                {
                    failedMethod = def;
                    return false;
                }
            }

            foreach (PropertyDefinition def in t.Properties)
            {
                if (!CheckMethod(asm, type, def.GetMethod, out illegalType, out failedMethod))
                {
                    failedMethod = def.GetMethod;
                    return false;
                }
                if (!CheckMethod(asm, type, def.SetMethod, out illegalType, out failedMethod))
                {
                    failedMethod = def.SetMethod;
                    return false;
                }
            }

            return true;
        }


        //http://stackoverflow.com/a/5819935 
        private static bool IsDelegate(Type checkType)
        {
            var delegateType = typeof(Delegate);
            return delegateType.IsAssignableFrom(checkType.BaseType) 
                || checkType == delegateType 
                || checkType == delegateType.BaseType;
        }

        private static bool CheckMethod(Assembly asm, Type type, MethodDefinition method, out Type failedType, out MethodDefinition failedMethod)
        {
            failedType = type;
            failedMethod = method;

            if (!CheckMethodAttributes(method.Attributes)) return false;
            if (!type.IsGenericTypeDefinition && type.IsGenericType && CheckGenericType(asm, type.GetGenericTypeDefinition(), method, out failedType, out failedMethod)) return false;

            if (AllowedMethods.ContainsKey(type))
            {
                if (AllowedMethods[type].Contains(method.Name)) return true;
                return false;
            }

            if (DisallowedMethods.ContainsKey(type))
            {
                if (!DisallowedMethods[type].Contains(method.Name)) return true;
                return false;
            }

            foreach (Instruction ins in method.Body.Instructions)
            {
                if (ins.OpCode == OpCodes.Calli) //can call unmanaged code
                {
                    failedType = ((MemberInfo)ins.Operand).DeclaringType;
                    return false;
                }
            }

            return true;
        }

        private static bool CheckGenericType(Assembly asm, Type type, MethodDefinition definition, out Type failedType, out MethodDefinition failedMethod)
        {
            if (!CheckMethod(asm, type, definition, out failedType, out failedMethod)) return false;
            if (definition == null || !definition.DeclaringType.HasGenericParameters) return true;
            foreach (GenericParameter t in definition.DeclaringType.GenericParameters)
            {
                TypeDefinition def = t.Resolve();
                if (def == null) continue;
                Type gType = asm.GetType(def.FullName);
                if (!CheckType(asm, gType, out failedType, out failedMethod))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CheckMethodAttributes(Mono.Cecil.MethodAttributes attributes)
        {
            return (attributes & (Mono.Cecil.MethodAttributes.PInvokeImpl | Mono.Cecil.MethodAttributes.UnmanagedExport)) != 0;
        }
    }
}