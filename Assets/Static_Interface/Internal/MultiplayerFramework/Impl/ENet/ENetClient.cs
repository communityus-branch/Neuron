using System;
using System.Collections.Generic;
using System.Threading;
using ENet;
using Static_Interface.API.Level;
using Static_Interface.API.Network;
using Static_Interface.API.Player;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework.Client;
using Static_Interface.Internal.MultiplayerFramework.MultiplayerProvider;

namespace Static_Interface.Internal.MultiplayerFramework.Impl.ENet
{
    public class ENetClient : ClientMultiplayerProvider
    {
        private bool _listen;
        private readonly Dictionary<byte, List<ENetQueuedData>> _queue = new Dictionary<byte, List<ENetQueuedData>>();
        private readonly Dictionary<ENetIdentity, Peer> _peers = new Dictionary<ENetIdentity, Peer>();
        private static ENetIdentity _ident = new ENetIdentity(1);
        private static readonly string RandName = "Player" + new Random().Next(MAX_PLAYERS);
        private Host _host;
        private Peer _serverPeer;

        public ENetClient(Connection connection) : base(connection)
        {
            
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

            new Thread(Ping).Start();
        }

        private void Ping()
        {
            LogUtils.Debug("Adding server peer");
            var servIdent = new ENetIdentity(0);
            _peers.Add(servIdent, _serverPeer);

            ulong currentTime = GetServerRealTime();
            bool timeout = false;
            while (_serverPeer.State == PeerState.Connecting)
            {
                if (GetServerRealTime() - currentTime <= 1000 * 3) continue;
                timeout = true;
                break;
            }

            if (timeout)
            {
                if (!((ClientConnection)Connection).OnPingFailed())
                {
                    LogUtils.Error("Couldn't connect to host");
                    LevelManager.Instance.GoToMainMenu();
                }
                return;
            }

            ServerInfo info = new ServerInfo();
            info.ServerID = servIdent;
            info.MaxPlayers = -1;
            info.Name = "ENet Server";

            ((ClientConnection)Connection).Connect(info);
            Listen();
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

        private void Listen()
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
            return Convert.ToUInt32(DateTime.Now.Millisecond);
        }

        public override void Dispose()
        {
            _listen = false;
            _host.Dispose();
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
            return new byte[] {0};
        }

        public override void CloseTicket()
        {
            //do nothing
        }

        public override void FavoriteServer(string ip, ushort port)
        {
            //do nothing
        }

        public override void SetIdentity(ulong serializedIdent)
        {
            _ident = new ENetIdentity(serializedIdent);
        }
    }
}