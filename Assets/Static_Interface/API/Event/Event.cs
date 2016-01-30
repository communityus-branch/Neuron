namespace Static_Interface.API.Event
{
    public abstract class Event
    {
        public string Name { get; private set; }
        public bool IsAsync { get; private set; }

        protected Event() : this(false) { }

        protected Event(bool isAsync)
        {
            Name = GetType().Name;
            IsAsync = isAsync;
        }
    }
}