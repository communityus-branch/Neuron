using System;
using System.Collections.Generic;
using System.Linq;
using Static_Interface.API.EventFramework;
using Static_Interface.API.LevelFramework;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.PlayerFramework.Events;
using Static_Interface.API.SerializationFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework.Impl.Lidgren;
using Static_Interface.Internal.MultiplayerFramework.MultiplayerProvider;
using Static_Interface.Neuron;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

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

        public ServerInfo CurrentServerInfo { get; protected set; }
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
                Send(ServerID, EPacket.TICK, new byte[] { }, 0, 0);
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
                ((ClientMultiplayerProvider)Provider).CloseTicket();
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

            ((ClientMultiplayerProvider)Provider).AttemptConnect(ip, port, password);
        }


        internal void Connect(ServerInfo info)
        {
            ThreadPool.Instance.RunOnMainThread(delegate
            {
                if (IsConnected) return;
                ClientID = ((ClientMultiplayerProvider)Provider).GetUserID();
                LogUtils.Debug("Connected to server: " + info.Name);
                CurrentServerInfo = info;
                ServerID = info.ServerID;
                IsConnected = true;
                _pings = new float[4];
                Lag((info.Ping) / 1000f);
                LastNet = Time.realtimeSinceStartup;
                OffsetNet = 0f;

                Send(ServerID, EPacket.WORKSHOP, new byte[] { }, 0, 0);
                //Todo: Load Level specified by server
                LevelManager.Instance.LoadLevel(info.Map, false, true);
            });
        }

        private bool _levelLoadedCalled;
        protected override void OnLevelWasLoaded(int level)
        {
            base.OnLevelWasLoaded(level);
            //skip first OnLevelWasLoaded since its the loading scene
            if (_levelLoadedCalled) return;
            _levelLoadedCalled = true;

            ulong group = 1; //Todo
            object[] args = { ClientName, group, GameInfo.VERSION, CurrentServerInfo.Ping / 1000f };
            byte[] packet = ObjectSerializer.GetBytes(0, args);
            Send(ServerID, EPacket.CONNECT, packet, packet.Length, 0);
        }

        private void Lag(float currentPing)
        {
            NetworkUtils.GetAveragePing(currentPing, out _ping, _pings);
        }

        protected override Transform AddPlayer(Identity ident, string playerName, ulong @group, Vector3 point, Vector3 angle, int channel, bool mainPlayer)
        {
            LogUtils.Debug("AddPlayer: new player \"" + playerName + "\" with ch id " + channel);
            var playerTransform = base.AddPlayer(ident, playerName, @group, point, angle, channel, mainPlayer);
            if (!mainPlayer)
            {
                LogUtils.Debug("Adding foreign player: " + ident);
                ((ClientMultiplayerProvider)Provider).SetPlayedWith(ident);
            }
            else
            {
                SetupMainPlayer(playerTransform);
            }
            playerTransform.BroadcastMessage("OnPlayerLoaded");
            if (!IsSinglePlayer)
            {
                //If singleplayer (local server) we will already fire this event at ServerConnection
                PlayerJoinEvent @event = new PlayerJoinEvent(ident.Owner.Player);
                EventManager.Instance.CallEvent(@event);
            }

            return playerTransform;
        }

        public static void SetupMainPlayer(Transform playerTransform)
        {
            if (playerTransform == null) throw new ArgumentNullException(nameof(playerTransform));
            LogUtils.Debug("Setting up main player");
            if (Player.MainPlayer != null)
            {
                DestroyImmediate(Player.MainPlayer.gameObject);
            }

            Player.MainPlayer = playerTransform.GetComponent<Player>();

            playerTransform.GetComponent<Channel>().IsOwner = true;
            playerTransform.gameObject.AddComponent<SmoothMouseLook>();
            var sunShafts = playerTransform.GetComponentInChildren<SunShafts>();
            if (sunShafts != null && GameObject.Find("Sun_Moon") != null)
            {
                sunShafts.sunTransform = GameObject.Find("Sun_Moon").transform;
            }

            LogUtils.Debug("Setting up Camera");
            var cam = playerTransform.FindChild("MainCamera");

            cam.tag = "MainCamera";
            CameraManager.Instance.CurrentCamera = cam.GetComponent<Camera>();

            LogUtils.Debug("Loading WeatherParticles");
            var fallLeaves = ((GameObject)Resources.Load("ParticleEffects/FallLeaves"));
            var lightningBugs = ((GameObject)Resources.Load("ParticleEffects/LightningBugs"));
            var lightningPosition = ((GameObject)Resources.Load("ParticleEffects/LightningPosition"));
            var rain = ((GameObject)Resources.Load("ParticleEffects/Rain"));
            var rainMist = ((GameObject)Resources.Load("ParticleEffects/RainMist"));
            var rainStreaks = (GameObject)Resources.Load("ParticleEffects/RainStreaks");
            var snow = ((GameObject)Resources.Load("ParticleEffects/Snow"));
            var snowDust = ((GameObject)Resources.Load("ParticleEffects/SnowDust"));

            fallLeaves = Instantiate(fallLeaves);
            lightningBugs = Instantiate(lightningBugs);
            lightningPosition = Instantiate(lightningPosition);
            rain = Instantiate(rain);
            rainMist = Instantiate(rainMist);
            rainStreaks = Instantiate(rainStreaks);
            snow = Instantiate(snow);
            snowDust = Instantiate(snowDust);

            var rotation = Quaternion.Euler(new Vector3(-90, 0, 0));

            fallLeaves.transform.SetParent(playerTransform);
            fallLeaves.transform.localPosition = new Vector3(0, 46, 0);
            fallLeaves.transform.localRotation = rotation;

            lightningBugs.transform.SetParent(playerTransform);
            lightningBugs.transform.localPosition = new Vector3(0, 7.8f, 0);
            lightningBugs.transform.localRotation = rotation;

            lightningPosition.transform.SetParent(playerTransform);
            lightningPosition.transform.localPosition = new Vector3(-2, 27, 5);
            lightningPosition.transform.localRotation = Quaternion.identity;

            rain.transform.SetParent(playerTransform);
            rain.transform.localPosition = new Vector3(0, 30, 0);
            rain.transform.localRotation = rotation;

            rainMist.transform.SetParent(playerTransform);
            rainMist.transform.localPosition = new Vector3(0, 37, 0);
            rainMist.transform.localRotation = rotation;

            rainStreaks.transform.SetParent(playerTransform);
            rainStreaks.transform.localPosition = new Vector3(0, 180, 0);
            rainStreaks.transform.localRotation = Quaternion.identity;

            snow.transform.SetParent(playerTransform);
            snow.transform.localPosition = new Vector3(0, 25, 0);
            snow.transform.localRotation = rotation;

            snowDust.transform.SetParent(playerTransform);
            snowDust.transform.localPosition = new Vector3(0, 37, 0);
            snowDust.transform.localRotation = rotation;


            LogUtils.Debug("Loading WeatherSystem");
            try
            {
                var weather = World.Instance.Weather.GetComponentInChildren<UniStormWeatherSystem_C>();
                weather.butterflies = lightningBugs.GetComponent<ParticleSystem>();
                weather.windyLeaves = fallLeaves.GetComponent<ParticleSystem>();
                weather.mistFog = rainStreaks;
                weather.snowMistFog = snowDust.GetComponent<ParticleSystem>();
                weather.snow = snow.GetComponent<ParticleSystem>();
                weather.rainMist = rainMist.GetComponent<ParticleSystem>();
                weather.rain = rain.GetComponent<ParticleSystem>();
                weather.lightningSpawn = lightningPosition.transform;
                weather.cameraObject = CameraManager.Instance.UnistormCamera;
                weather.cameraObjectComponent = CameraManager.Instance.UnistormCamera.GetComponent<Camera>();
                weather.rainSplashes = rain.transform.FindChild("Splashes").GetComponent<ParticleSystem>();
            }
            catch (Exception e)
            {
                LogUtils.LogError("Couldn't load weather");
                LogUtils.Debug(e.ToString());
            }

            cam = CameraManager.Instance.UnistormCamera.transform;
            var worldAxle = World.Sun_Moon.transform.FindChild("WorldAxle");
            var sun = worldAxle.FindChild("WorldAxle").FindChild("Sun");
            cam.gameObject.GetComponents<SunShafts>()[0].enabled = true;
            cam.gameObject.GetComponents<SunShafts>()[0].sunTransform = sun;

            var moon = worldAxle.FindChild("WorldAxle").FindChild("Moon");
            cam.gameObject.GetComponents<SunShafts>()[1].enabled = true;
            cam.gameObject.GetComponents<SunShafts>()[1].sunTransform = moon;
        }

        protected override void OnChannelCountUpdate()
        {
            LogUtils.Log("ChannelCount Updated: " + ChannelCount);
        }

        internal override void Receive(Identity id, byte[] packet, int size, int channel)
        {
            base.Receive(id, packet, size, channel);
            EPacket parsedPacket = (EPacket)packet[0];

            StripPacketByte(ref packet, ref size);

            if (parsedPacket.IsUpdate())
            {
                foreach (Channel ch in Receivers.Where(ch => ch.ID == channel))
                {
                    ch.Receive(id, packet, 0, size);
                }
                return;
            }

            if (id != ServerID) return;
            switch (parsedPacket)
            {
                case EPacket.WORKSHOP:
                    //todo
                    break;
                case EPacket.TICK:
                    {
                        var data = ObjectSerializer.GetBytes(0, Time.realtimeSinceStartup);
                        Send(ServerID, EPacket.TIME, data, 0);
                        break;
                    }
                case EPacket.TIME:
                    {
                        object[] args = ObjectSerializer.GetObjects(0, 0, packet, typeof(float));
                        LastNet = Time.realtimeSinceStartup;
                        OffsetNet = ((float)args[0]) + ((Time.realtimeSinceStartup - LastPing) / 2f);
                        Lag(Time.realtimeSinceStartup - LastPing);
                        break;
                    }
                case EPacket.SHUTDOWN:
                    Disconnect();
                    break;

                case EPacket.CONNECTED:
                {
                    {
                        Type[] argTypes =
                        {
                            //[0] id, [1] name, [2] group, [3] position, [4], angle, [5] channel
                            typeof (Identity), typeof (string), typeof (ulong), typeof (Vector3), typeof (Vector3),
                            typeof (int), typeof (bool)
                        };

                        object[] args = ObjectSerializer.GetObjects(0, 0, packet, argTypes);
                        if (IsSinglePlayer) return;
                        if (World.Loaded)
                        {
                            AddPlayer(Provider.Deserialilze((Identity) args[0]), (string) args[1], (ulong) args[2],
                                (Vector3) args[3], (Vector3) args[4], (int) args[5], (bool) args[6]);
                        }
                        else
                        {
                            QueuePlayer(Provider.Deserialilze((Identity) args[0]), (string) args[1], (ulong) args[2],
                                (Vector3) args[3], (Vector3) args[4], (int) args[5], (bool) args[6]);
                        }
                        break;
                    }
                }
                case EPacket.VERIFY:
                    LogUtils.Debug("Opening ticket");
                    byte[] ticket = ((ClientMultiplayerProvider)Provider).OpenTicket();
                    if (ticket == null)
                    {
                        LogUtils.Debug("ticket equals null");
                        Disconnect();
                        break;
                    }
                    Send(ServerID, EPacket.AUTHENTICATE, ticket, ticket.Length, 0);
                    break;
                case EPacket.DISCONNECTED:
                {
                    //If singleplayer (local server) we will already do this at ServerConnection
                    if (IsSinglePlayer) return;
                    object[] args = ObjectSerializer.GetObjects(0, 0, packet, typeof (byte));
                    var index = (byte)args[0];

                    var user = GetUser(index);

                    PlayerQuitEvent @event = new PlayerQuitEvent(user.Player);
                    EventManager.Instance.CallEvent(@event);

                    RemovePlayer(index);
                    break;
                }
                case EPacket.REJECTED:
                case EPacket.KICKED:
                    Disconnect();
                    break;
                case EPacket.ACCEPTED:
                    {
                        object[] args = ObjectSerializer.GetObjects(0, 0, packet, typeof(ulong), typeof(int));
                        LogUtils.Debug("Setting MainPlayer channel to: " + (int)args[1]);
                        ((ClientMultiplayerProvider)Provider).SetIdentity((ulong)args[0]);
                        ((ClientMultiplayerProvider)Provider).AdvertiseGame(ServerID, _currentIp, _currentPort);
                        ((ClientMultiplayerProvider)Provider).SetConnectInfo(_currentIp, _currentPort);
                        IsFavoritedServer = ((ClientMultiplayerProvider)Provider).IsFavoritedServer(_currentIp, _currentPort);
                        ((ClientMultiplayerProvider)Provider).FavoriteServer(_currentIp, _currentPort);
                        break;
                    }
                case EPacket.UPDATE_CHANNELS:
                {
                    object[] args = ObjectSerializer.GetObjects(0, 0, packet, ChannelCount.GetType());
                    ChannelCount = (int) args[0];
                    break;
                }
                default:
                    LogUtils.LogWarning("Couldn't handle packet: " + parsedPacket);
                    break;
            }
        }

        private readonly List<QueuedPlayer> _players = new List<QueuedPlayer>();
        private void QueuePlayer(Identity identity, string playerName, ulong group, Vector3 pos, Vector3 rotation, int channel, bool mainplayer)
        {
            QueuedPlayer player = new QueuedPlayer
            {
                Identity = identity,
                Name = playerName,
                Group = @group,
                Pos = pos,
                Rotation = rotation,
                Channel = channel,
                IsMainPlayer = mainplayer
            };
            _players.Add(player);
        }

        private void OnWorldInit(World world)
        {
            foreach (QueuedPlayer player in _players)
            {
                AddPlayer(player.Identity, player.Name, player.Group, player.Pos, player.Rotation, player.Channel,
                    player.IsMainPlayer);
            }
            _players.Clear();
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

        private struct QueuedPlayer
        {
            public Identity Identity { get; set; }
            public String Name { get; set; }
            public ulong Group { get; set; }
            public Vector3 Pos { get; set; }
            public Vector3 Rotation { get; set; }
            public int Channel { get; set; }
            public bool IsMainPlayer { get; set; }
        }
    }
}