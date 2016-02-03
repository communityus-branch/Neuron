using System;
using System.Collections.Generic;

namespace Static_Interface.Internal.MultiplayerFramework.Impl.Steamworks
{
    public class SteamworksCommon
    {
        public static uint GetUInt32FromIp(string ip)
        {
            string[] strArray = GetComponentsFromSerial(ip, '.');
            return ((((UInt32.Parse(strArray[0]) << 0x18) | (UInt32.Parse(strArray[1]) << 0x10)) | (UInt32.Parse(strArray[2]) << 8)) | UInt32.Parse(strArray[3]));
        }

        public static string[] GetComponentsFromSerial(string serial, char delimiter)
        {
            int index;
            List<string> list = new List<string>();
            for (int i = 0; i < serial.Length; i = index + 1)
            {
                index = serial.IndexOf(delimiter, i);
                if (index == -1)
                {
                    list.Add(serial.Substring(i, serial.Length - i));
                    break;
                }
                list.Add(serial.Substring(i, index - i));
            }
            return list.ToArray();
        }
    }
}