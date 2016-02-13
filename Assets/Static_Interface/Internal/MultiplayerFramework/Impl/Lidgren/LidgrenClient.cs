using System;
using System.Collections.Generic;
using Lidgren.Network;
using Static_Interface.API.LevelFramework;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework.Client;
using Static_Interface.Internal.MultiplayerFramework.MultiplayerProvider;
using Static_Interface.Neuron;

namespace Static_Interface.Internal.MultiplayerFramework.Impl.Lidgren
{
    public class LidgrenClient : ClientMultiplayerProvider
    {
        private readonly Dictionary<int, List<QueuedData>> _queue = new Dictionary<int, List<QueuedData>>();
        private readonly Dictionary<ulong, NetConnection> _peers = new Dictionary<ulong, NetConnection>();
        private static readonly string RandName = "Player" + new Random().Next(MAX_PLAYERS);
        private NetClient _client;
        private IPIdentity _ident;
        private bool _listen;
        private string _ip;
        private ushort _port;
        public LidgrenClient(Connection connection) : base(connection)
        {
            SupportsPing = true;
        }

        ~LidgrenClient()
        {
            Dispose();
        }

        public override void AttemptConnect(string ip, ushort port, string password)
        {
            _ip = ip;
            _port = port;

            NetPeerConfiguration config = new NetPeerConfiguration(GameInfo.NAME)
            {
                Port = port
            };
            _client = new NetClient(config);
            _client.Start();
            NetConnection conn = _client.Connect(_ip, _port);
            _listen = true;
            LogUtils.Debug("Adding server connection");
            var servIdent = new IPIdentity(0);
            _peers.Add(servIdent.Serialize(), conn);
        }

        public override Identity Deserialilze(ulong ident)
        {
            return new IPIdentity(ident);
        }

        public override Identity GetServerIdent()
        {
            return IPIdentity.Server;
        }

        public override void Update()
        {
            base.Update();
            if (!_listen) return;
            List<NetIncomingMessage> msgs;
            LidgrenCommon.Listen(_client, Connection, _queue, _peers, out msgs);

            foreach (NetIncomingMessage msg in msgs)
            {
                if (msg.MessageType != NetIncomingMessageType.StatusChanged) continue;
                NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                if (status == NetConnectionStatus.Connected)
                {
                    ServerInfo info = new ServerInfo()
                    {
                        //Todo
                        ServerID = new IPIdentity(0),
                        MaxPlayers = MAX_PLAYERS,
                        Name = "A Lidgren Server",
                        Map = "TestMap"
                    };
                    ((ClientConnection)Connection).Connect(info);
                }

                if (status == NetConnectionStatus.Disconnected)
                {
                    if (((ClientConnection)Connection).OnConnectionFailed()) continue;
                    LogUtils.Debug("Couldn't connect to host");
                    LevelManager.Instance.GoToMainMenu();
                    _listen = false;
                }
                _client.Recycle(msg);
            }
        }

        public override bool Read(out Identity user, byte[] data, out ulong length, int channel)
        {
            return LidgrenCommon.Read(out user, data, out length, channel, _queue);
        }

        public override bool Write(Identity target, byte[] data, ulong length, SendMethod method, int channel)
        {
            return LidgrenCommon.Write(target, data, length, method, channel, _client, _peers);
        }

        public override void CloseConnection(Identity user)
        {
            LidgrenCommon.CloseConnection(user, _peers);
        }

        public override uint GetServerRealTime()
        {
            return TimeUtil.GetCurrentTime();
        }

        public override void Dispose()
        {
            _listen = false;
            _client?.Shutdown(nameof(Dispose));
            _client = null;
        }
        public override Identity GetUserID()
        {
            return _ident;
        }

        public override string GetClientName()
        {
            return RandName;
        }

        public override void SetIdentity(ulong serializedIdent)
        {
            _ident = new IPIdentity(serializedIdent);
        }

        public override byte[] OpenTicket()
        {
            throw new NotSupportedException();
        }

        public override void CloseTicket()
        {
            throw new NotSupportedException();
        }

        public override bool IsFavoritedServer(string ip, ushort port)
        {
            return false;
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

        public override void FavoriteServer(string ip, ushort port)
        {
            //do nothing
        }
    }
}