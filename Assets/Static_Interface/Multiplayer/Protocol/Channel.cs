﻿using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Static_Interface.Multiplayer.Client;
using Static_Interface.Multiplayer.Server;
using Static_Interface.Objects;
using Static_Interface.PlayerFramework;
using UnityEngine;

namespace Static_Interface.Multiplayer.Protocol
{
    public class Channel : MonoBehaviour
    {
        private Connection _connection;
        public Connection Connection
        {
            get { return _connection; }
        }

        private ChannelMethod[] _calls;
        public int ID;
        public bool IsOwner;
        public User Owner;
        private static readonly object[] Voice = new object[3];

        void Awake()
        {
            Build();
            Init();
            _connection = Connection;
        }

        public void Build()
        {
            var list = new List<ChannelMethod>();
            var components = GetComponents(typeof (Component));
            foreach (var c in components)
            {
                var members = c.GetType().GetMembers();
                foreach (var m in members)
                {
                    if (m.MemberType != MemberTypes.Method) continue;
                    var newMethod = (MethodInfo) m;
                    if (newMethod.GetCustomAttributes(typeof (RPCCall), true).Length <= 0) continue;
                    var parameters = newMethod.GetParameters();
                    var newTypes = new Type[parameters.Length];
                    for (var k = 0; k < parameters.Length; k++)
                    {
                        newTypes[k] = parameters[k].ParameterType;
                    }
                    list.Add(new ChannelMethod(c, newMethod, newTypes));
                }
            }
            _calls = list.ToArray();
        }

        public bool CheckOwner(CSteamID steamID)
        {
            if (Owner == null)
            {
                return false;
            }
            return (steamID == Owner.Identity.ID);
        }

        public bool ValidateServer(CSteamID steamID)
        {
            return (steamID == Connection.ServerID);
        }

        public void CloseWrite(string channelName, ECall mode, EPacket type)
        {
            if (!IsChunk(type))
            {
                Debug.LogError("Failed to stream non chunk.");
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

        public void CloseWrite(string channelName, CSteamID steamID, EPacket type)
        {
            if (!IsChunk(type))
            {
                Debug.LogError("Failed to stream non chunk.");
            }
            else
            {
                var index = GetCall(channelName);
                if (index == -1) return;
                int length;
                byte[] buffer;
                GetPacket(type, index, out length, out buffer);
                if (IsOwner && (steamID == Connection.ClientID))
                {
                    Receive(Connection.ClientID, buffer, 0, length);
                }
                else if (Connection is ServerConnection && (steamID == Connection.ServerID))
                {
                    Receive(Connection.ServerID, buffer, 0, length);
                }
                else
                {
                    Connection.Send(steamID, type, buffer, length, ID);
                }
            }
        }

        public void CloseWrite(string channelName, ECall mode, byte bound, EPacket type)
        {
            if (!IsChunk(type))
            {
                Debug.LogError("Failed to stream non chunk.");
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

        public void CloseWrite(string channelName, ECall mode, byte x, byte y, byte area, EPacket type)
        {
            if (!IsChunk(type))
            {
                Debug.LogError("Failed to stream non chunk.");
            }
            else
            {
                var index = GetCall(channelName);
                if (index == -1) return;
                int length;
                byte[] buffer;
                GetPacket(type, index, out length, out buffer);
                Send(mode, x, y, area, type, length, buffer);
            }
        }

        private int GetCall(string callName)
        {
            for (var i = 0; i < Calls.Length; i++)
            {
                if (Calls[i].Method.Name == callName)
                {
                    return i;
                }
            }
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

        public object[] Read(params Type[] types)
        {
            return ObjectSerializer.Read(types);
        }

        public void Receive(CSteamID steamID, byte[] packet, int offset, int size)
        {
            if (size >= 2)
            {
                int index = packet[offset + 1];
                if ((index >= 0) && (index < Calls.Length))
                {
                    EPacket packet2 = (EPacket) packet[offset];
                    if ((packet2 != EPacket.UPDATE_VOICE) || (size >= 4))
                    {
                        if ((packet2 == EPacket.UPDATE_UNRELIABLE_CHUNK_BUFFER) ||
                            (packet2 == EPacket.UPDATE_RELIABLE_CHUNK_BUFFER))
                        {
                            ObjectSerializer.OpenRead(offset + 2, packet);
                            object[] parameters = {steamID};
                            Calls[index].Method.Invoke(Calls[index].Component, parameters);
                            ObjectSerializer.CloseRead();
                        }
                        else if (Calls[index].Types.Length > 0)
                        {
                            if (packet2 == EPacket.UPDATE_VOICE)
                            {
                                Voice[0] = steamID;
                                Voice[1] = packet;
                                Voice[2] = BitConverter.ToUInt16(packet, offset + 2);
                                Calls[index].Method.Invoke(Calls[index].Component, Voice);
                            }
                            else
                            {
                                object[] objArray = ObjectSerializer.GetObjects(steamID, offset, 2, packet,
                                    Calls[index].Types);
                                if (objArray != null)
                                {
                                    Calls[index].Method.Invoke(Calls[index].Component, objArray);
                                }
                            }
                        }
                        else
                        {
                            Calls[index].Method.Invoke(Calls[index].Component, null);
                        }
                    }
                }
            }
        }

        public void Send(ECall mode, EPacket type, int size, byte[] packet)
        {
            if (mode == ECall.SERVER)
            {
                if (Connection is ServerConnection)
                {
                    Receive(Connection.ServerID, packet, 0, size);
                }
                else
                {
                    Connection.Send(Connection.ServerID, type, packet, size, ID);
                }
            }
            else if (mode == ECall.ALL)
            {
                if (!(Connection is ServerConnection))
                {
                    Connection.Send(Connection.ServerID, type, packet, size, ID);
                }
                foreach (User user in Connection.Clients)
                {
                    if (user.Identity.ID != Connection.ClientID)
                    {
                        Connection.Send(user.Identity.ID, type, packet, size, ID);
                    }
                }
                if (Connection is ServerConnection)
                {
                    Receive(Connection.ServerID, packet, 0, size);
                }
                else
                {
                    Receive(Connection.ClientID, packet, 0, size);
                }
            }
            else if (mode == ECall.OTHERS)
            {
                if (!(Connection is ServerConnection))
                {
                    Connection.Send(Connection.ServerID, type, packet, size, ID);
                }

                foreach (var t in Connection.Clients.Where(t => t.Identity.ID != Connection.ClientID))
                {
                    Connection.Send(t.Identity.ID, type, packet, size, ID);
                }
            }
            else if (mode == ECall.OWNER)
            {
                if (IsOwner)
                {
                    Receive(Owner.Identity.ID, packet, 0, size);
                }
                else
                {
                    Connection.Send(Owner.Identity.ID, type, packet, size, ID);
                }
            }
            else if (mode == ECall.NOT_OWNER)
            {
                if (!(Connection is ServerConnection))
                {
                    Connection.Send(Connection.ServerID, type, packet, size, ID);
                }
                foreach (User user in Connection.Clients.Where(user => user.Identity.ID != Owner.Identity.ID))
                {
                    Connection.Send(user.Identity.ID, type, packet, size, ID);
                }
            }
            else if (mode == ECall.CLIENTS)
            {
                foreach (User user in Connection.Clients)
                {
                    if (user.Identity.ID != Connection.ClientID)
                    {
                        Connection.Send(user.Identity.ID, type, packet, size, ID);
                    }
                }
                if (Connection is ClientConnection)
                {
                    Receive(Connection.ClientID, packet, 0, size);
                }
            }
            else if (mode == ECall.PEERS)
            {
                foreach (User user in Connection.Clients)
                {
                    if (user.Identity.ID != Connection.ClientID)
                    {
                        Connection.Send(user.Identity.ID, type, packet, size, ID);
                    }
                }
            }
        }

        public void Send(string pName, ECall mode, EPacket type, params object[] arguments)
        {
            var index = GetCall(pName);
            if (index == -1) return;
            int size;
            byte[] buffer;
            GetPacket(type, index, out size, out buffer, arguments);
            Send(mode, type, size, buffer);
        }

        public void Send(string pName, CSteamID steamID, EPacket type, params object[] arguments)
        {
            var index = GetCall(pName);
            if (index == -1) return;
            int size;
            byte[] buffer;
            GetPacket(type, index, out size, out buffer, arguments);
            if (IsOwner && (steamID == Connection.ClientID))
            {
                Receive(Connection.ClientID, buffer, 0, size);
            }
            else if (Connection is ServerConnection && (steamID == Connection.ServerID))
            {
                Receive(Connection.ServerID, buffer, 0, size);
            }
            else
            {
                Connection.Send(steamID, type, buffer, size, ID);
            }
        }

        public void Send(ECall mode, byte bound, EPacket type, int size, byte[] packet)
        {
            switch (mode)
            {
                case ECall.SERVER:
                    if (Connection is ServerConnection)
                    {
                        Receive(Connection.ServerID, packet, 0, size);
                    }
                    else
                    {
                        Connection.Send(Connection.ServerID, type, packet, size, ID);
                    }
                    break;
                case ECall.ALL:
                    if (!(Connection is ServerConnection))
                    {
                        Connection.Send(Connection.ServerID, type, packet, size, ID);
                    }
                    foreach (User user in Connection.Clients)
                    {
                        if (((user.Identity.ID != Connection.ClientID) &&
                             (user.Player != null)) && (user.Player.MovementController.Bound == bound))
                        {
                            Connection.Send(user.Identity.ID, type, packet, size, ID);
                        }
                    }
                    if (Connection is ServerConnection)
                    {
                        Receive(Connection.ServerID, packet, 0, size);
                    }
                    else
                    {
                        Receive(Connection.ClientID, packet, 0, size);
                    }
                    break;
                case ECall.OTHERS:
                    if (!(Connection is ServerConnection))
                    {
                        Connection.Send(Connection.ServerID, type, packet, size, ID);
                    }
                    foreach (User user in Connection.Clients)
                    {
                        if (((user.Identity.ID != Connection.ClientID) &&
                             (user.Player != null)) && (user.Player.MovementController.Bound == bound))
                        {
                            Connection.Send(user.Identity.ID, type, packet, size, ID);
                        }
                    }
                    break;
                case ECall.OWNER:
                    if (IsOwner)
                    {
                        Receive(Owner.Identity.ID, packet, 0, size);
                    }
                    else
                    {
                        Connection.Send(Owner.Identity.ID, type, packet, size, ID);
                    }
                    break;
                case ECall.NOT_OWNER:
                    if (!(Connection is ServerConnection))
                    {
                        Connection.Send(Connection.ServerID, type, packet, size, ID);
                    }
                    foreach (User user in Connection.Clients)
                    {
                        if (((user.Identity.ID != Owner.Identity.ID) &&
                             (user.Player != null)) && (user.Player.MovementController.Bound == bound))
                        {
                            Connection.Send(user.Identity.ID, type, packet, size, ID);
                        }
                    }
                    break;
                case ECall.CLIENTS:
                    foreach (User user in Connection.Clients)
                    {
                        if (((user.Identity.ID != Connection.ClientID) &&
                             (user.Player != null)) && (user.Player.MovementController.Bound == bound))
                        {
                            Connection.Send(user.Identity.ID, type, packet, size, ID);
                        }
                    }
                    if (Connection is ClientConnection)
                    {
                        Receive(Connection.ClientID, packet, 0, size);
                    }
                    break;
                case ECall.PEERS:
                    foreach (User user in Connection.Clients)
                    {
                        if (((user.Identity.ID != Connection.ClientID) &&
                             (user.Player != null)) && (user.Player.MovementController.Bound == bound))
                        {
                            Connection.Send(user.Identity.ID, type, packet, size, ID);
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
                case ECall.SERVER:
                    if (Connection is ServerConnection)
                    {
                        Receive(Connection.ServerID, packet, 0, size);
                    }
                    else
                    {
                        Connection.Send(Connection.ServerID, type, packet, size, ID);
                    }
                    break;
                case ECall.ALL:
                    if (!(Connection is ServerConnection))
                    {
                        Connection.Send(Connection.ServerID, type, packet, size, ID);
                    }
                    foreach (User user in Connection.Clients)
                    {
                        if ((user.Identity.ID == Connection.ClientID) || (user.Player == null)) continue;
                        Vector3 vector = user.Player.transform.position - point;
                        if (vector.sqrMagnitude < radius)
                        {
                            Connection.Send(user.Identity.ID, type, packet, size, ID);
                        }
                    }
                    if (Connection is ServerConnection)
                    {
                        Receive(Connection.ServerID, packet, 0, size);
                    }
                    else
                    {
                        Receive(Connection.ClientID, packet, 0, size);
                    }
                    break;
                case ECall.OTHERS:
                    if (!(Connection is ServerConnection))
                    {
                        Connection.Send(Connection.ServerID, type, packet, size, ID);
                    }
                    foreach (User user in Connection.Clients)
                    {
                        if ((user.Identity.ID == Connection.ClientID) || (user.Player == null)) continue;
                        Vector3 vector2 = user.Player.transform.position - point;
                        if (vector2.sqrMagnitude < radius)
                        {
                            Connection.Send(user.Identity.ID, type, packet, size, ID);
                        }
                    }
                    break;
                case ECall.OWNER:
                    if (IsOwner)
                    {
                        Receive(Owner.Identity.ID, packet, 0, size);
                    }
                    else
                    {
                        Connection.Send(Owner.Identity.ID, type, packet, size, ID);
                    }
                    break;
                case ECall.NOT_OWNER:
                    if (!(Connection is ServerConnection))
                    {
                        Connection.Send(Connection.ServerID, type, packet, size, ID);
                    }
                    foreach (User user in Connection.Clients)
                    {
                        if ((user.Identity.ID == Owner.Identity.ID) || (user.Player == null)) continue;
                        Vector3 vector3 = user.Player.transform.position - point;
                        if (vector3.sqrMagnitude < radius)
                        {
                            Connection.Send(user.Identity.ID, type, packet, size, ID);
                        }
                    }
                    break;
                case ECall.CLIENTS:
                    foreach (User user in Connection.Clients)
                    {
                        if ((user.Identity.ID == Connection.ClientID) || (user.Player == null)) continue;
                        Vector3 vector4 = user.Player.transform.position - point;
                        if (vector4.sqrMagnitude < radius)
                        {
                            Connection.Send(user.Identity.ID, type, packet, size, ID);
                        }
                    }
                    if (Connection is ServerConnection)
                    {
                        Receive(Connection.ClientID, packet, 0, size);
                    }
                    break;
                case ECall.PEERS:
                    foreach (User user in Connection.Clients)
                    {
                        if ((user.Identity.ID == Connection.ClientID) || (user.Player == null)) continue;
                        Vector3 vector5 = user.Player.transform.position - point;
                        if (vector5.sqrMagnitude < radius)
                        {
                            Connection.Send(user.Identity.ID, type, packet, size, ID);
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

        public void Send(ECall mode, byte x, byte y, byte area, EPacket type, int size, byte[] packet)
        {
            switch (mode)
            {
                case ECall.SERVER:
                    if (Connection is ServerConnection)
                    {
                        Receive(Connection.ServerID, packet, 0, size);
                    }
                    else
                    {
                        Connection.Send(Connection.ServerID, type, packet, size, ID);
                    }
                    break;
                case ECall.ALL:
                    if (!(Connection is ServerConnection))
                    {
                        Connection.Send(Connection.ServerID, type, packet, size, ID);
                    }
                    foreach (var user in Connection.Clients)
                    {
                        if (((user.Identity.ID != Connection.ClientID) &&
                             (user.Player != null)) &&
                            Regions.CheckArea(x, y, user.Player.MovementController.RegionX,
                                user.Player.MovementController.RegionY, area))
                        {
                            Connection.Send(user.Identity.ID, type, packet, size, ID);
                        }
                    }
                    if (Connection is ServerConnection)
                    {
                        Receive(Connection.ServerID, packet, 0, size);
                    }
                    else
                    {
                        Receive(Connection.ClientID, packet, 0, size);
                    }
                    break;
                case ECall.OTHERS:
                    if (!(Connection is ServerConnection))
                    {
                        Connection.Send(Connection.ServerID, type, packet, size, ID);
                    }
                    foreach (User user in Connection.Clients)
                    {
                        if (((user.Identity.ID != Connection.ClientID) &&
                             (user.Player != null)) &&
                            Regions.CheckArea(x, y, user.Player.MovementController.RegionX,
                                user.Player.MovementController.RegionY, area))
                        {
                            Connection.Send(user.Identity.ID, type, packet, size, ID);
                        }
                    }
                    break;
                case ECall.OWNER:
                    if (IsOwner)
                    {
                        Receive(Owner.Identity.ID, packet, 0, size);
                    }
                    else
                    {
                        Connection.Send(Owner.Identity.ID, type, packet, size, ID);
                    }
                    break;
                case ECall.NOT_OWNER:
                    if (!(Connection is ServerConnection))
                    {
                        Connection.Send(Connection.ServerID, type, packet, size, ID);
                    }
                    foreach (User user in Connection.Clients)
                    {
                        if (((user.Identity.ID != Owner.Identity.ID) &&
                             (user.Player != null)) &&
                            Regions.CheckArea(x, y, user.Player.MovementController.RegionX,
                            user.Player.MovementController.RegionY, area))
                        {
                            Connection.Send(user.Identity.ID, type, packet, size, ID);
                        }
                    }
                    break;
                case ECall.CLIENTS:
                    foreach (User user in Connection.Clients)
                    {
                        if (((user.Identity.ID != Connection.ClientID) &&
                             (user.Player != null)) &&
                            Regions.CheckArea(x, y, user.Player.MovementController.RegionX,
                                user.Player.MovementController.RegionY, area))
                        {
                            Connection.Send(user.Identity.ID, type, packet, size, ID);
                        }
                    }
                    if (Connection is ClientConnection)
                    {
                        Receive(Connection.ClientID, packet, 0, size);
                    }
                    break;
                case ECall.PEERS:
                    foreach (User user in Connection.Clients)
                    {
                        if (((user.Identity.ID != Connection.ClientID) &&
                             (user.Player != null)) &&
                            Regions.CheckArea(x, y, user.Player.MovementController.RegionX,
                                user.Player.MovementController.RegionY, area))
                        {
                            Connection.Send(user.Identity.ID, type, packet, size, ID);
                        }
                    }
                    break;
            }
        }

        public void Send(string pName, ECall mode, byte x, byte y, byte area, EPacket type,
            params object[] arguments)
        {
            var index = GetCall(pName);
            if (index == -1) return;
            int size;
            byte[] buffer;
            GetPacket(type, index, out size, out buffer, arguments);
            Send(mode, x, y, area, type, size, buffer);
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

        public void Send(string pName, ECall mode, byte x, byte y, byte area, EPacket type, byte[] bytes, int length)
        {
            var index = GetCall(pName);
            if (index == -1) return;
            int size;
            byte[] buffer;
            GetPacket(type, index, out size, out buffer, bytes, length);
            Send(mode, x, y, area, type, size, buffer);
        }

        public void SendAside(string pName, CSteamID steamID, EPacket type, params object[] arguments)
        {
            var index = GetCall(pName);
            if (index == -1) return;
            int size;
            byte[] buffer;
            GetPacket(type, index, out size, out buffer, arguments);
            foreach (User user in Connection.Clients.Where(user => user.Identity.ID != steamID))
            {
                Connection.Send(user.Identity.ID, type, buffer, size, ID);
            }
        }

        public void Setup()
        {
            Connection.OpenChannel(this);
        }

        public void Write(params object[] objects)
        {
            ObjectSerializer.Write(objects);
        }

        public ChannelMethod[] Calls
        {
            get { return _calls; }
        }

        public bool IsChunk(EPacket packet)
        {
            return ((packet == EPacket.UPDATE_UNRELIABLE_CHUNK_BUFFER) ||
                    ((packet == EPacket.UPDATE_RELIABLE_CHUNK_BUFFER) ||
                     ((packet == EPacket.UPDATE_UNRELIABLE_CHUNK_INSTANT) ||
                      (packet == EPacket.UPDATE_RELIABLE_CHUNK_INSTANT))));
        }
    }
}

