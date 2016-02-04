using System.Collections.Generic;
using Static_Interface.API.ExtensionFramework;
using UnityEngine;

namespace Static_Interface.API.Utils
{
    /// <summary>
    /// Extensions may not use some methods for Components. However, they can use these safe-ones instead.
    /// </summary>
    public static class ComponentManager
    {
        private static readonly Dictionary<Extension, Dictionary<GameObject, List<Component>>> RegisteredComponents = new Dictionary<Extension, Dictionary<GameObject, List<Component>>>();

        public static T AddComponentExtension<T>(this GameObject @object, Extension ext) where T : Component
        {
            return AddComponent<T>(ext, @object);
        }


        /// <summary>
        /// See <see cref="GameObject.AddComponent(System.Type)"/>
        /// </summary>
        public static T AddComponent<T>(this Extension ext, GameObject @object) where T : Component
        {
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
            //Todo: add checks
            Object.Destroy(obj);
        }

        /// <summary>
        /// See <see cref="Object.DestroyImmediate(Object)"/>
        /// </summary>
        public static void DestroyImmediate(this Extension ext, Object obj)
        {
            //Todo: add checks
            Object.DestroyImmediate(obj);
        }

        /// <summary>
        /// See <see cref="Object.DestroyObject(Object)"/>
        /// </summary>
        public static void DestroyObject(this Extension ext, Object obj, float t = 0.00f)
        {
            //Todo: add checks
            Object.DestroyObject(obj, t);
        }
    }
}