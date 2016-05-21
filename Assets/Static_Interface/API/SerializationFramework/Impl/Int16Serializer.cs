using System;

namespace Static_Interface.API.SerializationFramework.Impl
{
    public class Int16Serializer : Serializer<short>
    {
        public override byte[] Serialize(short obj)
        {
            return BitConverter.GetBytes(obj);
        }

        protected override short Deserialize(byte[] data)
        {
            return BitConverter.ToInt16(data, 0);
        }

        public override int GetLength(DataBuffer data)
        {
            return sizeof(short);
        }
    }
}