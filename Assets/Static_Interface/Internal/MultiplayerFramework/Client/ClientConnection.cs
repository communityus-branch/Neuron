using System;
using System.Collections.Generic;
using System.Linq;
using Static_Interface.API.LevelFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.Internal.MultiplayerFramework.Service.MultiplayerProviderService;
using Static_Interface.Internal.Objects;
using Static_Interface.The_Collapse;
using Steamworks;
using UnityEngine;
using Types = Static_Interface.Internal.Objects.Types;
using Time = UnityEngine.Time;

namespace Static_Interface.Internal.MultiplayerFramework.Client
{
    public class ClientConnection : Connection
    {
        private float[] _pings;
        private float _ping;
        public const int CONNECTION_TRIES = 5;
        private CSteamID _user;
 
        private int _serverQueryAttempts;
        private ISteamMatchmakingPingResponse _serverPingResponse;
        private HServerQuery _serverQuery = HServerQuery.Invalid;
        private string _currentPassword;
        private uint _currentIp;
        private ushort _currentPort;

        public ServerInfo CurrentServerInfo { get; private set; }
        public bool IsFavoritedServer { get; private set; }
        public static byte[] ClientHash { get; private set; }

        internal override void Listen()
        {
            if (((Time.realtimeSinceStartup - LastNet) > CLIENT_TIMEOUT))
            {
                LogUtils.Log("Timeout occurred");
                //Disconnect(); //Timeout
            }
            else if (((Time.realtimeSinceStartup - LastCheck) > CHECKRATE) && (((Time.realtimeSinceStartup - LastPing) > 1f) || (LastPing < 0f)))
            {
                LastCheck = Time.realtimeSinceStartup;
                LastPing = Time.realtimeSinceStartup;
                Send(ServerID, EPacket.TICK, new byte[] {}, 0, 0);
            }

            Send(ServerID, EPacket.TICK, new byte[] {}, 0, 0);
        }

        public override void Disconnect(string reason = null)
        {
            SteamNetworking.CloseP2PSessionWithUser(ServerID);
            foreach(User user in Clients)
            {
                SteamNetworking.CloseP2PSessionWithUser(user.Identity.ID);
            }

            CloseTicket();
            IsConnected = false;

            //Todo: OnDisconnectedFromServer()
            LevelManager.Instance.GoToMainMenu();

            SteamFriends.SetRichPresence("connect", null);
            SteamFriends.SetRichPresence("status", "Menu");
            ((ClientMultiplayerProvider)Provider).CurrentServer = null;
            Destroy(this);
        }


        internal override void Awake()
        {
            base.Awake();
            Provider = new ClientMultiplayerProvider();
            _serverPingResponse = new ISteamMatchmakingPingResponse(OnPingResponded, OnPingFailedToRespond);

            if (SteamAPI.RestartAppIfNecessary(GameInfo.ID))
            {
                throw new Exception("Restarting app from Steam.");
            }
            if (!SteamAPI.Init())
            {
                throw new Exception("Steam API initialization failed.");
            }

            SteamUtils.SetWarningMessageHook(OnAPIWarningMessage);
            CurrentTime = SteamUtils.GetServerRealTime();
            Callback<PersonaStateChange_t>.Create(OnPersonaStateChange);
            Callback<GameServerChangeRequested_t>.Create(OnGameServerChangeRequested);
            Callback<GameRichPresenceJoinRequested_t>.Create(OnGameRichPresenceJoinRequested);
            _user = Steamworks.SteamUser.GetSteamID();
            ClientID = _user;
            ClientHash = Hash.SHA1(ClientID);
            ClientName = SteamFriends.GetPersonaName();
            IsReady = true;
        }

        private void OnPersonaStateChange(PersonaStateChange_t callback)
        {
            if ((callback.m_nChangeFlags == EPersonaChange.k_EPersonaChangeName) && (callback.m_ulSteamID == ClientID.m_SteamID))
            {
                ClientName = SteamFriends.GetPersonaName();
                //Todo: OnNameChangeEvent
            }
        }

        private void OnGameServerChangeRequested(GameServerChangeRequested_t callback)
        {
            if (!IsConnected)
            {
                //Todo 
            }
        }

        private void OnGameRichPresenceJoinRequested(GameRichPresenceJoinRequested_t callback)
        {
            uint ip;
            ushort port;
            string password;
            if (!IsConnected && TryGetConnect(callback.m_rgchConnect, out ip, out port, out password))
            {
                AttemptConnect(ip, port, password);
            }
        }

        private static bool TryGetConnect(string line, out uint ip, out ushort port, out string pass)
        {
            ip = 0;
            port = 0;
            pass = string.Empty;
            return true; //TODO
        }

        public static uint GetUInt32FromIp(string ip)
        {
            string[] strArray = GetComponentsFromSerial(ip, '.');
            return ((((uint.Parse(strArray[0]) << 0x18) | (uint.Parse(strArray[1]) << 0x10)) | (uint.Parse(strArray[2]) << 8)) | uint.Parse(strArray[3]));
        }

        public static string[] GetComponentsFromSerial(string serial, char delimiter)
        {
            int index;
            List<string> list = new List<string>();
            for (int i = 0; i < serial.Length; i = index + 1)
            {
                index = serial.IndexOf(delimiter, i);
                if (index == -1)
                {
                    list.Add(serial.Substring(i, serial.Length - i));
                    break;
                }
                list.Add(serial.Substring(i, index - i));
            }
            return list.ToArray();
        }


        public void AttemptConnect(string ipRaw, ushort port, string password)
        {
            LogUtils.Log("Attempting conncetion to " + ipRaw + ":" + port + " (using password: " + (string.IsNullOrEmpty(password) ? "NO" : "YES") + ")");
            AttemptConnect(GetUInt32FromIp(ipRaw), port, password);
        }

        private void AttemptConnect(uint ip, ushort port, string password)
        {
            if (IsConnected)
            {
                LogUtils.Debug("Already connnected");
                return;
            }
            _serverQueryAttempts = 0;
            CleanupServerQuery();

            _currentIp = ip;
            _currentPort = port;
            _currentPassword = password;

            _serverQuery = SteamMatchmakingServers.PingServer(ip, (ushort)(port + 1), _serverPingResponse);
            //Todo: OnConnect event?
        }

        private void CleanupServerQuery()
        {
            if (_serverQuery == HServerQuery.Invalid) return;
            SteamMatchmakingServers.CancelServerQuery(_serverQuery);
            _serverQuery = HServerQuery.Invalid;
        }

        private void OnPingResponded(gameserveritem_t data)
        {
            LogUtils.Log("Server is up, connecting...");
            CleanupServerQuery();
            if ((AppId_t)data.m_nAppID == GameInfo.ID)
            {
                ServerInfo info = new ServerInfo(data);

                if (!data.m_bPassword || (_currentPassword != string.Empty))
                {
                    if (((info.Players >= info.MaxPlayers) || (info.MaxPlayers < MultiplayerProvider.MIN_PLAYERS)) ||
                        (info.MaxPlayers > MultiplayerProvider.MAX_PLAYERS)) return;
                    Connect(info);
                    return;
                    // Todo: server full
                }
                // Todo: no password
            }
            else
            {
                CleanupServerQuery();
                LogUtils.Log("Wrong game ID received: " + data.m_nAppID + ", expected: " + GameInfo.ID);
                //Todo: Timeout
            }
        }


        private void OnPingFailedToRespond()
        {
            LogUtils.Error("Connection failed");
            if (_serverQueryAttempts < CONNECTION_TRIES)
            {
                _serverQueryAttempts++;
                LogUtils.Log("Retrying #" + _serverQueryAttempts);
                AttemptConnect(_currentIp, _currentPort, _currentPassword);
            }
            else
            {
                LogUtils.Error("Couldn't connect to host");
                CleanupServerQuery();
                LevelManager.Instance.GoToMainMenu();
                //Todo: Timeout
            }
        }

        private void Connect(ServerInfo info)
        {
            if (IsConnected) return;
            LogUtils.Debug("Connected to server: " + info.Name);
            ((ClientMultiplayerProvider) Provider).CurrentServer = info;
            IsConnected = true;
            ResetChannels();
            CurrentServerInfo = info;
            ServerID = info.SteamID;
            _pings = new float[4];
            Lag((info.Ping) / 1000f);
            LastNet = Time.realtimeSinceStartup;
            OffsetNet = 0f;

            Send(ServerID, EPacket.WORKSHOP, new byte[] { }, 0, 0);
            //Todo: Load Level specified by server
            LevelManager.Instance.LoadLevel("DefaultMap");    
        }

        //Todo
        private void OnLevelLoaded()
        {
            int size;
            const string serverPasswordHash = "";
            CSteamID group = CSteamID.Nil;

            object[] args = { ClientName, serverPasswordHash, GameInfo.VERSION, CurrentServerInfo.Ping / 1000f, group};
            byte[] packet = ObjectSerializer.GetBytes(0, out size, args);
            Send(ServerID, EPacket.CONNECT, packet, size, 0);
        }

        private void Lag(float currentPing)
        {
            NetworkUtils.GetAveragePing(currentPing, out _ping, _pings);
        }

        protected override Transform AddPlayer(UserIdentity ident, Vector3 point, byte angle, int channel)
        {
            if (ident.ID != ClientID)
            {
                SteamFriends.SetPlayedWith(ident.ID);
            }
            return base.AddPlayer(ident, point, angle, channel);
        }

        internal override void Receive(CSteamID id, byte[] packet, int offset, int size, int channel)
        {
            base.Receive(id, packet, offset, size, channel);
            EPacket parsedPacket = (EPacket) packet[offset];
            if (parsedPacket.IsUpdate())
            {
                foreach (Channel ch in Receivers.Where(ch => ch.ID == channel))
                {
                    ch.Receive(id, packet, offset, size);
                    return;
                }
            }
            else if (id == ServerID)
            {
                uint ip;
                ushort port;
                switch (parsedPacket)
                {
                    case EPacket.TICK:
                        {
                            Send(ServerID, EPacket.TIME, new byte[] { }, 0, 0);
                            return;
                        }
                    case EPacket.TIME:
                        if (LastPing > 0f)
                        {
                            Type[] argTypes = { Types.BYTE_TYPE, Types.SINGLE_TYPE };
                            object[] args = ObjectSerializer.GetObjects(id, offset, 0, packet, argTypes);
                            LastNet = Time.realtimeSinceStartup;
                            OffsetNet = ((float)args[1]) + ((Time.realtimeSinceStartup - LastPing) / 2f);
                            Lag(Time.realtimeSinceStartup - LastPing);
                            LastPing = -1f;
                        }
                        return;

                    case EPacket.SHUTDOWN:
                        Disconnect();
                        return;

                    case EPacket.CONNECTED:
                        {
                            Type[] argTypes = {
                                //[0] package id, [1] steamID, [2] name, [3] group, [4] position, [5], angle, [6] channel
                                Types.STRING_TYPE, Types.STEAM_ID_TYPE, Types.STRING_TYPE, Types.VECTOR3_TYPE, Types.BYTE_TYPE, Types.INT32_TYPE
                            };

                            object[] args = ObjectSerializer.GetObjects(id, offset, 0, packet, argTypes);
                            AddPlayer(new UserIdentity((CSteamID)args[1], (string) args[2], (CSteamID)args[3]), (Vector3)args[4], (byte)args[5], (int)args[6]);
                            return;
                        }
                    case EPacket.VERIFY:
                        byte[] ticket = OpenTicket();
                        if (ticket == null)
                        {
                            Disconnect();
                            return;
                        }
                        Send(ServerID, EPacket.AUTHENTICATE, ticket, ticket.Length, 0);
                        break;
                    case EPacket.DISCONNECTED:
                        RemovePlayer(packet[offset + 1]);
                        return;
                    default:
                    {
                        if (parsedPacket != EPacket.ACCEPTED)
                        {
                            if (parsedPacket != EPacket.REJECTED)
                            {
                                //Todo: handle reason
                                Disconnect();
                            }

                            return;
                        }
                        Type[] args = {Types.BYTE_TYPE, Types.UINT32_TYPE, Types.UINT16_TYPE};
                        object[] objects = ObjectSerializer.GetObjects(id, offset, 0, packet, args);
                        ip = (uint) objects[1];
                        port = (ushort) objects[2];

                        //Todo: OnConnectedToServer

                        Steamworks.SteamUser.AdvertiseGame(ServerID, ip, port);

                        //Todo: implement a command line parser
                        SteamFriends.SetRichPresence("connect", string.Concat("+connect ", ip, ":", port));
                        var favoriteIP = ip;
                        var favoritePort = port;
                        IsFavoritedServer = false;
                        for (var game = 0; game < SteamMatchmaking.GetFavoriteGameCount(); game++)
                        {
                            AppId_t appIdT;
                            uint pnIp;
                            ushort connPort;
                            ushort pnQueryPort;
                            uint punFlags;
                            uint lastPlayedOnServer;
                            SteamMatchmaking.GetFavoriteGame(game, out appIdT, out pnIp, out connPort, out pnQueryPort,
                                out punFlags, out lastPlayedOnServer);
                            if (((appIdT != GameInfo.ID) || (pnIp != favoriteIP)) ||
                                (favoritePort != connPort)) continue;
                            IsFavoritedServer = true;
                            break;
                        }
                        SteamMatchmaking.AddFavoriteGame(GameInfo.ID, ip, port, (ushort) (port + 1), 2,
                            SteamUtils.GetServerRealTime());
                        break;
                    }
                }
            }
        }
        private HAuthTicket _ticketHandle = HAuthTicket.Invalid;
        private byte[] OpenTicket()
        {
            uint size;
            if (_ticketHandle != HAuthTicket.Invalid)
            {
                return null;
            }
            byte[] pTicket = new byte[1024];
            _ticketHandle = Steamworks.SteamUser.GetAuthSessionTicket(pTicket, pTicket.Length, out size);
            if (size == 0)
            {
                return null;
            }
            byte[] dst = new byte[size];
            System.Buffer.BlockCopy(pTicket, 0, dst, 0, (int)size);
            return dst;
        }

        private void CloseTicket()
        {
            if (_ticketHandle == HAuthTicket.Invalid) return;
            Steamworks.SteamUser.CancelAuthTicket(_ticketHandle);
            _ticketHandle = HAuthTicket.Invalid;
        }
    }
}