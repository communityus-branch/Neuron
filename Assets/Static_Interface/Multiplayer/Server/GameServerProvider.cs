using System;
using System.IO;
using JetBrains.Annotations;
using Steamworks;

namespace Static_Interface.Multiplayer.Server
{
    public abstract class GameServerProvider
    {
        public const int MIN_PLAYERS = 0;
        public const int MAX_PLAYERS = 16;

        protected readonly uint Ip;
        protected readonly ushort Port;

        protected GameServerProvider(uint ip, ushort port)
        {
            Ip = ip;
            Port = port;
        }

        public string Description = "A " + Game.NAME + " Server";
        private bool _hosted;
        private readonly byte[] _buffer = new byte[1024];
        private MemoryStream _stream;
        protected BinaryReader Deserializer;
        protected BinaryWriter Serializer;
        protected WrappedUser InvalidUser;

        public void Start()
        {
            if (_hosted) return;
            OnStart();
            _hosted = true;

            _stream = new MemoryStream(_buffer);
            Deserializer = new BinaryReader(_stream);
            Serializer = new BinaryWriter(_stream);
            InvalidUser = new InvalidUser();

        }

        public abstract bool Read(out WrappedUser wrappedUser, byte[] data, out ulong length, int channel);

        public abstract void Write(WrappedUser user, byte[] data, ulong length);

        public abstract void Write(WrappedUser user, byte[] data, ulong length, EP2PSend method, int channel);


        protected abstract void OnStart();

        public void Stop()
        {
            if (!_hosted) return;
            OnStop();
            _hosted = false;
        }

        protected abstract void OnStop();
    }
}
