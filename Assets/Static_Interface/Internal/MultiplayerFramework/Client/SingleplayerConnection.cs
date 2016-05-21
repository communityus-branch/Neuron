using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.SerialisationFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework.Impl;
using Static_Interface.Internal.MultiplayerFramework.Impl.Lidgren;
using Static_Interface.Internal.MultiplayerFramework.Server;
using Static_Interface.Internal.Objects;
using Static_Interface.Neuron;

namespace Static_Interface.Internal.MultiplayerFramework.Client
{
    public class SingleplayerConnection : ClientConnection
    {
        private ServerConnection _server;

        protected override void Awake()
        {
            base.Awake();
            _server = gameObject.AddComponent<ServerConnection>();
        }

        public void Init()
        {
            Provider = new LidgrenClient(this);
            IsSinglePlayer = true;
            _server.OpenGameServer(true);
            _server.SinglePlayerConnection = this;
            ClientName = "Player";
            ClientID = new IPIdentity(0);
            ServerID = ClientID;
            ConnectToServer();
        }

        public void ConnectToServer()
        {
            LogUtils.Debug("<b>Connecting to local server</b>");
            int size;
            ulong group = 1;
            object[] args = { ClientName, group, GameInfo.VERSION, 0f };
            byte[] packet = ObjectSerializer.GetBytes(0, out size, args);
            Send(ServerID, EPacket.CONNECT, packet, size, 0);
        }

        protected override bool OnPreSend(Identity receiver, EPacket type, byte[] data, int length, int channel)
        {
            if (ServerID == ClientID)
            {
                _server.Receive(ServerID, data, length, channel);
                return true;
            }
            return false;
        }

        internal override void Listen()
        {

        }

        protected override void OnLevelWasLoaded(int level)
        {

        }

        public override void Dispose()
        {
            _server.CloseGameServer();
            _server.Dispose();    
        }
    }
}