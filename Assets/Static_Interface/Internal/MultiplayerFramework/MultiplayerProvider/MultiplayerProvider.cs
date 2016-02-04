using System.IO;
using System.Text;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;

namespace Static_Interface.Internal.MultiplayerFramework.MultiplayerProvider
{
    public abstract class MultiplayerProvider
    {
        public const int MIN_PLAYERS = 0;
        public const int MAX_PLAYERS = 16;

        public BinaryReader Deserializer { get; }
        public MemoryStream Stream { get; }
        public BinaryWriter Serializer { get; }
        public Connection Connection { get; }
        protected byte[] Buffer = new byte[1024];

        protected MultiplayerProvider(Connection connection)
        {
            Stream = new MemoryStream(Buffer);
            Deserializer = new BinaryReader(Stream);
            Serializer = new BinaryWriter(Stream);
            Connection = connection;
        }

        public abstract bool Read(out Identity user, byte[] data, out ulong length, int channel);

        public abstract bool Write(Identity target, byte[] data, ulong length, SendMethod method, int channel);

        public abstract void CloseConnection(Identity user);
  
        protected void OnAPIWarningMessage(int severity, StringBuilder warning)
        {
            LogUtils.Log("Warning: " + warning + " (Severity: " + severity + ")");
        }

        public abstract uint GetServerRealTime();
        public abstract void Dispose();
    }
}