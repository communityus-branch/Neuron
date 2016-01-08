using System;

namespace Static_Interface.API.EventFramework
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EventHandler : Attribute
    {
        public EventPriority Priority = EventPriority.NORMAL;
        public bool IgnoreCancelled = false;
    }
}