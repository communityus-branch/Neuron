using System.Collections.Generic;
using Static_Interface.API.PlayerFramework;

namespace Static_Interface.Internal.MultiplayerFramework
{
    public class QueuedData
    {
        public Identity Ident { get; set; }
        public List<byte> Data { get; } = new List<byte>();
    }
}