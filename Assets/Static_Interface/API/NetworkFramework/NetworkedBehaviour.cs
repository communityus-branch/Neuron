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
            Channel = SetupChannel();
        }

        protected virtual Channel SetupChannel()
        {
            var ch = GetComponent<Channel>();

            if (ch == null)
            {
                ch = gameObject.AddComponent<Channel>();
                ch.ID = PreferredChannelID != 0 ? PreferredChannelID : Connection.CurrentConnection.ChannelCount;
                ch.Setup();
                return ch;
            }

            ch.Build(this);
            return ch;
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