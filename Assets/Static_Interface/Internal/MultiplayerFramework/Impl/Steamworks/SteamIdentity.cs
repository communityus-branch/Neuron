using Static_Interface.API.PlayerFramework;
using Steamworks;

namespace Static_Interface.Internal.MultiplayerFramework.Impl.Steamworks
{
    public class SteamIdentity : Identity
    {
        public CSteamID SteamID { get; }
        public SteamIdentity(CSteamID id)
        {
            SteamID = id;
        }

        public override bool IsValid()
        {
            return SteamID != CSteamID.Nil;
        }

        public override ulong Serialize()
        {
            return SteamID.m_SteamID;
        }

        public static SteamIdentity Deserialize(ulong steamId)
        {
            CSteamID id = new CSteamID(steamId);
            return (SteamIdentity) id;
        }

        public static explicit operator CSteamID(SteamIdentity ident)
        {
            return ident.SteamID;
        }

        public static explicit operator SteamIdentity(CSteamID id)
        {
            //Todo;
            return null;
        }

        public static bool operator ==(SteamIdentity a, SteamIdentity b)
        {
            if ((object)a == null && (object)b == null) return true;
            if ((object)a == null || (object)b == null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(SteamIdentity a, SteamIdentity b)
        {
            return !(a == b);
        }

        public static bool operator ==(SteamIdentity a, CSteamID b)
        {
            if (a == null && b == CSteamID.Nil) return true;
            if (a == null || b == CSteamID.Nil) return false;
            return a.SteamID == b;
        }

        public static bool operator !=(SteamIdentity a, CSteamID b)
        {
            return !(a == b);
        }

        public static bool operator ==(CSteamID a, SteamIdentity b)
        {
            return b == a;
        }

        public static bool operator !=(CSteamID a, SteamIdentity b)
        {
            return b != a;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is SteamIdentity)
            {
                return ((SteamIdentity) obj).SteamID == SteamID;
            }
            if (obj is CSteamID)
            {
                return (CSteamID) obj == SteamID;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return SteamID.GetHashCode();
        }
    }
}