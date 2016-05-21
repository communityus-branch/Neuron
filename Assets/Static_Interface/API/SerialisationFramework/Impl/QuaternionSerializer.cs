using UnityEngine;

namespace Static_Interface.API.SerialisationFramework.Impl
{
    public class QuaternionSerializer : Serializer<Quaternion>
    {
        public override byte[] Serialize(Quaternion obj)
        {
            DataBuffer buffer = new DataBuffer();
            buffer.Write(obj.eulerAngles);
            return buffer.GetBytes();
        }

        protected override Quaternion Deserialize(byte[] data)
        {
            DataBuffer buffer = new DataBuffer(data);
            Vector3 euler = buffer.Read<Vector3>();
            return Quaternion.Euler(euler);
        }

        public override int GetLength(DataBuffer buffer)
        {
            return sizeof (float)*3;
        }
    }
}