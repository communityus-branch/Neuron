using System;

namespace Static_Interface.API.CommandFramework.Impl
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandPermissionAttribute : Attribute
    {
        public readonly string Permission;

        public CommandPermissionAttribute(string permission)
        {
            Permission = permission;
        }
    }
}