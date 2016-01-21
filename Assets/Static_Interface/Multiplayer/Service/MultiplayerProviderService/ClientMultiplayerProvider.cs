using Static_Interface.Multiplayer.Server;
using Static_Interface.PlayerFramework;
using Steamworks;

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
            Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);
        }

        public void Connect(ServerInfo serverInfo)
        {
            CurrentServer = serverInfo;
        }

        public void Disconnect()
        {
            // Todo
        }

        public override bool Read(out CSteamID user, byte[] data, out ulong length, int channel)
        {
            uint num;
            user = CSteamID.Nil;
            length = 0L;
            if (!SteamNetworking.IsP2PPacketAvailable(out num, channel) || (num > data.Length))
            {
                return false;
            }
            if (!SteamNetworking.ReadP2PPacket(data, num, out num, out user, channel))
            {
                return false;
            }
            length = num;
            return true;
        }

        public override void Write(User user, byte[] data, ulong length)
        {
            SteamNetworking.SendP2PPacket(user.Identity.ID, data, (uint)length, EP2PSend.k_EP2PSendUnreliable);
        }

        public override void Write(User user, byte[] data, ulong length, EP2PSend method, int channel)
        {
            SteamNetworking.SendP2PPacket(user.Identity.ID, data, (uint)length, method, channel);
        }
    }
}