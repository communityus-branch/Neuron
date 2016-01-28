using Static_Interface.API.NetworkFramework;
using Static_Interface.Internal;
using UnityEngine;

namespace Static_Interface.API.PlayerFramework
{
    public abstract class User
    {
        public float LastChat { get; internal set; }
        public float LastNet { get; internal set; }
        public float LastPing { get; internal set; }

        public readonly float Joined;
        private readonly float[] _pings = new float[4];

        protected User()
        {
            Joined = Time.realtimeSinceStartup;
            LastNet = Time.realtimeSinceStartup;
            LastChat = Time.realtimeSinceStartup;
        }

        public void Lag(float value)
        {
            float lastPing;
            NetworkUtils.GetAveragePing(value, out lastPing, _pings);
            LastPing = lastPing;
        }

        public Transform Model { get; protected set; }
        public UserIdentity Identity { get; protected set; }
        public Player Player { get; protected set; }
    }
}