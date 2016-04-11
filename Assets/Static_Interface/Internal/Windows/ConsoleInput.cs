using System;
using UnityEngine;

namespace Static_Interface.Internal.Windows
{
    public class ConsoleInput
    {
        public string InputString = string.Empty;
        internal float NextUpdate;
        public string[] StatusText = { string.Empty, string.Empty, string.Empty };

        public event Action<string> OnInputText;

        public void ClearLine(int numLines)
        {
            System.Console.CursorLeft = 0;
            System.Console.Write(new string(' ', LineWidth * numLines));
            System.Console.CursorTop -= numLines;
            System.Console.CursorLeft = 0;
        }

        internal void OnBackspace()
        {
            if (InputString.Length < 1) return;
            InputString = InputString.Substring(0, InputString.Length - 1);
            RedrawInputLine();
        }

        internal void OnEnter()
        {
            ClearLine(StatusText.Length);
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("> " + InputString);
            string inputString = InputString;
            InputString = string.Empty;
            OnInputText?.Invoke(inputString);
            RedrawInputLine();
        }

        internal void OnEscape()
        {
            InputString = string.Empty;
            RedrawInputLine();
        }

        public void RedrawInputLine()
        {
            try
            {
                System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.CursorTop++;
                foreach (string str in StatusText)
                {
                    System.Console.CursorLeft = 0;
                    System.Console.Write(str.PadRight(LineWidth));
                }
                System.Console.CursorTop -= StatusText.Length + 1;
                System.Console.CursorLeft = 0;
                System.Console.BackgroundColor = ConsoleColor.Black;
                System.Console.ForegroundColor = ConsoleColor.Green;
                ClearLine(1);
                if (InputString.Length == 0) return;
                System.Console.Write(InputString.Length < (LineWidth - 2)
                    ? InputString
                    : InputString.Substring(InputString.Length - (LineWidth - 2)));
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void Update()
        {
            if (!Valid) return;
            if (NextUpdate < Time.realtimeSinceStartup)
            {
                RedrawInputLine();
                NextUpdate = Time.realtimeSinceStartup + 0.5f;
            }
            try
            {
                if (!System.Console.KeyAvailable)
                {
                    return;
                }
            }
            catch (Exception)
            {
                return;
            }
            ConsoleKeyInfo info = System.Console.ReadKey();
            if (info.Key == ConsoleKey.Enter)
            {
                OnEnter();
            }
            else if (info.Key == ConsoleKey.Backspace)
            {
                OnBackspace();
            }
            else if (info.Key == ConsoleKey.Escape)
            {
                OnEscape();
            }
            else if (info.KeyChar != '\0')
            {
                InputString = InputString + info.KeyChar;
                RedrawInputLine();
            }
        }

        public int LineWidth => System.Console.BufferWidth;

        public bool Valid => (System.Console.BufferWidth > 0);
    }
}
