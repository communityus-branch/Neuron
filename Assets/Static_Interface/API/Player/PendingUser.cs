using UnityEngine;

namespace Static_Interface.API.Player
{
    public class PendingUser
    {
        public PendingUser(Identity ident, string name, ulong group)
        {
            Identity = ident;
            Joined = Time.realtimeSinceStartup;
            Group = group;
            Name = name;
        }

        public ulong Group { get; }
        public Identity Identity {get;}
        public readonly float Joined;
        public bool HasAuthentication { get; internal set; }
        public string Name { get; }
    }
}