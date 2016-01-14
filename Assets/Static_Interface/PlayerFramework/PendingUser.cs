using UnityEngine;

namespace Static_Interface.PlayerFramework
{
    public abstract class PendingUser
    {
        public UserIdentity Identity {get; protected set; }
        public readonly float Joined;
        internal bool HasAuthentication;

        protected PendingUser()
        {
            Joined = Time.realtimeSinceStartup;
        }
    }
}