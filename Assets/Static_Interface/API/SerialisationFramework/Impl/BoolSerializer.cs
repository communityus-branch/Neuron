using System;

namespace Static_Interface.API.SerialisationFramework.Impl
{
    public class BoolSerializer : Serializer<bool>
    {
        public override byte[] Serialize(bool obj)
        {
            return BitConverter.GetBytes(obj);
        }

        protected override bool Deserialize(byte[] data)
        {
            return BitConverter.ToBoolean(data, 0);
        }

        public override int GetLength(DataBuffer data)
        {
            return sizeof (bool);
        } 
    }
}