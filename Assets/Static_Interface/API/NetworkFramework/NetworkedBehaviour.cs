using System;
using Static_Interface.Internal.MultiplayerFramework;

namespace Static_Interface.API.NetworkFramework
{
    public abstract class NetworkedBehaviour : UnityExtensions.MonoBehaviour
    {
        public Channel Channel { get; private set; }

        protected override void Awake()
        {
            Channel = GetComponent<Channel>();
            
            if (Channel != null)
            {
                Channel.Build(this);
                return;
            }

            Channel = gameObject.AddComponent<Channel>();
            Channel.Setup();
        }

        public void CheckServer()
        {
            if (!Connection.IsServer())
            {
                throw new Exception("This can be only called from server-side!");
            }
        }
    }
}