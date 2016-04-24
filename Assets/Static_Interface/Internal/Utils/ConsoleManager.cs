using System;
using Static_Interface.API.UnityExtensions;
using Static_Interface.API.Utils;
using Static_Interface.Internal.Windows;
using UnityEngine;
using Console = Static_Interface.API.ConsoleFramework.Console;
namespace Static_Interface.Internal.Utils
{
    public class ConsoleManager : SingletonComponent<ConsoleManager>
    {
        public ConsoleWindow ConsoleWindow = new ConsoleWindow();
        private readonly ConsoleInput _input = new ConsoleInput();

        private void HandleLog(string message, string stackTrace, LogType type)
        {
            if (string.IsNullOrEmpty(message)) return;
            switch (type)
            {
                case LogType.Warning:
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogType.Error:
                    System.Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogType.Exception:
                    System.Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogType.Assert:
                    System.Console.ForegroundColor = ConsoleColor.Red;
                    break;
                default:
                    System.Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }

            _input.ClearLine(3);
            System.Console.WriteLine(message);
            _input.RedrawInputLine();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            LogUtils.Debug("Destroying console");
            Application.logMessageReceived -= HandleLog;
            _input.OnInputText -= OnInputText;
            ConsoleWindow.Shutdown();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            DontDestroyOnLoad(gameObject);
            ConsoleWindow.Initialize();
            ConsoleWindow.SetTitle("Neuron Console");
            Application.logMessageReceived += HandleLog;
            _input.OnInputText += OnInputText;
            _input.ClearLine(System.Console.WindowHeight);
        }

        private void OnInputText(string obj)
        {
            Console.Instance.Eval(obj);
        }

        protected override void Update()
        {
            base.Update();
            _input.Update();
            
            foreach (var line in Console.Instance.Lines)
            {
                 HandleLog(line, null, LogType.Log);
            }

            Console.Instance.Lines.Clear();
        }
    }
}
