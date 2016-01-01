namespace Assets.API.EventFramework
{
    public abstract class Event
    {
        public string Name { get; private set; }

        private bool _async;

        public bool IsAsync
        {
            get { return _async; }
            set { _async = value; } 
        }

        protected Event() : this(false) { }

        protected Event(bool isAsync)
        {
            Name = GetType().Name;
            IsAsync = isAsync;
        }
    }
}