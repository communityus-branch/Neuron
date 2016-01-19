using System.Runtime.InteropServices;
using UnityEngine;

namespace Static_Interface.Multiplayer.Protocol
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NetworkSnapshot
    {
        public Vector3 pos;
        public Quaternion rot;
        public float timestamp;
    }
}