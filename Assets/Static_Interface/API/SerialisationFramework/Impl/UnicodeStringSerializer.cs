using System;
namespace Static_Interface.API.SerialisationFramework.Impl
{
    public class UnicodeStringSerializer : Serializer<string>
    {
        protected override string Deserialize(byte[] data)
        {
            char[] charArray = new char[data.Length/sizeof(char)];
            int dataPos = 0;
            for (int i = 0; i < charArray.Length; i++)
            {
                byte[] charData = new byte[sizeof(char)];
                for (int j = 0; j < sizeof(char); j++)
                {
                    charData[j] = data[dataPos];
                    dataPos++;
                }
                char c = BitConverter.ToChar(charData, 0);
                charArray[i] = c;
            }
            return new string(charArray);
        }

        public override byte[] Serialize(string obj)
        {
            //length = string char length * 2 + string length (every char has 2 bytes)
            var arr = obj.ToCharArray();
            byte[] data = new byte[arr.Length * sizeof(char) + sizeof(int)];
            byte[] sizeData = BitConverter.GetBytes(obj.Length);
            for (int i = 0; i < sizeData.Length; i++)
            {
                data[i] = sizeData[i];
            }

            int pos = sizeData.Length;

            foreach (char c in arr)
            {
                byte[] charData = BitConverter.GetBytes(c);
                for (int i = 0; i < sizeof(char); i++)
                {
                    data[pos] = charData[i];
                    pos++;
                }
            }
            return data;
        }
        public override int GetLength(DataBuffer buffer)
        {
            byte[] sizeData = buffer.ReadBytes(sizeof(int));
            int size = BitConverter.ToInt32(sizeData, 0);
            return size * sizeof(char); 
        }
    }
}