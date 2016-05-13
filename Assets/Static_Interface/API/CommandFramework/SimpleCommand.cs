using System;
using System.Collections.Generic;
using Fclp;

namespace Static_Interface.API.CommandFramework
{
    public abstract class SimpleCommand<T> : SimpleCommand where T : new()
    {
        protected SimpleCommand()
        {
            _parser = new FluentCommandLineParser<T>();
        }

        public FluentCommandLineParser<T> Parser => _parser;
        private readonly FluentCommandLineParser<T> _parser;

        public sealed override void Execute(ICommandSender sender, string cmdLine, string[] args)
        {
            if (!HasPermission(sender))
            {
                sender.Message("<color=red>You don't have permission</color>");
                return;
            }

            var result = _parser.Parse(args);

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

            if (!OnExecute(sender, cmdLine, args, _parser.Object))
            {
                SendUsage(sender);
            }
        }

        protected override bool OnExecute(ICommandSender sender, string cmdLine, string[] args)
        {
            throw new NotSupportedException();
        }

        protected abstract bool OnExecute(ICommandSender sender, string cmdLine, string[] args, T parsedArgs);
    }

    public abstract class SimpleCommand : ICommand
    {
        public abstract string Name { get; }
        public abstract List<string> Aliases { get; }
        public virtual void OnRegistered()
        {

        }

        public abstract string Permission { get; }
        public virtual bool HasPermission(ICommandSender sender)
        {
            return sender.HasPermission(Permission);
        }

        public virtual void Execute(ICommandSender sender, string cmdLine, string[] args)
        {
            if (!HasPermission(sender))
            {
                sender.Message("<color=#B71C1C>You don't have permission</color>");
                return;
            }

            if (!OnExecute(sender, cmdLine, args))
            {
                SendUsage(sender);
            }
        }

        public void SendUsage(ICommandSender sender)
        {
            string usage = GetUsage(sender);
            if (string.IsNullOrEmpty(usage)) return;
            sender.Message(usage);
        }

        protected abstract bool OnExecute(ICommandSender sender, string cmdLine, string[] args);

        public abstract string GetUsage(ICommandSender sender);
    }
}