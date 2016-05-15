﻿using System;
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
using Static_Interface.API.UnityExtensions;
using Static_Interface.API.Utils;
using Steamworks;
using UnityEngine;
using MonoBehaviour = Static_Interface.API.UnityExtensions.MonoBehaviour;
using Object = UnityEngine.Object;
using ThreadPool = System.Threading.ThreadPool;

namespace Static_Interface.ExtensionSandbox
{
    //When I wrote this, only God and I understood what I was doing
    //Now, God only knows   
    public static class SafeCodeHandler
    {
        private static readonly Dictionary<Assembly, List<Type>> AllowedTypesFromAssembly = new Dictionary<Assembly, List<Type>>();
        private static readonly Disassembler Disassembler = new Disassembler();
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
            typeof(Directory),
            typeof(File),
            typeof(Path),
            typeof(FileSystemInfo),
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
            typeof(IComparable<>),
            typeof(Type)
        };

        private static readonly List<Type> DisallowedTypes = new List<Type>
        {
            typeof(AssetBundle),
            typeof(Physics2D),
            typeof(Network),
            typeof(Process),
            typeof(ProcessStartInfo),
            typeof(DllImportAttribute),
            typeof(Activator),
            typeof(Application),
            typeof(AsyncOperation),
            typeof(Thread),
            typeof(Resolution),
            typeof(UnityScheduler),
            typeof(Resources),
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
            typeof(ThreadPool),
            typeof(PersistentScript<>)
        };

        private static readonly Dictionary<Type, List<string>> DisallowedMethods = new Dictionary<Type, List<string>>
        {
            {typeof(Object), new List<string> { "Destroy", "DestroyImmediate", "DestroyObject", "DontDestroyOnLoad" }},
            {typeof(Behaviour), new List<string> {"set_enabled" }}, //dont allow disabling critical components like ExtensionManager
            {typeof(GameObject), new List<string> { "set_active" }}
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
            return IsAllowedType(type.FullName ?? type.Name);
        }

        public static void AddWhitelist(Assembly asm)
        {
            var list = new List<Type>();
            AllowedTypesFromAssembly[asm] = list;
            foreach (Type type in asm.GetTypes()
                .Where(type => !IsInNamespaceList(type.Namespace ?? type.Name, DisallowedNamespaces)
                            && !IsInNamespaceList(type.Namespace ?? type.Name, AllowedNamespaces)))
            {
                AllowedTypes.Add(type);
                list.Add(type);
            }
        }

        private static void RemoveWhitelist(Assembly asm)
        {
            var list = AllowedTypesFromAssembly[asm];
            foreach (Type t in list)
            {
                AllowedTypes.Remove(t);
            }
            AllowedTypesFromAssembly.Remove(asm);
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

        public static bool IsSafeAssembly(Assembly baseAssembly, out string illegalInstruction, out string failReason)
        {
            illegalInstruction = null;
            failReason = null;
            AddWhitelist(baseAssembly);
            foreach (Type type in baseAssembly.GetTypes())
            {
                if ((!type.IsSubclassOf(typeof(MonoBehaviour)) &&  type != typeof (MonoBehaviour) && !typeof(MonoBehaviour).IsAssignableFrom(type))&&
                    (type.IsSubclassOf(typeof (Behaviour)) || type == typeof (Behaviour) || typeof(Behaviour).IsAssignableFrom(type)))
                {
                    RemoveWhitelist(baseAssembly);
                    illegalInstruction = type.FullName;
                    failReason = "Extending Unitys Behaviour [please use UnityExtensions.MonoBehaviour]";
                    return false;
                }
                
                if (!CheckType(baseAssembly, type, ref illegalInstruction, ref failReason))
                {
                    RemoveWhitelist(baseAssembly);
                    return false;
                }
            }

            return true;
        }

        private static bool CheckType(Assembly asm, Type type, ref string illegalInstruction, ref string failReason)
        {
            if (type == null) return true;
            if (IsDelegate(type)) return true;
            if (!IsAllowedType(type))
            {
                illegalInstruction = type.FullName;
                failReason = "Type restricted: " + type.FullName;
                return false;
            }

            foreach (MethodInfo method in type.GetMethods())
            {
                if (!CheckMethod(asm, type, method, ref illegalInstruction, ref failReason))
                {
                    return false;
                }
            }

            foreach (PropertyInfo def in type.GetProperties())
            {
                if (def.GetGetMethod() != null && !CheckMethod(asm, type, def.GetGetMethod(), ref illegalInstruction, ref failReason))
                {
                    return false;
                }
                if (def.GetSetMethod() != null && !CheckMethod(asm, type, def.GetSetMethod(), ref illegalInstruction, ref failReason))
                {
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

        private static bool CheckMethod(Assembly asm, Type type, MethodInfo method, ref string illegalInstruction, ref string failReason, bool recur = true)
        {
            if (method.DeclaringType != type) return true; // this method is from super class
            if (!IsAllowedMethod(type, method.Name))
            {
                failReason = "Method or property restricted" + type.FullName + "." + method.Name;
                return false;
            }

            if (!CheckMethodAttributes(method.Attributes)) return false;
            if (!type.IsGenericTypeDefinition && type.IsGenericType && CheckGenericType(asm, type.GetGenericTypeDefinition(), method, ref illegalInstruction, ref failReason)) return false;

            foreach (Instruction ins in Disassembler.ReadInstructions(method))
            {
                if (recur)
                {
                    Type t = ins.Operand as Type;
                    MethodInfo m = ins.Operand as MethodInfo;
                    
                    if (m == method) continue;

                    if (m != null)
                    {
                        if (m.DeclaringType != null && type != m.DeclaringType && !IsAllowedType(m.DeclaringType))
                        {
                            illegalInstruction = type.FullName + "." + method.Name;
                            failReason = "Type restricted: " + (m.DeclaringType.FullName ?? m.DeclaringType.Name);
                            return false;
                        }
                        if (!CheckMethod(asm, type, m, ref illegalInstruction, ref failReason, false))
                        {
                            illegalInstruction = type.FullName + "." + method.Name;
                            failReason = "Method or property restricted: " + m.DeclaringType+ "." + m.Name;
                            return false;
                        }
                    }

                    if (t != null && t != type)
                    {
                        if (!IsAllowedType(t))
                        {
                            illegalInstruction = type.FullName + "." + method.Name;
                            failReason = "Type restricted: " + (t.FullName ?? t.Name);
                            return false;
                        }
                    }

                }
                if (ins.OpCode == OpCodes.Calli) //can call unmanaged code
                {
                    failReason = "Operand not allowed: " + OpCodes.Calli;
                    illegalInstruction = type.FullName + "." + method.Name;
                    return false;
                }
            }

            return true;
        }
        
        private static bool CheckGenericType(Assembly asm, Type type, MethodInfo method, ref string illegalInstruction, ref string failedAt)
        {
            if (!CheckMethod(asm, type, method, ref illegalInstruction, ref failedAt)) return false;
            if (method?.DeclaringType == null) return true;
            foreach (var gType in method.DeclaringType.GetGenericArguments())
            {
                if (!CheckType(asm, gType, ref illegalInstruction, ref failedAt))
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