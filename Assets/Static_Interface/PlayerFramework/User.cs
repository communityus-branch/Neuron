using Static_Interface.Utils;
using UnityEngine;

namespace Static_Interface.PlayerFramework
{
    public abstract class User
    {
        private float _lastChat;

        public float LastChat
        {
            get { return _lastChat; }
            internal set { _lastChat = value; }
        }

        private float _lastNet;

        public float LastNet
        {
            get { return _lastNet; }
            internal set { _lastNet = value;  }
        }

        private float _lastPing;

        public float LastPing
        {
            get { return _lastPing; }
            internal set { _lastPing = value;  }
        }

        public readonly float Joined;
        private readonly float[] _pings = new float[4];

        protected User()
        {
            Joined = Time.realtimeSinceStartup;
            _lastNet = Time.realtimeSinceStartup;
            _lastChat = Time.realtimeSinceStartup;
        }

        public void Lag(float value)
        {
            NetworkUtils.GetAveragePing(value, out _lastPing, _pings);
        }

        public abstract Player Player { get; }
        public abstract UserIdentity Identity { get; }
        public abstract Transform Model { get;  }
    }
}