using System;
using ENet;
using Static_Interface.API.PlayerFramework;

namespace Static_Interface.Internal.MultiplayerFramework.Impl.ENet
{
    public class ENetIdentity : Identity
    {
        public ulong ID { get; }

        public ENetIdentity(Peer peer)
        {
            ID = BitConverter.ToUInt64(peer.GetRemoteAddress().Address.GetAddressBytes(), 0);
        }

        public ENetIdentity(ulong id)
        {
            ID = id;
        }

        public override bool IsValid()
        {
            return true;
        }

        public override ulong Serialize()
        {
            return ID;
        }

        public static ENetIdentity Deserialze(ulong u)
        {
            return (ENetIdentity) u;
        }

        public static explicit operator ulong(ENetIdentity ident)
        {
            return ident.ID;
        }

        public static explicit operator ENetIdentity(ulong id)
        {
            return new ENetIdentity(id);
        }

        public static bool operator ==(ENetIdentity a, ENetIdentity b)
        {
            if ((object)a == null && (object)b == null) return true;
            if ((object)a == null || (object)b == null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(ENetIdentity a, ENetIdentity b)
        {
            return !(a == b);
        }

        public static bool operator ==(ENetIdentity a, ulong b)
        {
            return a?.ID == b;
        }

        public static bool operator !=(ENetIdentity a, ulong b)
        {
            return !(a == b);
        }

        public static bool operator ==(ulong a, ENetIdentity b)
        {
            return b == a;
        }

        public static bool operator !=(ulong a, ENetIdentity b)
        {
            return b != a;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            var identity = obj as ENetIdentity;
            if (identity != null)
            {
                return identity.ID == ID;
            }
            return obj as ulong? == ID;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }
}