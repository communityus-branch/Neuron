using System;

namespace Static_Interface.API.CommandFramework.Impl
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandDescriptionAttribute : Attribute
    {
        public readonly string Description;

        public CommandDescriptionAttribute(string description)
        {
            Description = description;
        }
    }
}