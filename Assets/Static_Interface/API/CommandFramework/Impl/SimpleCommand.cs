using System;
using System.Collections.ObjectModel;
using System.Linq;
using Fclp;

namespace Static_Interface.API.CommandFramework.Impl
{
    public abstract class SimpleCommand<T> : SimpleCommand where T : new()
    {
        protected IHelpCommandLineOptionFluent CommandLineHelp;
        protected SimpleCommand()
        {
            _commandLineParser = new FluentCommandLineParser<T>();
            CommandLineHelp = _commandLineParser.SetupHelp("help");
        }

        public FluentCommandLineParser<T> CommandLineParser => _commandLineParser;
        private readonly FluentCommandLineParser<T> _commandLineParser;

        public sealed override void Execute(ICommandSender sender, string cmdLine, string[] args)
        {
            CommandLineHelp.WithHeader(sender.CommandPrefix + Name + " " + Usage + ": " + Description);

            if (!HasPermission(sender))
            {
                sender.Message("<color=red>You don't have permission</color>");
                return;
            }

            var result = _commandLineParser.Parse(args);

            if (result.HasErrors)
            {
                SendUsage(sender);
                if (string.IsNullOrEmpty(result.ErrorText)) return;
                sender.Message("<b>" + result.ErrorText + "</b>");
                return;
            }

            if (result.HelpCalled)
            {
                SendUsage(sender);
                return;
            }

            if (!OnExecute(sender, cmdLine, args, _commandLineParser.Object))
            {
                SendUsage(sender);
            }
        }

        protected sealed override bool OnExecute(ICommandSender sender, string cmdLine, string[] args)
        {
            throw new NotSupportedException("Please call OnExecute(ICommandSender sender, string cmdLine, string[] args, T parsedArgs) instead!");
        }

        protected abstract bool OnExecute(ICommandSender sender, string cmdLine, string[] args, T parsedArgs);
    }

    public abstract class SimpleCommand : ICommand
    {
        public virtual void OnRegistered()
        {

        }

        public virtual bool HasPermission(ICommandSender sender)
        {
            return sender.HasPermission(Permission);
        }

        public virtual void Execute(ICommandSender sender, string cmdLine, string[] args)
        {
            if (!OnExecute(sender, cmdLine, args))
            {
                SendUsage(sender);
            }
        }
        public virtual void SendUsage(ICommandSender sender)
        {
            if (string.IsNullOrEmpty(Usage)) return;
            sender.Message(Usage);
        }

        protected abstract bool OnExecute(ICommandSender sender, string cmdLine, string[] args);

        public ReadOnlyCollection<string> Aliases
        {
            get
            {
                var attrs = GetType().GetCustomAttributes(typeof(CommandAliasesAttribute), true);
                if (attrs.Length < 1) return null;
                return ((CommandAliasesAttribute)attrs[0]).Aliases.ToList().AsReadOnly();
            }
        }

        public string Name
        {
            get
            {
                var attrs = GetType().GetCustomAttributes(typeof(CommandNameAttribute), true);
                if (attrs.Length < 1) throw new Exception(GetType().FullName + " does not have CommandNameAttribute set!");
                return ((CommandNameAttribute)attrs[0]).Name;
            }
        }

        public string Usage
        {
            get
            {
                var attrs = GetType().GetCustomAttributes(typeof(CommandUsageAttribute), true);
                return attrs.Length < 1 ? null : ((CommandUsageAttribute)attrs[0]).Usage;
            }
        }

        public string Description
        {
            get
            {
                var attrs = GetType().GetCustomAttributes(typeof (CommandDescriptionAttribute), true);
                return attrs.Length < 1 ? null : ((CommandDescriptionAttribute)attrs[0]).Description;
            }
        }

        public string Permission
        {
            get
            {
                var attrs = GetType().GetCustomAttributes(typeof(CommandPermissionAttribute), true);
                return attrs.Length < 1 ? null : ((CommandPermissionAttribute)attrs[0]).Permission;
            }
        }
    }
}