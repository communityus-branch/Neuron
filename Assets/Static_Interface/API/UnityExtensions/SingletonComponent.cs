using UnityEngine;

namespace Static_Interface.API.UnityExtensions
{
    public abstract class SingletonComponent<T> : SingletonComponent where T : Component
    {
        public static T Instance => InternalInstance;

        protected static T InternalInstance;

        public override void Setup()
        {
            if (InternalInstance?.GetType() == GetType())
            {
                Destroy(this);
                return;
            }

            if (InternalInstance != this) InternalInstance = this as T;
        }

        public override void Clear()
        {
            if (InternalInstance == this) InternalInstance = null;
        }
    }

    public abstract class SingletonComponent : MonoBehaviour
    {
        public abstract void Setup();
        public abstract void Clear();

        protected override void Awake()
        {
            Setup();
        }

        protected override void OnDestroySafe()
        {
            Clear();
        }
    }
}