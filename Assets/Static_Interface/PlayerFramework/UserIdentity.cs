using Steamworks;

namespace Static_Interface.PlayerFramework
{
    public class UserIdentity
    {
        private readonly CSteamID _id;
        public CSteamID ID { get { return _id; } }

        private readonly string _playerName;

        public string PlayerName
        {
            get { return _playerName; }
        }

        private readonly CSteamID _group;
        public CSteamID Group { get { return _group; } }

        public UserIdentity(CSteamID id, string playerName, CSteamID @group)
        {
            _id = id;
            _playerName = playerName;
            _group = @group;
        }
    }
}