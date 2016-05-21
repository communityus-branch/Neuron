namespace Static_Interface.API.SerialisationFramework.Impl
{
    public class SignedByteSerializer : Serializer<sbyte>
    {
        public override byte[] Serialize(sbyte obj)
        {
            return new[] {(byte) obj};
        }

        protected override sbyte Deserialize(byte[] data)
        {
            return (sbyte) data[0];
        }

        public override int GetLength(DataBuffer data)
        {
            return sizeof(sbyte);
        }
    }
}