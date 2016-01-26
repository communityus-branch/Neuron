using System.Collections.Generic;
using UnityEngine;

namespace Static_Interface.API.ExtensionsFramework
{
    public class ComponentManager
    {
        private static ComponentManager _instance;
        public static ComponentManager Instance => _instance ?? (_instance = new ComponentManager());

        private Dictionary<Extension, Dictionary<GameObject, List<Component>>> registeredComponents = new Dictionary<Extension, Dictionary<GameObject, List<Component>>>();
        public T AddComponent<T> (Extension ext, GameObject @object) where T : Component
        {
            //Todo: track addded components
            return @object.AddComponent<T>();
        }

        public void Destroy(Extension ext, Object obj)
        {
            //Todo: add checks
            Object.Destroy(obj);
        }

        public void DestroyImmediate(Extension ext, Object obj)
        {
            //Todo: add checks
            Object.DestroyImmediate(obj);
        }

        public void DestroyObject(Extension ext, Object obj)
        {
            //Todo: add checks
            Object.DestroyObject(obj);
        }
    }
}