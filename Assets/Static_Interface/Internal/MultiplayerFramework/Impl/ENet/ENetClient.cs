using System;
using System.Collections.Generic;
using System.Threading;
using ENet;
using Static_Interface.API.LevelFramework;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework.Client;
using Static_Interface.Internal.MultiplayerFramework.MultiplayerProvider;

namespace Static_Interface.Internal.MultiplayerFramework.Impl.ENet
{
    public class ENetClient : ClientMultiplayerProvider
    {
        private bool _listen;
        private readonly Dictionary<byte, List<QueuedData>> _queue = new Dictionary<byte, List<QueuedData>>();
        private readonly Dictionary<ulong, Peer> _peers = new Dictionary<ulong, Peer>();
        private static IPIdentity _ident = new IPIdentity(1);
        private static readonly string RandName = "Player" + new Random().Next(MAX_PLAYERS);
        private Host _host;
        private Peer _serverPeer;
        private Thread _thread;
        public ENetClient(Connection connection) : base(connection)
        {
            SupportsPing = true;
        }

        ~ENetClient()
        {
            Dispose();
        }

        public override void AttemptConnect(string ip, ushort port, string password)
        {
            _host = new Host();

            LogUtils.Debug("Initializing ENet Client");
            _host.InitializeClient(1);

            LogUtils.Debug("Connecting to host");
            _serverPeer = _host.Connect(ip, port, 0);
            _listen = true;

            _thread = new Thread(OnConnect);
            _thread.Start();
        }

        private void OnConnect()
        {
            LogUtils.Debug("Adding server peer");

            var servIdent = new IPIdentity(0);
            _peers.Add(servIdent.Serialize(), _serverPeer);

            bool timeout = false;
            ulong checkTime = GetServerRealTime();
            while (_serverPeer.State == PeerState.Connecting)
            {
                if (GetServerRealTime() - checkTime <= 1000 * 5) continue;
                timeout = true;
                break;
            }

            if (_serverPeer.State != PeerState.Connected)
            {
                timeout = true;
            }

            if (timeout)
            {
                LogUtils.LogError("Timeout with state: " + _serverPeer.State);
                if (((ClientConnection)Connection).OnConnectionFailed()) return;
                LogUtils.Debug("Couldn't connect to host");
                LevelManager.Instance.GoToMainMenu();
                return;
            }

            ServerInfo info = new ServerInfo
            {
                ServerID = servIdent,
                MaxPlayers = -1,
                Name = "ENet Server",
                Map = "TestMap"
            };


            ((ClientConnection)Connection).Connect(info);
            ListenLoop();
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

        private void ListenLoop()
        {
            while (_listen)
            {
                ENetCommon.Listen(_host, Connection, _queue, _peers);
            }
        }

        public override string GetClientName()
        {
            return RandName;
        }

        public override Identity GetUserID()
        {
            return _ident;
        }

        public override uint GetServerRealTime()
        {
            return Convert.ToUInt32(DateTime.UtcNow.Millisecond);
        }

        public override void Dispose()
        {
            _listen = false;
            if (_host.IsInitialized)
            {
                _host.Dispose();
            }
            _thread = null;
        }

        public override void AdvertiseGame(Identity serverID, string ip, ushort port)
        {
            //do nothing
        }

        public override void SetPlayedWith(Identity ident)
        {
            //do nothing
        }

        public override void SetStatus(string status)
        {
            //do nothing
        }

        public override void SetConnectInfo(string ip, ushort port)
        {
            //do nothing
        }

        public override bool IsFavoritedServer(string ip, ushort port)
        {
            return false;
        }

        public override byte[] OpenTicket()
        {
            throw new NotSupportedException();
        }

        public override void CloseTicket()
        {
            throw new NotSupportedException();
        }

        public override void FavoriteServer(string ip, ushort port)
        {
            //do nothing
        }

        public override void SetIdentity(ulong serializedIdent)
        {
            _ident = new IPIdentity(serializedIdent);
        }
    }
}