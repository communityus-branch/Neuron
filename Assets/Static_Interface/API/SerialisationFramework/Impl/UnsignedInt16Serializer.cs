using System;

namespace Static_Interface.API.SerialisationFramework.Impl
{
    public class UnsignedInt16Serializer  : Serializer<ushort>
    {
        public override byte[] Serialize(ushort obj)
        {
            return BitConverter.GetBytes(obj);
        }

        protected override ushort Deserialize(byte[] data)
        {
            return BitConverter.ToUInt16(data, 0);
        }

        public override int GetLength(DataBuffer data)
        {
            return sizeof(ushort);
        }
    }
}