using System.Collections.Generic;

namespace Static_Interface.API.CommandFramework
{
    public interface ICommand
    {
        bool HasPermission(ICommandSender sender);
        void Execute(ICommandSender sender, string cmdLine, string[] args);
        string GetUsage(ICommandSender sender);
        string Name { get; }
        List<string> Aliases { get; }
        void OnRegistered();
    }
}