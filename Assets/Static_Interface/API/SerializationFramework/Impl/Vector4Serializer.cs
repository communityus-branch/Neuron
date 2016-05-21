using UnityEngine;

namespace Static_Interface.API.SerializationFramework.Impl
{
    public class Vector4Serializer : Serializer<Vector4>
    {
        public override byte[] Serialize(Vector4 obj)
        {
            DataBuffer buffer = new DataBuffer();
            buffer.Write(obj.x);
            buffer.Write(obj.y);
            buffer.Write(obj.z);
            buffer.Write(obj.w);
            return buffer.GetBytes();
        }

        protected override Vector4 Deserialize(byte[] data)
        {
            DataBuffer buffer = new DataBuffer(data);
            float x = buffer.Read<float>();
            float y = buffer.Read<float>();
            float z = buffer.Read<float>();
            float w = buffer.Read<float>();
            return new Vector4(x, y, z, w);
        }

        public override int GetLength(DataBuffer buffer)
        {
            return sizeof(float) * 4;
        }
    }
}