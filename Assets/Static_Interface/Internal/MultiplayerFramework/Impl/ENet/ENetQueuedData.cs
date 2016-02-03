using System.Collections.Generic;

namespace Static_Interface.Internal.MultiplayerFramework.Impl.ENet
{
    public class ENetQueuedData
    {
        public ENetIdentity Ident { get; set; }
        public List<byte> Data { get; } = new List<byte>();
    }
}