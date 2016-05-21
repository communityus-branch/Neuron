using UnityEngine;

namespace Static_Interface.API.SerializationFramework.Impl
{
    public class Vector3Serializer : Serializer<Vector3>
    {
        public override byte[] Serialize(Vector3 obj)
        {
            DataBuffer buffer = new DataBuffer();
            buffer.Write(obj.x);
            buffer.Write(obj.y);
            buffer.Write(obj.z);
            return buffer.GetBytes();
        }

        protected override Vector3 Deserialize(byte[] data)
        {
            DataBuffer buffer = new DataBuffer(data);
            float x = buffer.Read<float>();
            float y = buffer.Read<float>();
            float z = buffer.Read<float>();
            return new Vector3(x,y,z);
        }

        public override int GetLength(DataBuffer buffer)
        {
            return sizeof (float) * 3;
        }
    }
}