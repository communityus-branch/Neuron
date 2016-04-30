using System;

namespace Static_Interface.API.AssetsFramework
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AttachToGameObjectAttribute : Attribute
    {
        public readonly string GameObject;
        public AttachToGameObjectAttribute(string gameObject)
        {
            GameObject = gameObject;
        }
    }
}