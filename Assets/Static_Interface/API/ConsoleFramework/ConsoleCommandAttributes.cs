using System;

namespace Static_Interface.API.ConsoleFramework
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ConsoleCommandAttribute : Attribute
    {
        public string Name = null;
        public ConsoleCommandRuntime Runtime = ConsoleCommandRuntime.BOTH;
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CommandHelpAttribute : Attribute
    {
        public readonly string Help;
        public CommandHelpAttribute(string help)
        {
            Help = help;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CommandUsageAttribute : Attribute
    {
        public readonly string Usage;
        public CommandUsageAttribute(string usage)
        {
            Usage = usage;
        }
    }

    public enum ConsoleCommandRuntime
    {
        CLIENT,
        SERVER,
        BOTH,
        NONE
    }
}