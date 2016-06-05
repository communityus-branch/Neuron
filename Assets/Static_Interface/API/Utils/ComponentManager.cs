using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Static_Interface.API.PluginFramework;
using Static_Interface.API.UnityExtensions;
using Static_Interface.Internal.MultiplayerFramework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Static_Interface.API.Utils
{
    /// <summary>
    /// Plugins may not use some methods for Components. However, they can use these safe-ones instead.
    /// </summary>
    public static class ComponentManager
    {
        private static readonly Dictionary<Plugin, Dictionary<GameObject, List<Component>>> RegisteredComponents = new Dictionary<Plugin, Dictionary<GameObject, List<Component>>>();
        private static readonly List<Type> CriticalComponents = new List<Type>
        {
            typeof(PersistentScript<>),
            typeof(PluginManager),
            typeof(Connection)
            //Todo
        };

        internal static void CheckCriticialObject(Object obj)
        {
            if (!(obj is GameObject)) return;
            if (obj.name.StartsWith("PauseMenu")) throw new SecurityException("Access to object is restricted");
            foreach (Component c in ((GameObject)obj).GetComponents<Component>())
            {
                CheckCriticialComponent(c);
            }
        }

        internal static void CheckCriticialComponent(Component c)
        {
            if (c == null) return;
            CheckCriticialComponent(c.GetType());
        }


        internal static void CheckCriticialComponent(Type t)
        {
            bool critical = CriticalComponents.Any(c => c == t || t.IsSubclassOf(c));

            if (critical)
            {
                throw new SecurityException("Access to component " + t.FullName + " is restricted");
            }
        }

        public static void DestroyPlugin(this Object obj, Plugin ext)
        {
            Destroy(ext, obj);
        }

        /// <summary>
        /// See <see cref="Object.Destroy(Object)"/>
        /// </summary>
        public static void Destroy(this Plugin ext, Object obj)
        {
            CheckCriticialObject(obj);
            Object.Destroy(obj);
        }

        /// <summary>
        /// See <see cref="Object.DestroyImmediate(Object)"/>
        /// </summary>
        public static void DestroyImmediate(this Plugin ext, Object obj)
        {
            CheckCriticialObject(obj);
            Object.DestroyImmediate(obj);
        }

        /// <summary>
        /// See <see cref="Object.DestroyObject(Object)"/>
        /// </summary>
        public static void DestroyObject(this Plugin ext, Object obj, float t = 0.00f)
        {
            CheckCriticialObject(obj);
            Object.DestroyObject(obj, t);
        }
    }
}