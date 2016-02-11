using System.Collections.Generic;
using System.Linq;
using Lidgren.Network;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework.Server;

namespace Static_Interface.Internal.MultiplayerFramework.Impl.Lidgren
{
    public static class LidgrenCommon
    {
        public static void CloseConnection(Identity user, Dictionary<ulong, NetConnection> peers)
        {
            LogUtils.Debug("Closing connection with peer: " + user);
            var ident = user.Serialize();
            NetConnection p = peers[ident];
            p.Disconnect(nameof(CloseConnection));
            peers.Remove(ident);
        }

        public static void Listen(NetPeer host, Connection connection, Dictionary<int, List<QueuedData>> queue, Dictionary<ulong, NetConnection> peers, out List<NetIncomingMessage> skippedMsgs)
        {
            skippedMsgs = new List<NetIncomingMessage>();
            NetIncomingMessage msg;
            while ((msg = host.ReadMessage()) != null)
            {
                LogUtils.Debug("NetworkEvent: " + msg.MessageType);
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus) msg.PeekByte();
                        LogUtils.Debug("Status: " + status);
                        if (status == NetConnectionStatus.Disconnected)
                        {
                            var sConIdent = GetIdentFromConnection(msg.SenderConnection, peers);
                            ((ServerConnection)Connection.CurrentConnection).DisconnectClient(sConIdent);
                            host.Recycle(msg);
                            continue;
                        }
                        break;
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                        LogUtils.Debug(msg.ReadString());
                        continue;
                    case NetIncomingMessageType.WarningMessage:
                        LogUtils.LogWarning(msg.ReadString());
                        continue;
                    case NetIncomingMessageType.ErrorMessage:
                        LogUtils.LogError(msg.ReadString());
                        continue;
                }

                if (msg.MessageType != NetIncomingMessageType.Data)
                {
                    skippedMsgs.Add(msg); 
                    continue;
                }

                LogUtils.Debug("SequenceChannel: " + msg.SequenceChannel);
                var channel = msg.SequenceChannel;
                if (!queue.ContainsKey(channel))
                {
                    queue.Add(channel, new List<QueuedData>());
                }
                
                var ident = GetIdentFromConnection(msg.SenderConnection, peers);

                QueuedData qData = new QueuedData
                {
                    Ident = ident,
                    Data = msg.ReadBytes(msg.LengthBytes).ToList()
                };
                
                queue[channel].Add(qData);
                host.Recycle(msg);
            }
        }

        public static IPIdentity GetIdentFromConnection(NetConnection senderConnection, Dictionary<ulong, NetConnection> peers)
        {
            foreach (ulong ident in peers.Keys)
            {
                if (Equals(peers[ident].RemoteEndPoint, senderConnection.RemoteEndPoint))
                {
                    return new IPIdentity(ident);
                }
            }

            IPIdentity newIdent = new IPIdentity(senderConnection.RemoteEndPoint.Address);
            peers.Add(newIdent.Serialize(), senderConnection);
            return newIdent;
        }

        public static bool Read(out Identity user, byte[] data, out ulong length, int channel, Dictionary<int, List<QueuedData>> queue)
        {
            user = null;
            length = 0;
            if (!queue.ContainsKey(channel) || queue[channel].Count == 0)
            {
                return false;
            }

            QueuedData queuedData = queue[channel].ElementAt(0);

            for (int i = 0; i < data.Length; i++)
            {
                if (queuedData.Data.Count == 0)
                {
                    break;
                }
                data[i] = queuedData.Data.ElementAt(0);
                queuedData.Data.RemoveAt(0);
                length++;
            }

            user = queuedData.Ident;

            if (queuedData.Data.Count == 0)
            {
                queue[channel].Remove(queuedData);
            }

            return true;
        }

        public static bool Write(Identity target, byte[] data, ulong length, SendMethod method, int channel, NetPeer host, Dictionary<ulong, NetConnection> peers)
        {
            NetDeliveryMethod deliveryMethod = NetDeliveryMethod.Unknown;
            switch (method)
            {
                case SendMethod.SEND_RELIABLE:
                    deliveryMethod = NetDeliveryMethod.ReliableOrdered;
                    break;
                case SendMethod.SEND_RELIABLE_WITH_BUFFERING:
                    deliveryMethod = NetDeliveryMethod.ReliableSequenced;
                    break;
                case SendMethod.SEND_UNRELIABLE:
                    deliveryMethod = NetDeliveryMethod.UnreliableSequenced;
                    break;
                case SendMethod.SEND_UNRELIABLE_NO_DELAY:
                    deliveryMethod = NetDeliveryMethod.Unreliable;
                    break;
            }
            
            NetConnection p = peers[target.Serialize()];
            int iLength = (int)length;
            NetOutgoingMessage msg = host.CreateMessage(iLength);
            msg.Write(data, 0, iLength);

            var result = p.SendMessage(msg, deliveryMethod, channel);
            if (result != NetSendResult.Dropped && result != NetSendResult.FailedNotConnected)
            {
                return true;
            }

            if (channel == 0 && (((EPacket)data[0]) == EPacket.REJECTED || ((EPacket)data[0]) == EPacket.KICKED))
            {
                CloseConnection(target, peers);
            }

            LogUtils.LogError("Failed to deliver message: " + result);
            return false;
        }
    }
}