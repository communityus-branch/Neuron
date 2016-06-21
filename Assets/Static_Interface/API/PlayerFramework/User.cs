using Static_Interface.API.NetworkFramework;
using Static_Interface.Internal.MultiplayerFramework;
using UnityEngine;

namespace Static_Interface.API.PlayerFramework
{
    public class User
    {
        public float LastChat { get; internal set; }
        public float LastNet { get; internal set; }
        public float LastPing { get; internal set; }
        public ulong Group { get; internal set; }

        public readonly float Joined;
        private readonly float[] _pings = new float[4];

        public User(Connection connection, Identity ident, Transform model, int channelId)
        {
            Joined = Time.realtimeSinceStartup;
            LastNet = Time.realtimeSinceStartup;
            LastChat = Time.realtimeSinceStartup;
            Identity = ident;
            Model = model;
            var playerContainer = model.GetChild(0);
            var created = playerContainer.GetComponent<Channel>() == null;
            Channel ch = playerContainer.GetComponent<Channel>();
            ch.ID = channelId;
            ch.Owner = ident;
            ch.IsOwner = ident == connection.ClientID;
            if(created) ch.Setup();
            Player = playerContainer.GetComponent<Player>();
            Player.User = this;
        }

        public void Lag(float value)
        {
            float lastPing;
            NetworkUtils.GetAveragePing(value, out lastPing, _pings);
            LastPing = lastPing;
        }

        public Transform Model { get; protected set; }
        public Identity Identity { get; protected set; }
        public Player Player { get; protected set; }
        public string Name { get; set; }
    }
}