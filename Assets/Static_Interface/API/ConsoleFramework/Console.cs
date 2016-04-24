using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Static_Interface.API.NetvarFramework;
using Static_Interface.API.PlayerFramework;
using Static_Interface.API.UnityExtensions;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework;
using UnityEngine;

namespace Static_Interface.API.ConsoleFramework
{
    //Todo - Extensions: Destroy all commands when world unloads

    public class Console : PersistentScript<Console>
    {
        private readonly Dictionary<Type, object> _parsers = new Dictionary<Type, object>();
        public delegate bool ParserCallback<T>(string str, out T obj);
        public List<string> Lines = new List<string>();
        private readonly List<AbstractConsoleCommand> _commands = new List<AbstractConsoleCommand>();
        public int MaxLineWidth { get; set; }

        protected override void Awake()
        {
            base.Awake();

            //C# primitives
            RegisterParser<byte>(ParseByte);
            RegisterParser<sbyte>(ParseSByte);
            RegisterParser<int>(ParseInt);
            RegisterParser<uint>(ParseUInt);
            RegisterParser<short>(ParseShort);
            RegisterParser<ushort>(ParseUShort);
            RegisterParser<long>(ParseLong);
            RegisterParser<ulong>(ParseULong);
            RegisterParser<float>(ParseFloat);
            RegisterParser<double>(ParseDouble);
            RegisterParser<char>(ParseChar);
            RegisterParser<bool>(ParseBool);
            RegisterParser<decimal>(ParseDecimal);
            RegisterParser<string>(ParseString);

            //Unity
            RegisterParser<Vector3>(ParseVector3);
            RegisterParser<Vector2>(ParseVector2);
            
            //Neuron API
            RegisterParser<Identity>(ParseIdentity);
            RegisterParser<User>(ParseUser);
            RegisterParser<Player>(ParsePlayer);
            RegisterParser<Netvar>(ParseNetvar);
        }

        private bool ParseNetvar(string line, out Netvar obj)
        {
            obj = NetvarManager.Instance.GetNetvar(line);
            return obj != null;
        }
        
        private bool ParsePlayer(string str, out Player obj)
        {
            obj = null;
            User user;
            bool success = ParseUser(str, out user, false);
            if (!success)
            {
                Print("Player \"" + str + "\" not found");
                return false;
            }
            obj = user.Player;
            return true;
        }

        private bool ParseUser(string str, out User obj, bool showMessage)
        {
            obj = null;
            if (Connection.CurrentConnection == null) return false;
            foreach (var client in Connection.CurrentConnection.Clients.Where(client => client.Name.ToLower().Equals(str.ToLower())))
            {
                obj = client;
                return true;
            }

            if(showMessage) Print("User \"" + str + "\" not found");
            return false;
        }

        private bool ParseUser(string str, out User obj)
        {
            return ParseUser(str, out obj, true);
        }

        private bool ParseIdentity(string str, out Identity obj)
        {
            obj = null;
            User user;
            bool success = ParseUser(str, out user, false);
            if (success)
            {
                obj = user.Identity;
                return true;
            }

            ulong val;
            success = ParseULong(str, out val);
            if (success)
            {
                obj = val;
                return true;
            }

            Print("Identity \"" + str + "\" not valid");
            return false;
        }

        private bool ParseVector2(string str, out Vector2 obj)
        {
            obj = new Vector2();
            var comp = str.Split(',');
            if (comp.Length != 2) return false;
            float t;
            if (!float.TryParse(comp[0], out t))
            {
                return false;
            }
            obj.x = t;
            if (!float.TryParse(comp[1], out t))
            {
                return false;
            }
            obj.y = t;
            return true;
        }

        private bool ParseVector3(string str, out Vector3 obj)
        {
            obj = new Vector3();
            var comp = str.Split(',');
            if (comp.Length != 3) return false;
            float t;
            if (!float.TryParse(comp[0], out t))
            {
                return false;
            }
            obj.x = t;
            if (!float.TryParse(comp[1], out t))
            {
                return false;
            }
            obj.y = t;
            if (!float.TryParse(comp[2], out t))
            {
                return false;
            }
            obj.z = t;
            
            return true;
        }

        private bool ParseBool(string str, out bool obj)
        {
            str = str.Trim().ToLower();
            switch (str)
            {
                case "1":
                case "true":
                case "yes":
                    obj = true;
                    break;
                case "0":
                case "false":
                case "no":
                    obj = false;
                    break;
                default:
                    obj = false;
                    return false;
            }
            return true;
        }

        private bool ParseDecimal(string str, out decimal obj)
        {
            return decimal.TryParse(str, out obj);
        }

        private bool ParseChar(string str, out char obj)
        {
            return char.TryParse(str, out obj);
        }

        private bool ParseDouble(string str, out double obj)
        {
            return double.TryParse(str, out obj);
        }

        private bool ParseULong(string str, out ulong obj)
        {
            return ulong.TryParse(str, out obj);
        }

        private bool ParseLong(string str, out long obj)
        {
            return long.TryParse(str, out obj);
        }

        private bool ParseUShort(string str, out ushort obj)
        {
            return ushort.TryParse(str, out obj);
        }

        private bool ParseShort(string str, out short obj)
        {
            return short.TryParse(str, out obj);
        }

        private bool ParseUInt(string str, out uint obj)
        {
            return uint.TryParse(str, out obj);
        }

        private bool ParseSByte(string str, out sbyte obj)
        {
            return sbyte.TryParse(str, out obj);
        }

        private bool ParseByte(string str, out byte obj)
        {
            return byte.TryParse(str, out obj);
        }

        private bool ParseString(string str, out string obj)
        {
            obj = str;
            return true;
        }

        private bool ParseFloat(string str, out float obj)
        {
            return float.TryParse(str, out obj);
        }

        private bool ParseInt(string str, out int obj)
        {
            return int.TryParse(str, out obj);
        }


        public void RegisterParser<T>(ParserCallback<T> callback)
        {
            _parsers[typeof (T)] = callback;
        }

        public ParserCallback<T> GetParser<T>()
        {
            Type t = typeof (T);
            if (!_parsers.ContainsKey(t)) return null;
            return (ParserCallback<T>) _parsers[t];
        }

        public object[] ParseArgs(string[] args, Type[] types)
        {
            int length = args.Length;
            object[] parameters = new object[args.Length];
            for (int paramIndex = 0; paramIndex < length; paramIndex++)
            {
                var type = types[paramIndex];
                var arg = args[paramIndex];

                object parserCallback;
                if (!_parsers.TryGetValue(type, out parserCallback))
                {
                    Print("Invalid Parameter Type: Parameter " + paramIndex + "  has no parser for that type");
                    return null;
                }

                object obj;
                if (!InvokeParseCallback(parserCallback, type, arg, out obj))
                    return null; // Parsing failed

                parameters[paramIndex] = obj;
            }
            return parameters;
        }

        public void Print(string s)
        {
            Lines.Add(s);
        }

        public void UnregisterCommands(object o)
        {
            UnregisterCommands(o.GetType());
        }

        public void UnregisterCommands(Type type)
        {
            foreach (MethodInfo m in type.GetMethods())
            {
                if (m.GetCustomAttributes(typeof(ConsoleCommandAttribute), true).Length <= 0) continue;
                ConsoleCommandAttribute attribute = (ConsoleCommandAttribute)m.GetCustomAttributes(typeof(ConsoleCommandAttribute), true)[0];
                var cmdName = m.Name.ToLower();
                if (!string.IsNullOrEmpty(attribute.Name))
                {
                    cmdName = attribute.Name;
                }

                UnregisterCommand(cmdName);
            }
        }

        private void UnregisterCommand(string cmd)
        {
            AbstractConsoleCommand command =
                _commands.FirstOrDefault(c => c.CommandName.ToLower().Trim().Equals(cmd.Trim().ToLower()));
            if (command == null) return;
            _commands.Remove(command);
        }

        public void RegisterCommands(object o)
        {
            RegisterCommands(o.GetType(), o);
        }

        public void RegisterCommands(Type type)
        {
            RegisterCommands(type, null);
        }

        private void RegisterCommands(Type type, object instance)
        {
            foreach (MethodInfo m in type.GetMethods())
            {
                if (m.GetCustomAttributes(typeof (ConsoleCommandAttribute), true).Length <= 0) continue;
                ConsoleCommandAttribute attribute = (ConsoleCommandAttribute) m.GetCustomAttributes(typeof(ConsoleCommandAttribute), true)[0];
                CommandHelpAttribute helpAttribute = null;
                if (m.GetCustomAttributes(typeof (CommandHelpAttribute), true).Length > 0)
                {
                    helpAttribute = (CommandHelpAttribute) m.GetCustomAttributes(typeof(CommandHelpAttribute), true)[0];
                }

                string helpMessage = string.Empty;
                if (helpAttribute != null)
                    helpMessage = helpAttribute.Help;


                CommandUsageAttribute usageAttribute = null;
                if (m.GetCustomAttributes(typeof(CommandUsageAttribute), true).Length > 0)
                {
                    usageAttribute = (CommandUsageAttribute)m.GetCustomAttributes(typeof(CommandUsageAttribute), true)[0];
                }

                string usageMessage = string.Empty;
                if (usageAttribute != null)
                    usageMessage = usageAttribute.Usage;
                var cmdName = m.Name.ToLower();
                if (!string.IsNullOrEmpty(attribute.Name))
                {
                    cmdName = attribute.Name;
                }

                var contextInstance = instance;
                if (m.IsStatic)
                {
                    contextInstance = null;
                }

                ConsoleCommand command = new ConsoleCommand(cmdName, contextInstance, m);
                command.Help = helpMessage;
                command.Usage = usageMessage;
                command.CommandRuntime = attribute.Runtime;
                RegisterCommand(command);
            }
        }

        public void RegisterCommand(AbstractConsoleCommand command)
        {
            if (_commands.Any(c => c.CommandName.Trim().ToLower().Equals(command.CommandName.Trim().ToLower())))
            {
                LogUtils.LogError("Command: " + command.CommandName + " is already registered");
                return;
            }

            _commands.Add(command);
        }

        public void Eval(string commandLine)
        {
            string cmd = Regex.Split(commandLine, " ").ToArray()[0];
            string[] args = Regex.Split(commandLine, " ").Skip(1).ToArray();

            AbstractConsoleCommand command = _commands.FirstOrDefault(c => c.CommandName.ToLower() == cmd);
            if (command != null && command.CommandRuntime != ConsoleCommandRuntime.NONE)
            {

                switch (command.CommandRuntime)
                {
                    case ConsoleCommandRuntime.CLIENT:
                        if (!Connection.IsClient())
                        {
                            command = null;
                        }
                        break;
                    case ConsoleCommandRuntime.SERVER:
                        if (!Connection.IsServer())
                        {
                            command = null;
                        }
                        break;
                    case ConsoleCommandRuntime.BOTH:
                        if (Connection.CurrentConnection == null)
                        {
                            command = null;
                        }
                        break;
                }
            }
            if (command == null)
            {
                Print("Command \"" + cmd + "\" not found");
                return;
            }
            command.Execute(args);
        }

        private bool InvokeParseCallback(object parser, Type generic, string s, out object obj)
        {
            MethodInfo method = parser.GetType().GetMethod("Invoke");
            object[] parameters = new object[2];
            parameters[0] = s;
            parameters[1] = null;
            bool result = (bool) method.Invoke(parser, parameters);
            obj = parameters[1];
            return result;
        }
    }
}