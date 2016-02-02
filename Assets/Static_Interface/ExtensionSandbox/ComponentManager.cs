using System.Collections.Generic;
using Static_Interface.API.Extension;
using UnityEngine;

namespace Static_Interface.ExtensionSandbox
{
    public static class ComponentManager
    {
        private static readonly Dictionary<Extension, Dictionary<GameObject, List<Component>>> RegisteredComponents = new Dictionary<Extension, Dictionary<GameObject, List<Component>>>();

        public static T AddComponentExtension<T>(this GameObject @object, Extension ext) where T : Component
        {
            return AddComponent<T>(ext, @object);
        }

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

        public static void Destroy(this Extension ext, Object obj)
        {
            //Todo: add checks
            Object.Destroy(obj);
        }

        public static void DestroyImmediateExtension(this Object obj, Extension ext)
        {
            DestroyImmediate(ext, obj);
        }

        public static void DestroyImmediate(this Extension ext, Object obj)
        {
            //Todo: add checks
            Object.DestroyImmediate(obj);
        }

        public static void DestroyObjectExtension(this Object obj, Extension ext)
        {
            DestroyObject(ext, obj);
        }

        public static void DestroyObject(this Extension ext, Object obj)
        {
            //Todo: add checks
            Object.DestroyObject(obj);
        }
    }
}