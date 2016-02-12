using System;
using Static_Interface.API.ExtensionFramework;

namespace Static_Interface.API.SchedulerFramework
{
    public class Task
    {
        internal Task()
        {
        }

        public Action Action { get; internal set; }
        public int Id { get; internal set; }
        public Extension Extension { get; internal set; }
        public bool IsAsync { get; internal set; }
        public uint Delay { get; internal set; }
        public uint Period { get; internal set; }
        public uint ScheduledTime { get; internal set; }
        public uint LastRunTime { get; internal set; }

        public bool Pause { get; set; }

        public bool IsCancelled { get; private set; }

        public void Remove()
        {
            IsCancelled = true;
        }
    }
}