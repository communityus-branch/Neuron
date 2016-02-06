using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Static_Interface.API.ExtensionFramework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Static_Interface.API.Utils
{
    /// <summary>
    /// Extensions may not use some methods for Components. However, they can use these safe-ones instead.
    /// </summary>
    public static class ComponentManager
    {
        private static readonly Dictionary<Extension, Dictionary<GameObject, List<Component>>> RegisteredComponents = new Dictionary<Extension, Dictionary<GameObject, List<Component>>>();
        private static readonly List<Type> CriticalComponents = new List<Type>
        {
            typeof(ExtensionManager)
        }; 
        public static T AddComponentExtension<T>(this GameObject @object, Extension ext) where T : Component
        {
            return AddComponent<T>(ext, @object);
        }

        public static void SetEnabled(this Component c, Extension ext, bool value)
        {
            CheckCriticialComponent(c);
        }

        private static void CheckCriticialObject(Object obj)
        {
            if (!(obj is GameObject)) return;
            foreach (Component c in ((GameObject)obj).GetComponents<Component>())
            {
                CheckCriticialComponent(c);
            }
        }

        private static void CheckCriticialComponent(Component c)
        {
            if (c == null) return;
            CheckCriticialComponent(c.GetType());
        }


        private static void CheckCriticialComponent(Type t)
        {
            bool critical = CriticalComponents.Any(c => c == t || t.IsSubclassOf(c));
            if (critical)
            {
                throw new SecurityException("Access to component " + t.FullName + " is restricted");
            }
        }

        /// <summary>
        /// See <see cref="GameObject.AddComponent(System.Type)"/>
        /// </summary>
        public static T AddComponent<T>(this Extension ext, GameObject @object) where T : Component
        {
            CheckCriticialComponent(typeof (T));
            var dictionary = !RegisteredComponents.ContainsKey(ext) ? new Dictionary<GameObject, List<Component>>() : RegisteredComponents[ext];

            var list = !dictionary.ContainsKey(@object) ? new List<Component>() : dictionary[@object];

            var comp = @object.AddComponent<T>();
            list.Add(comp);

            if (RegisteredComponents.ContainsKey(ext))
            {
                RegisteredComponents[ext] = dictionary;
            }
            else
            {
                RegisteredComponents.Add(ext, dictionary);
            }


            if (dictionary.ContainsKey(@object))
            {
                dictionary[@object] = list;
            }
            else
            {
                dictionary.Add(@object, list);
            }

            return comp;
        }

        public static void DestroyExtension(this Object obj, Extension ext)
        {
            Destroy(ext, obj);
        }

        /// <summary>
        /// See <see cref="Object.Destroy(Object)"/>
        /// </summary>
        public static void Destroy(this Extension ext, Object obj)
        {
            CheckCriticialObject(obj);
            Object.Destroy(obj);
        }

        /// <summary>
        /// See <see cref="Object.DestroyImmediate(Object)"/>
        /// </summary>
        public static void DestroyImmediate(this Extension ext, Object obj)
        {
            CheckCriticialObject(obj);
            Object.DestroyImmediate(obj);
        }

        /// <summary>
        /// See <see cref="Object.DestroyObject(Object)"/>
        /// </summary>
        public static void DestroyObject(this Extension ext, Object obj, float t = 0.00f)
        {
            CheckCriticialObject(obj);
            Object.DestroyObject(obj, t);
        }
    }
}