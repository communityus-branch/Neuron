using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Static_Interface.Internal;
using Steamworks;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Static_Interface.ExtensionSandbox
{
    public static class SafeCodeHandler
    {
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
            "UnityEditor.*",
            "UnityEngine.Windows.*",
            "UnityEngine.Tizen.*",
            "UnityEngine.iOS.*",
            "UnityEngine.Purchasing.*",
            "UnityEngine.Network.*",
            "UnityEngine.SceneManagment.*",
            "Static_Interface.Internal.*",
            "Static_Interface.ExtensionSandbox.*",
        };

        private static readonly List<Type> AllowedTypes = new List<Type>
        {
            typeof(object),
            typeof(void),
            typeof(CSteamID),
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
            typeof(Activator),
            typeof(Application),
            typeof(AsyncOperation),
            typeof(Thread),
            typeof(Resolution),
            typeof(Resources),
            typeof(SamsungTV),
            typeof(ScriptableObject),
            typeof(SystemInfo),
            typeof(WebCamDevice),
            typeof(AddComponentMenu),
            typeof(ContextMenu),
            typeof(ExecuteInEditMode),
            typeof(RPC)
        };

        private static readonly Dictionary<Type, List<string>> DisallowedMethods = new Dictionary<Type, List<string>>
        {
            {typeof(Object), new List<string> { "Destroy", "DestroyImmediate", "DestroyObject", "DontDestroyOnLoad" }},
            {typeof(GameObject), new List<string> { "AddComponent" } }
        };

        private static readonly Dictionary<Type, List<string>> AllowedMethods = new Dictionary<Type, List<string>>
        {

        };

        public static bool IsAllowedMethod(Type type, string method)
        {
            if (DisallowedMethods.ContainsKey(type) && DisallowedMethods[type].Contains(method))
            {
                return false;
            }
            if (AllowedMethods.ContainsKey(type))
            {
                return AllowedMethods[type].Contains(method);
            }
            return IsAllowedType(type);
        }


        public static bool IsAllowedType(Type type)
        {
            return IsAllowedType(type.FullName);
        }


        public static bool IsAllowedType(TypeDefinition type)
        {
            return IsAllowedType(type.FullName);
        }

        public static void AddWhitelist(Assembly asm)
        {
            foreach (Type type in asm.GetTypes()
                .Where(type => !IsInNamespaceList(type.Namespace ?? type.Name, DisallowedNamespaces)
                            && !IsInNamespaceList(type.Namespace ?? type.Name, AllowedNamespaces)))
            {
                AllowedTypes.Add(type);
            }
        }

        public static bool IsAllowedType(string fullName)
        {
            bool allowedTypeContains = AllowedTypes.Any(t => t.FullName.Equals(fullName));
            
            bool disallowedTypeContains = DisallowedTypes.Any(t => t.FullName.Equals(fullName));
            if (allowedTypeContains &&
                    !disallowedTypeContains)
            {
                return true;
            }

            if (IsInNamespaceList(fullName, DisallowedNamespaces))
            {
                return false;
            }

            if (IsInNamespaceList(fullName, AllowedNamespaces))
            {
                return !disallowedTypeContains;
            }

            return false;
        }

        private static bool IsInNamespaceList(string fullName, List<string> namespaces)
        {
            string typeNamespace = fullName;

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

        public static bool IsSafeAssembly(Assembly baseAssembly, out string illegalInstruction, out string failedAt)
        {
            illegalInstruction = null;
            failedAt = null;
            DefaultAssemblyResolver assemblyResolver = new DefaultAssemblyResolver();
            var readerParameters = new ReaderParameters { AssemblyResolver = assemblyResolver };
            AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(baseAssembly.Location, readerParameters);
            foreach (ModuleDefinition module in asm.Modules)
            {
                foreach (TypeDefinition t in module.Types)
                {
                    Type type = baseAssembly.GetType(t.FullName);
                    if (!CheckType(baseAssembly, type, out illegalInstruction, out failedAt)) return false;
                }    
            }

            return true;
        }

        private static bool CheckType(Assembly asm, Type type, out string illegalInstruction, out string failedAt)
        {
            illegalInstruction = null;
            failedAt = null;
            if (type == null) return true;
            if (IsDelegate(type)) return true;
            if (!IsAllowedType(type))
            {
                illegalInstruction = type.FullName;
                failedAt = illegalInstruction;
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
                illegalInstruction = type.FullName;
                return false;
            }

            foreach (MethodDefinition def in t.Methods)
            {
                if (!CheckMethod(asm, type, def, out illegalInstruction, out failedAt))
                {
                    failedAt = def.FullName;
                    return false;
                }
            }

            foreach (PropertyDefinition def in t.Properties)
            {
                if (!CheckMethod(asm, type, def.GetMethod, out illegalInstruction, out failedAt))
                {
                    failedAt = def.GetMethod.FullName;
                    return false;
                }
                if (!CheckMethod(asm, type, def.SetMethod, out illegalInstruction, out failedAt))
                {
                    failedAt = def.SetMethod.FullName;
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

        private static bool CheckMethod(Assembly asm, Type type, MethodDefinition method, out string illegalInstruction, out string failedAt, bool recur = true)
        {
            illegalInstruction = null;
            failedAt = null;

            if (!IsAllowedMethod(type, method.Name))
            {
                failedAt = method.FullName;
                illegalInstruction = type.FullName;
                return false;
            }

            if (CheckInvalidMethodAttributes(method.Attributes)) return false;
            if (!type.IsGenericTypeDefinition && type.IsGenericType && CheckGenericType(asm, type.GetGenericTypeDefinition(), method, out illegalInstruction, out failedAt)) return false;
            if (method.Body == null) return true;
            foreach (Instruction ins in method.Body.Instructions)
            {
                if (recur)
                {
                    TypeReference tRef = ins.Operand as TypeReference;
                    TypeDefinition tDef = tRef?.Resolve() ?? ins.Operand as TypeDefinition;
                    MethodReference mRef = ins.Operand as MethodReference;
                    MethodDefinition mDef = mRef?.Resolve() ?? ins.Operand as MethodDefinition;
                    if (mDef == method) continue;

                    if (mDef != null)
                    {
                        tDef = mDef.DeclaringType;
                        if (!CheckMethod(asm, type, mDef, out illegalInstruction, out failedAt, false))
                        {
                            return false;
                        }
                    }
                    

                    if (tDef != null)
                    {
                        if (!IsAllowedType(tDef))
                        {
                            failedAt = method.FullName;
                            illegalInstruction = tDef.FullName;
                            return false;
                        }
                    }

                }
                if (ins.OpCode == OpCodes.Calli) //can call unmanaged code
                {
                    var t = ((MemberReference)ins.Operand).DeclaringType;
                    illegalInstruction = OpCodes.Calli.ToString();
                    failedAt = method.FullName;
                    return false;
                }
            }

            return true;
        }

        private static bool CheckGenericType(Assembly asm, Type type, MethodDefinition definition, out string illegalInstruction, out string failedAt)
        {
            if (!CheckMethod(asm, type, definition, out illegalInstruction, out failedAt)) return false;
            if (definition == null || !definition.DeclaringType.HasGenericParameters) return true;
            foreach (GenericParameter t in definition.DeclaringType.GenericParameters)
            {
                TypeDefinition def = t.Resolve();
                if (def == null) continue;
                Type gType = asm.GetType(def.FullName);
                if (!CheckType(asm, gType, out illegalInstruction, out failedAt))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CheckInvalidMethodAttributes(Mono.Cecil.MethodAttributes attributes)
        {
            var s = (attributes & (Mono.Cecil.MethodAttributes.PInvokeImpl | Mono.Cecil.MethodAttributes.UnmanagedExport));
            return s != 0;
        }
    }
}