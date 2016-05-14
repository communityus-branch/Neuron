using UnityEngine;

namespace Static_Interface.API.NetworkFramework
{
    public class NetworkedSingletonBehaviour<T> : NetworkedBehaviour  where T: Component
    {
        public static T Instance => InternalInstance;

        protected static T InternalInstance;

        protected override void Awake()
        {
            base.Awake();
            if (InternalInstance != this) InternalInstance = this as T;
        }

        protected internal override void OnDestroy()
        {
            base.OnDestroy();
            if (InternalInstance == this) InternalInstance = null;
        }
    }
}