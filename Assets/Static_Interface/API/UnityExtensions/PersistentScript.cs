using Static_Interface.Internal.Objects;
using UnityEngine;

namespace Static_Interface.API.UnityExtensions
{
    public class PersistentScript<T> : SingletonComponent<T> where T: Component
    {
        public new static T Instance
        {
            get
            {
                if (InternalInstance != null) return InternalInstance;
                Init();
                return InternalInstance;
            }
        }

        public static void Init()
        {
            if (InternalInstance != null) return;
            //InternalObjectUtils.CheckObjects();
            GameObject persistentScripts = GameObject.Find("PersistentScripts");
            InternalInstance = persistentScripts.AddComponent<T>();
            DontDestroyOnLoad(InternalInstance);
        }

        public static bool HasInitied()
        {
            return InternalInstance != null;
        }
    }
}