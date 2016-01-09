using Steamworks;

namespace Static_Interface.Multiplayer.Server.Impl.Steamworks
{
    public class SteamWrappedUser : WrappedUser
    {
        private readonly CSteamID _steamID;
        public CSteamID SteamID { get { return _steamID; } }
        public SteamWrappedUser(CSteamID userId)
        {
            _steamID = userId;
        }
    }
}