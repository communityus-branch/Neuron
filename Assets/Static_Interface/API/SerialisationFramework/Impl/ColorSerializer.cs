using UnityEngine;

namespace Static_Interface.API.SerialisationFramework.Impl
{
    public class ColorSerializer : Serializer<Color>
    {
        public override byte[] Serialize(Color obj)
        {
            byte[] data = {
                (byte)(obj.r * 255f),
                (byte)(obj.g * 255f),
                (byte)(obj.b * 255f),
                (byte)(obj.a * 255f)
             };
            return data;
        }

        protected override Color Deserialize(byte[] data)
        {
            Color color = new Color
            {
                r = data[0]/255,
                g = data[1]/255,
                b = data[2]/255,
                a = data[3]/255
            };
            return color;
        }

        public override int GetLength(DataBuffer data)
        {
            return sizeof(byte) * 4;
        }
    }
}