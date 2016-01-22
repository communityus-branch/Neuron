using UnityEngine;

namespace Assets.Static_Interface.API.ExtensionsFramework
{
    public static class ComponentManager
    {
        public static T AddComponent<T> (GameObject @object) where T : Component
        {
            //Todo: track addded components
            return @object.AddComponent<T>();
        }

        public static void Destroy(Object obj)
        {
            //Todo: add checks
            Object.Destroy(obj);
        }

        public static void DestroyImmediate(Object obj)
        {
            //Todo: add checks
            Object.DestroyImmediate(obj);
        }

        public static void DestroyObject(Object obj)
        {
            //Todo: add checks
            Object.DestroyObject(obj);
        }
    }
}