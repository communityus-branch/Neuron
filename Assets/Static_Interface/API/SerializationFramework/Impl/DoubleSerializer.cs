using System;

namespace Static_Interface.API.SerializationFramework.Impl
{
    public class DoubleSerializer : Serializer<double>
    {
        public override byte[] Serialize(double obj)
        {
            return BitConverter.GetBytes(obj);
        }

        protected override double Deserialize(byte[] data)
        {
            return BitConverter.ToDouble(data, 0);
        }

        public override int GetLength(DataBuffer buffer)
        {
            return sizeof(double);
        }
    }
}