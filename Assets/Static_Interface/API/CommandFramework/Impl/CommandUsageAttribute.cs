using System;

namespace Static_Interface.API.CommandFramework.Impl
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandUsageAttribute : Attribute
    {
        public readonly string Usage;

        public CommandUsageAttribute(string usage)
        {
            Usage = usage;
        }
    }
}