using System;
using System;
using System.Linq;
using Plugins.ConsoleUI.FrontEnd.UnityGUI;
using Static_Interface.API.LevelFramework;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework.Impl.Lidgren;
using Static_Interface.Internal.MultiplayerFramework.MultiplayerProvider;
using Static_Interface.Internal.Objects;
using Static_Interface.Neuron;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Types = Static_Interface.Internal.Objects.Types;

namespace Static_Interface.Internal.MultiplayerFramework.Client
{
    public class ClientConnection : Connection
    {
        private float[] _pings;
        private float _ping;
        public const int CONNECTION_TRIES = 5;
        private int _serverQueryAttempts;

        internal string CurrentPassword;
        private string _currentIp;
        private ushort _currentPort;

        public ServerInfo CurrentServerInfo { get; private set; }
        public bool IsFavoritedServer { get; private set; }

        internal override void Listen()
        {
            if (Provider == null || Provider.SupportsPing) return;
            if (((Time.realtimeSinceStartup - LastNet) > CLIENT_TIMEOUT))
            {
                LogUtils.Log("Timeout occurred");
                Disconnect(); //Timeout
            }
            else if (((Time.realtimeSinceStartup - LastCheck) > CHECKRATE) && (((Time.realtimeSinceStartup - LastPing) > CHECKRATE) || (LastPing <= 0f)))
            {
                LastCheck = Time.realtimeSinceStartup;
                LastPing = Time.realtimeSinceStartup;
                Send(ServerID, EPacket.TICK, new byte[] {}, 0, 0);
            }
        }

        public override void Disconnect(string reason = null)
        {
            IsConnecting = false;
            LevelManager.Instance.GoToMainMenu();
            Dispose();
        }

        public override void Dispose()
        {
            LogUtils.Debug(nameof(Dispose));
            Provider.CloseConnection(ServerID);
            foreach (User user in Clients)
            {
                Provider.CloseConnection(user.Identity);
            }
            if (Provider.SupportsAuthentification)
            {
                ((ClientMultiplayerProvider) Provider).CloseTicket();
            }
            IsConnected = false;

            //Todo: OnDisconnectedFromServer()

            ((ClientMultiplayerProvider)Provider).SetStatus("Menu");
            ((ClientMultiplayerProvider)Provider).SetConnectInfo(null, 0);
            Provider.Dispose();
            Destroy(this);
        }

        public void AttemptConnect(string ip, ushort port, string password, bool reset = true)
        {
            IsConnecting = true;
            Provider = new LidgrenClient(this);
            ClientID = ((ClientMultiplayerProvider)Provider).GetUserID();
            ClientName = ((ClientMultiplayerProvider)Provider).GetClientName();
            CurrentTime = Provider.GetServerRealTime();
            LogUtils.Log("Attempting conncetion to " + ip + ":" + port + " (using password: " + (string.IsNullOrEmpty(password) ? "NO" : "YES") + ")");
            if (IsConnected)
            {
                LogUtils.Debug("Already connnected to a server");
                return;
            }

            if (reset)
            {
                _serverQueryAttempts = 0;
            }

            _currentIp = ip;
            _currentPort = port;
            CurrentPassword = password;

            ((ClientMultiplayerProvider) Provider).AttemptConnect(ip, port, password);
        }

        internal void Connect(ServerInfo info)
        {
            ThreadPool.RunOnMainThread(delegate
            {
                if (IsConnected) return;
                ClientID = ((ClientMultiplayerProvider) Provider).GetUserID();
                LogUtils.Debug("Connected to server: " + info.Name);
                ResetChannels();
                CurrentServerInfo = info;
                ServerID = info.ServerID;
                IsConnected = true;
                _pings = new float[4];
                Lag((info.Ping)/1000f);
                LastNet = Time.realtimeSinceStartup;
                OffsetNet = 0f;

                Send(ServerID, EPacket.WORKSHOP, new byte[] {}, 0, 0);
                //Todo: Load Level specified by server
                LevelManager.Instance.LoadLevel(info.Map);
            });
        }

        protected override void OnLevelWasLoaded(int level)
        {
            base.OnLevelWasLoaded(level);
            int size;
            ulong group = 1; //Todo
            object[] args = { ClientName, group, GameInfo.VERSION, CurrentServerInfo.Ping / 1000f};
            byte[] packet = ObjectSerializer.GetBytes(0, out size, args);
            Send(ServerID, EPacket.CONNECT, packet, size, 0);
        }

        private void Lag(float currentPing)
        {
            NetworkUtils.GetAveragePing(currentPing, out _ping, _pings);
        }

        protected override Transform AddPlayer(Identity ident, string @name, ulong group, Vector3 point, byte angle, int channel)
        {
            var playerTransform = base.AddPlayer(ident, @name, group, point, angle, channel);;
            if (ident.Serialize() != ClientID.Serialize())
            {
                ((ClientMultiplayerProvider) Provider).SetPlayedWith(ident);
            }
            else
            {
                SetupMainPlayer(playerTransform);
            }
            return playerTransform;
        }

        public static void SetupMainPlayer(Transform playerTransform)
        {
            if (Player.MainPlayer != null)
            {
                var comp = Player.MainPlayer.Model.GetComponent<AudioListener>();
                if (comp != null) DestroyImmediate(comp);
            }
            LogUtils.Debug("Setting up main player");
            Player.MainPlayer = playerTransform.GetComponent<Player>();
            if (Camera.current != null && Camera.current.enabled)
            {
                Camera.current.enabled = false;
            }

            playerTransform.gameObject.AddComponent<MouseLook>();

            LogUtils.Debug("Setting console character");
            GameObject.Find("Console").GetComponent<ConsoleGUI>().Character = playerTransform.gameObject;

            if(playerTransform.gameObject.GetComponent<AudioListener>() == null)
                playerTransform.gameObject.AddComponent<AudioListener>();

            LogUtils.Debug("Setting up Camera");
            var cam = playerTransform.FindChild("MainCamera");
            var sunshafts = cam.gameObject.AddComponent<SunShafts>();
            cam.tag = "MainCamera";
            cam.GetComponent<Camera>().enabled = true;
            LogUtils.Debug("Loading WeatherParticles");
            var fallLeaves = ((GameObject)Resources.Load("ParticleEffects/FallLeaves")).GetComponent<ParticleSystem>();
            var lightningBugs = ((GameObject)Resources.Load("ParticleEffects/LightningBugs")).GetComponent<ParticleSystem>();
            var lightningPosition = ((GameObject)Resources.Load("ParticleEffects/LightningPosition")).transform;
            var rain = ((GameObject)Resources.Load("ParticleEffects/Rain")).GetComponent<ParticleSystem>(); 
            var rainMist = ((GameObject)Resources.Load("ParticleEffects/RainMist")).GetComponent<ParticleSystem>();
            var rainStreaks = (GameObject) Resources.Load("ParticleEffects/RainStreaks");
            var snow = ((GameObject)Resources.Load("ParticleEffects/Snow")).GetComponent<ParticleSystem>(); 
            var snowDust = ((GameObject)Resources.Load("ParticleEffects/SnowDust")).GetComponent<ParticleSystem>();

            LogUtils.Debug("Loading WeatherGetComponents");
            var weatherController = cam.gameObject.AddComponent<GetUniStormComponents_C>();
            weatherController.windyLeaves = fallLeaves;
            weatherController.lightningBugs = lightningBugs;
            weatherController.lightningPosition = lightningPosition;
            weatherController.rain = rain;
            weatherController.rainSplash = rainMist;
            weatherController.rainMist = rainMist;
            weatherController.rainStreaks = rainStreaks;
            weatherController.snow = snow;
            weatherController.snowDust = snowDust;
            weatherController.unistormCamera = cam.gameObject;

            LogUtils.Debug("Loading WeatherSystem");
            var weather = World.Instance.Weather.GetComponentInChildren<UniStormWeatherSystem_C>();
            GameObject garbage= new GameObject();
            var light = garbage.AddComponent<Light>();
            weather.butterflies = fallLeaves;
            weather.moon = light;
            weather.moonLight = light;
            weather.windyLeaves = fallLeaves;
            weather.rain = rain;
            weather.rainMist = rainMist;
            weather.snow = snow;
            weather.snowMistFog = snowDust;
            weather.mistFog = rainStreaks;
            weather.cameraObject = cam.gameObject;
            weather.cameraObjectComponent = cam.GetComponent<Camera>();
            weatherController.unistorm = weather.gameObject;
        }

        internal override void Receive(Identity id, byte[] packet, int size, int channel)
        {
            base.Receive(id, packet,  size, channel);
            EPacket parsedPacket = (EPacket) packet[0];
            StripPacketByte(ref packet, ref size);

            if (parsedPacket.IsUpdate())
            {
                foreach (Channel ch in Receivers.Where(ch => ch.ID == channel))
                {
                    ch.Receive(id, packet, 0, size);
                    return;
                }
            }
            else if (id == ServerID)
            {
                switch (parsedPacket)
                {
                    case EPacket.TICK:
                        {
                            Send(ServerID, EPacket.TIME, new byte[] { }, 0, 0);
                            return;
                        }
                    case EPacket.TIME:
                    {
                        Type[] argTypes = {Types.SINGLE_TYPE};
                        object[] args = ObjectSerializer.GetObjects(id, 0, 0, packet, argTypes);
                        LastNet = Time.realtimeSinceStartup;
                        OffsetNet = ((float) args[0]) + ((Time.realtimeSinceStartup - LastPing)/2f);
                        Lag(Time.realtimeSinceStartup - LastPing);
                        return;
                    }
                    case EPacket.SHUTDOWN:
                        Disconnect();
                        return;

                    case EPacket.CONNECTED:
                        {
                            Type[] argTypes = {
                                //[0] id, [1] name, [2] group, [3] position, [4], angle, [5] channel
                                Types.UINT64_TYPE, Types.STRING_TYPE, Types.UINT64_TYPE, Types.VECTOR3_TYPE, Types.BYTE_TYPE, Types.INT32_TYPE
                            };

                            object[] args = ObjectSerializer.GetObjects(id, 0, 0, packet, argTypes);
                            AddPlayer(id, (string)args[1], (ulong)args[2], (Vector3)args[3], (byte)args[4], (int)args[5]);
                            return;
                        }
                    case EPacket.VERIFY:
                        LogUtils.Debug("Opening ticket");
                        byte[] ticket = ((ClientMultiplayerProvider)Provider).OpenTicket();
                        if (ticket == null)
                        {
                            LogUtils.Debug("ticket equals null");
                            Disconnect();
                            return;
                        }
                        Send(ServerID, EPacket.AUTHENTICATE, ticket, ticket.Length, 0);
                        break;
                    case EPacket.DISCONNECTED:
                        RemovePlayer(packet[1]);
                        return;
                    case EPacket.REJECTED:
                    case EPacket.KICKED:
                        Disconnect();
                        return;
                    case EPacket.ACCEPTED:
                    {
                        object[] args = ObjectSerializer.GetObjects(id, 0, 0, packet, Types.UINT64_TYPE);
                        ((ClientMultiplayerProvider)Provider).SetIdentity((ulong) args[0]);    
                        ((ClientMultiplayerProvider) Provider).AdvertiseGame(ServerID, _currentIp, _currentPort);    
                        ((ClientMultiplayerProvider)Provider).SetConnectInfo(_currentIp, _currentPort);
                        IsFavoritedServer = ((ClientMultiplayerProvider)Provider).IsFavoritedServer(_currentIp, _currentPort);
                        ((ClientMultiplayerProvider) Provider).FavoriteServer(_currentIp, _currentPort);

                        //Todo: load extensions
                        break;
                    }
                    default:
                        LogUtils.LogWarning("Couldn't handle packet: " + parsedPacket);
                        break;
                }
            }
        }

        public bool OnConnectionFailed()
        {
            if (_serverQueryAttempts >= CONNECTION_TRIES)
            {
                IsConnecting = false;
                return false;
            }
            IsConnected = false;
            _serverQueryAttempts++;
            LogUtils.Log("Retrying #" + _serverQueryAttempts);
            AttemptConnect(_currentIp, _currentPort, CurrentPassword, false);
            Provider.Dispose();
            return true;
        }
    }
}