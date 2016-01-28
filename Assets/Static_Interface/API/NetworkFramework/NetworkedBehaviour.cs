using UnityEngine;

namespace Static_Interface.API.NetworkFramework
{
    public abstract class NetworkedBehaviour : MonoBehaviour
    {
        public Channel Channel { get; protected set; }

        protected virtual void Awake()
        {
            Channel = GetComponent<Channel>();
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            // Channel = GetComponent<Channel>() ?? gameObject.AddComponent<Channel>();
            if (Channel == null)
            {
                Channel = gameObject.AddComponent<Channel>();
            }
        }

        protected virtual void Start()
        {
            
        }

        protected virtual void Update()
        {
            
        }

        protected virtual void FixedUpdate()
        {

        }
    }
}