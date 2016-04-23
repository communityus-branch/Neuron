using System;
using Static_Interface.Internal.MultiplayerFramework;

namespace Static_Interface.API.NetworkFramework
{
    public abstract class NetworkedBehaviour : UnityExtensions.MonoBehaviour
    {
        public virtual Channel Channel => GetComponent<Channel>();
        public Connection Connection => Connection.CurrentConnection;
        protected virtual int PreferredChannelID  => 0;

        protected override void Awake()
        {
            SetupChannel();
        }

        protected virtual void SetupChannel()
        {
            var ch = Channel;
            if (ch == null)
            {
                ch = gameObject.AddComponent<Channel>();
                ch.ID = PreferredChannelID != 0 ? PreferredChannelID : Connection.CurrentConnection.ChannelCount;
                ch.Setup();
                return;
            }

            ch.Build(this);
        }

        public void CheckServer()
        {
            if (!Connection.IsServer())
            {
                throw new Exception("This can be only called from server-side!");
            }
        }

        public bool IsServer()
        {
            return Connection.IsServer();
        }

        public bool IsClient()
        {
            return Connection.IsClient();
        }

        public bool IsDedicatedServer()
        {
            return Connection.IsDedicated;
        }
    }
}