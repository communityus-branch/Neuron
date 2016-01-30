using Static_Interface.API.Network;
using Static_Interface.API.Utils;
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
            Callback<P2PSessionConnectFail_t>.Create(OnP2PSessionConnectFail);
        }

        public void Connect(ServerInfo serverInfo)
        {
            CurrentServer = serverInfo;
        }

        public void Disconnect()
        {
            // Todo
        }

        private void OnP2PSessionConnectFail(P2PSessionConnectFail_t callback)
        {
            LogUtils.Error("P2P connection failed for: " + callback.m_steamIDRemote + ", error: " + callback.m_eP2PSessionError);
        }


        public void OnP2PSessionRequest(P2PSessionRequest_t callback)
        {
            if (!SteamNetworking.AcceptP2PSessionWithUser(callback.m_steamIDRemote))
            {
                LogUtils.Debug("Failde to accept P2P Request: " + callback.m_steamIDRemote);
            }
            else
            {
                LogUtils.Debug("Accepted P2P Request: " + callback.m_steamIDRemote);
            }
        }

        public override bool Read(out CSteamID user, byte[] data, out ulong length, int channel)
        {
            uint num;
            user = CSteamID.Nil;
            length = 0L;
            if (!SteamNetworking.IsP2PPacketAvailable(out num, channel) || (num > data.Length))
            {
                LogUtils.Debug("No P2P Packet available on channel " + channel);
                return false;
            }
            if (!SteamNetworking.ReadP2PPacket(data, num, out num, out user, channel))
            {
                LogUtils.Debug("P2P Packet reading failed on chnanel " + channel);
                return false;
            }
            length = num;
            return true;
        }

        public override bool Write(CSteamID target, byte[] data, ulong length)
        {
            LogUtils.Debug("Writing default...");
            return SteamNetworking.SendP2PPacket(target, data, (uint)length, EP2PSend.k_EP2PSendUnreliable);
        }

        public override bool Write(CSteamID target, byte[] data, ulong length, EP2PSend method, int channel)
        {
            LogUtils.Debug("Writing with method: " + method + " in channel " + channel);
            return SteamNetworking.SendP2PPacket(target, data, (uint)length, method, channel);
        }
    }
}