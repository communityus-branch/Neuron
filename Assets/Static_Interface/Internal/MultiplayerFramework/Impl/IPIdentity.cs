using System;
using System.Net;
using Static_Interface.API.PlayerFramework;

namespace Static_Interface.Internal.MultiplayerFramework.Impl
{
    public class IPIdentity : Identity
    {
        public ulong ID { get; }

        public static readonly IPIdentity Server = new IPIdentity(0);

        public IPIdentity(IPAddress address)
        {
            ID = BitConverter.ToUInt64(address.GetAddressBytes(), 0);
        }

        public IPIdentity(ulong id)
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

        public static IPIdentity Deserialze(ulong u)
        {
            return (IPIdentity) u;
        }

        public static explicit operator ulong(IPIdentity ident)
        {
            return ident.ID;
        }

        public bool IsServer => this == Server;

        public static explicit operator IPIdentity(ulong id)
        {
            return new IPIdentity(id);
        }

        public static bool operator ==(IPIdentity a, IPIdentity b)
        {
            if ((object)a == null && (object)b == null) return true;
            if ((object)a == null || (object)b == null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(IPIdentity a, IPIdentity b)
        {
            return !(a == b);
        }

        public static bool operator ==(IPIdentity a, ulong b)
        {
            return a?.ID == b;
        }

        public static bool operator !=(IPIdentity a, ulong b)
        {
            return !(a == b);
        }

        public static bool operator ==(ulong a, IPIdentity b)
        {
            return b == a;
        }

        public static bool operator !=(ulong a, IPIdentity b)
        {
            return b != a;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            var identity = obj as IPIdentity;
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