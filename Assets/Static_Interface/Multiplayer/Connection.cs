using System.Collections.Generic;
using System.Text;
using Static_Interface.Multiplayer.Protocol;
using Static_Interface.Objects;
using Static_Interface.Multiplayer.Service.MultiplayerProviderService;
using Static_Interface.PlayerFramework;
using Static_Interface.Utils;
using Steamworks;
using SteamUser = Static_Interface.PlayerFramework.SteamUser;
using UnityEngine;

namespace Static_Interface.Multiplayer
{
    public abstract class Connection : MonoBehaviour
    {
        public const float CHECKRATE = 1f;
        public const int CLIENT_TIMEOUT = 30;
        public const int SERVER_TIMEOUT = 30;
        public const int PENDING_TIMEOUT = 30;
        public const float UPDATE_TIME = 0.15f;

        public static bool IsServer()
        {
            return CurrentConnection.Provider is ServerMultiplayerProvider;
        }

        public static bool IsClient()
        {
            return CurrentConnection.Provider is ClientMultiplayerProvider;
        }

        public static Connection CurrentConnection { get; private set; }
        public bool IsReady { get; protected set; }

        protected byte[] Buffer  = new byte[Block.BUFFER_SIZE];

        public CSteamID ServerID { get; protected set; }

        protected float LastPing;
        protected float LastNet;
        protected float LastCheck;
        protected float OffsetNet;

        public string ClientName { get; protected set; }

        public uint CurrentTime { get; protected set; }

        public MultiplayerProvider Provider { get; protected set; }

        public CSteamID ClientID { get; internal set; }

        public int Channels { get; private set; } = 1;

        private List<User> _clients;

        public bool IsConnected { get; protected set; }

        protected void OnAPIWarningMessage(int severity, StringBuilder warning)
        {
            Console.Instance.Print("Warning: " + warning + " (Severity: " + severity + ")");
        }

        public ICollection<User> Clients => _clients?.AsReadOnly();

        internal virtual void Awake()
        {
            Debug.Log("Initializing connection...");
            CurrentConnection = this;
            DontDestroyOnLoad(this);
        }

        protected virtual void OnDestroy()
        {
            if (CurrentConnection == this) CurrentConnection = null;
            Debug.Log("Destroying connection...");
        }

        internal virtual void Receive(CSteamID source, byte[] packet, int offset, int size, int channel)
        {
            if (!IsConnected) return;
            if (packet.Length > 0)
            {
                Debug.Log("Received packet: " + (EPacket) packet[0]);
            }
        }

        private static List<Channel> _receivers;
        public static ICollection<Channel> Receivers => _receivers?.AsReadOnly();

        protected void AddReceiver(Channel ch)
        {
            _receivers.Add(ch);
            Channels++;
        }

        protected void ResetChannels()
        {
            Channels = 1;
            _receivers = new List<Channel>();
            var channelArray = FindObjectsOfType<Channel>();
            foreach (var ch in channelArray)
            {
                OpenChannel(ch);
            }
            _clients = new List<User>();
            //pending = new List<SteamPending>();
        }


        internal virtual void Update()
        {
            if (!IsConnected) return;
            Listen();
            Listen(0);
            foreach (Channel ch in Receivers)
            {
                Listen(ch.ID);
            }
        }

        protected void Listen(int channelId)
        {
            CSteamID user;
            ulong length;
            while (Provider.Read(out user, Buffer, out length, channelId))
            {
                Receive(user, Buffer, 0, (int)length, channelId);
            }
        }

        internal abstract void Listen();

        protected virtual Transform AddPlayer(UserIdentity ident, Vector3 point, byte angle, int channel)
        {
            Transform newModel = ((GameObject)Instantiate(Resources.Load("Player"), point, Quaternion.Euler(0f, (angle * 2), 0f))).transform;
            _clients.Add(new SteamUser(CurrentConnection, ident, newModel, channel));
            return newModel;
        }

        protected void RemovePlayer(byte index)
        {
            if ((index >= _clients.Count))
            {
                Debug.LogError("Failed to find player: " + index);
                return;
            }
            //Todo: on player disconnected event
            Destroy(_clients[index].Model.gameObject);
            _clients.RemoveAt(index);
        }

        public virtual void Send(CSteamID receiver, EPacket type, byte[] data, int length, int id)
        {
            if (!IsConnected) return;
            if (receiver.m_SteamID == 0)
            {
                Debug.LogError("Failed to send to invalid steam ID.");
                return;
            }
            Debug.Log("Sending packet: " + type);
        }

        public abstract void Disconnect(string reason = null);

        internal void OpenChannel(Channel ch)
        {
            if (Receivers == null)
            {
                ResetChannels();
                return;
            }
            _receivers.Add(ch);
            Channels++;
        }

        internal void CloseChannel(Channel ch)
        {
            for (var i = 0; i < Receivers.Count; i++)
            {
                if (_receivers[i].ID != ch.ID) continue;
                _receivers.RemoveAt(i);
                return;
            }
            Channels--;
        }
    }
}