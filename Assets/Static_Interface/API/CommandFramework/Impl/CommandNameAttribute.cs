using System;

namespace Static_Interface.API.CommandFramework.Impl
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandNameAttribute : Attribute
    {
        public readonly string Name;

        public CommandNameAttribute(string name)
        {
            Name = name;
        }
    }
}