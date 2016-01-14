using System.IO;
using Static_Interface.PlayerFramework;
using Steamworks;

namespace Static_Interface.Multiplayer.Service.MultiplayerProviderService
{
    public abstract class MultiplayerProvider : Service
    {
        public const int MIN_PLAYERS = 0;
        public const int MAX_PLAYERS = 16;

        public BinaryReader Deserializer { get; }
        public MemoryStream Stream { get; }
        public BinaryWriter Serializer { get; }
        protected byte[] Buffer = new byte[1024];

        protected MultiplayerProvider()
        {
            Stream = new MemoryStream(Buffer);
            Deserializer = new BinaryReader(Stream);
            Serializer = new BinaryWriter(Stream);

            Callback<P2PSessionRequest_t>.CreateGameServer(OnP2PSessionRequest);
        }

        public bool Read(out CSteamID user, byte[] data, out ulong length, int channel)
        {
            uint num;
            user = CSteamID.Nil;
            length = 0L;
            if (!SteamGameServerNetworking.IsP2PPacketAvailable(out num, channel) || (num > data.Length))
            {
                return false;
            }
            if (!SteamGameServerNetworking.ReadP2PPacket(data, num, out num, out user, channel))
            {
                return false;
            }
            length = num;
            return true;
        }

        public void OnP2PSessionRequest(P2PSessionRequest_t callback)
        {
            SteamNetworking.AcceptP2PSessionWithUser(callback.m_steamIDRemote);
        }

        public void Write(User user, byte[] data, ulong length)
        {
            SteamGameServerNetworking.SendP2PPacket(user.Identity.ID, data, (uint)length, EP2PSend.k_EP2PSendUnreliable);
        }

        public void Write(User user, byte[] data, ulong length, EP2PSend method, int channel)
        {
            SteamGameServerNetworking.SendP2PPacket(user.Identity.ID, data, (uint)length, method, channel);
        }
    }
}