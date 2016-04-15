using System;
using Static_Interface.Internal.MultiplayerFramework;

namespace Static_Interface.API.NetworkFramework
{
    public abstract class NetworkedBehaviour : UnityExtensions.MonoBehaviour
    {
        public Channel Channel { get; private set; }
        public Connection Connection => Connection.CurrentConnection;
        protected virtual int PreferredChannelID  => 0;

        protected override void Awake()
        {
            Channel = GetComponent<Channel>();
            
            if (Channel == null)
            {
                Channel = gameObject.AddComponent<Channel>();
                Channel.ID = PreferredChannelID != 0 ? PreferredChannelID : Connection.CurrentConnection.ChannelCount;
                Channel.Setup();
                return;
            }

            Channel.Build(this);
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