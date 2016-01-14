namespace Static_Interface.PlayerFramework
{
    public class SteamPendingUser : PendingUser
    {
        public SteamPendingUser(UserIdentity identity)
        {
            Identity = identity;
        }
    }
}