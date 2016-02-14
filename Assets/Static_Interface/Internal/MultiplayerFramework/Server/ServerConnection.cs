using System;
using System.Collections.Generic;
using System.Linq;
using Static_Interface.API.LevelFramework;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework.Impl.Lidgren;
using Static_Interface.Internal.MultiplayerFramework.Impl.Steamworks;
using Static_Interface.Internal.MultiplayerFramework.MultiplayerProvider;
using Static_Interface.Internal.Objects;
using Static_Interface.Neuron;
using UnityEngine;
using Types = Static_Interface.Internal.Objects.Types;

namespace Static_Interface.Internal.MultiplayerFramework.Server
{
    public class ServerConnection : Connection
    {
        public string Map { get; } = "TestMap";

        public int MaxPlayers = 8;

        private const float Timeout = 0.75f;

        public ushort Port { get; } = 27015;

        private readonly List<PendingUser> _pendingPlayers = new List<PendingUser>();
        public ICollection<PendingUser> PendingPlayers => _pendingPlayers.AsReadOnly();

        public bool IsSecure { get; internal set; }

        internal override void Listen()
        {
            if (Provider == null || Provider.SupportsPing) return;

            if ((Time.realtimeSinceStartup - LastCheck) > CHECKRATE)
            {
                LastCheck = Time.realtimeSinceStartup;
                foreach (var c in Clients)
                {
                    if (((Time.realtimeSinceStartup - c.LastPing) > 1f) || (c.LastPing < 0f))
                    {
                        c.LastPing = Time.realtimeSinceStartup;
                        Send(c.Identity, EPacket.TICK, new byte[] { }, 0, 0);
                    }
                }
            }

            foreach (User c in Clients.Where(
                c => ((Time.realtimeSinceStartup - c.LastNet) > SERVER_TIMEOUT) ||
                (((Time.realtimeSinceStartup - c.Joined) > SERVER_TIMEOUT) && (c.LastPing > Timeout))))
            {
                DisconnectClient(c.Identity);
            }

            foreach (PendingUser c in _pendingPlayers.Where(c => 
                (Time.realtimeSinceStartup - c.Joined) > PENDING_TIMEOUT))
            {
                Reject(c.Identity, ERejectionReason.TIMEOUT);
            }
        }

        public override void Disconnect(string reason = null)
        {
            Dispose();
        }

        public override void Dispose()
        {
            CloseGameServer();
            Destroy(this);
        }

        internal override void Receive(Identity source, byte[] packet, int size, int channel)
        {
            base.Receive(source, packet, size, channel);
            var net = ((OffsetNet + Time.realtimeSinceStartup) - LastNet);
            EPacket parsedPacket = (EPacket)packet[0];
            StripPacketByte(ref packet, ref size);

            if (parsedPacket.IsUpdate())
            {
                if (source == ServerID)
                {
                    foreach(Channel ch in Receivers)
                    {
                       ch.Receive(source, packet, 0, size);
                    }
                }
                else
                {
                    if (Clients.All(client => client.Identity != source)) return;
                    foreach (Channel ch in Receivers)
                    {
                        ch.Receive(source, packet, 0, size);
                    }
                }
                return;
            }

            PendingUser currentPending;
            switch (parsedPacket)
            {
                case EPacket.WORKSHOP:
                {
                    //workshop list {none for now}
                    List<ulong> workshoplist = new List<ulong>();

                    byte[] args = new byte[1 + (workshoplist.Count * 8)];
                    args[0] = (byte)workshoplist.Count;
                    for (byte i = 0; i < workshoplist.Count; i = (byte)(i + 1))
                    {
                        BitConverter.GetBytes(workshoplist[i]).CopyTo(args, (1 + (i * 8)));
                    }
                    Send(source, EPacket.WORKSHOP, args, args.Length, 0);
                    return;
                }

                case EPacket.TICK:
                {
                    int length;
                    object[] objects = { net };
                    byte[] buffer2 = ObjectSerializer.GetBytes(0, out length, objects);
                    Send(source, EPacket.TIME, buffer2, length, 0);
                    return;
                }
                case EPacket.TIME:
                    foreach (User c in Clients.Where(c => c.Identity == source))
                    {
                        if (!(c.LastPing > 0f)) return;
                        c.LastNet = Time.realtimeSinceStartup;
                        c.Lag(Time.realtimeSinceStartup - c.LastPing);
                        c.LastPing = -1f;
                        return;
                    }
                    return;

                case EPacket.CONNECT:
                {
                    if (_pendingPlayers.Any(p => p.Identity == source))
                    {
                        Reject(source, ERejectionReason.ALREADY_PENDING);
                        return;
                    }

                    if (Clients.Any(c => c.Identity == source))
                    {
                        Reject(source, ERejectionReason.ALREADY_CONNECTED);
                        return;
                    }

                    Type[] argTypes =
                    {
                        // [0] name, [1] group, [2] version, [3] ping
                        Types.STRING_TYPE, Types.UINT64_TYPE, Types.STRING_TYPE, Types.SINGLE_TYPE
                    };
                
                    var args = ObjectSerializer.GetObjects(source, 0, 0, packet, true, argTypes);
                    var playerName = (string) args[0];
                    var group = (ulong) args[1];
                    var version = (string) args[2];
                    var ping = (float) args[3];

					LogUtils.Log("Player connecting: " + playerName);
                    if (version != GameInfo.VERSION)
                    {
                        Reject(source, ERejectionReason.WRONG_VERSION);
                        return;
                    }

                    if ((Clients.Count + 1) > MultiplayerProvider.MultiplayerProvider.MAX_PLAYERS)
                    {
                        Reject(source, ERejectionReason.SERVER_FULL);
                        return;
                    }
                    var pendingPlayer = new PendingUser(source, playerName, group, ping);
                    _pendingPlayers.Add(pendingPlayer);
                    if (Provider.SupportsAuthentification)
                    {
                        Send(source, EPacket.VERIFY, new byte[] { }, 0, 0);
                        return;
                    }
                    pendingPlayer.HasAuthentication = true;
                    Accept(pendingPlayer);
                    return;
                }

                default:
                    if (parsedPacket != EPacket.AUTHENTICATE)
                    {
                        LogUtils.LogError("Failed to handle message: " + parsedPacket);
                        return;
                    }

                    currentPending = _pendingPlayers.FirstOrDefault(p => p.Identity == source);
                    break;
            }

            if (currentPending == null)
            {
                Reject(source, ERejectionReason.NOT_PENDING);
            }
            else if ((Clients.Count + 1) > MultiplayerProvider.MultiplayerProvider.MAX_PLAYERS)
            {
                Reject(source, ERejectionReason.SERVER_FULL);
            }
            else
            {
                object[] args = ObjectSerializer.GetObjects(source, 0, 0, packet, true, Types.BYTE_ARRAY_TYPE);
                if (!((ServerMultiplayerProvider)Provider).VerifyTicket(source, (byte[])args[0]))
                {
                    Reject(source, ERejectionReason.AUTH_VERIFICATION);
                }
            }
        }

        public void Reject(Identity user, ERejectionReason reason)
        {
            LogUtils.Log("Rejecting user " + user.Serialize() + ", reason: " + reason);
            foreach (var player in _pendingPlayers.Where(player => player.Identity == user))
            {
                _pendingPlayers.Remove(player);
            }

            byte[] data = {(byte)reason};

            Send(user, EPacket.REJECTED, data, data.Length, 0);
        }

        public void DisconnectClient(Identity user)
        {
            Chat.Instance.SendServerMessage("<b>" + user.GetUser().Name+ "</b> disconnected.");
            byte index = GetUserIndex(user);
            RemovePlayer(index);
            byte[] packet = { index };
            AnnounceToAll(EPacket.DISCONNECTED, packet, packet.Length, 0);
            Send(user, EPacket.KICKED, new byte[0], 0);
        }

        public byte GetUserIndex(Identity user)
        {
            byte index = 0;
            foreach (User client in Clients)
            {
                if (client.Identity == user)
                {
                    return index;
                }
                index++;
            }

            throw new Exception("User not found: " + user);
        }

        public void AnnounceToAll(EPacket packet, byte[] data, int size, int channel)
        {
            foreach (var c in Clients)
            {
                Send(c.Identity, packet, data, size, channel);
            }
        }

        public void Accept(Identity ident)
        {
            foreach (PendingUser user in _pendingPlayers.Where(user => user.Identity == ident))
            {
                Accept(user);
                break;
            }
        }

        public void Accept(PendingUser user)
        {
            Identity ident = user.Identity;
			LogUtils.Log("Player accepted: " + user.Name);
			if (!user.HasAuthentication) return;
            _pendingPlayers.Remove(user);
            ((ServerMultiplayerProvider)Provider).UpdateScore(ident, 0);
            Vector3 spawn = Vector3.zero;
            byte angle = 0;
            int size;
            //Todo: savefile

            int ch = Channels;
            Transform player = AddPlayer(ident, user.Name, user.Group, spawn, angle, ch);
            object[] data;
            byte[] packet;
            foreach (var c in Clients)
            {
                data = new object[] { c.Identity.Serialize(), c.Name, c.Group, c.Model.position, (byte)c.Model.rotation.eulerAngles.y / 2f, c.Player.GetComponent<Channel>().ID };
                packet = ObjectSerializer.GetBytes(0, out size, data);
                Send(user.Identity, EPacket.CONNECTED, packet, data.Length, 0);
            }

            data = new object[] { ident.Serialize(), ch};
            packet = ObjectSerializer.GetBytes(0, out size, data);
            Send(user.Identity, EPacket.ACCEPTED, packet, data.Length, 0);

            data = new object[] { ident.Serialize(), user.Name, user.Group, player.position, (byte)player.rotation.eulerAngles.y / 2f, ch};
            packet = ObjectSerializer.GetBytes(0, out size, data);
            player.GetComponent<Channel>().Owner = user.Identity;
            foreach (var c in Clients.Where(c => c.Identity != ident))
            {
                Send(c.Identity, EPacket.CONNECTED, packet, size, 0);
            }
            Chat.Instance.SendServerMessage("<b>" + user.Name + "</b> connected.");
            //Todo: OnUserConnectedEvent
        }

        public void OpenGameServer(bool lan = false)
        {
            if(Provider == null) Provider = new LidgrenServer(this);
            try
            {
                ((ServerMultiplayerProvider)Provider).Open("*", Port, lan);
            }
            catch (Exception exception)
            {
                exception.Log();
                Application.Quit();
                return;
            }

            IsConnected = true;

            CurrentTime = ((ServerMultiplayerProvider)Provider).GetServerRealTime();
            ((ServerMultiplayerProvider)Provider).SetMaxPlayerCount(MaxPlayers);
            ((ServerMultiplayerProvider)Provider).SetServerName(((ServerMultiplayerProvider)Provider).Description);
            ((ServerMultiplayerProvider)Provider).SetPasswordProtected(false); //Todo
            ((ServerMultiplayerProvider)Provider).SetMapName(Map);
            LevelManager.Instance.LoadLevel(Map); //Todo


            ServerID = Provider.GetServerIdent();
            ClientID = ServerID;

            ClientName = "Console";
            LastNet = Time.realtimeSinceStartup;
            OffsetNet = 0f;

            //Todo: OnServerStart
        }

        public void CloseGameServer()
        {
            //Todo: OnServerShutdown
            ((ServerMultiplayerProvider)Provider).Close();
            Application.Quit();
        }
    }
}