﻿using Static_Interface.API.PlayerFramework;
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

        public static implicit operator CSteamID(SteamIdentity ident)
        {
            return ident.SteamID;
        }

        public static implicit operator SteamIdentity(CSteamID id)
        {
            return new SteamIdentity(id);
        }
        public static bool operator ==(SteamIdentity a, CSteamID b)
        {
            if ((object)a == null && b == CSteamID.Nil) return true;
            if ((object)a == null || b == CSteamID.Nil) return false;
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
            bool baseEquals = base.Equals(obj);
            if (baseEquals) return true;

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