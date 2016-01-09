using Steamworks;
namespace Static_Interface.Multiplayer.Server
{
    public class ServerInfo
    {
        private readonly bool _hasPassword;
        private readonly bool _isSecure;
        private readonly string _map;
        private readonly int _maxPlayers;
        private readonly string _name;
        private readonly int _ping;
        private readonly int _players;
        private readonly CSteamID _steamID;

        public ServerInfo(gameserveritem_t data)
        {
            _steamID = data.m_steamID;
            _name = data.GetServerName();
            _map = data.GetMap();
            _ping = data.m_nPing;
            _players = data.m_nPlayers;
            _maxPlayers = data.m_nMaxPlayers;
            _hasPassword = data.m_bPassword;
            _isSecure = data.m_bSecure;
        }

        public ServerInfo(string newName, bool newSecure)
        {
            _name = newName;
            _isSecure = newSecure;
        }

        public bool HasPassword
        {
            get
            {
                return _hasPassword;
            }
        }

        public bool IsSecure
        {
            get
            {
                return _isSecure;
            }
        }

        public string Map
        {
            get
            {
                return _map;
            }
        }

        public int MaxPlayers
        {
            get
            {
                return _maxPlayers;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public int Ping
        {
            get
            {
                return _ping;
            }
        }

        public int Players
        {
            get
            {
                return _players;
            }
        }

        public CSteamID SteamID
        {
            get
            {
                return _steamID;
            }
        }
    }
}
