using System;

namespace Static_Interface.API.NetworkFramework
{
    [AttributeUsage(AttributeTargets.Method)]
    public class NetworkCallAttribute : Attribute
    {
        public ConnectionEnd ConnectionEnd = ConnectionEnd.BOTH;
        public bool ValidateServer;
        public bool ValidateOwner;
        public float MaxRadius;
        public EPacket PacketType = EPacket.UPDATE_RELIABLE_BUFFER; 
    }

    public enum ConnectionEnd
    {
        BOTH,
        CLIENT,
        SERVER
    }
}