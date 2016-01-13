using System.Collections.Generic;
using System.Text;
using Static_Interface.Multiplayer.Protocol;
using Static_Interface.Objects;
using Static_Interface.Multiplayer.Service.ConnectionProviderService;
using Static_Interface.PlayerFramework;
using Steamworks;
using SteamUser = Static_Interface.PlayerFramework.SteamUser;
using UnityEngine;

namespace Static_Interface.Multiplayer
{
    public abstract class Connection : MonoBehaviour
    {
        public const float CHECKRATE = 1f;
        public static readonly int CLIENT_TIMEOUT = 30;
        public static readonly int SERVER_TIMEOUT = 30;
        public static readonly int PENDING_TIMEOUT = 30;
        public static Connection CurrentConnection;

        protected byte[] Buffer  = new byte[Block.BUFFER_SIZE];

        private CSteamID _serverId;
        public CSteamID ServerID
        {
            get { return _serverId;}
            protected set { _serverId = value; }
        }

        protected float LastPing;
        protected float LastNet;
        protected float LastCheck;
        protected float OffsetNet;

        private string _clientName;

        public string ClientName
        {
            get { return _clientName;}
            protected set { _clientName = value; }
        }
        private uint _currentTime;
        public uint CurrentTime
        {
            get { return _currentTime; }
            protected set { _currentTime = value; }
        }
        public abstract MultiplayerProvider Provider { get; }

        private CSteamID _clientId;
        public CSteamID ClientID
        {
            get { return _clientId; }
            internal set { _clientId = value; }
        }

        private int _channels = 1;

        public int Channels
        {
            get { return _channels; }
        }

        private List<User> _clients;
        private static bool _isInitialized;

        protected bool IsConnectedInternal;
        public bool IsConnected
        {
            get { return IsConnectedInternal; } 
        }

        protected void OnAPIWarningMessage(int severity, StringBuilder warning)
        {
            Console.Instance.Print("Warning: " + warning + " (Severity: " + severity + ")");
        }

        public ICollection<User> Clients
        {
            get { return _clients == null ? null : _clients.AsReadOnly(); }
        }

        protected Connection()
        {
            CurrentConnection = this;
        }

        private void Awake()
        {
            if (_isInitialized)
            {
                Destroy(this);
                return;
            }

            _isInitialized = true;
            DontDestroyOnLoad(this);
            OnAwake();
            SteamFriends.SetRichPresence("status", "Menu");
        }

        protected abstract void OnAwake();
        protected abstract void Receive(CSteamID source, byte[] packet, int offset, int size, int channel);
        private static List<Channel> _receivers;
        public static ICollection<Channel> Receivers
        {
            get
            {
                return _receivers == null ? null : _receivers.AsReadOnly();
            }
        }

        protected void AddReceiver(Channel ch)
        {
            _receivers.Add(ch);
            _channels++;
        }

        protected void ResetChannels()
        {
            _channels = 1;
            _receivers = new List<Channel>();
            var channelArray = FindObjectsOfType<Channel>();
            foreach (var ch in channelArray)
            {
                OpenChannel(ch);
            }
            _clients = new List<User>();
            //pending = new List<SteamPending>();
        }


        public void Update()
        {
            if (!IsConnected || !_isInitialized) return;
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

        protected abstract void Listen();

        protected virtual Transform AddPlayer(UserIdentity ident, Vector3 point, byte angle, int channel)
        {
            Transform newModel = ((GameObject)Instantiate(Resources.Load("Player"), point, Quaternion.Euler(0f, (angle * 2), 0f))).transform;
            _clients.Add(new SteamUser(this, ident, newModel, channel));
            return newModel;
            //Todo!! Add Player prefab with "Channel" and "Player" components
            //Todo: OnPlayerConnected
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
            }
            else if (type.IsUnreliable())
            {
                if (!SteamNetworking.SendP2PPacket(receiver, data, (uint)length, !type.IsInstant() ? EP2PSend.k_EP2PSendUnreliable : EP2PSend.k_EP2PSendUnreliableNoDelay, id))
                {
                    Debug.LogError("Failed to send UDP packet to " + receiver + "!");
                }
            }
            else if (!SteamNetworking.SendP2PPacket(receiver, data, (uint)length, !type.IsInstant() ? EP2PSend.k_EP2PSendReliableWithBuffering : EP2PSend.k_EP2PSendReliable, id))
            {
                Debug.LogError("Failed to send TCP packet to " + receiver + "!");
            }
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
            _channels++;
        }

        internal void CloseChannel(Channel ch)
        {
            for (var i = 0; i < Receivers.Count; i++)
            {
                if (_receivers[i].ID != ch.ID) continue;
                _receivers.RemoveAt(i);
                return;
            }
            _channels--;
        }
    }
}