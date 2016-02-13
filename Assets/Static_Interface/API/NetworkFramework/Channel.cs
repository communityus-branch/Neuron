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
    public class Channel : MonoBehaviour
    {
        [HideInInspector] public Connection Connection;
        public int ID { get; set; }
        public bool IsOwner { get; internal set; }
        public Identity Owner { get; internal set; }
        public List<ChannelMethod> Calls { get; private set; } = new List<ChannelMethod>();
        
        private static readonly object[] Voice = new object[3];
        private readonly List<Component> _componentsRead = new List<Component>();

        void Awake()
        {
            Build();
            Init();
            Connection = Connection.CurrentConnection;
        }

        public void Build()
        {
            _componentsRead.Clear();
            Calls.Clear();
            var components = GetComponents(typeof (Component));
            foreach (var c in components)
            {
                Build(c);
            }
        }

        public void Build(Component c)
        {
            if (_componentsRead.Contains(c)) return;
            var members = c.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).ToList();
            members.AddRange(c.GetType().GetMethods(BindingFlags.Public| BindingFlags.Instance));
            bool log = c is PlayerInput;
            foreach (var m in members)
            {
                if(log) LogUtils.Debug(m.Name);
                if (m.GetCustomAttributes(typeof (NetworkCall), true).Length <= 0)
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

        public bool CheckOwner(Identity user)
        {
            if (Owner == null)
            {
                return false;
            }
            bool isOwner =(user == Owner);
            if (!isOwner)
            {
                LogUtils.LogWarning("Not Owner! User: " + user + ", owner: " + Owner);
            }
            return isOwner;
        }

        public bool CheckServer(Identity user)
        {
            bool matched =(user == Connection.ServerID);
            if (!matched)
            {
                LogUtils.Debug("CheckServer failed for user: " + user.Serialize() +", ServerID: " + Connection.ServerID?.Serialize() + ", ClientID: " + Connection.ClientID?.Serialize());
            }
            return matched;
        }

        public void CloseWrite(string channelName, ECall mode, EPacket type)
        {
            LogUtils.Debug(nameof(CloseWrite) + ": " + channelName);
            if (!type.IsChunk())
            {
                LogUtils.LogError("Failed to stream non chunk: " + type);
            }
            else
            {
                var index = 
                    GetCall(channelName);
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
                LogUtils.LogError("Failed to stream non chunk: " + type);
            }
            else
            {
                var index = GetCall(channelName);
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

        public void CloseWrite(string channelName, ECall mode, byte bound, EPacket type)
        {
            if (!type.IsChunk())
            {
                LogUtils.LogError("Failed to stream non chunk: " + type);
            }
            else
            {
                int index = GetCall(channelName);
                if (index != -1)
                {
                    int size;
                    byte[] buffer;
                    GetPacket(type, index, out size, out buffer);
                    Send(mode, bound, type, size, buffer);
                }
            }
        }

        private int GetCall(string callName)
        {
            if(Calls.Count == 0) Build();
            for (var i = 0; i < Calls.Count; i++)
            {
                if (Calls.ElementAt(i).Method.Name == callName)
                {
                    return i;
                }
            }
            LogUtils.Debug("Call index not found for: " + callName);
            return -1;
        }

        private void GetPacket(EPacket type, int index, out int size, out byte[] packet)
        {
            packet = ObjectSerializer.CloseWrite(out size);
            packet[0] = (byte) type;
            packet[1] = (byte) index;
        }

        private void GetPacket(EPacket type, int index, out int size, out byte[] packet, params object[] arguments)
        {
            packet = ObjectSerializer.GetBytes(2, out size, arguments);
            packet[0] = (byte) type;
            packet[1] = (byte) index;
        }

        private void GetPacket(EPacket type, int index, out int size, out byte[] packet, byte[] bytes, int length)
        {
            size = 4 + length;
            packet = bytes;
            packet[0] = (byte) type;
            packet[1] = (byte) index;
            byte[] buffer = BitConverter.GetBytes((ushort) length);
            packet[2] = buffer[0];
            packet[3] = buffer[1];
        }

        public virtual void Init()
        {
        }

        private void OnDestroy()
        {
            if (ID != 0)
            {
                Connection.CloseChannel(this);
            }
        }

        public void OpenWrite()
        {
            ObjectSerializer.OpenWrite(2);
        }

        public T Read<T>()
        {
            return (T) Read(typeof (T))[0];
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
            LogUtils.Debug(nameof(Receive) + "; ident: " + ident);
            if (size < sizeof(byte) * 2) return; // we need at least 2 bytes
            if (ident.GetUser() == null)
            {
                ident = Connection.Provider.GetServerIdent();
            }
            int index = packet[offset + 1];
            if ((index < 0) || (index >= Calls.Count)) return;
            EPacket packet2 = (EPacket) packet[offset];
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
                object[] objArray = ObjectSerializer.GetObjects(ident, offset, 2, packet,
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
            LogUtils.Debug(nameof(Send) + ": mode: " + mode + ", type: "+ type + ", size: " + size);
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
                        LogUtils.LogWarning("Im not the server, I can't send a message to all clients!!");
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
                        LogUtils.LogWarning("Im not the server, I can't send a message to all other clients!!");
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
                        LogUtils.LogWarning("Im not the server, I can't send a message to all non owners!!");
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

        public void Send(string pName, ECall mode, EPacket type, params object[] arguments)
        {
            var index = GetCall(pName);
            if (index < 0 )
            {
                LogUtils.LogError("pName: " + pName + ": invalid index: " + index);
                return;
            }
            int size;
            byte[] buffer;
            GetPacket(type, index, out size, out buffer, arguments);
            Send(mode, type, size, buffer);
        }

        public void Send(string pName, Identity user, EPacket type, params object[] arguments)
        {
            var index = GetCall(pName);
            if (index == -1) return;
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

        public void Send(ECall mode, byte bound, EPacket type, int size, byte[] packet)
        {
            if(Connection.ServerID == null) throw new Exception("Server id is null");
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
                        if (((user.Identity != Connection.ClientID) &&
                             (user.Player != null)) && (user.Player.MovementController.Bound == bound))
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
                        if (((user.Identity != Connection.ClientID) &&
                             (user.Player != null)) && (user.Player.MovementController.Bound == bound))
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
                        if (((user.Identity != Owner) &&
                             (user.Player != null)) && (user.Player.MovementController.Bound == bound))
                        {
                            Connection.Send(user.Identity, type, packet, size, ID);
                        }
                    }
                    break;
                case ECall.Clients:
                    foreach (User user in Connection.Clients)
                    {
                        if (((user.Identity != Connection.ClientID) &&
                             (user.Player != null)) && (user.Player.MovementController.Bound == bound))
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
                        if (((user.Identity != Connection.ClientID) &&
                             (user.Player != null)) && (user.Player.MovementController.Bound == bound))
                        {
                            Connection.Send(user.Identity, type, packet, size, ID);
                        }
                    }
                    break;
            }
        }

        public void Send(string pName, ECall mode, EPacket type, byte[] bytes, int length)
        {
            var index = GetCall(pName);
            if (index == -1) return;
            int size;
            byte[] buffer;
            GetPacket(type, index, out size, out buffer, bytes, length);
            Send(mode, type, size, buffer);
        }

        public void Send(string pName, ECall mode, byte bound, EPacket type, params object[] arguments)
        {
            var index = GetCall(pName);
            if (index == -1) return;
            int size;
            byte[] buffer;
            GetPacket(type, index, out size, out buffer, arguments);
            Send(mode, bound, type, size, buffer);
        }

        public void Send(ECall mode, Vector3 point, float radius, EPacket type, int size, byte[] packet)
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

        public void Send(string pName, ECall mode, byte bound, EPacket type, byte[] bytes, int length)
        {
            var index = GetCall(pName);
            if (index == -1) return;
            int size;
            byte[] buffer;
            GetPacket(type, index, out size, out buffer, bytes, length);
            Send(mode, bound, type, size, buffer);
        }

        public void Send(string pName, ECall mode, Vector3 point, float radius, EPacket type,
            params object[] arguments)
        {
            var index = GetCall(pName);
            if (index == -1) return;
            int size;
            byte[] buffer;
            GetPacket(type, index, out size, out buffer, arguments);
            Send(mode, point, radius, type, size, buffer);
        }

        public void Send(string pName, ECall mode, Vector3 point, float radius, EPacket type, byte[] bytes,
            int length)
        {
            var index = GetCall(pName);
            if (index == -1) return;
            int size;
            byte[] buffer;
            GetPacket(type, index, out size, out buffer, bytes, length);
            Send(mode, point, radius, type, size, buffer);
        }

        public void SendAside(string pName, Identity u, EPacket type, params object[] arguments)
        {
            var index = GetCall(pName);
            if (index == -1) return;
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
            Connection = Connection.CurrentConnection;
            ID = Connection.Channels;
            LogUtils.Debug("Setting up channel " + ID);
            Connection.OpenChannel(this);
        }

        public void Write(params object[] objects)
        {
            ObjectSerializer.Write(objects);
        }
    }
}

