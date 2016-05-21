namespace Static_Interface.API.SerializationFramework.Impl
{
    public class ByteSerializer : Serializer<byte>
    {
        public override byte[] Serialize(byte obj)
        {
            return new[] {obj};
        }

        protected override byte Deserialize(byte[] data)
        {
            return data[0];
        }

        public override int GetLength(DataBuffer data)
        {
            return sizeof(byte);
        }
    }
}