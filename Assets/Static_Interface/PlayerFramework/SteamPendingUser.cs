namespace Static_Interface.PlayerFramework
{
    public class SteamPendingUser : PendingUser
    {
        private readonly UserIdentity _identity;

        public override UserIdentity Identity
        {
            get { return _identity; }
        }

        public SteamPendingUser(UserIdentity identity)
        {
            _identity = identity;
        }
    }
}