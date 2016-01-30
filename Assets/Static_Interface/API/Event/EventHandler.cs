using System;

namespace Static_Interface.API.Event
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EventHandler : Attribute
    {
        public EventPriority Priority = EventPriority.Normal;
        public bool IgnoreCancelled = false;
    }
}