using System;
using System.Collections.Generic;

namespace Static_Interface.API.SerializationFramework.Impl
{
    public class DecimalSerializer : Serializer<decimal>
    {
        public override byte[] Serialize(decimal obj)
        {
            int[] bits = decimal.GetBits(obj);
            List<byte> bytes = new List<byte>();
            foreach (int i in bits)
            {
                bytes.AddRange(BitConverter.GetBytes(i));
            }

            return bytes.ToArray();
        }

        protected override decimal Deserialize(byte[] data)
        {
            int[] bits = new int[4];
            for (int i = 0; i < sizeof(decimal); i += 4)
            {
                bits[i / 4] = BitConverter.ToInt32(data, i);
            }
            return new decimal(bits);
        }

        public override int GetLength(DataBuffer buffer)
        {
            return sizeof (decimal);
        }
    }
}