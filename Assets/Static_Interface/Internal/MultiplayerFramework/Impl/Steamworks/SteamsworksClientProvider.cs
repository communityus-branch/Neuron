using System;
using Static_Interface.Neuron;
using Static_Interface.API.Level;
using Static_Interface.API.Network;
using Static_Interface.API.Player;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework.Client;
using Static_Interface.Internal.MultiplayerFramework.MultiplayerProvider;
using Steamworks;
using SteamUser = Steamworks.SteamUser;

namespace Static_Interface.Internal.MultiplayerFramework.Impl.Steamworks
{
    public class SteamsworksClientProvider : ClientMultiplayerProvider
    {
        public bool IsAttempting;
        private readonly ISteamMatchmakingPingResponse _serverPingResponse;
        private HServerQuery _serverQuery = HServerQuery.Invalid;

        public ServerInfo CurrentServer { get; internal set; }

        public SteamsworksClientProvider(Connection conn) : base(conn) 
        {
            Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);
            Callback<P2PSessionConnectFail_t>.Create(OnP2PSessionConnectFail);
            SteamUtils.SetWarningMessageHook(OnAPIWarningMessage);
            _serverPingResponse = new ISteamMatchmakingPingResponse(OnPingResponded, OnPingFailedToRespond);
            Callback<PersonaStateChange_t>.Create(OnPersonaStateChange);
            Callback<GameServerChangeRequested_t>.Create(OnGameServerChangeRequested);
            Callback<GameRichPresenceJoinRequested_t>.Create(OnGameRichPresenceJoinRequested);
        }


        private void OnGameServerChangeRequested(GameServerChangeRequested_t callback)
        {
            if (!Connection.IsConnected)
            {
                //Todo 
            }
        }

        private void OnGameRichPresenceJoinRequested(GameRichPresenceJoinRequested_t callback)
        {
            uint ip;
            ushort port;
            string password;
            if (!Connection.IsConnected && TryGetConnect(callback.m_rgchConnect, out ip, out port, out password))
            {
                //((ClientConnection)Connection).AttemptConnect(ip, port, password);
                //Todo
            }
        }

        private static bool TryGetConnect(string line, out uint ip, out ushort port, out string pass)
        {
            ip = 0;
            port = 0;
            pass = String.Empty;
            return true; //TODO
        }

        private void CleanupServerQuery()
        {
            if (_serverQuery == HServerQuery.Invalid) return;
            SteamMatchmakingServers.CancelServerQuery(_serverQuery);
            _serverQuery = HServerQuery.Invalid;
        }


        public void Connect(ServerInfo serverInfo)
        {
            CurrentServer = serverInfo;
        }

        public void Disconnect()
        {
            // Todo
        }

        private void OnP2PSessionConnectFail(P2PSessionConnectFail_t callback)
        {
            LogUtils.Error("P2P connection failed for: " + callback.m_steamIDRemote + ", error: " + callback.m_eP2PSessionError);
        }


        public void OnP2PSessionRequest(P2PSessionRequest_t callback)
        {
            if (!SteamNetworking.AcceptP2PSessionWithUser(callback.m_steamIDRemote))
            {
                LogUtils.Debug("Failde to accept P2P Request: " + callback.m_steamIDRemote);
            }
            else
            {
                LogUtils.Debug("Accepted P2P Request: " + callback.m_steamIDRemote);
            }
        }

        public override bool Read(out Identity user, byte[] data, out ulong length, int channel)
        {
            uint num;
            user = null;
            CSteamID id;
            length = 0L;
            if (!SteamNetworking.IsP2PPacketAvailable(out num, channel) || (num > data.Length))
            {
                LogUtils.Debug("No P2P Packet available on channel " + channel);
                return false;
            }
            if (!SteamNetworking.ReadP2PPacket(data, num, out num, out id, channel))
            {
                LogUtils.Debug("P2P Packet reading failed on chnanel " + channel);
                return false;
            }
            length = num;
            user = (SteamIdentity) id;
            return true;
        }

        public override bool Write(Identity target, byte[] data, ulong length, SendMethod method, int channel)
        {
            LogUtils.Debug("Writing with method: " + method + " in channel " + channel);
            return SteamNetworking.SendP2PPacket((CSteamID)(SteamIdentity)target, data, (uint)length, (EP2PSend)method, channel);
        }

        public override void CloseConnection(Identity user)
        {
            SteamNetworking.CloseP2PSessionWithUser((CSteamID) (SteamIdentity) user);
        }

        private void OnPersonaStateChange(PersonaStateChange_t callback)
        {
            if ((callback.m_nChangeFlags == EPersonaChange.k_EPersonaChangeName) && (callback.m_ulSteamID == ((SteamIdentity)Connection.ClientID).SteamID.m_SteamID))
            {
                Connection.ClientName = SteamFriends.GetPersonaName();
                //Todo: OnNameChangeEvent
            }
        }

        private void OnPingFailedToRespond()
        {
            LogUtils.Error("Connection failed");
            if (!((ClientConnection) Connection).OnPingFailed())
            {
                LogUtils.Error("Couldn't connect to host");
                CleanupServerQuery();
                LevelManager.Instance.GoToMainMenu();
                //Todo: Timeout
            }
        }


        public override Identity GetUserID()
        {
            return (SteamIdentity) SteamUser.GetSteamID();
        }

        public override void AdvertiseGame(Identity serverID, string ip, ushort port)
        {
            SteamUser.AdvertiseGame((CSteamID)(SteamIdentity)serverID, SteamworksCommon.GetUInt32FromIp(ip), port);
        }

        public override void SetPlayedWith(Identity ident)
        {
            SteamFriends.SetPlayedWith((CSteamID)(SteamIdentity)ident);
        }

        public override void AttemptConnect(string ip, ushort port, string password)
        {
            CleanupServerQuery();
            _serverQuery = SteamMatchmakingServers.PingServer(SteamworksCommon.GetUInt32FromIp(ip), (ushort)(port + 1), _serverPingResponse);
        }

        public override void SetStatus(string status)
        {
            SteamFriends.SetRichPresence("status", status);
        }

        public override void SetConnectInfo(string ip, ushort port)
        {
            if (ip == null && port == 0)
            {
                SteamFriends.SetRichPresence("connect", null);
                return;
            }
            SteamFriends.SetRichPresence("connect", string.Concat("+connect ", ip, ":", port));
        }

        public override bool IsFavoritedServer(string ip, ushort port)
        {
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
                if (((appIdT != GameInfo.ID) || (pnIp != SteamworksCommon.GetUInt32FromIp(ip))) ||
                    (port != connPort)) continue;
                return true;
            }
            return false;
        }

        public override string GetClientName()
        {
            return SteamFriends.GetPersonaName();
        }

        public override uint GetServerRealTime()
        {
            return SteamUtils.GetServerRealTime();
        }

        private void OnPingResponded(gameserveritem_t data)
        {
            LogUtils.Log("Server is up, connecting...");
            CleanupServerQuery();
            if ((AppId_t)data.m_nAppID == GameInfo.ID)
            {
                ServerInfo info = new ServerInfo
                {
                    ServerID = (SteamIdentity)data.m_steamID,
                    Name = data.GetServerName(),
                    Map = data.GetMap(),
                    Ping = data.m_nPing,
                    Players = data.m_nPlayers,
                    MaxPlayers = data.m_nMaxPlayers,
                    HasPassword = data.m_bPassword,
                    IsSecure = data.m_bSecure,
                };

                if (!data.m_bPassword || (((ClientConnection)Connection).CurrentPassword != String.Empty))
                {
                    if (((info.Players >= info.MaxPlayers) || (info.MaxPlayers < MIN_PLAYERS)) ||
                        (info.MaxPlayers > MAX_PLAYERS)) return;
                    ((ClientConnection)Connection).Connect(info);
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

        private HAuthTicket _ticketHandle = HAuthTicket.Invalid;
        public override byte[] OpenTicket()
        {
            uint size;
            if (_ticketHandle != HAuthTicket.Invalid)
            {
                return null;
            }
            byte[] pTicket = new byte[1024];
            _ticketHandle = SteamUser.GetAuthSessionTicket(pTicket, pTicket.Length, out size);
            if (size == 0)
            {
                return null;
            }
            byte[] dst = new byte[size];
            System.Buffer.BlockCopy(pTicket, 0, dst, 0, (int)size);
            return dst;
        }

        public override void CloseTicket()
        {
            if (_ticketHandle == HAuthTicket.Invalid) return;
            SteamUser.CancelAuthTicket(_ticketHandle);
            _ticketHandle = HAuthTicket.Invalid;
        }

        public override void FavoriteServer(string ip, ushort port)
        {
            SteamMatchmaking.AddFavoriteGame(GameInfo.ID, SteamworksCommon.GetUInt32FromIp(ip), port, (ushort)(port + 1), 2,
                            GetServerRealTime());
        }

        public override void SetIdentity(ulong serializedIdent)
        {
            //not needed
        }
    }
}