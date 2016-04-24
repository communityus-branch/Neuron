using System;
using UnityEngine;

namespace Static_Interface.Internal.Windows
{
    public class ConsoleInput
    {
        private const int CONSOLE_HEIGHT = 300;
        private const int CONSOLE_WIDTH = 500;

        public string InputString = string.Empty;
        internal float NextUpdate;

        public event Action<string> OnInputText;

        public void ClearLine(int numLines)
        {
            Console.CursorLeft = 0;
            Console.Write(new string(' ', LineWidth * numLines));
            Console.CursorTop -= numLines;
            Console.CursorLeft = 0;
        }

        internal void OnBackspace()
        {
            if (InputString.Length < 1) return;
            InputString = InputString.Substring(0, InputString.Length - 1);
            RedrawInputLine();
        }

        internal void OnEnter()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("> " + InputString);
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


        private void CheckAndResetWindowSize()
        {
            if (Console.WindowWidth != CONSOLE_WIDTH || Console.WindowHeight != CONSOLE_HEIGHT)
            {
                Console.SetWindowSize(CONSOLE_WIDTH, CONSOLE_HEIGHT);
            }
        }


        public void RedrawInputLine()
        {
            try
            {
                CheckAndResetWindowSize();
                Console.ForegroundColor = ConsoleColor.White;
                Console.CursorLeft = 0;
                Console.CursorTop = Console.WindowTop + Console.WindowHeight - 1;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Green;
                ClearLine(1);
                if (InputString.Length == 0) return;
                Console.Write(InputString.Length < (LineWidth - 2)
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
                if (!Console.KeyAvailable)
                {
                    return;
                }
            }
            catch (Exception)
            {
                return;
            }
            ConsoleKeyInfo info = Console.ReadKey();
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

        public int LineWidth => Console.BufferWidth;

        public bool Valid => (Console.BufferWidth > 0);
    }
}
