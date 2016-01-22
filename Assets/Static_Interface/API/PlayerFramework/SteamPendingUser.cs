namespace Static_Interface.API.PlayerFramework
{
    public class SteamPendingUser : PendingUser
    {
        public SteamPendingUser(UserIdentity identity)
        {
            Identity = identity;
        }
    }
}