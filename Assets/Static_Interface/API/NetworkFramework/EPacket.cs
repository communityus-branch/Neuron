using System;
using System.Reflection;

namespace Static_Interface.API.NetworkFramework
{
    class EPacketAttr : Attribute
    {
        public readonly bool IsUpdate;
        public readonly bool IsUnreliable;
        public readonly bool IsInstant;
        internal EPacketAttr(bool isUpdate, bool isUnreliable, bool isInstant)
        {
            IsUpdate = isUpdate;
            IsUnreliable = isUnreliable;
            IsInstant = isInstant;
        }
    } 
    public enum EPacket
    {
        [EPacketAttr(false, false, false)] SHUTDOWN,
        [EPacketAttr(false, false, false)] CONNECT,
        [EPacketAttr(false, false, false)] AUTHENTICATE,
        [EPacketAttr(false, false, false)] REJECTED,
        [EPacketAttr(false, false, false)] ACCEPTED,
        [EPacketAttr(false, false, false)] KICKED,
        [EPacketAttr(false, false, false)] CONNECTED,
        [EPacketAttr(false, false, false)] DISCONNECTED,
        [EPacketAttr(false, true, false)] TICK,
        [EPacketAttr(false, true, false)] TIME,
        [EPacketAttr(false, false, false)] WORKSHOP,
        [EPacketAttr(false, false, false)] VERIFY,
        [EPacketAttr(true, false, false)] UPDATE_RELIABLE_BUFFER,
        [EPacketAttr(true, true, false)] UPDATE_UNRELIABLE_BUFFER,
        [EPacketAttr(true, false, true)] UPDATE_RELIABLE_INSTANT,
        [EPacketAttr(true, true, true)] UPDATE_UNRELIABLE_INSTANT,
        [EPacketAttr(true, false, false)] UPDATE_RELIABLE_CHUNK_BUFFER,
        [EPacketAttr(true, true, false)] UPDATE_UNRELIABLE_CHUNK_BUFFER,
        [EPacketAttr(true, false, true)] UPDATE_RELIABLE_CHUNK_INSTANT,
        [EPacketAttr(true, true, true)] UPDATE_UNRELIABLE_CHUNK_INSTANT,
        [EPacketAttr(true, true, false)] UPDATE_VOICE
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
