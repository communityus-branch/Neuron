using System;

namespace Static_Interface.API.SerializationFramework.Impl
{
    public class UnsignedInt64Serializer : Serializer<ulong>
    {
        public override byte[] Serialize(ulong obj)
        {
            return BitConverter.GetBytes(obj);
        }

        protected override ulong Deserialize(byte[] data)
        {
            return BitConverter.ToUInt64(data, 0);
        }

        public override int GetLength(DataBuffer data)
        {
            return sizeof(ulong);
        }
    }
}