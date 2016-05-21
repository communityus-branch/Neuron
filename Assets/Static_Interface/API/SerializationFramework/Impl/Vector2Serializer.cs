using UnityEngine;

namespace Static_Interface.API.SerializationFramework.Impl
{
    public class Vector2Serializer : Serializer<Vector2>
    {
        public override byte[] Serialize(Vector2 obj)
        {
            DataBuffer buffer = new DataBuffer();
            buffer.Write(obj.x);
            buffer.Write(obj.y);
            return buffer.GetBytes();
        }

        protected override Vector2 Deserialize(byte[] data)
        {
            DataBuffer buffer = new DataBuffer(data);
            float x = buffer.Read<float>();
            float y = buffer.Read<float>();
            return new Vector2(x, y);
        }

        public override int GetLength(DataBuffer buffer)
        {
            return sizeof(float) * 2;
        }
    }
}