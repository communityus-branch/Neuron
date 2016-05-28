using System;
using System.Collections.Generic;
using System.Linq;
using Static_Interface.API.NetworkFramework;
using Static_Interface.API.PluginFramework;
using Static_Interface.API.UnityExtensions;
using Static_Interface.API.Utils;
using Static_Interface.Internal.MultiplayerFramework;

namespace Static_Interface.API.CommandFramework
{
    public class CommandManager : SingletonComponent<CommandManager>
    {
        protected internal override bool ForceSafeDestroy => true;
        private readonly Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>(); 
        private readonly Dictionary<Plugin, List<ICommand>> _extCommands = new Dictionary<Plugin, List<ICommand>>(); 

        protected override void Awake()
        {
            base.Awake();
            if (Connection.IsServer()) return;
            Destroy(this);
            throw new Exception("CommandManager can only init on server-side");
        }

        public void RegisterCommand(ICommand command, Plugin ext)
        {
            if (!NetworkUtils.IsServer()) return;
            LogUtils.Debug(nameof(RegisterCommand));
            if (!ext.Enabled) return;
            RegisterCommand(command, command.Name, ext);
            if (command.Aliases == null) return;
            foreach (var alias in command.Aliases)
            {
                RegisterCommand(command, alias, ext);
            }
        }

        public void UnregisterCommand(ICommand command)
        {
            //http://answers.unity3d.com/answers/928540/view.html
            var keys = _commands.Keys;
            var toRemove = keys.Where(cmdName => _commands[cmdName] == command).ToList();
            foreach (string commandName in toRemove)
            {
                _commands.Remove(commandName);
            }
        }

        public void UnregisterCommand(string cmdName)
        {
            if (!_commands.ContainsKey(cmdName)) return;
            _commands.Remove(cmdName);
        }

        internal void RegisterCommand(ICommand command, string cmdName, Plugin plugin)
        {
            LogUtils.Debug("Registering command: " + cmdName);
            if (GetCommand(cmdName) != null)
            {
                LogUtils.LogError("Command or command alias \"" + cmdName + "\" already exists!");
                return;
            }
            _commands.Add(cmdName, command);
            command.OnRegistered();

            if (plugin == null) return;
            if (!_extCommands.ContainsKey(plugin))
            {
                _extCommands.Add(plugin, new List<ICommand>());
            }

            var extCmds = _extCommands[plugin];
            extCmds.Add(command);
        }

        protected override void OnDestroySafe()
        {
            base.OnDestroySafe();
            _commands.Clear();
            _extCommands.Clear();
        }

        internal void OnPluginDisabled(Plugin plugin)
        {
            if (!_extCommands.ContainsKey(plugin)) return;
            var cmds = _extCommands[plugin];
            foreach (ICommand cmd in cmds)
            {
                UnregisterCommand(cmd);
            }
            _extCommands.Remove(plugin);
        }

        public ICommand GetCommand(string cmdName)
        {
            return (from cmd in _commands.Keys where string.Equals(cmd, cmdName, StringComparison.CurrentCultureIgnoreCase) select _commands[cmd]).FirstOrDefault();
        }

        public void Eval(ICommandSender sender, string cmd, string cmdLine)
        {
            LogUtils.Log(sender.Name + " executed command: " + cmdLine);
            var cmdInstance = GetCommand(cmd);
            if (cmdInstance == null)
            {
                sender.Message("<color=#B71C1C>Command \"" +  cmd + "\" not found</color>");
                return;
            }

            string[] parsedArgs = {};
            if (cmdLine.Trim().Length > cmd.Length)
            {
                parsedArgs = StringUtils.ToArguments(cmdLine.Substring(cmd.Length + 1));
            }

            if (!cmdInstance.HasPermission(sender))
            {
                sender.Message("<color=#B71C1C>You don't have permission</color>");
                return;
            }
            cmdInstance.Execute(sender, cmdLine, parsedArgs);
        }
    }
}