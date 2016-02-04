using System;
using System.Collections.Generic;
using System.Linq;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework.MultiplayerProvider;
using Static_Interface.Internal.Objects;
using UnityEngine;

namespace Static_Interface.Internal.MultiplayerFramework
{
    public abstract class Connection : MonoBehaviour
    {
        public const float CHECKRATE = 1f;
        public const int CLIENT_TIMEOUT = 30;
        public const int SERVER_TIMEOUT = 30;
        public const int PENDING_TIMEOUT = 30;
        public const float UPDATE_TIME = 0.15f;

        private GameObject _zeroChannel;

        public static bool IsServer()
        {
            return CurrentConnection.Provider is ServerMultiplayerProvider;
        }

        public static bool IsClient()
        {
            return CurrentConnection.Provider is ClientMultiplayerProvider;
        }

        public static Connection CurrentConnection { get; private set; }

        protected byte[] Buffer  = new byte[Block.BUFFER_SIZE];

        public Identity ServerID { get; protected set; }

        protected float LastPing;
        protected float LastNet;
        protected float LastCheck;
        protected float OffsetNet;

        public string ClientName { get; protected internal set; }

        public uint CurrentTime { get; protected set; }

        public MultiplayerProvider.MultiplayerProvider Provider { get; protected set; }

        public Identity ClientID { get; internal set; }

        public int Channels { get; private set; } = 1;

        private List<User> _clients = new List<User>();

        public bool IsConnected { get; protected set; }

        public ICollection<User> Clients => _clients?.AsReadOnly();

        internal virtual void Awake()
        {
            LogUtils.Log("Initializing connection...");
            CurrentConnection = this;
            DontDestroyOnLoad(this);
        }

        protected void DestroyPseudoChannel()
        {
            Destroy(_zeroChannel);
        }

        protected virtual void OnDestroy()
        {
            if (CurrentConnection == this) CurrentConnection = null;
            DestroyPseudoChannel();
            LogUtils.Log("Destroying connection...");
        }

        internal virtual void Receive(Identity source, byte[] packet, int offset, int size, int channel)
        {
            //if (!IsConnected) return;
            LogUtils.Debug("Received packet, channel: " + channel + ", size: " + packet.Length);
        }

        private static List<Channel> _receivers = new List<Channel>();
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
            //Listen(0);
            foreach (Channel ch in Receivers)
            {
                Listen(ch.ID);
            }
        }

        protected void Listen(int channelId)
        {
            LogUtils.Log("Listening channel: " + channelId);
            Identity user;
            ulong length;
            while (Provider.Read(out user, Buffer, out length, channelId))
            {
                Receive(user, Buffer, 0, (int)length, channelId);
            }
        }

        protected void SetupPseudoChannel()
        {
            _zeroChannel = new GameObject("ZeroChannel");
            var ch = _zeroChannel.AddComponent<Channel>();
            ch.ID = 0;
            ch.Connection = this;
            ch.Setup();
            AddReceiver(ch);
            DontDestroyOnLoad(_zeroChannel);
        }

        internal abstract void Listen();

        protected virtual Transform AddPlayer(Identity ident, string @name, ulong group, Vector3 point, byte angle, int channel)
        {
            Transform newModel = ((GameObject)Instantiate(Resources.Load("Player"), point, Quaternion.Euler(0f, (angle * 2), 0f))).transform;
            var user = new User(CurrentConnection, ident, newModel, channel) {Group = @group, Name = @name };
            ident.Owner = user;
            newModel.GetComponent<Player>().User = user;
            _clients.Add(user);
            return newModel;
        }

        protected void RemovePlayer(byte index)
        {
            if ((index >= _clients.Count))
            {
                LogUtils.LogError("Failed to find player: " + index);
                return;
            }
            //Todo: on player disconnected event
            Destroy(_clients[index].Model.gameObject);
            _clients.RemoveAt(index);
        }


        public virtual void Send(Identity receiver, EPacket type, byte[] data, int channel)
        {
            Send(receiver, type, data, data.Length, channel);
        }

        public virtual void Send(Identity receiver, EPacket type, byte[] data, int length, int channel)
        {
            if (receiver == null)
            {
                throw new ArgumentNullException(nameof(receiver));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var tmp = data.ToList();
            tmp.Insert(0, type.GetID());
            data = tmp.ToArray();
            length += 1;

            if ((IsClient() && receiver == ClientID && ClientID != null) || (IsServer() && receiver == ServerID && ServerID != null))
            {
                Receive(receiver, data, 0, length, channel);
                return;
            }

            if (!receiver.IsValid())
            {
                LogUtils.LogError("Failed to send to invalid steam ID.");
                return;
            }

            LogUtils.Debug("Sending packet: " + type + ", receiver: " + receiver + (receiver == ServerID ? " (Server)" : "") + ", ch: " + channel + ", size: " + data.Length);

            SendMethod sendType;
            if (type.IsUnreliable())
            {
                sendType = !type.IsInstant()
                    ? SendMethod.SEND_UNRELIABLE
                    : SendMethod.SEND_UNRELIABLE_NO_DELAY;
            }
            else
            {
                sendType = !type.IsInstant() ?
                    SendMethod.SEND_RELIABLE_WITH_BUFFERING:
                    SendMethod.SEND_RELIABLE;
            }
            if (!Provider.Write(receiver, data, (ulong) length, sendType, channel))
            {
                LogUtils.LogError("Failed to send data to " + receiver);
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
            Channels++;
        }

        internal void CloseChannel(Channel ch)
        {
            for (var i = 0; i < Receivers.Count; i++)
            {
                if (_receivers[i].ID != ch.ID) continue;
                _receivers.RemoveAt(i);
                break;
            }
            Channels--;
        }

        public abstract void Dispose();
    }
}