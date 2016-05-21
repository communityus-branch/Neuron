using System;

namespace Static_Interface.API.SerialisationFramework.Impl
{
    public class SingleSerializer : Serializer<float>
    {
        public override byte[] Serialize(float obj)
        {
            return BitConverter.GetBytes(obj);
        }

        protected override float Deserialize(byte[] data)
        {
            return BitConverter.ToSingle(data, 0);
        }

        public override int GetLength(DataBuffer buffer)
        {
            return sizeof (float);
        }
    }
}