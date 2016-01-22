using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Steamworks;

namespace Static_Interface.API.Utils
{
    public class Hash
    {
        private static readonly SHA1CryptoServiceProvider Service = new SHA1CryptoServiceProvider();
        public static byte[] Combine(params byte[][] hashes)
        {
            var array = new byte[hashes.Length * 20];
            for (var i = 0; i < hashes.Length; i++)
            {
                hashes[i].CopyTo(array, (i * 20));
            }
            return SHA1(array);
        }

        public static byte[] SHA1(byte[] bytes)
        {
            return Service.ComputeHash(bytes);
        }

        public static byte[] SHA1(CSteamID steamID)
        {
            return SHA1(BitConverter.GetBytes(steamID.m_SteamID));
        }

        public static byte[] SHA1(Stream stream)
        {
            return Service.ComputeHash(stream);
        }

        public static byte[] SHA1(string text)
        {
            return SHA1(Encoding.UTF8.GetBytes(text));
        }

        public static bool VerifyHash(byte[] 
            hash0, byte[] hash1)
        {
            if ((hash0.Length != 20) || (hash1.Length != 20))
            {
                return false;
            }
            return !hash0.Where((t, i) => t != hash1[i]).Any();
        }
    }
}