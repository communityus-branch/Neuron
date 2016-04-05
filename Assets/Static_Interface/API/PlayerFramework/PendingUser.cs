#pragma warning disable 8025
using UnityEngine;

namespace Static_Interface.API.PlayerFramework
{
    public class PendingUser
    {
        public PendingUser(Identity ident, string name, ulong group, float ping)
        {
            Identity = ident;
            Joined = Time.realtimeSinceStartup;
            Group = group;
            Name = name;
            Ping = ping;
        }

        public float Ping { get; }
        public ulong Group { get; }
        public Identity Identity {get;}
        public readonly float Joined;
        public bool HasAuthentication { get; internal set; }
        public string Name { get; }
    }
}