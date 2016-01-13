using UnityEngine;

namespace Static_Interface.PlayerFramework
{
    public abstract class PendingUser
    {
        public abstract UserIdentity Identity {get;}
        public readonly float Joined;
        internal bool HasAuthentication;

        protected PendingUser()
        {
            Joined = Time.realtimeSinceStartup;
        }
    }
}