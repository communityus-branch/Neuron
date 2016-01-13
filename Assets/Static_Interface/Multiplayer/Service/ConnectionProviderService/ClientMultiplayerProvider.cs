using System.IO;
using Static_Interface.Multiplayer.Server;
using Steamworks;

namespace Static_Interface.Multiplayer.Service.ConnectionProviderService
{
    public class ClientMultiplayerProvider : MultiplayerProvider
    {
        private ServerInfo _info;

        public bool IsAttempting;
        public bool IsConnected;

        public ServerInfo CurrentServer
        {
            get { return _info; }
        }

        public ClientMultiplayerProvider(ServerInfo info)
        {
            _info = info;
        }

        public void Connect(ServerInfo serverInfo)
        {
            _info = serverInfo;
        }

        public void Disconnect()
        {
            // Do nothing
        }
    }
}