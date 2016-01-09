using System;
using System.Collections.Generic;
using System.Text;
using Static_Interface.Multiplayer.Server;
using Static_Interface.Objects;
using Static_Interface.Utils;
using Steamworks;
namespace Static_Interface.Multiplayer.Client
{
    public class ClientConnection : Connection
    {
        private float[] _pings;
        private float _ping;
        public const int CONNECTION_TRIES = 5;
        private string _clientName;
        private CSteamID _user;
        private uint _time;
        private int _serverQueryAttempts;
        private readonly ISteamMatchmakingPingResponse _serverPingResponse;
        private HServerQuery _serverQuery = HServerQuery.Invalid;
        private static byte[] _clientHash;
        private string _currentPassword;
        private uint _currentIp;
        private ushort _currentPort;
        private ServerInfo _currentServerInfo;
        private bool isLoading;

        public static byte[] ClientHash
        {
            get { return _clientHash;  }
        }

        public ClientConnection(CSteamID id) : base(id)
        {
            _serverPingResponse = new ISteamMatchmakingPingResponse(OnPingResponded, OnPingFailedToRespond);
        }

        public override void Send(CSteamID receiver, EPacket type, byte[] data, int length, int id)
        {
            throw new NotImplementedException();
        }

        protected override void Listen()
        {
            throw new NotImplementedException();
        }

        public override void Disconnect()
        {
            throw new NotImplementedException();
        }

        protected override void OnAwake()
        {
            if (SteamAPI.RestartAppIfNecessary((AppId_t)Game.ID))
            {
                throw new Exception("Restarting app from Steam.");
            }
            if (!SteamAPI.Init())
            {
                throw new Exception("Steam API initialization failed.");
            }

            SteamAPIWarningMessageHook_t _apiWarningMessageHook = OnAPIWarningMessage;
            SteamUtils.SetWarningMessageHook(_apiWarningMessageHook);
            _time = SteamUtils.GetServerRealTime();
            Callback<PersonaStateChange_t>.Create(OnPersonaStateChange);
            Callback<GameServerChangeRequested_t>.Create(OnGameServerChangeRequested);
            Callback<GameRichPresenceJoinRequested_t>.Create(OnGameRichPresenceJoinRequested);
            _user = SteamUser.GetSteamID();
            ClientIDInternal = _user;
            _clientHash = Hash.SHA1(ClientID);
            _clientName = SteamFriends.GetPersonaName();
        }

        private void OnAPIWarningMessage(int severity, StringBuilder warning)
        {
            Console.Instance.Print("Warning: " + warning + " (Severity: " + severity + ")");
        }

        private void OnPersonaStateChange(PersonaStateChange_t callback)
        {
            if ((callback.m_nChangeFlags == EPersonaChange.k_EPersonaChangeName) && (callback.m_ulSteamID == ClientID.m_SteamID))
            {
                _clientName = SteamFriends.GetPersonaName();
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

        public static bool TryGetConnect(string line, out uint ip, out ushort port, out string pass)
        {
            ip = 0;
            port = 0;
            pass = string.Empty;
            var index = line.ToLower().IndexOf("+connect", StringComparison.Ordinal);
            if (index == -1)
            {
                return false;
            }
            var num2 = line.IndexOf(':', index + 9);
            string str = line.Substring(index + 9, (num2 - index) - 9);
            if (CheckIp(str))
            {
                ip = GetUInt32FromIp(str);
            }
            else if (!uint.TryParse(str, out ip))
            {
                return false;
            }
            var num3 = line.IndexOf(' ', num2 + 1);
            if (num3 == -1)
            {
                if (!ushort.TryParse(line.Substring(num2 + 1, (line.Length - num2) - 1), out port))
                {
                    return false;
                }
                var pwIndex = line.ToLower().IndexOf("+password", StringComparison.Ordinal);
                if (pwIndex != -1)
                {
                    pass = line.Substring(pwIndex + 10, (line.Length - pwIndex) - 10);
                }
                return true;
            }
            if (!ushort.TryParse(line.Substring(num2 + 1, (num3 - num2) - 1), out port))
            {
                return false;
            }
            var passwordIndex = line.ToLower().IndexOf("+password", StringComparison.Ordinal);
            if (passwordIndex != -1)
            {
                pass = line.Substring(passwordIndex + 10, (line.Length - passwordIndex) - 10);
            }
            return true;
        }

        private static bool CheckIp(string ip)
        {
            int index = ip.IndexOf('.');
            if (index == -1)
            {
                return false;
            }
            int num2 = ip.IndexOf('.', index + 1);
            if (num2 == -1)
            {
                return false;
            }
            int num3 = ip.IndexOf('.', num2 + 1);
            if (num3 == -1)
            {
                return false;
            }
            if (ip.IndexOf('.', num3 + 1) != -1)
            {
                return false;
            }
            return true;
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


        public void AttemptConnect(uint ip, ushort port, string password)
        {
            if (!IsConnected) return;
            _serverQueryAttempts = 0;
            CleanupServerQuery();
            
            _currentIp = ip;
            _currentPort = port;
            _currentPassword = password;
  
            _serverQuery = SteamMatchmakingServers.PingServer(ip, (ushort)(port + 1), this._serverPingResponse);
            _serverQueryAttempts++;
            //Todo: OnConnect event?
            IsConnectedInternal = true;
        }

        private void CleanupServerQuery()
        {
            if (_serverQuery == HServerQuery.Invalid) return;
            SteamMatchmakingServers.CancelServerQuery(_serverQuery);
            _serverQuery = HServerQuery.Invalid;
        }

        private void OnPingResponded(gameserveritem_t data)
        {
            CleanupServerQuery();
            if (data.m_nAppID == Game.ID)
            {
                ServerInfo info = new ServerInfo(data);

                if (!data.m_bPassword || (_currentPassword != string.Empty))
                {
                    if (((info.Players >= info.MaxPlayers) || (info.MaxPlayers < GameServerProvider.MIN_PLAYERS)) ||
                        (info.MaxPlayers > GameServerProvider.MAX_PLAYERS)) return;
                    Connect(info);
                    return;
                    // Todo: server full
                }
                else
                {
                    // Todo: password
                }
            }
            else
            {
                CleanupServerQuery();
                //Todo: Timeout
            }
        }


        private void OnPingFailedToRespond()
        {
            if (_serverQueryAttempts < CONNECTION_TRIES)
            {
                AttemptConnect(_currentIp, _currentPort, _currentPassword);
            }
            else
            {
                CleanupServerQuery();
                //Todo: Timeout
            }
        }

        private void Connect(ServerInfo info)
        {
            if (IsConnected) return;
            _currentServerInfo = info;
            IsConnectedInternal = true;
            ResetChannels();
            ServerID = info.SteamID;
            _pings = new float[4];
            Lag((info.Ping) / 1000f);
            isLoading = true;
            //LoadingUI.UpdateScene();
            //Level.Loading();
        }

        private void Lag(float value)
        {
            _ping = value;
            for (var i = _pings.Length - 1; i > 0; i--)
            {
                _pings[i] = _pings[i - 1];
                if (_pings[i] > 0.001f)
                {
                    _ping += _pings[i];
                }
            }
            _ping /= _pings.Length;
            _pings[0] = value;
        }
    }
}