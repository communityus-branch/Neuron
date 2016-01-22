using Steamworks;
using UnityEngine;
using System;
using Static_Interface.Internal;

namespace Static_Interface.Internal.Objects
{
    public class Types
    {
        public static readonly Type BOOLEAN_ARRAY_TYPE = typeof(bool[]);
        public static readonly Type BOOLEAN_TYPE = typeof(bool);
        public static readonly Type BYTE_ARRAY_TYPE = typeof(byte[]);
        public static readonly Type BYTE_TYPE = typeof(byte);
        public static readonly Type COLOR_TYPE = typeof(Color);
        public static readonly Type INT16_TYPE = typeof(short);
        public static readonly Type INT32_ARRAY_TYPE = typeof(int[]);
        public static readonly Type INT32_TYPE = typeof(int);
        public static readonly Type INT64_TYPE = typeof(long);
        public static readonly byte[] SHIFTS = { 1, 2, 4, 8, 0x10, 0x20, 0x40, 0x80 };
        public static readonly Type SINGLE_TYPE = typeof(float);
        public static readonly Type STEAM_ID_TYPE = typeof(CSteamID);
        public static readonly Type STRING_TYPE = typeof(string);
        public static readonly Type UINT16_TYPE = typeof(ushort);
        public static readonly Type UINT32_TYPE = typeof(uint);
        public static readonly Type UINT64_ARRAY_TYPE = typeof(ulong[]);
        public static readonly Type UINT64_TYPE = typeof(ulong);
        public static readonly Type VECTOR3_TYPE = typeof(Vector3);
        public static readonly Type KEYSTATE_TYPE = typeof (KeyState);
    }

    public class ObjectSerializer
    {
        private static readonly Block block = new Block();

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

        public static object[] GetObjects(CSteamID steamID, int offset, int prefix, byte[] bytes, params Type[] types)
        {
            block.Reset(offset + prefix, bytes);
            if (types[0] == Types.STEAM_ID_TYPE)
            {
                object[] objArray = block.Read(1, types);
                objArray[0] = steamID;
                return objArray;
            }
            return block.Read(types);
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
