using UnityEngine;

namespace Static_Interface.Multiplayer.Protocol
{
    public abstract class NetworkedBehaviour : MonoBehaviour
    {
        public Channel Channel { get; protected set; }

        protected virtual void Awake()
        {
            Channel = GetComponent<Channel>();
        }
    }
}