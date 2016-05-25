using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Static_Interface.API.CommandFramework
{
    public interface ICommand
    {
        bool HasPermission(ICommandSender sender);
        void Execute(ICommandSender sender, string cmdLine, string[] args);
        string Usage { get; }
        string Description { get; }
        string Name { get; }
        ReadOnlyCollection<string> Aliases { get; }
        void OnRegistered();
    }
}