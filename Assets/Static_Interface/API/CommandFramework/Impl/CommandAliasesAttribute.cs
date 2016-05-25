using System;

namespace Static_Interface.API.CommandFramework.Impl
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandAliasesAttribute : Attribute
    {
        public readonly string[] Aliases;

        public CommandAliasesAttribute(string[] aliases)
        {
            Aliases = aliases;
        }
    }
}