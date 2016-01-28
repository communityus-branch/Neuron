using Static_Interface.API.PlayerFramework;
using Steamworks;

namespace Static_Interface.Internal.MultiplayerFramework.Service.MultiplayerProviderService
{
    public class ClientMultiplayerProvider : MultiplayerProvider
    {
        public bool IsAttempting;
        public bool IsConnected;

        public ServerInfo CurrentServer { get; internal set; }

        public ClientMultiplayerProvider()
        {
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
                LogUtils.Debug("No P2P Packet available");
                return false;
            }
            if (!SteamNetworking.ReadP2PPacket(data, num, out num, out user, channel))
            {
                LogUtils.Debug("P2P Packet reading failed");
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