using System;

namespace Static_Interface.API.SerialisationFramework.Impl
{
    public class Int64Serializer : Serializer<long>
    {
        public override byte[] Serialize(long obj)
        {
            return BitConverter.GetBytes(obj);
        }

        protected override long Deserialize(byte[] data)
        {
            return BitConverter.ToInt16(data, 0);
        }

        public override int GetLength(DataBuffer data)
        {
            return sizeof(long);
        }
    }
}