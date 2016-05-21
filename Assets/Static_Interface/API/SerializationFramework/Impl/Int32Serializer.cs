using System;

namespace Static_Interface.API.SerializationFramework.Impl
{
    public class Int32Serializer : Serializer<int>
    {
        public override byte[] Serialize(int obj)
        {
            return BitConverter.GetBytes(obj);
        }

        protected override int Deserialize(byte[] data)
        {
            return BitConverter.ToInt32(data, 0);
        }

        public override int GetLength(DataBuffer data)
        {
            return sizeof(int);
        }
    }
}