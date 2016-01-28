using System.IO;
using Static_Interface.API.PlayerFramework;
using Steamworks;

namespace Static_Interface.Internal.MultiplayerFramework.Service.MultiplayerProviderService
{
    public abstract class MultiplayerProvider
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
        }

        public abstract bool Read(out CSteamID user, byte[] data, out ulong length, int channel);

        public abstract bool Write(CSteamID target, byte[] data, ulong length);

        public abstract bool Write(CSteamID target, byte[] data, ulong length, EP2PSend method, int channel);
    }
}