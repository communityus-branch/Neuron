using Static_Interface.Multiplayer.Server;

namespace Static_Interface.Multiplayer.Service.MultiplayerProviderService
{
    public class ClientMultiplayerProvider : MultiplayerProvider
    {
        public bool IsAttempting;
        public bool IsConnected;

        public ServerInfo CurrentServer { get; protected set; }

        public ClientMultiplayerProvider(ServerInfo info)
        {
            CurrentServer = info;
        }

        public void Connect(ServerInfo serverInfo)
        {
            CurrentServer = serverInfo;
        }

        public void Disconnect()
        {
            // Todo
        }
    }
}