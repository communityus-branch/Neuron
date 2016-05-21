using System;

namespace Static_Interface.API.SerializationFramework.Impl
{
    public class UnsignedInt32Serializer : Serializer<uint>
    {
        public override byte[] Serialize(uint obj)
        {
            return BitConverter.GetBytes(obj);
        }

        protected override uint Deserialize(byte[] data)
        {
            return BitConverter.ToUInt32(data, 0);
        }

        public override int GetLength(DataBuffer data)
        {
            return sizeof(uint);
        }
    }
}