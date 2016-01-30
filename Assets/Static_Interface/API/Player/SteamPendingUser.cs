namespace Static_Interface.API.Player
{
    public class SteamPendingUser : PendingUser
    {
        public SteamPendingUser(UserIdentity identity)
        {
            Identity = identity;
        }
    }
}