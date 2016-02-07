using System;
using System.Reflection;

namespace Static_Interface.API.NetworkFramework
{
    class EPacketAttr : Attribute
    {
        public readonly bool IsUpdate;
        public readonly bool IsUnreliable;
        public readonly bool IsInstant;
        public readonly bool IsChunk;
        internal EPacketAttr(bool isUpdate, bool isUnreliable, bool isInstant, bool isChunk)
        {
            IsUpdate = isUpdate;
            IsUnreliable = isUnreliable;
            IsInstant = isInstant;
            IsChunk = isChunk;
        }
    } 
    public enum EPacket : byte
    {
        [EPacketAttr(false, false, false, false)] SHUTDOWN = 0x0,
        [EPacketAttr(false, false, false, false)] CONNECT = 0x1,
        [EPacketAttr(false, false, false, false)] AUTHENTICATE = 0x2,
        [EPacketAttr(false, false, false, false)] REJECTED = 0x3,
        [EPacketAttr(false, false, false, false)] ACCEPTED = 0x4,
        [EPacketAttr(false, false, false, false)] KICKED = 0x5,
        [EPacketAttr(false, false, false, false)] CONNECTED = 0x6,
        [EPacketAttr(false, false, false, false)] DISCONNECTED = 0x7,
        [EPacketAttr(false, true, false, false)] TICK = 0x8,
        [EPacketAttr(false, true, false, false)] TIME = 0x9,
        [EPacketAttr(false, false, false, false)] WORKSHOP = 0xa,
        [EPacketAttr(false, false, false, false)] VERIFY = 0xb,
        [EPacketAttr(true, false, false, false)] UPDATE_RELIABLE_BUFFER = 0xc,
        [EPacketAttr(true, true, false, false)] UPDATE_UNRELIABLE_BUFFER = 0xd,
        [EPacketAttr(true, false, true, false)] UPDATE_RELIABLE_INSTANT = 0xe,
        [EPacketAttr(true, true, true, false)] UPDATE_UNRELIABLE_INSTANT = 0xf,
        [EPacketAttr(true, false, false, true)] UPDATE_RELIABLE_CHUNK_BUFFER = 0x10,
        [EPacketAttr(true, true, false, true)] UPDATE_UNRELIABLE_CHUNK_BUFFER = 0x11,
        [EPacketAttr(true, false, true, true)] UPDATE_RELIABLE_CHUNK_INSTANT = 0x12,
        [EPacketAttr(true, true, true, true)] UPDATE_UNRELIABLE_CHUNK_INSTANT = 0x13,
        [EPacketAttr(true, true, false, false)] UPDATE_VOICE = 0x14
    }

    public static class Packets
    {
        public static bool IsUpdate(this EPacket p)
        {
            return GetAttr(p).IsUpdate;
        }

        public static bool IsUnreliable(this EPacket p)
        {
            return GetAttr(p).IsUnreliable;
        }

        public static bool IsInstant(this EPacket p)
        {
            return GetAttr(p).IsInstant;
        }

        public static bool IsChunk(this EPacket p)
        {
            return GetAttr(p).IsChunk;
        }

        public static byte GetID(this EPacket p)
        {
            return (byte)p;
        }

        private static EPacketAttr GetAttr(EPacket p)
        {
            return (EPacketAttr)Attribute.GetCustomAttribute(ForValue(p), typeof(EPacketAttr));
        }

        private static MemberInfo ForValue(EPacket p)
        {
            return typeof(EPacket).GetField(Enum.GetName(typeof(EPacket), p));
        }
    }
}
