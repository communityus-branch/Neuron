using Static_Interface.Multiplayer.Server.Impl.Steamworks;
using Static_Interface.Objects;
using Steamworks;

namespace Static_Interface.Multiplayer.Server
{
    public class ServerConnection : Connection
    {
        private SteamServer server;
        public SteamServer Server
        {
            get { return server; }
        }

        public override void Send(CSteamID receiver, EPacket type, byte[] data, int length, int id)
        {
            throw new System.NotImplementedException();
        }

        protected override void Listen()
        {
            throw new System.NotImplementedException();
        }

        public override void Disconnect()
        {
            throw new System.NotImplementedException();
        }

        public ServerConnection(CSteamID id) : base(id)
        {

        }

        protected override void OnAwake()
        {
            uint ip = 0;
            ushort port = 27015;

            server = new SteamServer(ip, port);

            //Callback<GSPolicyResponse_t>.CreateGameServer(OnGSPolicyResponse);
            //Callback<P2PSessionConnectFail_t>.CreateGameServer(OnP2PSessionConnectFail);
            //Callback<ValidateAuthTicketResponse_t>.CreateGameServer(OnValidateAuthTicketResponse);

        }
    }
}