using System.IO;
using Static_Interface.PlayerFramework;
using Steamworks;

namespace Static_Interface.Multiplayer.Service.MultiplayerProviderService
{
    public abstract class MultiplayerProvider : Service
    {
        public const int MIN_PLAYERS = 0;
        public const int MAX_PLAYERS = 16;

        public BinaryReader Deserializer { get { return _deserializer; } }
        public MemoryStream Stream { get { return _stream; } }
        public BinaryWriter Serializer { get { return _serializer; } }
        protected byte[] Buffer = new byte[1024];
        private readonly MemoryStream _stream;
        private readonly BinaryReader _deserializer;
        private readonly BinaryWriter _serializer;

        protected MultiplayerProvider()
        {
            _stream = new MemoryStream(Buffer);
            _deserializer = new BinaryReader(_stream);
            _serializer = new BinaryWriter(_stream);

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
            SteamGameServerNetworking.SendP2PPacket(user.Identity.ID, data, (uint)length, EP2PSend.k_EP2PSendUnreliable, 0);
        }

        public void Write(User user, byte[] data, ulong length, EP2PSend method, int channel)
        {
            SteamGameServerNetworking.SendP2PPacket(user.Identity.ID, data, (uint)length, method, channel);
        }
    }
}