using Steamworks;
namespace Static_Interface.Internal
{
    public class ServerInfo
    {
        public ServerInfo(gameserveritem_t data)
        {
            SteamID = data.m_steamID;
            Name = data.GetServerName();
            Map = data.GetMap();
            Ping = data.m_nPing;
            Players = data.m_nPlayers;
            MaxPlayers = data.m_nMaxPlayers;
            HasPassword = data.m_bPassword;
            IsSecure = data.m_bSecure;
        }

        public ServerInfo(string newName, bool newSecure)
        {
            Name = newName;
            IsSecure = newSecure;
        }

        public bool HasPassword { get; }

        public bool IsSecure { get; }

        public string Map { get; }

        public int MaxPlayers { get; }

        public string Name { get; }

        public int Ping { get; }

        public int Players { get; }

        public CSteamID SteamID { get; }
    }
}
