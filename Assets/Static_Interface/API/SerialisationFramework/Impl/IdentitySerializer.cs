using Static_Interface.API.PlayerFramework;
using Static_Interface.Internal.MultiplayerFramework;

namespace Static_Interface.API.SerialisationFramework.Impl
{
    public class IdentitySerializer : Serializer<Identity>
    {
        public override byte[] Serialize(Identity obj)
        {
            ulong serialized = obj;
            DataBuffer buffer = new DataBuffer();
            buffer.Write(serialized);
            return buffer.GetBytes();
        }

        protected override Identity Deserialize(byte[] data)
        {
            DataBuffer buffer = new DataBuffer(data);
            ulong serialized = buffer.Read<ulong>();
            return Connection.CurrentConnection.Provider.Deserialilze(serialized);
        }

        public override int GetLength(DataBuffer buffer)
        {
            return sizeof (ulong);
        }
    }
}