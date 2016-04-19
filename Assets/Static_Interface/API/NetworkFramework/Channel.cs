using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework;
using Static_Interface.Internal.Objects;
using UnityEngine;

namespace Static_Interface.API.NetworkFramework
{
    public class Channel : UnityExtensions.MonoBehaviour
    {
        [HideInInspector]
        public bool Listen = false;
        [HideInInspector]
        public Connection Connection;
        private int _id;
        public int ID
        {
            get { return _id; }
            set
            {
                _id = value;
                InspectorID = value;
            }
        }

        public int InspectorID;

        public bool IsOwner { get; internal set; }
        public Identity Owner { get; internal set; }
        public List<ChannelMethod> Calls { get; } = new List<ChannelMethod>();

        private static readonly object[] Voice = new object[3];
        private readonly List<Component> _componentsRead = new List<Component>();

        public static Channel GetChannel(int id)
        {
            return FindObjectsOfType<Channel>().FirstOrDefault(ch => ch.ID == id);
        }

        protected override void Awake()
        {
            base.Awake();
            Build();
            Init();
            Connection = Connection.CurrentConnection;
        }

        public void Build()
        {
            _componentsRead.Clear();
            Calls.Clear();
            var components = GetComponents(typeof(Component));
            foreach (var c in components)
            {
                Build(c);
            }
        }

        public void Build(Component c)
        {
            if (_componentsRead.Contains(c)) return;
            var members = c.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).ToList();
            members.AddRange(c.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance));
            foreach (var m in members)
            {
                if (m.GetCustomAttributes(typeof(NetworkCallAttribute), true).Length <= 0)
                {
                    continue;
                }
                var parameters = m.GetParameters();
                var newTypes = new Type[parameters.Length];
                for (var k = 0; k < parameters.Length; k++)
                {
                    newTypes[k] = parameters[k].ParameterType;
                }
                Calls.Add(new ChannelMethod(c, m, newTypes));
            }
            _componentsRead.Add(c);
        }

        public bool ValidateOwner(Identity user, bool throwException = true)
        {
            if (Owner == null)
            {
                if (throwException) throw new Exception("Not channel owner");
                return false;
            }
            bool isOwner = (user == Owner);
            if (!isOwner)
            {
                LogUtils.LogNetwork("Error: Not Owner! User: " + user + ", owner: " + Owner);
            }

            if (isOwner) return true;
            if (throwException) throw new Exception("Not channel owner");
            return false;
        }

        public bool ValidateServer(Identity user, bool throwException = true)
        {
            bool matched = (user == Connection.ServerID);
            if (!matched)
            {
                LogUtils.LogNetwork("Error: CheckServer failed for user: " + user.Serialize() + ", ServerID: " + Connection.ServerID?.Serialize() + ", ClientID: " + Connection.ClientID?.Serialize());
            }

            if (!matched && throwException)
            {
                throw new Exception("Identitiy is not server");
            }
            return matched;
        }

        public void CloseWrite(string channelName, ECall mode, EPacket type)
        {
            LogUtils.Debug(nameof(CloseWrite) + ": " + channelName);
            if (!type.IsChunk())
            {
                LogUtils.LogNetwork("Error: Failed to stream non chunk: " + type);
            }
            else
            {
                NetworkCallAttribute networkCall;
                var index =
                    GetCall(channelName, out networkCall);
                if (index == -1) return;
                int size;
                byte[] buffer;
                GetPacket(type, index, out size, out buffer);
                Send(mode, type, size, buffer);
            }
        }

        public void CloseWrite(string channelName, Identity user, EPacket type)
        {
            if (!type.IsChunk())
            {
                LogUtils.LogNetwork("Error: Failed to stream non chunk: " + type);
            }
            else
            {
                NetworkCallAttribute networkCall;
                var index =
                    GetCall(channelName, out networkCall);
                if (index == -1) return;
                int length;
                byte[] buffer;
                GetPacket(type, index, out length, out buffer);
                if (IsOwner && (user == Connection.ClientID))
                {
                    Receive(Connection.ClientID, buffer, 0, length);
                }
                else if (Connection.IsServer() && (user == Connection.ServerID))
                {
                    Receive(Connection.ServerID, buffer, 0, length);
                }
                else
                {
                    Connection.Send(user, type, buffer, length, ID);
                }
            }
        }

        private int GetCall(string callName, out NetworkCallAttribute networkCall)
        {
            networkCall = null;
            if (Calls.Count == 0) Build();
            for (var i = 0; i < Calls.Count; i++)
            {
                if (Calls.ElementAt(i).Method.Name == callName)
                {
                    networkCall =
                        (NetworkCallAttribute)
                            Calls.ElementAt(i).Method.GetCustomAttributes(typeof (NetworkCallAttribute), true)[0];  
                    return i;
                }
            }
            LogUtils.LogError("Call index not found for: " + callName);
            return -1;
        }

        private void GetPacket(EPacket type, int index, out int size, out byte[] packet)
        {
            packet = ObjectSerializer.CloseWrite(out size);
            packet[0] = (byte)type;
            packet[1] = (byte)index;
        }

        private void GetPacket(EPacket type, int index, out int size, out byte[] packet, params object[] arguments)
        {
            packet = ObjectSerializer.GetBytes(2, out size, arguments);
            packet[0] = (byte)type;
            packet[1] = (byte)index;
        }

        private void GetPacket(EPacket type, int index, out int size, out byte[] packet, byte[] bytes, int length)
        {
            size = 4 + length;
            packet = bytes;
            packet[0] = (byte)type;
            packet[1] = (byte)index;
            byte[] buffer = BitConverter.GetBytes((ushort)length);
            packet[2] = buffer[0];
            packet[3] = buffer[1];
        }

        public virtual void Init()
        {
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (ID != 0)
            {
                Listen = false;
            }
        }

        public void OpenWrite()
        {
            ObjectSerializer.OpenWrite(2);
        }

        public T Read<T>()
        {
            return (T)Read(typeof(T))[0];
        }

        public T[] Read<T>(int size)
        {
            T[] arr = new T[size];
            for (int i = 0; i < size; i++)
            {
                arr[i] = Read<T>();
            }
            return arr;
        }

        public object[] Read(params Type[] types)
        {
            return ObjectSerializer.Read(types);
        }

        public void Receive(Identity ident, byte[] packet, int offset, int size)
        {
            LogUtils.LogNetwork(nameof(Receive) + "; ident: " + ident);
            if (size < sizeof(byte) * 2) return; // we need at least 2 bytes
            if (ident.GetUser() == null)
            {
                ident = Connection.Provider.GetServerIdent();
            }
            int index = packet[offset + 1];
            if ((index < 0) || (index >= Calls.Count)) return;
            EPacket packet2 = (EPacket)packet[offset];


            var call = Calls[index];
            var networkCall = (NetworkCallAttribute) call.Method.GetCustomAttributes(typeof (NetworkCallAttribute), true)[0];
            if (networkCall.ValidateOwner && networkCall.ValidateServer)
            {
                if (!ValidateOwner(ident, false) && !ValidateServer(ident, false))
                {
                    throw new Exception("Couldn't validate owner/server");
                }
            }
            else if (networkCall.ValidateOwner)
            {
                ValidateOwner(ident);
            }
            else if (networkCall.ValidateServer)
            {
                ValidateServer(ident);
            }

            if (networkCall.ConnectionEnd != ConnectionEnd.BOTH)
            {
                switch (networkCall.ConnectionEnd)
                {
                    case ConnectionEnd.CLIENT:
                        if (!Connection.IsClient())
                        {
                            throw new Exception(Calls[index].Method.Name + " is not supposed to run on this connection end");
                        }
                        break;
                    case ConnectionEnd.SERVER:
                        if (!Connection.IsServer())
                        {
                            throw new Exception(Calls[index].Method.Name + " is not supposed to run on this connection end");
                        }
                        break;
                }
            }

            if ((packet2 == EPacket.UPDATE_VOICE) && (size < 4)) return;
            if ((packet2 == EPacket.UPDATE_UNRELIABLE_CHUNK_BUFFER) ||
                (packet2 == EPacket.UPDATE_RELIABLE_CHUNK_BUFFER))
            {
                ObjectSerializer.OpenRead(offset + 2, packet);
                object[] parameters = { ident };
                Calls[index].Method.Invoke(Calls[index].Component, parameters);
                ObjectSerializer.CloseRead();
            }
            else if (Calls[index].Types.Length > 0)
            {
                if (packet2 == EPacket.UPDATE_VOICE)
                {
                    Voice[0] = ident;
                    Voice[1] = packet;
                    Voice[2] = BitConverter.ToUInt16(packet, offset + 2);
                    Calls[index].Method.Invoke(Calls[index].Component, Voice);
                    return;
                }

                object[] objArray = ObjectSerializer.GetObjects(ident, offset, 2, packet, true,
                    Calls[index].Types);
                if (objArray != null)
                {
                    Calls[index].Method.Invoke(Calls[index].Component, objArray);
                }
                return;
            }
            Calls[index].Method.Invoke(Calls[index].Component, null);
        }

        public void Send(ECall mode, EPacket type, int size, byte[] packet)
        {
            LogUtils.LogNetwork(nameof(Send) + ": mode: " + mode + ", type: " + type + ", size: " + size);
            switch (mode)
            {
                case ECall.Server:
                    if (Connection.IsServer())
                    {
                        Receive(Connection.ServerID, packet, 0, size);
                    }
                    else
                    {
                        Connection.Send(Connection.ServerID, type, packet, size, ID);
                    }
                    break;
                case ECall.All:
                    if (!(Connection.IsServer()))
                    {
                        LogUtils.LogNetwork("Warning: Im not the server, I can't send a message to all clients!!");
                        Connection.Send(Connection.ServerID, type, packet, size, ID);
                    }

                    foreach (User user in Connection.Clients)
                    {
                        if (user.Identity != Connection.ClientID)
                        {
                            Connection.Send(user.Identity, type, packet, size, ID);
                        }
                    }
                    Receive(Connection.ClientID, packet, 0, size);
                    break;
                case ECall.Others:
                    if (!(Connection.IsServer()))
                    {
                        LogUtils.LogNetwork("Warning: Im not the server, I can't send a message to all other clients!!");
                        Connection.Send(Connection.ServerID, type, packet, size, ID);
                    }

                    foreach (var t in Connection.Clients.Where(t => t.Identity != Connection.ClientID))
                    {
                        Connection.Send(t.Identity, type, packet, size, ID);
                    }
                    break;
                case ECall.Owner:
                    if (IsOwner)
                    {
                        Receive(Owner, packet, 0, size);
                    }
                    else
                    {
                        Connection.Send(Owner, type, packet, size, ID);
                    }
                    break;
                case ECall.NotOwner:
                    if (!(Connection.IsServer()))
                    {
                        LogUtils.LogNetwork("Warning: Im not the server, I can't send a message to all non owners!!");
                        Connection.Send(Connection.ServerID, type, packet, size, ID);
                    }
                    foreach (User user in Connection.Clients.Where(user => user.Identity != Owner))
                    {
                        Connection.Send(user.Identity, type, packet, size, ID);
                    }
                    break;
                case ECall.Clients:
                    foreach (User user in Connection.Clients)
                    {
                        if (user.Identity != Connection.ClientID)
                        {
                            Connection.Send(user.Identity, type, packet, size, ID);
                        }
                    }
                    if (Connection.IsClient())
                    {
                        Receive(Connection.ClientID, packet, 0, size);
                    }
                    break;
                case ECall.Peers:
                    foreach (User user in Connection.Clients)
                    {
                        if (user.Identity != Connection.ClientID)
                        {
                            Connection.Send(user.Identity, type, packet, size, ID);
                        }
                    }
                    break;
            }
        }

        public void Send(string pName, ECall mode, params object[] arguments)
        {
            NetworkCallAttribute networkCall;
            var index =
                GetCall(pName, out networkCall);
            if (index < 0)
            {
                return;
            }
            var type = networkCall.PacketType;
            int size;
            byte[] buffer;
            GetPacket(type, index, out size, out buffer, arguments);
            Send(mode, type, size, buffer);
        }

        public void Send(string pName, Identity user, params object[] arguments)
        {
            NetworkCallAttribute networkCall;
            var index =
                GetCall(pName, out networkCall);
            if (index == -1) return;
            var type = networkCall.PacketType;
            int size;
            byte[] buffer;
            GetPacket(type, index, out size, out buffer, arguments);
            if (IsOwner && (user == Connection.ClientID))
            {
                Receive(Connection.ClientID, buffer, 0, size);
            }
            else if (Connection.IsServer() && (user == Connection.ServerID))
            {
                Receive(Connection.ServerID, buffer, 0, size);
            }
            else
            {
                Connection.Send(user, type, buffer, size, ID);
            }
        }

        public void Send(string pName, ECall mode, byte[] bytes, int length)
        {
            NetworkCallAttribute networkCall;
            var index =
                GetCall(pName, out networkCall);
            if (index == -1) return;
            var type = networkCall.PacketType;
            int size;
            byte[] buffer;
            GetPacket(type, index, out size, out buffer, bytes, length);
            Send(mode, type, size, buffer);
        }

        public void Send(ECall mode, Vector3 point,  float radius, EPacket type, int size, byte[] packet)
        {
            radius *= radius;
            switch (mode)
            {
                case ECall.Server:
                    if (Connection.IsServer())
                    {
                        Receive(Connection.ServerID, packet, 0, size);
                    }
                    else
                    {
                        Connection.Send(Connection.ServerID, type, packet, size, ID);
                    }
                    break;
                case ECall.All:
                    if (!(Connection.IsServer()))
                    {
                        Connection.Send(Connection.ServerID, type, packet, size, ID);
                    }
                    foreach (User user in Connection.Clients)
                    {
                        if ((user.Identity == Connection.ClientID) || (user.Player == null)) continue;
                        Vector3 vector = user.Player.transform.position - point;
                        if (vector.sqrMagnitude < radius)
                        {
                            Connection.Send(user.Identity, type, packet, size, ID);
                        }
                    }
                    if (Connection.IsServer())
                    {
                        Receive(Connection.ServerID, packet, 0, size);
                    }
                    else
                    {
                        Receive(Connection.ClientID, packet, 0, size);
                    }
                    break;
                case ECall.Others:
                    if (!(Connection.IsServer()))
                    {
                        Connection.Send(Connection.ServerID, type, packet, size, ID);
                    }
                    foreach (User user in Connection.Clients)
                    {
                        if ((user.Identity == Connection.ClientID) || (user.Player == null)) continue;
                        Vector3 vector2 = user.Player.transform.position - point;
                        if (vector2.sqrMagnitude < radius)
                        {
                            Connection.Send(user.Identity, type, packet, size, ID);
                        }
                    }
                    break;
                case ECall.Owner:
                    if (IsOwner)
                    {
                        Receive(Owner, packet, 0, size);
                    }
                    else
                    {
                        Connection.Send(Owner, type, packet, size, ID);
                    }
                    break;
                case ECall.NotOwner:
                    if (!(Connection.IsServer()))
                    {
                        Connection.Send(Connection.ServerID, type, packet, size, ID);
                    }
                    foreach (User user in Connection.Clients)
                    {
                        if ((user.Identity == Owner) || (user.Player == null)) continue;
                        Vector3 vector3 = user.Player.transform.position - point;
                        if (vector3.sqrMagnitude < radius)
                        {
                            Connection.Send(user.Identity, type, packet, size, ID);
                        }
                    }
                    break;
                case ECall.Clients:
                    foreach (User user in Connection.Clients)
                    {
                        if ((user.Identity == Connection.ClientID) || (user.Player == null)) continue;
                        Vector3 vector4 = user.Player.transform.position - point;
                        if (vector4.sqrMagnitude < radius)
                        {
                            Connection.Send(user.Identity, type, packet, size, ID);
                        }
                    }
                    if (Connection.IsServer())
                    {
                        Receive(Connection.ClientID, packet, 0, size);
                    }
                    break;
                case ECall.Peers:
                    foreach (User user in Connection.Clients)
                    {
                        if ((user.Identity == Connection.ClientID) || (user.Player == null)) continue;
                        Vector3 vector5 = user.Player.transform.position - point;
                        if (vector5.sqrMagnitude < radius)
                        {
                            Connection.Send(user.Identity, type, packet, size, ID);
                        }
                    }
                    break;
            }
        }

        public void Send(string pName, ECall mode, Vector3 point, 
            params object[] arguments)
        {
            NetworkCallAttribute networkCall;
            var index =
                GetCall(pName, out networkCall);
            if (index == -1) return;
            var type = networkCall.PacketType;
            float radius = networkCall.MaxRadius;
            int size;
            byte[] buffer;
            GetPacket(type, index, out size, out buffer, arguments);
            Send(mode, point, radius, type, size, buffer);
        }

        public void Send(string pName, ECall mode, Vector3 point, byte[] bytes,
            int length)
        {
            NetworkCallAttribute networkCall;
            var index =
                GetCall(pName, out networkCall);
            if (index == -1) return;
            var type = networkCall.PacketType;
            float radius = networkCall.MaxRadius;
            int size;
            byte[] buffer;
            GetPacket(type, index, out size, out buffer, bytes, length);
            Send(mode, point, radius, type, size, buffer);
        }

        public void SendAside(string pName, Identity u, params object[] arguments)
        {
            NetworkCallAttribute networkCall;
            var index =
                GetCall(pName, out networkCall);
            if (index == -1) return;
            var type = networkCall.PacketType;
            int size;
            byte[] buffer;
            GetPacket(type, index, out size, out buffer, arguments);
            foreach (User user in Connection.Clients.Where(user => user.Identity != u))
            {
                Connection.Send(user.Identity, type, buffer, size, ID);
            }
        }

        public void Setup()
        {
            LogUtils.Debug("Setting up channel #" + ID);
            Connection = Connection.CurrentConnection;
            if (ID == 0) ID = Connection.ChannelCount;
            if (ID < 1) throw new Exception("Channel ID is < 0!! (ID: " + ID + ")");
            Listen = true;
        }

        public void Write(params object[] objects)
        {
            ObjectSerializer.Write(objects);
        }
    }
}

