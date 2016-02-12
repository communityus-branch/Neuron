using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Steamworks;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Static_Interface.ExtensionSandbox
{
    public static class SafeCodeHandler
    {
        private static readonly Disassembler _disassembler = new Disassembler();
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
            typeof(UnityEngine.Debug),
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
            typeof(UnityScheduler),
            typeof(Resources),
            typeof(SamsungTV),
            typeof(ScriptableObject),
            typeof(SystemInfo),
            typeof(WebCamDevice),
            typeof(AddComponentMenu),
            typeof(ContextMenu),
            typeof(ExecuteInEditMode),
            typeof(RPC),
            typeof(Timer),
            typeof(System.Timers.Timer),
            typeof(AsyncOperation),
            typeof(System.ComponentModel.AsyncOperation),
            typeof(ThreadPool)
        };

        private static readonly Dictionary<Type, List<string>> DisallowedMethods = new Dictionary<Type, List<string>>
        {
            {typeof(Object), new List<string> { "Destroy", "DestroyImmediate", "DestroyObject", "DontDestroyOnLoad" }},
            {typeof(GameObject), new List<string> { "AddComponent" } },
            {typeof(Component), new List<string> {"set_enabled"} } //dont allow disabling critical components like ExtensionManager
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

            foreach (Type type in baseAssembly.GetTypes())
            {
                if (!CheckType(baseAssembly, type, out illegalInstruction, out failedAt)) return false;
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

            foreach (MethodInfo method in type.GetMethods())
            {
                if (!CheckMethod(asm, type, method, out illegalInstruction, out failedAt))
                {
                    failedAt = type.Name + "." + method.Name;
                    return false;
                }
            }

            foreach (PropertyInfo def in type.GetProperties())
            {
                if (!CheckMethod(asm, type, def.GetGetMethod(), out illegalInstruction, out failedAt))
                {
                    failedAt = type.Name + def.GetGetMethod().Name;
                    return false;
                }
                if (!CheckMethod(asm, type, def.GetSetMethod(), out illegalInstruction, out failedAt))
                {
                    failedAt = type.Name + def.GetSetMethod().Name;
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

        private static bool CheckMethod(Assembly asm, Type type, MethodInfo method, out string illegalInstruction, out string failedAt, bool recur = true)
        {
            illegalInstruction = null;
            failedAt = null;

            if (!IsAllowedMethod(type, method.Name))
            {
                failedAt = type.FullName + "." + method.Name;
                illegalInstruction = type.FullName;
                return false;
            }

            if (!CheckMethodAttributes(method.Attributes)) return false;
            if (!type.IsGenericTypeDefinition && type.IsGenericType && CheckGenericType(asm, type.GetGenericTypeDefinition(), method, out illegalInstruction, out failedAt)) return false;

            foreach (Instruction ins in _disassembler.ReadInstructions(method))
            {
                if (recur)
                {
                    Type t = ins.Operand as Type;
                    MethodInfo m = ins.Operand as MethodInfo;

                    if (m == method) continue;

                    if (m != null)
                    {
                        t = t.DeclaringType;
                        if (!CheckMethod(asm, type, m, out illegalInstruction, out failedAt, false))
                        {
                            return false;
                        }
                    }
                    

                    if (t != null)
                    {
                        if (!IsAllowedType(t))
                        {
                            failedAt = t.FullName + "." + method.Name;
                            illegalInstruction = t.FullName;
                            return false;
                        }
                    }

                }
                if (ins.OpCode == OpCodes.Calli) //can call unmanaged code
                {
                    var t = ((MemberInfo)ins.Operand).DeclaringType;
                    illegalInstruction = OpCodes.Calli.ToString();
                    failedAt = t?.FullName + "." + method.Name;
                    return false;
                }
            }

            return true;
        }

        private static bool CheckGenericType(Assembly asm, Type type, MethodInfo method, out string illegalInstruction, out string failedAt)
        {
            if (!CheckMethod(asm, type, method, out illegalInstruction, out failedAt)) return false;
            if (method == null) return true;
            foreach (var gType in method.DeclaringType.GetGenericArguments())
            {
                if (!CheckType(asm, gType, out illegalInstruction, out failedAt))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CheckMethodAttributes(MethodAttributes attributes)
        {
            var val = (attributes & (MethodAttributes.PinvokeImpl | MethodAttributes.UnmanagedExport));
            return val == 0;
        }
    }
}