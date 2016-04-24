using System;
using System.Globalization;
using System.Reflection;

namespace Static_Interface.API.ConsoleFramework
{
    public abstract class AbstractConsoleCommand
    {
        public readonly string CommandName;
        public string Usage = string.Empty;
        public string Help = string.Empty;
        public ConsoleCommandRuntime CommandRuntime = ConsoleCommandRuntime.BOTH;

        protected AbstractConsoleCommand(string name)
        {
            CommandName = name;
        }

        public abstract void Execute(string[] args);
    }

    public class ConsoleCommand : AbstractConsoleCommand
    {
        private readonly object _instance;
        private readonly MethodInfo _method;

        public ConsoleCommand(string name, object instance, MethodInfo method) : base(name)
        {
            _instance = instance;
            _method = method;
        }

        public override void Execute(string[] args)
        {
            if (args.Length != _method.GetParameters().Length)
            {
                PrintUsage();
                return;
            }
            var parameters = _method.GetParameters();
            Type[] methodArgs = new Type[parameters.Length];
            for (int i = 0; i < args.Length; i++)
            {
                methodArgs[i] = parameters[i].ParameterType;
            }

            object[] parsedArgs = Console.Instance.ParseArgs(args, methodArgs);
            if (parsedArgs == null)
            {
                PrintUsage();
                return;
            }

            BindingFlags bindingFlags;
            if (_instance == null)
            {
                bindingFlags = BindingFlags.Public | BindingFlags.Static;
            }
            else
            {
                bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            }
            _method.Invoke(_instance, bindingFlags, null, parsedArgs, CultureInfo.CurrentCulture);
        }

        private void PrintUsage()
        {
            Console.Instance.Print("Usage: " + CommandName + " " + Usage);
        }
    }
}