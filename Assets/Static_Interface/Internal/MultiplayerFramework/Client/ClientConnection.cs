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

namespace Static_Interface.Internal.MultiplayerFramework.Client
{
    public class ClientConnection : Connection
    {
        private float[] _pings;
        private float _ping;
        public const int CONNECTION_TRIES = 5;
        private Identity _user;
 
        private int _serverQueryAttempts;

        internal string CurrentPassword;
        private uint _currentIp;
        private ushort _currentPort;

        public ServerInfo CurrentServerInfo { get; private set; }
        public bool IsFavoritedServer { get; private set; }

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
            Provider.CloseConnection(ServerID);
            foreach(User user in Clients)
            {
                Provider.CloseConnection(user.Identity);
            }

            ((ClientMultiplayerProvider)Provider).CloseTicket();
            IsConnected = false;

            //Todo: OnDisconnectedFromServer()
            LevelManager.Instance.GoToMainMenu();

            ((ClientMultiplayerProvider) Provider).SetStatus("Menu");
            ((ClientMultiplayerProvider)Provider).SetConnectInfo(0, 0);
            
            ((SteamsworksClientProvider)Provider).CurrentServer = null;
            Destroy(this);
        }


        internal override void Awake()
        {
            base.Awake();
            Provider = new SteamsworksClientProvider(this);
            CurrentTime = Provider.GetServerRealTime();
            _user = ((ClientMultiplayerProvider) Provider).GetUserID();
            ClientID = _user;
            ClientName = ((ClientMultiplayerProvider) Provider).GetClientName();
            IsReady = true;
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

        public void AttemptConnect(uint ip, ushort port, string password)
        {
            if (IsConnected)
            {
                LogUtils.Debug("Already connnected");
                return;
            }
            _serverQueryAttempts = 0;

            _currentIp = ip;
            _currentPort = port;
            CurrentPassword = password;

            ((ClientMultiplayerProvider) Provider).AttemptConnect(ip, port, password);
        }

        internal void Connect(ServerInfo info)
        {
            if (IsConnected) return;
            LogUtils.Debug("Connected to server: " + info.Name);
            ((SteamsworksClientProvider) Provider).CurrentServer = info;
            IsConnected = true;
            ResetChannels();
            CurrentServerInfo = info;
            ServerID = info.ServerID;
            _pings = new float[4];
            Lag((info.Ping) / 1000f);
            LastNet = Time.realtimeSinceStartup;
            OffsetNet = 0f;
            SetupPseudoChannel();
            Send(ServerID, EPacket.WORKSHOP, new byte[] { }, 0, 0);
            //Todo: Load Level specified by server
            LevelManager.Instance.LoadLevel("DefaultMap");    
        }

        //Todo
        private void OnLevelLoaded()
        {
            int size;
            const string serverPasswordHash = "";
            ulong group = 0;

            object[] args = { ClientName, serverPasswordHash, GameInfo.VERSION, CurrentServerInfo.Ping / 1000f, group};
            byte[] packet = ObjectSerializer.GetBytes(0, out size, args);
            Send(ServerID, EPacket.CONNECT, packet, size, 0);
        }

        private void Lag(float currentPing)
        {
            NetworkUtils.GetAveragePing(currentPing, out _ping, _pings);
        }

        protected override Transform AddPlayer(Identity ident, string @name, ulong group, Vector3 point, byte angle, int channel)
        {
            if (ident != ClientID)
            {
                ((ClientMultiplayerProvider)Provider).SetPlayedWith(ident);
            }
            return base.AddPlayer(ident, @name, group, point, angle, channel);
        }

        internal override void Receive(Identity id, byte[] packet, int offset, int size, int channel)
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
                                Types.BYTE_TYPE, Types.STRING_TYPE, Types.STEAM_ID_TYPE, Types.STRING_TYPE, Types.VECTOR3_TYPE, Types.BYTE_TYPE, Types.INT32_TYPE
                            };

                            object[] args = ObjectSerializer.GetObjects(id, offset, 0, packet, argTypes);
                            var name = (string) args[2];
                            AddPlayer(id, name, (ulong)args[3], (Vector3)args[4], (byte)args[5], (int)args[6]);
                            return;
                        }
                    case EPacket.VERIFY:
                        byte[] ticket = ((ClientMultiplayerProvider)Provider).OpenTicket();
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

                        ((ClientMultiplayerProvider) Provider).AdvertiseGame(ServerID, ip, port);
                       
                        //Todo: implement a command line parser
                        ((ClientMultiplayerProvider)Provider).SetConnectInfo(ip,port);
                        IsFavoritedServer = ((ClientMultiplayerProvider)Provider).IsFavoritedServer(ip, port);
                        ((ClientMultiplayerProvider) Provider).FavoriteServer(ip, port);
                        break;
                    }
                }
            }
        }

        public bool OnPingFailed()
        {
            if (_serverQueryAttempts >= CONNECTION_TRIES)
            {
                return false;
            }
            _serverQueryAttempts++;
            LogUtils.Log("Retrying #" + _serverQueryAttempts);
            AttemptConnect(_currentIp, _currentPort, CurrentPassword);
            return true;
        }
    }
}