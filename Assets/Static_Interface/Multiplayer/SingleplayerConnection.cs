using Static_Interface.Multiplayer.Client;
using Static_Interface.Multiplayer.Protocol;
using Static_Interface.Multiplayer.Server;
using Steamworks;

namespace Static_Interface.Multiplayer
{
    public class SingleplayerConnection : Connection
    {
        private ClientConnection _client;
        private ServerConnection _server;

        internal override void Awake()
        {
            base.Awake();
            _client = gameObject.AddComponent<ClientConnection>();
            _server = gameObject.AddComponent<ServerConnection>();
            _client.Awake();
            _server.Awake();
        }

        public void Start()
        {
            _server.OpenGameServer(true);
            _client.AttemptConnect(0, 27015, string.Empty);
        }

        public override void Send(CSteamID receiver, EPacket type, byte[] data, int length, int id)
        {
            _client.Send(receiver, type, data, length, id);
        }

        internal override void Receive(CSteamID source, byte[] packet, int offset, int size, int channel)
        {
            _server.Receive(source, packet, offset, size, channel);
        }

        internal override void Listen()
        {
            _client.Listen();
            _server.Listen();
        }

        public override void Disconnect(string reason = null)
        {
            _server.Disconnect();
            _client.Disconnect();
        }

        internal override void Update()
        {
            _server.Update();
            _client.Update();
        }
    }
}