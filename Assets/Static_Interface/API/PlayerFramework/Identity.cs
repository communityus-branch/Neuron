using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Static_Interface.Internal.MultiplayerFramework;
using Static_Interface.Internal.MultiplayerFramework.Impl;

namespace Static_Interface.API.PlayerFramework
{
    public abstract class Identity
    {
        public User Owner { get; internal set; }
        public abstract bool IsValid();
        public abstract ulong Serialize();
        public override string ToString()
        {
            return Serialize().ToString();
        }

        public override int GetHashCode()
        {
            return Serialize().GetHashCode();
        }

        [SuppressMessage("ReSharper", "RedundantCast.0")]
        public static bool operator ==(Identity a, Identity b)
        {
            if ((object) a == null && (object) b == null) return true;
            if ((object) a == null || ((object) b) == null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(Identity a, Identity b)
        {
            return !(a == b);
        }

        public static bool operator ==(Identity a, ulong b)
        {
            return a?.Serialize()== b;
        }

        public static bool operator !=(Identity a, ulong b)
        {
            return !(a == b);
        }

        public static bool operator ==(ulong a, Identity b)
        {
            return b == a;
        }

        public static bool operator !=(ulong a, Identity b)
        {
            return b != a;
        }

        public static implicit operator ulong(Identity ident)
        {
            return ident.Serialize();
        }

        public static implicit operator Identity(ulong var)
        {
            return Connection.CurrentConnection.Provider.Deserialilze(var);
        }

        public override bool Equals(object obj)
        {
            if (obj is Identity)
            {
                return Serialize() == ((Identity) obj).Serialize();
            }
            if (obj is long)
            {
                obj = (ulong)(long)obj;
            }
            if (obj is ulong)
            {
                return Serialize() == (ulong)obj;
            }
            return false;
        }

        public User GetUser()
        {
            return Connection.CurrentConnection.Clients.FirstOrDefault(c => c.Identity == this);
        }
    }
}