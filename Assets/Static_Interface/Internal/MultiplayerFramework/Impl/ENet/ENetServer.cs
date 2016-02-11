using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using ENet;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework.MultiplayerProvider;

namespace Static_Interface.Internal.MultiplayerFramework.Impl.ENet
{
    public class ENetServer : ServerMultiplayerProvider
    {
        private bool _listen;
        private readonly Dictionary<byte, List<QueuedData>> _queue = new Dictionary<byte, List<QueuedData>>();
        private readonly Dictionary<ulong, Peer> _peers = new Dictionary<ulong, Peer>(); 
        private Host _host;
        private Thread _thread;
        public ENetServer(Connection connection) : base(connection)
        {
            SupportsPing = true;
        }

        ~ENetServer()
        {
            Dispose();
        }

        public override bool Read(out Identity user, byte[] data, out ulong length, int channel)
        {
            return ENetCommon.Read(out user, data, out length, channel, _queue);
        }

        public override bool Write(Identity target, byte[] data, ulong length, SendMethod method, int channel)
        {
            return ENetCommon.Write(target, data, length, method, channel, _peers);
        }

        public override void CloseConnection(Identity user)
        {
            ENetCommon.CloseConnection(user, _peers);
        }

        public override uint GetServerRealTime()
        {
            return Convert.ToUInt32(DateTime.Now.Millisecond);
        }

        public override void Dispose()
        {
            _listen = false;
            if (_host.IsInitialized)
            {
                _host.Dispose();
            }
        }

        public override void EndAuthSession(Identity user)
        {
            CloseConnection(user);
        }

        public override void Open(string bindip, ushort port, bool lan)
        {
            var bind = bindip == "*" ? 
                new IPEndPoint(IPAddress.Any, port) : 
                new IPEndPoint(IPAddress.Parse(bindip), port);
            //Todo: implement bindip
            LogUtils.Log("Opening server listening on " + bindip + " with port "+ port);
            _host = new Host();
            _host.Initialize(bind, MAX_PLAYERS+1);
            _listen = true;
            _thread = new Thread(ListenLoop);
            _thread.Start();
        }

        private void ListenLoop()
        {
            while (_listen)
            {
                ENetCommon.Listen(_host, Connection, _queue, _peers);
            }
        }

        public override void Close()
        {
            foreach (var peer in _peers.Values)
            {
                peer.DisconnectNow(1);
            }

            Dispose();
        }


        public override bool VerifyTicket(Identity ident, byte[] data)
        {
            throw new NotSupportedException();
        }

        public override Identity GetServerIdent()
        {
            return IPIdentity.Server;
        }

        public override void UpdateScore(Identity ident, uint score)
        {
            //do nothing
        }

        public override Identity Deserialilze(ulong ident)
        {
            return new IPIdentity(ident);
        }

        public override void SetMaxPlayerCount(int maxPlayers)
        {
            //do nothing
        }

        public override void SetServerName(string name)
        {
            //do nothing
        }

        public override void SetPasswordProtected(bool b)
        {
            //do nothing
        }

        public override void SetMapName(string map)
        {
            //do nothing
        }
    }
}