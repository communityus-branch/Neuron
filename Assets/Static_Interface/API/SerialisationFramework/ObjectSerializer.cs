using System;
using Static_Interface.API.PlayerFramework;

namespace Static_Interface.API.SerialisationFramework
{
    public class ObjectSerializer
    {
        private static readonly Block block = new Block();
        public static Block Block => block;
        public static void CloseRead()
        {
        }

        public static byte[] CloseWrite(out int size)
        {
            return block.GetBytes(out size);
        }

        public static byte[] GetBytes(int prefix, out int size, params object[] objects)
        {
            block.Reset(prefix);
            block.Write(objects);
            return block.GetBytes(out size);
        }

        public static object[] GetObjects(int offset, int prefix, byte[] bytes, params Type[] types)
        {
            return GetObjects(null, offset, prefix, bytes, false, types);
        }

        public static object[] GetObjects(Identity ident, int offset, int prefix, byte[] bytes, bool isChannel, params Type[] types)
        {
            block.Reset(offset + prefix, bytes);
            if (!isChannel) return block.Read(types);

            object[] objArray = block.Read(1, types);
            objArray[0] = ident;
            return objArray;
        }

        public static void OpenRead(int prefix, byte[] bytes)
        {
            block.Reset(prefix, bytes);
        }

        public static void OpenWrite(int prefix)
        {
            block.Reset(prefix);
        }

        public static object[] Read(params Type[] types)
        {
            return block.Read(types);
        }

        public static void Write(params object[] objects)
        {
            block.Write(objects);
        }
    }
}
