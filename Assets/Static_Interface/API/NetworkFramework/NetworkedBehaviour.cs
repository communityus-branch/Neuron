using System;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework;

namespace Static_Interface.API.NetworkFramework
{
    public abstract class NetworkedBehaviour : UnityExtensions.MonoBehaviour
    {
        private Channel _ch;
        public virtual Channel Channel
        {
            get { return _ch ?? GetComponent<Channel>(); }
            set
            {
                _ch = value;
                _ch?.Build(this);
            }
        }
        public Connection Connection => Connection.CurrentConnection;
        protected virtual int PreferredChannelID  => 0;
        public bool SyncOwnerOnly = true;
        public uint UpdatePeriod = 20;
        protected virtual bool IsSyncable => false;
        protected long LastSync;
        protected virtual bool OnSync()
        {
            return false;
        }
        
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!IsSyncable) return;
            if (SyncOwnerOnly && !Channel.IsOwner) return;
            if (TimeUtil.GetCurrentTime() - LastSync < UpdatePeriod) return;
            if (OnSync())
            {
                LastSync = TimeUtil.GetCurrentTime();
            }
        }

        protected override void Awake()
        {
            if(_ch == null)
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