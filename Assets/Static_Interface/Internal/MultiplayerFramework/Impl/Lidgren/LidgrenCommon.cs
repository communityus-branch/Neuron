using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ENet;
using Lidgren.Network;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework.Server;
using UnityEngine.Networking;

namespace Static_Interface.Internal.MultiplayerFramework.Impl.Lidgren
{
    public static class LidgrenCommon
    {
        public static void CloseConnection(Identity user, Dictionary<IPIdentity, NetConnection> peers)
        {
            IPIdentity ident = (IPIdentity)user;
            NetConnection p = peers[ident];
            p.Disconnect(nameof(CloseConnection));
            peers.Remove(ident);
        }

        public static void Listen(NetPeer host, Connection connection, Dictionary<int, List<QueuedData>> queue, Dictionary<IPIdentity, NetConnection> peers, out List<NetIncomingMessage> skippedMsgs)
        {
            skippedMsgs = new List<NetIncomingMessage>();
            NetIncomingMessage msg;
            while ((msg = host.ReadMessage()) != null)
            {
                LogUtils.Debug("Received: " + msg.MessageType);
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.DebugMessage:
                        LogUtils.Debug(msg.ReadString());
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

                var ident = GetIdentFromConnection(msg.SenderConnection, peers);

                bool add = false;
                QueuedData qData;
                if (queue[channel].Count > 0 && (IPIdentity)queue[channel].ElementAt(queue.Count - 1).Ident == ident)
                {
                    LogUtils.Debug("Already queued");
                    qData = queue[channel].ElementAt(0);
                }
                else
                {

                    LogUtils.Debug("Queueing new data");
                    qData = new QueuedData { Ident = ident };
                    add = true;
                }

                LogUtils.Debug("Data size: " + msg.Data.Length);

                try
                {
                    EPacket packet = (EPacket)msg.Data[0];
                    LogUtils.Debug("Received packet: " + packet);
                }
                catch (Exception e)
                {
                    e.Log();
                }
                byte[] data = msg.Data;
                qData.Data.AddRange(data);

                if (add)
                {
                    queue[channel].Add(qData);
                }
                host.Recycle(msg);
            }
        }

        public static IPIdentity GetIdentFromConnection(NetConnection senderConnection, Dictionary<IPIdentity, NetConnection> peers)
        {
            foreach (IPIdentity ident in peers.Keys)
            {
                if (peers[ident].GetHashCode() == senderConnection.GetHashCode())
                {
                    return ident;
                }
            }

            throw new ArgumentException("Identity not found for requested peer(?)");
        }

        public static bool Read(out Identity user, byte[] data, out ulong length, int channel, Dictionary<int, List<QueuedData>> queue)
        {
            user = null;
            length = 0;

            var ch = Convert.ToByte(channel);
            if (!queue.ContainsKey(ch) || queue[ch].Count == 0)
            {
                return false;
            }

            QueuedData queuedData = queue[ch].ElementAt(0);

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
                queue[ch].Remove(queuedData);
            }

            return true;
        }

        public static bool Write(Identity target, byte[] data, ulong length, SendMethod method, int channel, NetPeer host, Dictionary<IPIdentity, NetConnection> peers)
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
            NetConnection p = peers[(IPIdentity)target];
            NetOutgoingMessage msg = host.CreateMessage((int) length);
            msg.Data = data;
            var result = p.SendMessage(msg, deliveryMethod, channel);
            if (result != NetSendResult.Dropped && result != NetSendResult.FailedNotConnected)
            {
                return true;
            }

            LogUtils.LogError("Failed to deliver message: " + result);
            return false;
        }
    }
}