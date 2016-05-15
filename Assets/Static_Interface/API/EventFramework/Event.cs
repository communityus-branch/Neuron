using System;

namespace Static_Interface.API.EventFramework
{
    public abstract class Event
    {
        /// <summary>
        /// Nameo f the event
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// True if the event gets called from an async thread
        /// Notice: You won't be able to use Unity's APIs in Async Events
        /// </summary>
        public bool IsAsync { get; private set; }

        protected Event() : this(false) { }

        /// <param name="isAsync">Should the event be called from an async thread? See <see cref="IsAsync"/></param>
        protected Event(bool isAsync)
        {
            Name = GetType().Name;
            IsAsync = isAsync;
        }

        public void Fire()
        {
            if(EventManager.Instance == null) throw new Exception("EventManager instance is null");
            EventManager.Instance.CallEvent(this);
        }
    }
}