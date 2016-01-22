using UnityEngine;

namespace Static_Interface.API.PlayerFramework
{
    public abstract class PendingUser
    {
        public UserIdentity Identity {get; protected set; }
        public readonly float Joined;
        public bool HasAuthentication { get; internal set; }

        protected PendingUser()
        {
            Joined = Time.realtimeSinceStartup;
        }
    }
}