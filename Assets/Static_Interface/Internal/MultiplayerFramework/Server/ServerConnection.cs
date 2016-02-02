using System;
using System.Collections.Generic;
using System.Linq;
using Static_Interface.API.Level;
using Static_Interface.API.Network;
using Static_Interface.API.Player;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework.Impl.Steamworks;
using Static_Interface.Internal.MultiplayerFramework.MultiplayerProvider;
using Static_Interface.Internal.Objects;
using Static_Interface.The_Collapse;
using UnityEngine;
using Types = Static_Interface.Internal.Objects.Types;

namespace Static_Interface.Internal.MultiplayerFramework.Server
{
    public class ServerConnection : Connection
    {
        public string Map { get; } = "DefaultMap";

        public uint BindIP { get; } = 0;

        public int MaxPlayers = 8;

        private const float Timeout = 0.75f;

        public ushort Port { get; private set; }

        public uint PublicIP => ((ServerMultiplayerProvider)Provider).GetPublicIP();

        private readonly List<PendingUser> _pendingPlayers = new List<PendingUser>();
        public ICollection<PendingUser> PendingPlayers => _pendingPlayers.AsReadOnly();

        public bool IsSecure { get; internal set; }

        internal override void Listen()
        {
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
            CloseGameServer();
            Destroy(this);
        }

        internal override void Receive(Identity source, byte[] packet, int offset, int size, int channel)
        {
            base.Receive(source, packet, offset, size, channel);
            var net = ((OffsetNet + Time.realtimeSinceStartup) - LastNet);

            EPacket parsedPacket = (EPacket)packet[offset];

            if (parsedPacket.IsUpdate())
            {
                if (source == ServerID)
                {
                    foreach(Channel ch in Receivers)
                    {
                       ch.Receive(source, packet, offset, size);
                    }
                }
                else
                {
                    if (Clients.All(client => client.Identity != source)) return;
                    foreach (Channel ch in Receivers)
                    {
                        ch.Receive(source, packet, offset, size);
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
                    List<ulong> list = new List<ulong>();

                    byte[] args = new byte[1 + (list.Count * 8)];
                    args[0] = (byte)list.Count;
                    for (byte i = 0; i < list.Count; i = (byte)(i + 1))
                    {
                        BitConverter.GetBytes(list[i]).CopyTo(args, (1 + (i * 8)));
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

                    Type[] argTypes = {
                        //[0] package, [1] name, [2] group, [3] version, [4] point, [5], angle, [6] channel
                        Types.BYTE_TYPE, Types.STRING_TYPE, Types.UINT64_TYPE, Types.STRING_TYPE, Types.VECTOR3_TYPE, Types.BYTE_TYPE, Types.INT32_TYPE
                    };

                    var args = ObjectSerializer.GetObjects(source, offset, 0, packet, argTypes);
                    var name = (string) args[1];
                    var group = (ulong) args[3];

					LogUtils.Log("Player connecting: " + name);
                    if (((string)args[4]) != GameInfo.VERSION)
                    {
                        Reject(source, ERejectionReason.WRONG_VERSION);
                        return;
                    }

                    if ((Clients.Count + 1) > MultiplayerProvider.MultiplayerProvider.MAX_PLAYERS)
                    {
                        Reject(source, ERejectionReason.SERVER_FULL);
                        return;
                    }

                    _pendingPlayers.Add(new PendingUser(source, name, group));
                    Send(source, EPacket.VERIFY, new byte[] { }, 0, 0);
                    return;
                }

                default:
                    if (parsedPacket != EPacket.AUTHENTICATE)
                    {
                        LogUtils.Error("Failed to handle message: " + parsedPacket);
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
                object[] args = ObjectSerializer.GetObjects(source, offset, 0, packet, Types.BYTE_TYPE,
                    Types.BYTE_ARRAY_TYPE);
                if (!((ServerMultiplayerProvider)Provider).VerifyTicket(source, (byte[])args[1]))
                {
                    Reject(source, ERejectionReason.AUTH_VERIFICATION);
                }
            }
        }

        public void Reject(Identity user, ERejectionReason reason)
        {
            foreach (var player in _pendingPlayers.Where(player => player.Identity == user))
            {
                PendingPlayers.Remove(player);
            }

            ((ServerMultiplayerProvider) Provider).EndAuthSession(user);

            byte[] data = {(byte)reason};
            Send(user, EPacket.REJECTED, data, data.Length, 0);
        }

        public void DisconnectClient(Identity user)
        {
            byte index = GetUserIndex(user);
            RemovePlayer(index);
            byte[] packet = { index };
            AnnounceToAll(EPacket.DISCONNECTED, packet, packet.Length, 0);
            Provider.CloseConnection(user);
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

        internal override void Awake()
        {
            base.Awake();
            Port = 27015;
            IsReady = true;
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

            int channels = Channels;
            Transform player = AddPlayer(ident, user.Name, user.Group, spawn, angle, channels);
            object[] data;
            byte[] packet;
            foreach (var c in Clients)
            {
                data = new object[] { c.Identity, c.Name, c.Group, c.Model.position, c.Model.rotation.eulerAngles.y / 2f };
                packet = ObjectSerializer.GetBytes(0, out size, data);
                Send(user.Identity, EPacket.CONNECTED, packet, data.Length, 0);
            }

            object[] objects = { PublicIP, Port };
            packet = ObjectSerializer.GetBytes(0, out size, objects);
            Send(ident, EPacket.ACCEPTED, packet, size, 0);
            data = new object[] { ident.Serialize(), user.Name, user.Group, player.position, player.rotation.eulerAngles.y / 2f };

            packet = ObjectSerializer.GetBytes(0, out size, data);
            AnnounceToAll(EPacket.CONNECTED, packet, size, 0);
            //Todo: OnUserConnectedEvent
        }

        public void OpenGameServer(bool lan = false)
        {
            if(Provider == null) Provider = new SteamworksServerProvider(this);
            try
            {
                ((ServerMultiplayerProvider)Provider).Open(BindIP, Port, lan);
            }
            catch (Exception exception)
            {
                exception.Log();
                Application.Quit();
                return;
            }

            SetupPseudoChannel();
            IsConnected = true;

            CurrentTime = ((ServerMultiplayerProvider)Provider).GetServerRealTime();
            ((ServerMultiplayerProvider)Provider).SetMaxPlayerCount(MaxPlayers);
            ((ServerMultiplayerProvider)Provider).SetServerName(((ServerMultiplayerProvider)Provider).Description);
            ((ServerMultiplayerProvider)Provider).SetPasswordProtected(false); //Todo
            ((ServerMultiplayerProvider)Provider).SetMapName(Map);
            LevelManager.Instance.LoadLevel(Map); //Todo


            ServerID = ((ServerMultiplayerProvider)Provider).GetServerIdentity();
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