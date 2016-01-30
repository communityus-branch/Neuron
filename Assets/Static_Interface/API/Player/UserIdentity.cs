using Steamworks;

namespace Static_Interface.API.Player
{
    public class UserIdentity
    {
        public CSteamID ID { get; }

        public string PlayerName { get; }

        public CSteamID Group { get; }

        public UserIdentity(CSteamID id, string playerName, CSteamID @group)
        {
            ID = id;
            PlayerName = playerName;
            Group = @group;
        }
    }
}