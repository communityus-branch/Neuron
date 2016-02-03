using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ENet;
using Static_Interface.API.Player;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework.Server;
using Event = ENet.Event;
using EventType = ENet.EventType;

namespace Static_Interface.Internal.MultiplayerFramework.Impl.ENet
{
    public class ENetCommon
    {
        public static bool Read(out Identity user, byte[] data, out ulong length, int channel, Dictionary<byte, List<ENetQueuedData>>  queue)
        {
            user = null;
            length = 0;

            var ch = Convert.ToByte(channel);
            if (!queue.ContainsKey(ch) || queue[ch].Count == 0)
            {
                return false;
            }

            ENetQueuedData eNetQueuedData = queue[ch].ElementAt(0);

            for (int i = 0; i < data.Length; i++)
            {
                if (eNetQueuedData.Data.Count == 0)
                {
                    break;
                }
                data[i] = eNetQueuedData.Data.ElementAt(0);
                eNetQueuedData.Data.RemoveAt(0);
                length++;
            }

            user = eNetQueuedData.Ident;

            if (eNetQueuedData.Data.Count == 0)
            {
                queue[ch].Remove(eNetQueuedData);
            }

            return true;
        }

        public static bool Write(Identity target, byte[] data, ulong length, SendMethod method, int channel, Dictionary<ENetIdentity, Peer> peers)
        {
            PacketFlags flags = PacketFlags.None;
            switch (method)
            {
                case SendMethod.SEND_RELIABLE_WITH_BUFFERING:
                case SendMethod.SEND_RELIABLE:
                    flags = PacketFlags.Reliable;
                    break;
                case SendMethod.SEND_UNRELIABLE:
                    flags = PacketFlags.UnreliableFragment;
                    break;
                case SendMethod.SEND_UNRELIABLE_NO_DELAY:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(method), method, null);
            }
            byte ch = Convert.ToByte(channel);
            ENetIdentity ident = (ENetIdentity) target;
            Peer peer = peers[ident];
            LogUtils.Debug("State: " + peer.State);
            peer.Send(ch, data, 0, Convert.ToInt32(length), flags);
            return true;
        }


        public static void CloseConnection(Identity user, Dictionary<ENetIdentity, Peer> peers)
        {
            ENetIdentity ident = (ENetIdentity) user;
            Peer p = peers[ident];
            p.DisconnectNow(0);
            peers.Remove(ident);
        }

        public static void Listen(Host host, Connection connection, Dictionary<byte, List<ENetQueuedData>> queue, Dictionary<ENetIdentity, Peer> peers)
        {
            Event @event;
            host.Service(1, out @event);
            LogUtils.Debug("Event: " + @event.Type);

            switch (@event.Type)
            {
                case EventType.Connect:
                    ENetIdentity newIdent = new ENetIdentity(@event.Peer);
                    peers.Add(newIdent, @event.Peer);
                    break;

                case EventType.Disconnect:
                    ((ServerConnection) connection).DisconnectClient(GetIdentFromPeer(@event.Peer, peers));
                    break;

                case EventType.Receive:

                    byte channel = @event.ChannelID;
                    if (!queue.ContainsKey(channel))
                    {
                        queue.Add(channel, new List<ENetQueuedData>());
                    }


                    var ident = GetIdentFromPeer(@event.Peer, peers);

                    bool add = false;
                    ENetQueuedData qData;
                    if (queue[channel].Count > 0 && queue[channel].ElementAt(queue.Count - 1).Ident == ident)
                    {
                        qData = queue[channel].ElementAt(0);
                    }
                    else
                    {
                        qData = new ENetQueuedData {Ident = ident};
                        add = true;
                    }

                    byte[] data = @event.Packet.GetBytes();
                    qData.Data.AddRange(data);

                    if (add)
                    {
                        queue[channel].Add(qData);
                    }

                    @event.Packet.Dispose();
                    break;
            }
            Thread.Sleep(1);
        }

        public static ENetIdentity GetIdentFromPeer(Peer peer, Dictionary<ENetIdentity, Peer> peers)
        {
            foreach (ENetIdentity ident in peers.Keys)
            {
                if (peers[ident].GetHashCode() == peer.GetHashCode())
                {
                    return ident;
                }
            }


            throw new ArgumentException("Identity not found for requested peer(?)");
            //ENetIdentity newIdent = new ENetIdentity(peer);
            //_peers.Add(newIdent, peer);
            //return newIdent;
        }
    }
}