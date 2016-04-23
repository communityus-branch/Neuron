using UnityEngine;

namespace Static_Interface.API.Utils
{
    public struct KeyState
    {
        public int KeyCode { get; set; }
        public bool IsDown { get; set; }
        public bool IsPressed { get; set; }

        public override string ToString()
        {
            return ((KeyCode) KeyCode).ToString();
        }
    }

    public class KeyStates
    {
        public const int UP = 0;
        public const int DOWN = 1;
    }
}