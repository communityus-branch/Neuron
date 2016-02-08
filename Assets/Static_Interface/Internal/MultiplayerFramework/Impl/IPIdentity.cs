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
            ID = BitConverter.ToUInt32(address.GetAddressBytes(), 0);
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

        public bool IsServer => this == Server;

        public static explicit operator IPIdentity(ulong id)
        {
            return new IPIdentity(id);
        }
    }
}