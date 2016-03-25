using System;
using System.Collections.Generic;
using System.Linq;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework.MultiplayerProvider;
using Static_Interface.Internal.Objects;
using UnityEngine;
using MonoBehaviour = Static_Interface.API.UnityExtensions.MonoBehaviour;

namespace Static_Interface.Internal.MultiplayerFramework
{
    public abstract class Connection : MonoBehaviour
    {
        public const float CHECKRATE = 2f;
        public const int CLIENT_TIMEOUT = 30;
        public const int SERVER_TIMEOUT = 30;
        public const int PENDING_TIMEOUT = 30;
        public const float UPDATE_TIME = 0.15f;

        public static bool IsServer()
        {
            if (IsSinglePlayer) return true;
            return CurrentConnection.Provider is ServerMultiplayerProvider;
        }

        public static bool IsDedicated()
        {
            return !IsSinglePlayer && IsServer();
        }

        public static bool IsClient()
        {
            return CurrentConnection.Provider is ClientMultiplayerProvider;
        }

        public static Connection CurrentConnection { get; internal set; }

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

        protected override void Awake()
        {
            base.Awake();
            CurrentConnection = this;
            DontDestroyOnLoad(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (CurrentConnection == this) CurrentConnection = null;
        }

        internal virtual void Receive(Identity source, byte[] packet,int size, int channel)
        {
            //if (!IsConnected) return;
            var type = "<channel data>";
            if (channel == 0)
            {
                type = ((EPacket) packet[0]).ToString();
            }
            LogUtils.LogNetwork("Received " + type + " packet, channel: " + channel + ", size: " + size);
        }

        private static List<Channel> _receivers = new List<Channel>();
        public static ICollection<Channel> Receivers => _receivers?.AsReadOnly();
        public bool IsConnecting { get; set; }
        public static bool IsSinglePlayer { get; internal set; }

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
        }


        protected override void Update()
        {
            base.Update();
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
            Identity user;
            ulong length;
            if (Provider == null)
            {
                throw new NullReferenceException("No provider was set!!");
            }
            while (Provider.Read(out user, Buffer, out length, channelId))
            {
                Receive(user, Buffer, (int)length, channelId);
            }
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            Provider?.Update();
        }

        internal abstract void Listen();

        protected virtual Transform AddPlayer(Identity ident, string playerName, ulong @group, Vector3 point, Vector3 angle, int channel, bool mainPlayer)
        {
            LogUtils.Debug(nameof(AddPlayer) + ": " + playerName);
            GameObject obj = (GameObject) Resources.Load("Player");
            obj.transform.FindChild("MainCamera").GetComponent<Camera>().enabled = false;
            Transform newModel = ((GameObject)Instantiate(obj, point, Quaternion.Euler(angle))).transform;
            LogUtils.Debug("Spawning player " + playerName + " at " + point);
            var user = new User(CurrentConnection, ident, newModel, channel) {Group = @group, Name = playerName };
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

//            if ((IsClient() && receiver == ClientID && ClientID != null) || (IsServer() && receiver == ServerID && ServerID != null))
//            {
//                LogUtils.Debug("Server/Client sending to itself");
//                Receive(receiver, data, length, channel);
//                return;
//            }

            if (!receiver.IsValid())
            {
                LogUtils.LogError("Failed to send to invalid ID.");
                return;
            }

            LogUtils.LogNetwork("Sending packet: " + type + ", receiver: " + receiver + (receiver == ServerID ? " (Server)" : "") + ", ch: " + channel + ", size: " + data.Length);

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

            if (OnPreSend(receiver, type, data, length, channel))
            {
                return;
            }

            if (!Provider.Write(receiver, data, (ulong) length, sendType, channel))
            {
                LogUtils.LogError("Failed to send data to " + receiver);
            }
        }

        protected virtual bool OnPreSend(Identity receiver, EPacket type, byte[] data, int length, int channel)
        {
            return false;
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

        protected void StripPacketByte(ref byte[] packet, ref int size)
        {
            var list = packet.ToList();
            list.RemoveAt(0);
            packet = list.ToArray();
            size--;
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