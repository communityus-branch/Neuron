using System;
using System.Collections.Generic;
using System.Net;
using Lidgren.Network;
using Static_Interface.API.PlayerFramework;
using Static_Interface.Internal.MultiplayerFramework.MultiplayerProvider;
using Static_Interface.Neuron;
using UnityEngine;

namespace Static_Interface.Internal.MultiplayerFramework.Impl.Lidgren
{
    public class LidgrenServer : ServerMultiplayerProvider
    {
        private NetServer _server;
        private bool _listen;
        private readonly Dictionary<int, List<QueuedData>> _queue = new Dictionary<int, List<QueuedData>>();
        private readonly Dictionary<ulong, NetConnection> _peers = new Dictionary<ulong, NetConnection>();
        public LidgrenServer(Connection connection) : base(connection)
        {
        }

        public override bool Read(out Identity user, byte[] data, out ulong length, int channel)
        {
            return LidgrenCommon.Read(out user, data, out length, channel, _queue);
        }

        public override bool Write(Identity target, byte[] data, ulong length, SendMethod method, int channel)
        {
            return LidgrenCommon.Write(target, data, length, method, channel, _server, _peers);
        }

        public override void CloseConnection(Identity user)
        {
            LidgrenCommon.CloseConnection(user, _peers);
        }

        public override uint GetServerRealTime()
        {
            return Convert.ToUInt32(DateTime.Now.Millisecond);
        }

        public override void Dispose()
        {
            _listen = false;
            _server.Shutdown(nameof(Dispose));
            _server = null;
        }

        public override void EndAuthSession(Identity user)
        {
            CloseConnection(user);
        }

        public override void Open(string bindip, ushort port, bool lan)
        {
            var bind = bindip == "*" ?
                IPAddress.Any :
                IPAddress.Parse(bindip);

            NetPeerConfiguration config = new NetPeerConfiguration(GameInfo.NAME)
            {
                Port = port,
                AcceptIncomingConnections = true
            };
            config.SetMessageTypeEnabled(NetIncomingMessageType.NatIntroductionSuccess, true);
            config.SetMessageTypeEnabled(NetIncomingMessageType.ConnectionApproval, true);
            config.SetMessageTypeEnabled(NetIncomingMessageType.DebugMessage, Debug.isDebugBuild);

            _server = new NetServer(config);
            _server.Start();
            _listen = true;
        }

        public override void Update()
        {
            base.Update();
            if (!_listen) return;
            List<NetIncomingMessage> msgs;
            LidgrenCommon.Listen(_server, Connection, _queue, _peers, out msgs);
            foreach (NetIncomingMessage msg in msgs)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.ConnectionApproval:
                        //Todo: check for password here?
                        msg.SenderConnection.Approve();
                        _server.Recycle(msg);
                        break;
                }
            }
        }

        public override void Close()
        {
            foreach (var conn in _server.Connections)
            {
                conn.Disconnect(nameof(Close));
            }
            Dispose();          
        }

        public override Identity GetServerIdentity()
        {
            return IPIdentity.Server;
        }

        public override bool VerifyTicket(Identity ident, byte[] data)
        {
            return true;
        }

        public override void UpdateScore(Identity ident, uint score)
        {
            //do nothing
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