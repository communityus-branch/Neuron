using System;

namespace Assets.API.EventFramework
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EventHandler : Attribute
    {
        public EventPriority Priority = EventPriority.NORMAL;
        public bool IgnoreCancelled = false;
    }
}