using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

using Object = UnityEngine.Object;
namespace Assets.Static_Interface.API.ExtensionsFramework
{
    public static class ApiUtils
    {
        //Todo: auto add loaded plugins namespaces (with caution for names like System.Runtime etc...)
        private static readonly List<string> AllowedNamespaces = new List<string>
        {
            "Static_Interface.API.*",
            "Static_Interface.The_Collapse.*",
            "UnityEngine.*",
            "System",
            "System.Linq.*",
            "System.Collections.*"
        };

        private static readonly List<string> DisallowedNamespaces = new List<string>
        {
            "Static_Interface.Internal.*",
            "System.Diagnostics.*",
            "System.Runtime.*",
            "System.Reflection.*",
            "System.Runtime.*"
        };

        private static readonly List<Type> AllowedTypes = new List<Type>
        {
            
        };

        private static readonly List<Type> DisallowedTypes = new List<Type>
        {
            typeof(ApiUtils),
            typeof(Sandbox),
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
            if (AllowedTypes.Contains(type) && !DisallowedTypes.Contains(type))
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
    }
}