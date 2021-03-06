﻿using System;
using System.Collections.Generic;
using System.Linq;
using Static_Interface.API.EventFramework;
using Static_Interface.API.LevelFramework;
using Static_Interface.API.NetvarFramework;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.PlayerFramework.Events;
using Static_Interface.API.SerializationFramework;
using Static_Interface.API.Utils;
using Static_Interface.API.WeatherFramework;
using Static_Interface.Internal.MultiplayerFramework.Client;
using Static_Interface.Internal.MultiplayerFramework.Impl.Lidgren;
using Static_Interface.Internal.MultiplayerFramework.MultiplayerProvider;
using Static_Interface.Internal.Objects;
using Static_Interface.Neuron;
using UnityEngine;

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
        public SingleplayerConnection SinglePlayerConnection { get; internal set; }

        internal override void Listen()
        {
            if (Provider == null || Provider.SupportsPing) return;

            if ((Time.realtimeSinceStartup - LastCheck) > CHECKRATE)
            {
                LastCheck = Time.realtimeSinceStartup;
                foreach (var c in Clients.Where(c => ((Time.realtimeSinceStartup - c.LastPing) > 1f) || (c.LastPing < 0f)))
                {
                    c.LastPing = Time.realtimeSinceStartup;
                    Send(c.Identity, EPacket.TICK, new byte[] { }, 0, 0);
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

        internal override void Disconnect(string reason, bool unload)
        {
            Dispose();
        }

        public override void Dispose()
        {
            CloseGameServer();
            Destroy(this);
        }

        protected override void OnChannelCountUpdate()
        {
            object[] objects = { ChannelCount };
            byte[] data = ObjectSerializer.GetBytes(0, objects);
            foreach (User u in Clients.Where(u => u.Identity != ServerID))
            {
                Send(u.Identity, EPacket.UPDATE_CHANNELS, data, 0);
            }
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
                    foreach (Channel ch in Receivers.Where(ch => ch.ID == channel))
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
                    object[] objects = { net };
                    byte[] buffer2 = ObjectSerializer.GetBytes(0, objects);
                    Send(source, EPacket.TIME, buffer2, 0);
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
                        typeof(string), typeof(ulong), typeof(string), typeof(float)
                    };
                
                    var args = ObjectSerializer.GetObjects(0, 0, packet, argTypes);
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
                object[] args = ObjectSerializer.GetObjects(0, 0, packet, typeof(byte[]));
                if (!((ServerMultiplayerProvider)Provider).VerifyTicket(source, (byte[])args[0]))
                {
                    Reject(source, ERejectionReason.AUTH_VERIFICATION);
                }
            }
        }

        protected override bool OnPreSend(Identity receiver, EPacket type, byte[] data, int length, int channel)
        {
            if (!IsSinglePlayer || receiver != ServerID) return false;

            SinglePlayerConnection.Receive(ServerID, data, length, channel);
            return true;
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

        public void DisconnectClient(Identity ident, bool sendKicked = true)
        {
            PlayerQuitEvent @event = new PlayerQuitEvent(ident.Owner.Player);
            var user = ident.GetUser();
            @event.QuitMessage = "<b>" + user.Name + "</b> disconnected.";
        
            byte index = GetUserIndex(ident);
            RemovePlayer(index);
            byte[] packet = ObjectSerializer.GetBytes(0, index);
            AnnounceToAll(EPacket.DISCONNECTED, packet, 0);
            if(sendKicked) Send(ident, EPacket.KICKED, new byte[0], 0);
            ((ServerMultiplayerProvider) Provider).RemoveClient(ident);
            
            EventManager.Instance.CallEvent(@event);
            if (string.IsNullOrEmpty(@event.QuitMessage)) return;
            Chat.Instance.SendServerMessage(@event.QuitMessage);
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

        public void AnnounceToAll(EPacket packet, byte[] data, int channel)
        {
            foreach (var c in Clients)
            {
                Send(c.Identity, packet, data, channel);
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

        private readonly List<PendingUser> _queuedUsers = new List<PendingUser>();
        public void Accept(PendingUser user)
        {
            if(user == null) throw new ArgumentNullException(nameof(user));
			LogUtils.Log("Player accepted: " + user.Name);
			if (!user.HasAuthentication) return;
            _pendingPlayers.Remove(user);

            LogUtils.Debug("Loading spawn");
            ((ServerMultiplayerProvider)Provider)?.UpdateScore(user.Identity, 0);

            QueueUser(user);

            if (World.Instance == null)
            {
                return;
            }

            LoadQueuedPlayers(Chat.Instance);
        }

        protected void OnChatInit(Chat chat)
        {
            LoadQueuedPlayers(chat);
        }

        protected void QueueUser(PendingUser user)
        {
            LogUtils.Debug("Queueing user: " + user.Identity);
            _queuedUsers.Add(user);
        }

        public void LoadQueuedPlayers(Chat chat)
        {
            foreach (PendingUser user in _queuedUsers)
            {
                Identity ident = user.Identity;
                Vector3? spawn = World.Instance.DefaultSpawnPosition?.position ?? Vector3.zero;
                LogUtils.Debug("Adding player");
                var angle = new Vector3(0, 90, 0);

                int chCount = ChannelCount;

                Transform player = AddPlayer(ident, user.Name, user.Group, spawn.Value, angle, chCount,
                    user.Identity == ServerID);
                var container = player.GetChild(0);
                var ch = container.GetComponent<Channel>();
                ch.Owner= user.Identity;
                container.BroadcastMessage("OnPlayerLoaded");

                if (!IsDedicated && user.Identity == ServerID)
                {
                    ClientConnection.SetupMainPlayer(player);
                }
                object[] data;
                byte[] packet;

                //Send all connected players to the accepted player
                //[0] id, [1] name, [2] group, [3] position, [4], angle, [5] channel, [6] isSelf
                LogUtils.Debug("Sending connected to all clients");
                foreach (var c in Clients.ToList().Where(c => c.Identity != ident))
                {
                    data = new object[]
                    {
                        user.Identity.Serialize(), user.Name, user.Group, spawn,
                        angle, chCount, false
                    };
                    packet = ObjectSerializer.GetBytes(0, data);
                    Send(c.Identity, EPacket.CONNECTED, packet, 0);
                }

                LogUtils.Debug("Sending connected player data to client");
                foreach (var c in Clients.ToList().Where(c => c.Identity != ident))
                {
                    data = new object[]
                    {
                        c.Identity.Serialize(), c.Name, c.Group, c.Model.transform.position,
                        c.Model.transform.rotation.eulerAngles, c.Player.GetComponent<Channel>().ID, false
                    };
                    packet = ObjectSerializer.GetBytes(0, data);
                    Send(user.Identity, EPacket.CONNECTED, packet, 0);
                }

                LogUtils.Debug("Sending accepted data to client");
                data = new object[]
                {ident.Serialize(), user.Name, user.Group, spawn.Value, angle, chCount, true};
                packet = ObjectSerializer.GetBytes(0, data);
                Send(user.Identity, EPacket.CONNECTED, packet, 0);

                data = new object[] {ident.Serialize(), chCount };
                packet = ObjectSerializer.GetBytes(0, data);
                Send(user.Identity, EPacket.ACCEPTED, packet, 0);

                WeatherManager.Instance.SendWeatherTimeUpdate(ident);
                NetvarManager.Instance.SendAllNetvars(ident);

                PlayerJoinEvent @event = new PlayerJoinEvent(ident.GetUser().Player);
                @event.JoinMessage = "<b>" + user.Name + "</b> connected.";
                EventManager.Instance.CallEvent(@event);
                if (string.IsNullOrEmpty(@event.JoinMessage)) return;
                chat?.SendServerMessage(@event.JoinMessage);
            }
            _queuedUsers.Clear();
        }

        public void OpenGameServer(bool lan = false)
        {
            InternalObjectUtils.CheckObjects();
            if (Provider == null) Provider = new LidgrenServer(this);
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
            LevelManager.Instance.LoadLevel(Map, false, true); //Todo


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